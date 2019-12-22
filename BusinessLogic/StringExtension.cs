using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLogic
{
    public static class StringExtension
    {
        private static readonly string YouTubeHost = "www.youtube.com";
        public static bool IsYoutubeLinkValid(this string str)
        {
            if(Uri.IsWellFormedUriString(str, UriKind.Absolute))
            {
                Uri uri = new Uri(str);
                
                return uri.Host.ToLower() == YouTubeHost 
                    && uri.AbsoluteUri.ToLower().Contains("/watch?v=") 
                    && Regex.Matches(str, "//").Count < 2;
            }
            return false;
        }
    }
}
