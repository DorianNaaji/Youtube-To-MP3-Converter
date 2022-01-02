using YoutubeToMP3.BusinessLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

#pragma warning disable CS4014
namespace YoutubeToMP3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class YoutubeToMP3Form : Window
    {
        public YoutubeToMP3Form()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;
            this._tbTip.IsReadOnly = true;
            this._tbTip.IsTabStop = false;
            this._tbLink.TextWrapping = TextWrapping.WrapWithOverflow;
            this._tbLink.AcceptsReturn = true;
            this.label_version.Content = "Version : " + typeof(YoutubeToMP3Form).Assembly.GetName().Version;
        }

        private readonly List<String> Urls = new List<string>();

        private async void _convertButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            this._progressBar.SetPercentFast(0);
            this.UpdateLayout();
            double cpt = 0;
            List<string> undownloadedFiles = new List<string>();

            for (int i = 0; i < Urls.Count; i++)
            {
                try
                {
                    Process p = await Task.Run(() => Converter.DownloadWebmAudio(this.Urls[i]));
                    double di = (double)i;
                    String currentUrl = this.Urls[i];
                    //this._progressBar.SetPercentDefault(((double)i / (double)this.Urls.Count) * 100);
                    //if(i == Urls.Count - 1)
                    //{
                    //    this._progressBar.SetPercentFast(100);
                    //    this.IsEnabled = true;
                    //}
                    Task.Run(() => // Task to update progress bar
                    {
                        while (!p.HasExited) { } // Waits for the process to exit in another thread
                        cpt++;
                        this.Dispatcher.Invoke(() =>
                        {
                            this._progressBar.SetPercentDefault(cpt/this.Urls.Count * 100);
                            Console.WriteLine(cpt / this.Urls.Count * 100);
                        });
                    });
                }
                catch (Exception err)
                {
                    MessageBox.Show("Oops.. Something went wrong with the following url :\n" + this.Urls[i] + "\n" +
                        "No choice but not to download it ! :(. Press Ok to keep downloading.\n\n\n" + err, "Oops...", MessageBoxButton.OK, MessageBoxImage.Error);
                    undownloadedFiles.Add(this.Urls[i]);
                    Console.WriteLine(err);
                }
            }
            if (!(undownloadedFiles.Count == 0))
            {
                String txtPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\ConvertedMp3.Logs.txt";
                File.WriteAllLines(txtPath, undownloadedFiles.ToArray());
            }
        }

        private void _checkLinksButton_Click(object sender, RoutedEventArgs e)
        {
            this.Urls.Clear();
            if (this.ParseTextBox())
            {
                this._convertButton.Visibility = Visibility.Visible;
            }
            else
            {
                this.Urls.Clear();
                MessageBox.Show("The given links contain errors. Please enter one youtube link per line.\n", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool ParseTextBox()
        {
            TextReader reader = new StringReader(this._tbLink.Text);
            while (reader.Peek() >= 0)
            {
                string youtubeLink = reader.ReadLine();
                if (youtubeLink.IsYoutubeLinkValid())
                {
                    this.Urls.Add(youtubeLink);
                }
                else
                {
                    return false;
                }
            }
            return this.Urls.Count > 0;
        }

        private void _tbLink_TextChanged(object sender, TextChangedEventArgs e)
        {
            this._convertButton.Visibility = Visibility.Hidden;
        }
    }
}
