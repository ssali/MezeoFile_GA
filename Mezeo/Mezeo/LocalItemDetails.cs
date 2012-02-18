using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MezeoFileSupport;

namespace Mezeo
{
    class LocalItemDetails
    {
        ItemDetails[] contents;
        string strPath;

        public ItemDetails[] ItemDetails
        {
            get
            {
                return this.contents;
            }
            set
            {
                this.contents = value;
            }
        }

        public string Path
        {
            get
            {
                return this.strPath;
            }
            set
            {
                this.strPath = value;
            }
        }
    }
}
