using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YoutubeToMP3.BusinessLogic
{
    /// <summary>Appends a diagnosable record of every failed batch next to the converted files.</summary>
    public static class DownloadLog
    {
        public static string FilePath =>
            Path.Combine(AppSettings.OutputFolder, "ConvertedMp3.Logs.txt");

        public static void WriteFailures(IReadOnlyCollection<DownloadResult> failures)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"===== {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {failures.Count} failed download(s) =====");
            sb.AppendLine($"yt-dlp: {BinaryExtractor.YtDlpPath}");
            sb.AppendLine($"ffmpeg: {BinaryExtractor.FfmpegPath}");
            sb.AppendLine();

            foreach (var failure in failures)
            {
                sb.AppendLine($"URL       : {failure.Url}");
                sb.AppendLine($"Exit code : {failure.ExitCode}");
                sb.AppendLine($"Attempts  : {failure.Attempts}");
                sb.AppendLine($"Reason    : {failure.Reason}");

                if (failure.RawError is string raw)
                    sb.AppendLine($"yt-dlp    : {raw}");

                sb.AppendLine("--- yt-dlp output ---");
                sb.AppendLine(string.IsNullOrWhiteSpace(failure.Output) ? "(no output)" : failure.Output);
                sb.AppendLine("---------------------");
                sb.AppendLine();
            }

            try
            {
                Directory.CreateDirectory(AppSettings.OutputFolder);
                File.AppendAllText(FilePath, sb.ToString());
            }
            catch (Exception ex)
            {
                // Losing the log must never mask the download failures the user is being told about.
                System.Diagnostics.Debug.WriteLine($"Could not write log: {ex.Message}");
            }
        }
    }
}
