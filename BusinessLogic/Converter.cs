using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;

namespace BusinessLogic
{
    public static class Converter
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
            string fileName = Converter.CleanFileName(video.Title.Replace(" - YouTube", ""));
            File.WriteAllBytes(pathToFolder +  fileName + ".mp3", video.GetBytes());
        }

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), ""));
        }
    }
}
