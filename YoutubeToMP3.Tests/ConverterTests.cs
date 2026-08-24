using Xunit;
using YoutubeToMP3.BusinessLogic;

namespace YoutubeToMP3.Tests
{
    public class ConverterTests
    {
        [Fact]
        public void BuildArguments_SingleVideo_OptsOutOfThePlaylist()
        {
            string args = Converter.BuildArguments("https://www.youtube.com/watch?v=abc&list=PL123");

            Assert.Contains("--no-playlist", args);
        }

        [Fact]
        public void BuildArguments_PlaylistUrl_KeepsThePlaylist()
        {
            string args = Converter.BuildArguments("https://www.youtube.com/playlist?list=PL123");

            Assert.DoesNotContain("--no-playlist", args);
        }

        [Fact]
        public void BuildArguments_QuotesTheUrlAndSilencesProgressSpam()
        {
            const string url = "https://www.youtube.com/watch?v=xiLhc4AY-90";

            string args = Converter.BuildArguments(url);

            Assert.Contains($"\"{url}\"", args);
            Assert.Contains("--no-progress", args);
            Assert.Contains("--audio-format mp3", args);
        }
    }
}
