using System;
using System.IO;
using System.Reflection;

namespace YoutubeToMP3.BusinessLogic
{
    public static class BinaryExtractor
    {
        private static readonly string TempDir =
            Path.Combine(Path.GetTempPath(), "YoutubeToMP3");

        public static string YtDlpPath { get; private set; } = string.Empty;
        public static string FfmpegPath { get; private set; } = string.Empty;

        public static void Extract()
        {
            Directory.CreateDirectory(TempDir);
            YtDlpPath = ExtractResource("yt-dlp.exe");
            FfmpegPath = ExtractResource("ffmpeg.exe");
        }

        private static string ExtractResource(string resourceName)
        {
            string outputPath = Path.Combine(TempDir, resourceName);
            if (File.Exists(outputPath))
                return outputPath;

            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");

            using var file = File.Create(outputPath);
            stream.CopyTo(file);

            return outputPath;
        }
    }
}
