using Xunit;
using YoutubeToMP3.BusinessLogic;

namespace YoutubeToMP3.Tests
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData("https://www.youtube.com/watch?v=xiLhc4AY-90")]
        [InlineData("https://youtube.com/shorts/abc123")]
        [InlineData("https://youtu.be/xiLhc4AY-90")]
        [InlineData("https://music.youtube.com/watch?v=abc123")]
        [InlineData("https://www.youtube.com/playlist?list=PL123")]
        public void IsYoutubeLinkValid_AcceptsSupportedUrls(string url)
        {
            Assert.True(url.IsYoutubeLinkValid());
        }

        [Theory]
        [InlineData("https://vimeo.com/123456")]
        [InlineData("not a url")]
        [InlineData("https://www.youtube.com/")]
        [InlineData("https://www.youtube.com/watch")]
        public void IsYoutubeLinkValid_RejectsEverythingElse(string url)
        {
            Assert.False(url.IsYoutubeLinkValid());
        }

        [Theory]
        [InlineData("https://www.youtube.com/playlist?list=PL123", true)]
        [InlineData("https://www.youtube.com/watch?v=abc&list=PL123", false)]
        [InlineData("https://youtu.be/abc", false)]
        [InlineData("not a url", false)]
        public void IsPlaylistUrl_OnlyMatchesWholePlaylists(string url, bool expected)
        {
            Assert.Equal(expected, url.IsPlaylistUrl());
        }
    }
}
