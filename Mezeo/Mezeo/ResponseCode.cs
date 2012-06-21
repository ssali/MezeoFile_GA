using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezeo
{
    public static class  ResponseCode
    {
        public static int LOGIN = 200;
        public static int DOWNLOADITEMDETAILS = 200;
        public static int DOWNLOADFILE = 200;
        public static int GETETAG = 200;
        public static int GETCONTINERRESULT = 200;
        public static int NQGETLENGTH = 200;
        public static int NQGETDATA = 200;
        public static int NQCREATE = 202;
        public static int NQDELETEVALUE = 200;
        public static int NQPARENTURI = 200;
        public static int NEWCONTAINER = 201;
        public static int UPLOADINGFILE = 201;
        public static int OVERWRITEFILE = 204;
        public static int GETPARENTNAME = 200;
        public static int GETDELETEDINFO = 200;
        public static int FILERENAME = 204;
        public static int CONTAINERRENAME = 204;
        public static int DELETE = 204;
        public static int FILEMOVE = 204;
        public static int CONTAINERMOVE = 204;
        public static int COPY = 201;
        public static int GETNAMESPACERESULT = 200;
        public static int GETETAGMATCHING = 200;
        public static int GETSTORAGEUSED = 200;
        public static int STATUSCONNECTION = 200;
        public static int NQDELETE = 200;
        
        public static int LOGINFAILED1 = 401;
        public static int LOGINFAILED2 = 403;
        public static int NOTFOUND = 404;

        public static int SERVER_INACCESSIBLE = -1;
        public static int INSUFFICIENT_STORAGE_AVAILABLE = -4;
    }
}
