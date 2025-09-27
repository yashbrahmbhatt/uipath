using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Config.ConfigurationFile
{
    [Serializable]
    public class NameValueFolderItem
    {
        public string Name = string.Empty;
        public string Value = string.Empty;
        public string Folder = string.Empty;

        public NameValueFolderItem() { }
    }
}
