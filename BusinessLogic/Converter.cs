using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoLibrary;
using NReco.VideoConverter;

namespace BusinessLogic
{
    public static class Converter
    {
        private static readonly string _pathToFolder =
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\ConvertedMp3\";

        private static readonly FFMpegConverter _converter = new FFMpegConverter();

        public static void SetEnvironment()
        {
            Directory.CreateDirectory(_pathToFolder);
        }

        public static async Task DownloadAndConvertYoutubeToLocal(string url)
        {
            YouTube yt = YouTube.Default;
            YouTubeVideo video =  await yt.GetVideoAsync(url);
            string fileName = Converter.CleanFileName(video.Title.Replace(" - YouTube", ""));
            string fullPath = _pathToFolder + fileName + ".temp";
            File.WriteAllBytes(fullPath, video.GetBytes());
            await ConvertYoutubeMp4ToMp3(fullPath);
        }

        private static Task ConvertYoutubeMp4ToMp3(string pathToOldFile)
        {
            return Task.Run(() =>
            {
                _converter.ConvertMedia(pathToOldFile, pathToOldFile.Replace(".temp", "") + ".mp3", "mp3");
                File.Delete(pathToOldFile);
            });
        }

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), ""));
        }
    }
}
