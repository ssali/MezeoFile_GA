using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezeo
{
    class FileFolderInfo: System.IDisposable
    {
        public string Key { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public double FileSize { get; set; }
        public string ContentUrl { get; set; }
        public string ParentUrl { get; set; }
        public string ETag { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public bool IsPublic { get; set; }
        public bool IsShared { get; set; }
        public string Status { get; set; }
        public string ParentDir { get; set; }
        public string Type { get; set; }

        public void Dispose()
        {
            
        }
    }
}
