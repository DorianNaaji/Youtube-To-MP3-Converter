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
using VideoLibrary;
using YouTubeSearch;

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

        private static Task ConvertYoutubeMp4ToMp3(string pathToOldFile)
        {
            return Task.Run(() =>
            {
                _converter.ConvertMedia(pathToOldFile, pathToOldFile.Replace(".mp4", "") + ".mp3", "mp3");
                File.Delete(pathToOldFile);
            });
        }

        public static async Task DownloadAndConvertYoutubeToLocal(string url)
        {
            //{
            //    VideoSearch vs = new VideoSearch();
            //    IEnumerable<YouTubeSearch.VideoInfo> videoInfos = YouTubeSearch.DownloadUrlResolver.GetDownloadUrls(url, false);

            //    VideoInfo video = videoInfos
            //        .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);

            //    if (video.RequiresDecryption)
            //    {
            //        DownloadUrlResolver.DecryptDownloadUrl(video);
            //    }

            //    VideoDownloader dl = new VideoDownloader();
            //    dl.DownloadFile(video.DownloadUrl, Converter.CleanFileName(video.Title), true, _pathToFolder, ".mp4" +
            //        "");

            //    String pathToOldFile = Path.Combine(_pathToFolder, Converter.CleanFileName(video.Title)) + ".mp4";
            //await ConvertYoutubeMp4ToMp3(pathToOldFile);

            //string source = _pathToFolder;
            //var youtube = YouTube.Default;
            //var vid = youtube.GetVideo(url);

            //string cleanFileName = Converter.CleanFileName(vid.FullName);
            //string filePath = source + cleanFileName;
            //File.WriteAllBytes(filePath, vid.GetBytes());

            //var inputFile = new MediaFile
            //{
            //    Filename = filePath
            //};

            //var outputFile = new MediaFile
            //{
            //    Filename = filePath + ".mp3"
            //};

            //using (Engine engine = new Engine())
            //{
            //    engine.GetMetadata(inputFile);

            //    engine.Convert(inputFile, outputFile);
            //}

            YoutubeClient youtube = new YoutubeClient();
            string manifest = url.Substring(url.LastIndexOf("/")).Replace("/watch?v=", "");

            StreamManifest StreamManifest = await youtube.Videos.Streams.GetManifestAsync(manifest);
            IStreamInfo streamInfo = StreamManifest.GetAudioOnly().WithHighestBitrate();
            Stream stream = await youtube.Videos.Streams.GetAsync(streamInfo);

            YoutubeExplode.Videos.Video vid = await youtube.Videos.GetAsync(url);
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
