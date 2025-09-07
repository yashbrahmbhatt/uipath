using System;
using System.Activities.DesignViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Frameworks.Activities.ViewModels
{
    public class QueueSettingsViewModel : DesignPropertiesViewModel
    {
        public DesignProperty<bool> EnableQueue { get; set; }
        public DesignProperty<string> QueueName { get; set; }
        public DesignProperty<string> QueueFolder { get; set; }
        public QueueSettingsViewModel(IDesignServices services) : base(services)
        {
        }
    }
}
