using System;
using System.Linq;

namespace YoutubeToMP3.BusinessLogic
{
    /// <summary>
    /// Turns raw yt-dlp console output into an explanation a user can act on.
    /// yt-dlp only ever reports failures on stderr, so without this the exit code is all we have.
    /// </summary>
    public static class YtDlpErrors
    {
        private const string OutdatedAdvice =
            "yt-dlp is out of date for YouTube's current player. Use Settings > Update yt-dlp, then retry.";

        // Ordered from most specific to most generic: the first match wins.
        private static readonly (string Pattern, string Explanation)[] Rules =
        {
            ("confirm you're not a bot",
                "YouTube is asking for a human check. " + OutdatedAdvice),
            ("confirm your age",
                "Age-restricted video. YouTube requires a signed-in account to download it."),
            ("age-restricted",
                "Age-restricted video. YouTube requires a signed-in account to download it."),
            ("private video",
                "This video is private and cannot be downloaded."),
            ("video is private",
                "This video is private and cannot be downloaded."),
            ("members-only",
                "This video is reserved for channel members."),
            ("this channel's members",
                "This video is reserved for channel members."),
            ("music premium",
                "This track requires a YouTube Music Premium account."),
            ("premium members",
                "This track requires a YouTube Premium account."),
            ("available in your country",
                "This video is blocked in your country."),
            ("blocked it in your country",
                "This video is blocked in your country."),
            ("has been removed",
                "This video was removed from YouTube."),
            ("account associated with this video has been terminated",
                "The channel that owned this video was terminated."),
            ("video unavailable",
                "This video is unavailable (removed, private, or region-locked)."),
            ("video is unavailable",
                "This video is unavailable (removed, private, or region-locked)."),
            ("http error 429",
                "YouTube is rate-limiting you. Lower Settings > Concurrent downloads and retry in a few minutes."),
            ("too many requests",
                "YouTube is rate-limiting you. Lower Settings > Concurrent downloads and retry in a few minutes."),
            ("http error 403",
                "YouTube refused the download. " + OutdatedAdvice),
            ("failed to extract any player response",
                OutdatedAdvice),
            ("nsig extraction failed",
                OutdatedAdvice),
            ("unable to extract",
                OutdatedAdvice),
            ("requested format is not available",
                "No downloadable audio track was offered for this video. " + OutdatedAdvice),
            ("only images are available",
                "This entry has no audio stream (it is a still image or a placeholder)."),
            ("ffprobe",
                "ffmpeg failed while converting to MP3. Restart the app so it re-extracts its bundled ffmpeg."),
            ("ffmpeg",
                "ffmpeg failed while converting to MP3. Restart the app so it re-extracts its bundled ffmpeg."),
            ("postprocessing",
                "Download succeeded but the MP3 conversion failed. Check that the output folder is writable."),
            ("no space left",
                "The disk is full. Free some space or pick another output folder."),
            ("not enough space",
                "The disk is full. Free some space or pick another output folder."),
            ("permission denied",
                "The output folder is not writable. Pick another folder in Settings > Output folder."),
            ("unable to download webpage",
                "Could not reach YouTube. Check your internet connection or proxy."),
            ("name resolution",
                "Could not reach YouTube. Check your internet connection or proxy."),
            ("getaddrinfo",
                "Could not reach YouTube. Check your internet connection or proxy."),
            ("connection timed out",
                "Could not reach YouTube. Check your internet connection or proxy."),
            ("certificate verify failed",
                "TLS verification failed. An antivirus or corporate proxy is likely intercepting the connection."),
            ("unsupported url",
                "yt-dlp does not recognise this URL."),
            ("is not a valid url",
                "yt-dlp does not recognise this URL."),
        };

        public static string Explain(string output, int exitCode)
        {
            // Match the ERROR lines first: yt-dlp retries noisily, and a transient WARNING
            // would otherwise shadow the failure that actually stopped the download.
            return Match(ErrorLines(output))
                   ?? Match(output)
                   ?? ExtractRawError(output)
                   ?? $"yt-dlp exited with code {exitCode} without reporting a reason. See the log for the full output.";
        }

        private static string? Match(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            string haystack = text.ToLowerInvariant();
            foreach (var (pattern, explanation) in Rules)
                if (haystack.Contains(pattern))
                    return explanation;

            return null;
        }

        private static string ErrorLines(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return string.Empty;

            return string.Join('\n', output
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>Returns the last "ERROR: ..." line emitted by yt-dlp, trimmed of its prefix.</summary>
        public static string? ExtractRawError(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return null;

            string? line = output
                .Split('\n')
                .Select(l => l.Trim())
                .LastOrDefault(l => l.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase));

            return line is null ? null : line["ERROR:".Length..].Trim();
        }

        /// <summary>True when YouTube throttled us, which is the only failure worth retrying automatically.</summary>
        public static bool IsRateLimited(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return false;

            string haystack = output.ToLowerInvariant();
            return haystack.Contains("http error 429") || haystack.Contains("too many requests");
        }
    }
}
