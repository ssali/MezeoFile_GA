using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezeo
{
    class StructureDownloaderEvent:EventArgs
    {
        public bool IsCompleted { get; set; }

        public StructureDownloaderEvent(bool completed)
        {
            IsCompleted = completed;
        }
    }
}
