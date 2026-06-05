using System;
using System.Text.RegularExpressions;

namespace YoutubeToMP3.BusinessLogic
{
    public static class StringExtension
    {
        private static readonly string[] SupportedHosts =
        {
            "www.youtube.com",
            "youtube.com",
            "music.youtube.com",
            "youtu.be",
            "www.youtu.be"
        };

        public static bool IsYoutubeLinkValid(this string str)
        {
            if (!Uri.IsWellFormedUriString(str, UriKind.Absolute))
                return false;

            var uri = new Uri(str);
            string host = uri.Host.ToLower();

            if (!Array.Exists(SupportedHosts, h => h == host))
                return false;

            string path = uri.AbsolutePath.ToLower();

            // youtu.be/<id>
            if (host is "youtu.be" or "www.youtu.be")
                return path.Length > 1;

            // youtube.com/watch?v=, /shorts/<id>, /playlist?list=
            return path.StartsWith("/watch") && uri.Query.Contains("v=")
                || path.StartsWith("/shorts/")
                || path.StartsWith("/playlist") && uri.Query.Contains("list=")
                || host == "music.youtube.com" && path.StartsWith("/watch") && uri.Query.Contains("v=");
        }
    }
}
