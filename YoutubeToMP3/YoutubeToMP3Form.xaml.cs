using YoutubeToMP3.BusinessLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace YoutubeToMP3
{
    public partial class YoutubeToMP3Form : Window
    {
        private readonly List<string> _urls = new();

        public YoutubeToMP3Form()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            _tbLink.TextWrapping = TextWrapping.WrapWithOverflow;
            _tbLink.AcceptsReturn = true;
            label_version.Content = "Version : " + typeof(YoutubeToMP3Form).Assembly.GetName().Version;
            Converter.SetEnvironment();
        }

        private async void _convertButton_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            _progressBar.SetPercentFast(0);

            var failedUrls = new List<string>();

            for (int i = 0; i < _urls.Count; i++)
            {
                try
                {
                    var process = Converter.DownloadAsMp3(_urls[i]);
                    await Task.Run(() => process.WaitForExit());
                }
                catch (Exception err)
                {
                    failedUrls.Add(_urls[i]);
                    MessageBox.Show(
                        $"Something went wrong with:\n{_urls[i]}\n\nPress OK to continue.\n\n{err}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                _progressBar.SetPercentDefault((i + 1.0) / _urls.Count * 100);
            }

            if (failedUrls.Count > 0)
            {
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "ConvertedMp3.Logs.txt");
                File.WriteAllLines(logPath, failedUrls);
                MessageBox.Show(
                    $"Some downloads failed. Failed URLs saved to:\n{logPath}",
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
                    "The links contain errors.\nPlease enter one YouTube link per line (e.g. https://www.youtube.com/watch?v=...).",
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
