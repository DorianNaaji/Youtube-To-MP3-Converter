using System;
using System.Diagnostics;

namespace YoutubeToMP3Form.BusinessLogic
{
    internal class YoutubeProcessInfo
    {
        public YoutubeProcessInfo(Process process, string fileName)
        {
            this.Process = process;
            this.FileName = fileName;
        }

        private Process Process { get; }

        private String FileName { get; }
    }
}
