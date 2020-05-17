using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NReco.VideoConverter;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using MediaToolkit.Model;
using MediaToolkit;
using YoutubeExplode.Videos;
using System.Collections.Generic;

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
            YoutubeClient youtube = new YoutubeClient();
            string manifest = url.Substring(url.LastIndexOf("/")).Replace("/watch?v=", "");

            StreamManifest StreamManifest = await youtube.Videos.Streams.GetManifestAsync(manifest);
            IStreamInfo streamInfo = StreamManifest.GetAudioOnly().WithHighestBitrate();
            Stream stream = await youtube.Videos.Streams.GetAsync(streamInfo);

            Video vid = await youtube.Videos.GetAsync(url);
            string cleanFileName = Converter.CleanFileName(vid.Title);
            string baseFilePath = _pathToFolder + cleanFileName;
            using (var fileStream = File.Create(baseFilePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
            var inputFile = new MediaFile
            {
                Filename = _pathToFolder + cleanFileName
            };
            var outputFile = new MediaFile
            {
                Filename = $"{_pathToFolder + cleanFileName}.mp3"
            };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);

                engine.Convert(inputFile, outputFile);
            }

            File.Delete(baseFilePath);
        }


        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), ""));
        }
    }
}
