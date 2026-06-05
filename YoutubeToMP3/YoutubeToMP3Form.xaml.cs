using YoutubeToMP3.BusinessLogic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace YoutubeToMP3
{
    public partial class YoutubeToMP3Form : Window
    {
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

            var failedUrls = new ConcurrentBag<string>();
            int completed = 0;
            int total = _urls.Count;
            var semaphore = new SemaphoreSlim(AppSettings.ConcurrentDownloads);

            var tasks = _urls.Select(async url =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var process = Converter.DownloadAsMp3(url);
                    await Task.Run(() => process.WaitForExit());
                    if (process.ExitCode != 0)
                        failedUrls.Add(url);
                }
                catch
                {
                    failedUrls.Add(url);
                }
                finally
                {
                    semaphore.Release();
                    int done = Interlocked.Increment(ref completed);
                    Dispatcher.Invoke(() => _progressBar.SetPercentDefault(done * 100.0 / total));
                }
            });

            await Task.WhenAll(tasks);

            if (!failedUrls.IsEmpty)
            {
                string logPath = Path.Combine(AppSettings.OutputFolder, "ConvertedMp3.Logs.txt");
                File.WriteAllLines(logPath, failedUrls);
                MessageBox.Show(
                    $"{failedUrls.Count} download(s) failed. URLs saved to:\n{logPath}",
                    "Partial failure", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            IsEnabled = true;
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
