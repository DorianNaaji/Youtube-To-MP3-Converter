using System;
using System.Windows;

namespace YoutubeToMP3
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            try
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fatal error:\n\n{ex.Message}", "Fatal Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
