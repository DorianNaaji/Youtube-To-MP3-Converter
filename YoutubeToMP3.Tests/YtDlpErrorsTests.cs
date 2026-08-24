using Xunit;
using YoutubeToMP3.BusinessLogic;

namespace YoutubeToMP3.Tests
{
    public class YtDlpErrorsTests
    {
        [Fact]
        public void Explain_BotCheck_TellsUserToUpdate()
        {
            const string output = "ERROR: [youtube] abc: Sign in to confirm you're not a bot. Use --cookies-from-browser.";

            string reason = YtDlpErrors.Explain(output, 1);

            Assert.Contains("human check", reason);
            Assert.Contains("Update yt-dlp", reason);
        }

        [Fact]
        public void Explain_RateLimit_SuggestsLoweringConcurrency()
        {
            const string output = "ERROR: unable to download video data: HTTP Error 429: Too Many Requests";

            Assert.Contains("Concurrent downloads", YtDlpErrors.Explain(output, 1));
        }

        [Theory]
        [InlineData("ERROR: [youtube] abc: Private video. Sign in if you've been granted access.", "private")]
        [InlineData("ERROR: [youtube] abc: Video unavailable", "unavailable")]
        [InlineData("ERROR: [youtube] abc: The uploader has not made this video available in your country", "blocked in your country")]
        [InlineData("ERROR: Unable to extract nsig function name", "out of date")]
        [InlineData("ERROR: Postprocessing: ffprobe/ffmpeg not found", "ffmpeg failed")]
        [InlineData("ERROR: [youtube] JMLnhZLRTvY: This video is unavailable", "unavailable")]
        public void Explain_KnownFailures_MapToActionableMessage(string output, string expected)
        {
            Assert.Contains(expected, YtDlpErrors.Explain(output, 1));
        }

        [Fact]
        public void Explain_TransientWarning_DoesNotShadowTheRealError()
        {
            const string output =
                "WARNING: unable to download webpage: retrying\n" +
                "ERROR: [youtube] abc: Video unavailable";

            Assert.Contains("unavailable", YtDlpErrors.Explain(output, 1));
            Assert.DoesNotContain("internet connection", YtDlpErrors.Explain(output, 1));
        }

        [Fact]
        public void Explain_UnknownError_FallsBackToRawYtDlpLine()
        {
            const string output = "[youtube] abc: Downloading webpage\nERROR: something nobody mapped yet";

            Assert.Equal("something nobody mapped yet", YtDlpErrors.Explain(output, 1));
        }

        [Fact]
        public void Explain_NoOutputAtAll_ReportsExitCode()
        {
            string reason = YtDlpErrors.Explain(string.Empty, 137);

            Assert.Contains("137", reason);
        }

        [Fact]
        public void ExtractRawError_ReturnsLastErrorLineWithoutPrefix()
        {
            const string output = "ERROR: first problem\n[info] noise\nERROR: last problem";

            Assert.Equal("last problem", YtDlpErrors.ExtractRawError(output));
        }

        [Fact]
        public void ExtractRawError_WithoutErrorLine_ReturnsNull()
        {
            Assert.Null(YtDlpErrors.ExtractRawError("[download] 100% of 3.20MiB"));
        }

        [Theory]
        [InlineData("ERROR: HTTP Error 429: Too Many Requests", true)]
        [InlineData("ERROR: Video unavailable", false)]
        [InlineData("", false)]
        public void IsRateLimited_DetectsOnlyThrottling(string output, bool expected)
        {
            Assert.Equal(expected, YtDlpErrors.IsRateLimited(output));
        }
    }
}
