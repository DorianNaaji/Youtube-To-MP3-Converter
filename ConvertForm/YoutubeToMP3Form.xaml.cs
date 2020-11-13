using BusinessLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConvertForm
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
            Converter.SetEnvironment();
        }


        private List<string> Urls = new List<string>();

        private async void _convertButton_Click(object sender, RoutedEventArgs e)
        {
            this._progressBar.SetPercentFast(0);
            this.UpdateLayout();
            this.IsEnabled = false;
            List<string> undownloadedFiles = new List<string>();
            for (int i = 0; i < Urls.Count; i++)
            {
                try
                {
                    await Task.Run(() => Converter.DownloadAndConvertYoutubeToLocal(this.Urls[i]));
                    this._progressBar.SetPercentDefault(((double)i / (double)this.Urls.Count) * 100);
                }
                catch (Exception err)
                {
                    MessageBox.Show("Oops.. Something went wrong with the following url :\n" + this.Urls[i] + "\n" +
                        "No choice but not to download it ! :(.\n" + err, "Oops...", MessageBoxButton.OK, MessageBoxImage.Error);
                    undownloadedFiles.Add(this.Urls[i]);
                    Console.WriteLine(err);
                }
            }
            if(!(undownloadedFiles.Count == 0))
            {
                String txtPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\ConvertedMp3.Logs.txt";
                File.WriteAllLines(txtPath, undownloadedFiles.ToArray());
            }
            this._progressBar.SetPercentFast(100);
            this.IsEnabled = true;
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
                MessageBox.Show("The given links present errors. Please enter one youtube link per line.\n", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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
