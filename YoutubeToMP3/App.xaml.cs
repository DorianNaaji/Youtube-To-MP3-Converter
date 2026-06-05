using System.Windows;
using YoutubeToMP3.BusinessLogic;

namespace YoutubeToMP3
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            BinaryExtractor.Extract();
            base.OnStartup(e);
        }
    }
}
