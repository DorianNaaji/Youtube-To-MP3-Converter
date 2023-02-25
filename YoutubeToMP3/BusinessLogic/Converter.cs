using System;
using System.IO;
using NReco.VideoConverter;
using System.Diagnostics;
using YoutubeToMP3Form.BusinessLogic;
using System.Collections.Generic;

namespace YoutubeToMP3.BusinessLogic
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

        public static List<String> GetAllConvertedFiles()
        {
            DirectoryInfo d = new DirectoryInfo(_pathToFolder);
            FileInfo[] files = d.GetFiles();

            List<String> filesList = new List<String>();
            foreach (FileInfo fileInfo in files)
            {
                filesList.Add(fileInfo.Name);
            }

            return filesList;
        }

        public static void ConvertFile(String filename)
        {
            _converter.ConvertMedia(_pathToFolder + filename, _pathToFolder + filename + ".mp3", ".mp3");
        }

        public static Process DownloadWebmAudio(string url)
        {
            return Converter.InitProcess("lib\\yt-dlp.exe", 
                " -f bestaudio  --extract-audio --audio-format mp3 --audio-quality 0 " + url + " -o " + _pathToFolder + "\\%(title)s.%(ext)s");
        }

        private static Process InitProcess(String fileName, String args)
        {
            Process process = new Process();
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
