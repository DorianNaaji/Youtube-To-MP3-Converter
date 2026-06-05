using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using YoutubeToMP3.BusinessLogic;

namespace YoutubeToMP3
{
    public partial class App : Application
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "YoutubeToMP3.log");

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                BinaryExtractor.Extract();
            }
            catch (Exception ex)
            {
                ShowError(ex);
                Shutdown(1);
                return;
            }

            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ShowError(e.Exception);
            Shutdown(1);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                ShowError(ex);
        }

        private static void ShowError(Exception ex)
        {
            try { File.AppendAllText(LogPath, $"[{DateTime.Now}]\n{ex}\n\n"); } catch { }

            MessageBox.Show(
                $"An error occurred:\n\n{ex.Message}\n\nFull details saved to:\n{LogPath}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
