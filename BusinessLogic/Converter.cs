using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NReco.VideoConverter;
using System.Collections.Generic;
using VideoLibrary;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

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
            //YouTube yt = YouTube.Default;
            //YouTubeVideo video = await yt.GetVideoAsync(url);
            //string fileName = Converter.CleanFileName(video.Title.Replace(" - YouTube", ""));
            //string fullPath = _pathToFolder + fileName + ".temp";
            //File.WriteAllBytes(fullPath, video.GetBytes());
            //await ConvertYoutubeMp4ToMp3(fullPath);
            //IEnumerable <VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(url);
            //VideoInfo video = videoInfos
            //    .Where(info => info.CanExtractAudio)
            //    .OrderByDescending(info => info.AudioBitrate)
            //    .First();

            //if(video.RequiresDecryption)
            //{
            //    DownloadUrlResolver.DecryptDownloadUrl(video);
            //}

            //AudioDownloader audioDownloader = new AudioDownloader(video, Path.Combine(_pathToFolder + video.Title + video.AudioExtension));

            //return Task.Run(() =>
            //{
            //    audioDownloader.Execute();
            //});

            YoutubeClient youtube = new YoutubeClient();

            var video = await youtube.Videos.GetAsync(url);
            string manifest = url.Substring(url.LastIndexOf("/")).Replace("/watch?v=", "");

            StreamManifest StreamManifest = await youtube.Videos.Streams.GetManifestAsync(manifest);
            IStreamInfo streamInfo = StreamManifest.GetAudioOnly().WithHighestBitrate();
            Stream stream = await youtube.Videos.Streams.GetAsync(streamInfo);

            string fileName = Converter.CleanFileName(video.Title.Replace(" - YouTube", ""));
            string fullPath = _pathToFolder + fileName + ".mp3";
            await youtube.Videos.Streams.DownloadAsync(streamInfo, fullPath);

        }

        //private static Task ConvertYoutubeMp4ToMp3(string pathToOldFile)
        //{
        //    return Task.Run(() =>
        //    {
        //        _converter.ConvertMedia(pathToOldFile, pathToOldFile.Replace(".temp", "") + ".mp3", "mp3");
        //        File.Delete(pathToOldFile);
        //    });
        //}

        private static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), ""));
        }
    }
}
