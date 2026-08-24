using System;
using System.IO;
using System.Reflection;

namespace YoutubeToMP3.BusinessLogic
{
    public static class BinaryExtractor
    {
        private static readonly string BinRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "YoutubeToMP3", "bin");

        // Binaries live under the app version so an upgrade never inherits the previous yt-dlp,
        // and outside %TEMP% so "yt-dlp -U" can replace the binary in place.
        private static readonly string BinDir = Path.Combine(
            BinRoot, Assembly.GetExecutingAssembly().GetName().Version!.ToString());

        public static string YtDlpPath { get; private set; } = string.Empty;
        public static string FfmpegPath { get; private set; } = string.Empty;

        public static void Extract()
        {
            Directory.CreateDirectory(BinDir);
            YtDlpPath = ExtractResource("yt-dlp.exe");
            FfmpegPath = ExtractResource("ffmpeg.exe");
            PruneStaleBinaries();
        }

        private static string ExtractResource(string resourceName)
        {
            string outputPath = Path.Combine(BinDir, resourceName);
            string stampPath = outputPath + ".stamp";

            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");

            // The stamp tracks the embedded payload, not the file on disk: a binary updated in place
            // by "yt-dlp -U" keeps its stamp and must not be rolled back to the bundled version.
            string stamp = stream.Length.ToString();
            if (File.Exists(outputPath) && ReadStamp(stampPath) == stamp)
                return outputPath;

            using (var file = File.Create(outputPath))
                stream.CopyTo(file);

            File.WriteAllText(stampPath, stamp);

            return outputPath;
        }

        private static string? ReadStamp(string stampPath)
        {
            try
            {
                return File.Exists(stampPath) ? File.ReadAllText(stampPath).Trim() : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Drops binaries from previous app versions; they weigh ~70 MB per version.</summary>
        private static void PruneStaleBinaries()
        {
            TryDelete(Path.Combine(Path.GetTempPath(), "YoutubeToMP3"));

            try
            {
                foreach (string dir in Directory.GetDirectories(BinRoot))
                    if (!string.Equals(dir, BinDir, StringComparison.OrdinalIgnoreCase))
                        TryDelete(dir);
            }
            catch
            {
                // Pruning is best effort: a locked folder from a second running instance is not an error.
            }
        }

        private static void TryDelete(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, recursive: true);
            }
            catch
            {
                // Same rationale as PruneStaleBinaries.
            }
        }
    }
}
