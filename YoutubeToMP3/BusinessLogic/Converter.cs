using System;
using System.Diagnostics;
using System.IO;

namespace YoutubeToMP3.BusinessLogic
{
    public static class Converter
    {
        public static readonly string OutputFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ConvertedMp3");

        public static void SetEnvironment()
        {
            Directory.CreateDirectory(OutputFolder);
        }

        public static Process DownloadAsMp3(string url)
        {
            string args = $"-x --audio-format mp3 --audio-quality 0 " +
                          $"--ffmpeg-location \"{BinaryExtractor.FfmpegPath}\" " +
                          $"-o \"{OutputFolder}/%(title)s.%(ext)s\" \"{url}\"";

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
