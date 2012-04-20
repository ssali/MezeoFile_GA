using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezeo
{
    public enum CancelReason
    {
        INSUFFICIENT_STORAGE,
        USER_CANCEL,
        DOWNLOAD_FAILED,
        LOGIN_FAILED,
        SERVER_INACCESSIBLE
    }
}
