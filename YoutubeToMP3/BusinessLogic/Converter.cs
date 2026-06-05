using System.Diagnostics;
using System.IO;

namespace YoutubeToMP3.BusinessLogic
{
    public static class Converter
    {
        public static void SetEnvironment()
        {
            if (AppSettings.IsOutputFolderCustomized || !Directory.Exists(AppSettings.OutputFolder))
                Directory.CreateDirectory(AppSettings.OutputFolder);
        }

        public static Process DownloadAsMp3(string url)
        {
            string thumbnail = AppSettings.EmbedThumbnail ? "--embed-thumbnail " : "";
            string metadata  = AppSettings.EmbedMetadata  ? "--embed-metadata "  : "";
            string args = $"-x --audio-format mp3 --audio-quality 0 " +
                          $"{thumbnail}{metadata}" +
                          $"--ffmpeg-location \"{BinaryExtractor.FfmpegPath}\" " +
                          $"-o \"{AppSettings.OutputFolder}/%(title)s.%(ext)s\" \"{url}\"";

            return StartProcess(BinaryExtractor.YtDlpPath, args);
        }

        private static Process StartProcess(string fileName, string args)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = args;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return process;
        }
    }
}
