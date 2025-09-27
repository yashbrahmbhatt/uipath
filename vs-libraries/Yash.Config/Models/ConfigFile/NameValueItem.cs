using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Config.ConfigurationFile
{
    [Serializable]
    public class NameValueItem
    {
        public string Name = string.Empty;
        public string Value = string.Empty;
        public NameValueItem() { }
    }
}
