using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezeo
{
    class IssueFound
    {
        public string LocalFilePath { get; set; }

        public string IssueTitle { get; set; }

        public ConflictType cType { get; set; }

        public DateTime LocalIssueDT { get; set; }

        public string LocalSize { get; set; }

        public string ServerFileInfo { get; set; }

        public DateTime ServerIssueDT { get; set; }

        public string ServerSize { get; set; }

        public DateTime ConflictTimeStamp { get; set; }

        public enum ConflictType
        {
            CONFLICT_UPLOAD,
            CONFLICT_MODIFIED
        }
    }
}
