using System;
using System.Windows;
using YoutubeToMP3.BusinessLogic;

namespace YoutubeToMP3
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                BinaryExtractor.Extract();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to initialize required components:\n\n{ex.Message}\n\nThe application will now close.",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
                return;
            }

            base.OnStartup(e);
        }
    }
}
