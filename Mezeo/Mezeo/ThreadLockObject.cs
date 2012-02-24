using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezeo
{
    class ThreadLockObject:Object
    {
        public bool StopThread { get; set; }
        public bool ExitApplication { get; set; }
    }
}
