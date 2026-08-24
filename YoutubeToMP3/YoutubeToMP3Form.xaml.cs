using YoutubeToMP3.BusinessLogic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace YoutubeToMP3
{
    public partial class YoutubeToMP3Form : Window
    {
        private const int MaxUrlsShownPerReason = 5;

        private readonly List<string> _urls = new();
        private MenuItem[] _dlItems = null!;

        public YoutubeToMP3Form()
        {
            InitializeComponent();
            var v = Assembly.GetExecutingAssembly().GetName().Version!;
            label_version.Text = $"v{v.Major}.{v.Minor}.{v.Build}";
            _outputLabel.Text = $"Output: {AppSettings.OutputFolder}";
            _dlItems = new[] { _dl1, _dl2, _dl3, _dl4, _dl5 };
            foreach (var mi in _dlItems)
                mi.IsChecked = int.Parse((string)mi.Tag) == AppSettings.ConcurrentDownloads;
            _thumbnailItem.IsChecked = AppSettings.EmbedThumbnail;
            _metadataItem.IsChecked  = AppSettings.EmbedMetadata;
            Converter.SetEnvironment();
        }

        private void OutputFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select output folder",
                InitialDirectory = AppSettings.OutputFolder
            };
            if (dialog.ShowDialog() == true)
            {
                AppSettings.SetOutputFolder(dialog.FolderName);
                _outputLabel.Text = $"Output: {dialog.FolderName}";
                Converter.SetEnvironment();
            }
        }

        private void Downloads_Click(object sender, RoutedEventArgs e)
        {
            var clicked = (MenuItem)sender;
            int value = int.Parse((string)clicked.Tag);
            foreach (var mi in _dlItems)
                mi.IsChecked = mi == clicked;
            AppSettings.SetConcurrentDownloads(value);
        }

        private void Thumbnail_Click(object sender, RoutedEventArgs e) =>
            AppSettings.SetEmbedThumbnail(_thumbnailItem.IsChecked);

        private void Metadata_Click(object sender, RoutedEventArgs e) =>
            AppSettings.SetEmbedMetadata(_metadataItem.IsChecked);

        private async void _convertButton_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            _progressBar.SetPercentFast(0);

            var failures = new ConcurrentBag<DownloadResult>();
            int completed = 0;
            int total = _urls.Count;
            var semaphore = new SemaphoreSlim(AppSettings.ConcurrentDownloads);

            var tasks = _urls.Select(async url =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var result = await Converter.DownloadAsMp3Async(url);
                    if (!result.Success)
                        failures.Add(result);
                }
                catch (Exception ex)
                {
                    failures.Add(new DownloadResult
                    {
                        Url = url,
                        ExitCode = -1,
                        Output = $"ERROR: {ex.GetType().Name}: {ex.Message}"
                    });
                }
                finally
                {
                    semaphore.Release();
                    int done = Interlocked.Increment(ref completed);
                    Dispatcher.Invoke(() => _progressBar.SetPercentDefault(done * 100.0 / total));
                }
            });

            await Task.WhenAll(tasks);

            if (!failures.IsEmpty)
                ReportFailures(failures.ToList());

            IsEnabled = true;
        }

        private static void ReportFailures(IReadOnlyList<DownloadResult> failures)
        {
            DownloadLog.WriteFailures(failures);

            var message = new StringBuilder();
            message.AppendLine($"{failures.Count} download(s) failed.");
            message.AppendLine();

            // Grouping by reason keeps a 50-URL batch that failed for one cause readable.
            foreach (var group in failures.GroupBy(f => f.Reason))
            {
                message.AppendLine($"- {group.Key}");
                foreach (var failure in group.Take(MaxUrlsShownPerReason))
                    message.AppendLine($"    {failure.Url}");

                int hidden = group.Count() - MaxUrlsShownPerReason;
                if (hidden > 0)
                    message.AppendLine($"    (+{hidden} more)");

                message.AppendLine();
            }

            message.AppendLine("Open the full log?");

            var answer = MessageBox.Show(message.ToString(), "Download failures",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (answer == MessageBoxResult.Yes)
                OpenLog();
        }

        private static void OpenLog()
        {
            try
            {
                Process.Start(new ProcessStartInfo(DownloadLog.FilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open the log:\n\n{DownloadLog.FilePath}\n\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdateYtDlp_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            try
            {
                var result = await Converter.UpdateYtDlpAsync();
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(result.Output) ? "yt-dlp is already up to date." : result.Output,
                    result.Success ? "yt-dlp update" : "yt-dlp update failed",
                    MessageBoxButton.OK,
                    result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not run the updater:\n\n{ex.Message}",
                    "yt-dlp update failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void _checkLinksButton_Click(object sender, RoutedEventArgs e)
        {
            _urls.Clear();
            if (ParseTextBox())
            {
                _convertButton.Visibility = Visibility.Visible;
            }
            else
            {
                _urls.Clear();
                MessageBox.Show(
                    "One or more links are invalid.\nSupported: youtube.com/watch, /shorts, /playlist, music.youtube.com, youtu.be",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool ParseTextBox()
        {
            using var reader = new StringReader(_tbLink.Text);
            while (reader.Peek() >= 0)
            {
                string? line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.IsYoutubeLinkValid())
                    _urls.Add(line);
                else
                    return false;
            }
            return _urls.Count > 0;
        }

        private void _tbLink_TextChanged(object sender, TextChangedEventArgs e)
        {
            _convertButton.Visibility = Visibility.Hidden;
        }
    }
}
