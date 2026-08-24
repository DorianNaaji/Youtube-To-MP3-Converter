namespace YoutubeToMP3.BusinessLogic
{
    /// <summary>Outcome of a single yt-dlp invocation, including the captured console output.</summary>
    public sealed class DownloadResult
    {
        public required string Url { get; init; }
        public required int ExitCode { get; init; }
        public required string Output { get; init; }
        public int Attempts { get; init; } = 1;

        public bool Success => ExitCode == 0;

        /// <summary>Human-readable, actionable explanation of the failure.</summary>
        public string Reason => YtDlpErrors.Explain(Output, ExitCode);

        /// <summary>The raw "ERROR: ..." line reported by yt-dlp, when there is one.</summary>
        public string? RawError => YtDlpErrors.ExtractRawError(Output);
    }
}
