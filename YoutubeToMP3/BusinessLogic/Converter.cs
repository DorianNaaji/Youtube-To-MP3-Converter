using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeToMP3.BusinessLogic
{
    public static class Converter
    {
        private const int MaxAttempts = 3;

        /// <summary>yt-dlp emits one progress line per chunk; keeping only the tail bounds memory and log size.</summary>
        private const int MaxCapturedLines = 200;

        public static void SetEnvironment()
        {
            if (AppSettings.IsOutputFolderCustomized || !Directory.Exists(AppSettings.OutputFolder))
                Directory.CreateDirectory(AppSettings.OutputFolder);
        }

        public static async Task<DownloadResult> DownloadAsMp3Async(string url, CancellationToken ct = default)
        {
            string args = BuildArguments(url);
            int exitCode = -1;
            string output = string.Empty;

            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                (exitCode, output) = await RunAsync(BinaryExtractor.YtDlpPath, args, ct);

                bool worthRetrying = exitCode != 0
                                     && YtDlpErrors.IsRateLimited(output)
                                     && attempt < MaxAttempts;

                if (!worthRetrying)
                    return new DownloadResult { Url = url, ExitCode = exitCode, Output = output, Attempts = attempt };

                await Task.Delay(TimeSpan.FromSeconds(5 * Math.Pow(2, attempt - 1)), ct);
            }

            return new DownloadResult { Url = url, ExitCode = exitCode, Output = output, Attempts = MaxAttempts };
        }

        /// <summary>Runs yt-dlp's own updater. Requires the binary to live in a writable folder.</summary>
        public static async Task<DownloadResult> UpdateYtDlpAsync(CancellationToken ct = default)
        {
            var (exitCode, output) = await RunAsync(BinaryExtractor.YtDlpPath, "-U", ct);
            return new DownloadResult { Url = "yt-dlp -U", ExitCode = exitCode, Output = output };
        }

        internal static string BuildArguments(string url)
        {
            var args = new StringBuilder("-x --audio-format mp3 --audio-quality 0 --no-progress ");

            // A watch?v=...&list=... URL would otherwise pull the whole playlist behind the user's back.
            if (!url.IsPlaylistUrl())
                args.Append("--no-playlist ");

            if (AppSettings.EmbedThumbnail) args.Append("--embed-thumbnail ");
            if (AppSettings.EmbedMetadata) args.Append("--embed-metadata ");

            args.Append($"--ffmpeg-location \"{BinaryExtractor.FfmpegPath}\" ");
            args.Append($"-o \"{AppSettings.OutputFolder}/%(title)s.%(ext)s\" ");
            args.Append($"\"{url}\"");

            return args.ToString();
        }

        private static async Task<(int ExitCode, string Output)> RunAsync(
            string fileName, string args, CancellationToken ct)
        {
            using var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = args;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            var captured = new Queue<string>(MaxCapturedLines);

            void Capture(object _, DataReceivedEventArgs e)
            {
                if (e.Data is null)
                    return;

                lock (captured)
                {
                    if (captured.Count == MaxCapturedLines)
                        captured.Dequeue();
                    captured.Enqueue(e.Data);
                }
            }

            process.OutputDataReceived += Capture;
            process.ErrorDataReceived += Capture;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            try
            {
                await process.WaitForExitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                throw;
            }

            string output;
            lock (captured)
                output = string.Join(Environment.NewLine, captured);

            return (process.ExitCode, output);
        }

        private static void TryKill(Process process)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch
            {
                // The process died on its own between the check and the kill; nothing to clean up.
            }
        }
    }
}
