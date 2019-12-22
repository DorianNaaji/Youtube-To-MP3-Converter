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
            Converter.SetEnvironment();
        }

        private List<string> Urls = new List<string>();

        private async void _convertButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            for (int i = 0; i < Urls.Count; i++)
            {
                await Task.Run(() => Converter.ConvertYoutubeToMP3(this.Urls[i]));
                this._progressBar.SetPercent(((double)i / (double)this.Urls.Count) * 100);
            }
            this._progressBar.SetPercent(100);
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
