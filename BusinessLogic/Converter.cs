using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;

namespace BusinessLogic
{
    public class Converter
    {
        private static readonly string pathToFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\ConvertedMp3\";

        public static void SetEnvironment()
        {
            Directory.CreateDirectory(pathToFolder);
        }

        public static async Task ConvertYoutubeToMP3(string url)
        {
            YouTube yt = YouTube.Default;
            YouTubeVideo video =  await yt.GetVideoAsync(url);
            File.WriteAllBytes(pathToFolder + video.Title.Replace(" - YouTube", "") + ".mp3", video.GetBytes());
        }
    }
}
