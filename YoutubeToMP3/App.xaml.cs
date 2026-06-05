using System;
using System.Windows;
using System.Windows.Threading;
using YoutubeToMP3.BusinessLogic;

namespace YoutubeToMP3
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                AppSettings.Load();
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

        private static void ShowError(Exception ex) =>
            MessageBox.Show($"An error occurred:\n\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
