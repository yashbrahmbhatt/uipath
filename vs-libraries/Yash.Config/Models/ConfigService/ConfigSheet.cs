using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Config.ConfigurationService
{
    /// <summary>
    /// Result of validating a single sheet's headers.
    /// </summary>
    public class SheetMetadata
    {
        public string SheetName { get; set; } = "";
        public string ConfigType { get; set; } = ConfigSheetType.Unknown;
        public DataTable? Sheet { get; set; } = null;
    }
}
