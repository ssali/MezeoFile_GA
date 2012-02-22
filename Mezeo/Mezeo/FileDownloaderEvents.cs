using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezeo
{
    class FileDownloaderEvents:EventArgs
    {
        public string FileName { get; set; }
        public int Progress { get; set; }

        public FileDownloaderEvents(string filenName, int progress)
        {
            FileName = filenName;
            Progress = progress;
        }
    }
}
