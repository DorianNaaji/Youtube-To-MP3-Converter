using System;
using System.IO;
using System.Text.Json;

namespace YoutubeToMP3.BusinessLogic
{
    public static class AppSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YoutubeToMP3Converter", "settings.json");

        private static readonly string DefaultOutputFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ConvertedMp3");

        public static string OutputFolder { get; private set; } = DefaultOutputFolder;
        private static bool _outputFolderCustomized;

        public static int ConcurrentDownloads { get; private set; } = 3;
        public static bool EmbedThumbnail { get; private set; } = true;
        public static bool EmbedMetadata { get; private set; } = true;

        public static void Load()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return;
                var doc = JsonDocument.Parse(File.ReadAllText(SettingsPath));
                var root = doc.RootElement;
                if (root.TryGetProperty("outputFolder", out var v) && v.GetString() is string folder)
                {
                    OutputFolder = folder;
                    _outputFolderCustomized = true;
                }
                if (root.TryGetProperty("concurrentDownloads", out v)) ConcurrentDownloads = Math.Clamp(v.GetInt32(), 1, 5);
                if (root.TryGetProperty("embedThumbnail", out v)) EmbedThumbnail = v.GetBoolean();
                if (root.TryGetProperty("embedMetadata", out v)) EmbedMetadata = v.GetBoolean();
            }
            catch { }
        }

        public static bool IsOutputFolderCustomized => _outputFolderCustomized;

        public static void SetOutputFolder(string path) { OutputFolder = path; _outputFolderCustomized = true; Save(); }
        public static void SetConcurrentDownloads(int value) { ConcurrentDownloads = Math.Clamp(value, 1, 5); Save(); }
        public static void SetEmbedThumbnail(bool value) { EmbedThumbnail = value; Save(); }
        public static void SetEmbedMetadata(bool value) { EmbedMetadata = value; Save(); }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(new
                {
                    outputFolder = OutputFolder,
                    concurrentDownloads = ConcurrentDownloads,
                    embedThumbnail = EmbedThumbnail,
                    embedMetadata = EmbedMetadata
                }));
            }
            catch { }
        }
    }
}
