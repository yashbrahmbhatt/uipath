using System.Activities.DesignViewModels;
using UiPath.Core;
using Activity = System.Activities.Activity;

namespace Yash.Frameworks.Activities.ViewModels
{
    public class LazyFrameworkViewModel : DesignPropertiesViewModel
    {
        public DesignProperty<bool> EnableInitializeConfig { get; set; }
        public DesignProperty<Activity> FrameworkInitializeConfig { get; set; }
        public DesignProperty<Activity> FrameworkInitializeApplications { get; set; }
        public DesignProperty<Activity> FrameworkCloseApplications { get; set; }
        public DesignProperty<bool> EnableInitializeApplications { get; set; }
        public DesignProperty<Activity> FrameworkProcessTransaction { get; set; }
        public DesignProperty<Activity> FrameworkGetTransaction { get; set; }
        public DesignProperty<Activity> FrameworkBusinessException { get; set; }
        public DesignProperty<Activity> FrameworkSystemException { get; set; }
        public DesignProperty<Activity> FrameworkSuccessful { get; set; }
        public DesignProperty<Activity> FrameworkEnd { get; set; }
        public DesignInArgument<string> QueueName { get; set; }
        public DesignInArgument<string> QueueFolder { get; set; }
        public DesignProperty<bool> EnableQueue { get; set; }
        public DesignProperty<bool> EnableBusinessException { get; set; }
        public DesignProperty<bool> EnableSystemException { get; set; }
        public DesignProperty<bool> EnableSuccessful { get; set; }
        public DesignProperty<bool> EnableEnd { get; set; }
        public DesignProperty<bool> EnableEmailNotifications { get; set; }
        public DesignInArgument<QueueItem> QueueItem { get; set; }

        public LazyFrameworkViewModel(IDesignServices services) : base(services)
        {
        }

        protected override void InitializeModel()
        {
            //Debugger.Break(); 
            /*
             * The base call will initialize the properties of the view model with the values from the xaml or with the default values from the activity
             */
            base.InitializeModel();

            PersistValuesChangedDuringInit(); // just for heads-up here; it's a mandatory call only when you change the values of properties during initialization

            var orderIndex = 0;

            EnableQueue.IsPrincipal = true;
            EnableQueue.IsVisible = true;
            EnableQueue.DisplayName = "Enable Queue";
            EnableQueue.Tooltip = "Enable or disable queue processing.";
            EnableQueue.OrderIndex = orderIndex;

            QueueName.IsPrincipal = true;
            QueueName.IsVisible = false;
            QueueName.DisplayName = "Queue Name";
            QueueName.ColumnOrder = 0;
            QueueName.Tooltip = "The name of the queue to process.";
            QueueName.OrderIndex = orderIndex;

            QueueFolder.IsPrincipal = true;
            QueueFolder.IsVisible = false;
            QueueFolder.DisplayName = "Queue Folder";
            QueueFolder.ColumnOrder = 1;
            QueueFolder.Tooltip = "The folder containing the queue.";
            QueueFolder.OrderIndex = orderIndex++;

            EnableInitializeConfig.IsPrincipal = true;
            EnableInitializeConfig.IsVisible = true;
            EnableInitializeConfig.DisplayName = "Enable Initialize Settings";
            EnableInitializeConfig.Tooltip = "Enable or disable loading configuration.";
            EnableInitializeConfig.OrderIndex = orderIndex++;

            FrameworkInitializeConfig.IsPrincipal = true;
            FrameworkInitializeConfig.IsVisible = false; // hidden by default, visibility is controlled by the EnableInitializeConfig property
            FrameworkInitializeConfig.DisplayName = "Initialize Settings Workflow";
            FrameworkInitializeConfig.Tooltip = "The workflow that initializes the settings.";
            FrameworkInitializeConfig.OrderIndex = orderIndex++;
            FrameworkInitializeConfig.Widget =new DefaultWidget() { Type = "Container" };

            EnableInitializeApplications.IsPrincipal = true;
            EnableInitializeApplications.IsVisible = true;
            EnableInitializeApplications.ColumnOrder = 1;
            EnableInitializeApplications.DisplayName = "Enable Initialize Applications";
            EnableInitializeApplications.Tooltip = "Enable or disable framework initialization.";
            EnableInitializeApplications.OrderIndex = orderIndex++;

            FrameworkCloseApplications.IsPrincipal = true;
            FrameworkCloseApplications.IsVisible = false;
            FrameworkCloseApplications.ColumnOrder = 1;
            FrameworkCloseApplications.DisplayName = "Close Applications Workflow";
            FrameworkCloseApplications.Tooltip = "The workflow that closes the applications.";
            FrameworkCloseApplications.OrderIndex = orderIndex++;
            FrameworkCloseApplications.Widget = new DefaultWidget() { Type = "Container" };

            FrameworkInitializeApplications.IsPrincipal = true;
            FrameworkInitializeApplications.IsVisible = false;
            FrameworkInitializeApplications.ColumnOrder = 1;
            FrameworkInitializeApplications.DisplayName = "Initialize Applications Workflow";
            FrameworkInitializeApplications.Tooltip = "The workflow that initializes the applications.";
            FrameworkInitializeApplications.OrderIndex = orderIndex++;
            FrameworkInitializeApplications.Widget = new DefaultWidget() { Type = "Container" };

            FrameworkGetTransaction.IsPrincipal = EnableQueue.Value;
            FrameworkGetTransaction.IsVisible = EnableQueue.Value;
            FrameworkGetTransaction.DisplayName = "Get Transaction Workflow";
            FrameworkGetTransaction.Tooltip = "The workflow that retrieves transactions from the queue.";
            FrameworkGetTransaction.OrderIndex = orderIndex++;
            FrameworkGetTransaction.Widget = new DefaultWidget() { Type = "Container" };

            QueueItem.IsPrincipal = EnableQueue.Value;
            QueueItem.IsVisible = EnableQueue.Value;
            QueueItem.DisplayName = "Queue Item";
            QueueItem.Tooltip = "The current queue item being processed.";
            QueueItem.OrderIndex = orderIndex++;
            

            FrameworkProcessTransaction.IsPrincipal = true;
            FrameworkProcessTransaction.IsVisible = true;
            FrameworkProcessTransaction.DisplayName = "Process Transaction Workflow";
            FrameworkProcessTransaction.Tooltip = "The workflow that processes transactions.";
            FrameworkProcessTransaction.OrderIndex = orderIndex++;
            FrameworkProcessTransaction.Widget = new DefaultWidget() { Type = "Container" };

            EnableBusinessException.IsPrincipal = EnableQueue.Value;
            EnableBusinessException.IsVisible = EnableQueue.Value;
            EnableBusinessException.DisplayName = "Enable Business Exception";
            EnableBusinessException.Tooltip = "Enable or disable business exception handling.";
            EnableBusinessException.OrderIndex = orderIndex;

            FrameworkBusinessException.IsPrincipal = EnableQueue.Value;
            FrameworkBusinessException.IsVisible = EnableBusinessException.Value;
            FrameworkBusinessException.DisplayName = "Business Exception Workflow";
            FrameworkBusinessException.Tooltip = "The workflow that handles business exceptions.";
            FrameworkBusinessException.OrderIndex = orderIndex;
            FrameworkBusinessException.Widget = new DefaultWidget() { Type = "Container" };

            EnableSystemException.IsPrincipal = EnableQueue.Value;
            EnableSystemException.IsVisible = EnableQueue.Value;
            EnableSystemException.DisplayName = "Enable System Exception";
            EnableSystemException.Tooltip = "Enable or disable system exception handling.";
            FrameworkInitializeApplications.ColumnOrder = 1;
            EnableSystemException.OrderIndex = orderIndex;

            FrameworkSystemException.IsPrincipal = EnableQueue.Value;
            FrameworkSystemException.IsVisible = EnableSystemException.Value;
            FrameworkSystemException.DisplayName = "System Exception Workflow";
            FrameworkSystemException.Tooltip = "The workflow that handles system exceptions.";
            FrameworkSystemException.OrderIndex = orderIndex;
            FrameworkInitializeApplications.ColumnOrder = 1;
            FrameworkSystemException.Widget = new DefaultWidget() { Type = "Container" };

            EnableSuccessful.IsPrincipal = EnableQueue.Value;
            EnableSuccessful.IsVisible = EnableQueue.Value;
            EnableSuccessful.DisplayName = "Enable Successful";
            EnableSuccessful.Tooltip = "Enable or disable successful completion handling.";
            FrameworkInitializeApplications.ColumnOrder = 2;
            EnableSuccessful.OrderIndex = orderIndex;

            FrameworkSuccessful.IsPrincipal = EnableQueue.Value;
            FrameworkSuccessful.IsVisible = EnableSuccessful.Value;
            FrameworkSuccessful.DisplayName = "Successful Workflow";
            FrameworkSuccessful.Tooltip = "The workflow that handles successful completion.";
            FrameworkSuccessful.OrderIndex = orderIndex++;
            FrameworkInitializeApplications.ColumnOrder = 2;
            FrameworkSuccessful.Widget = new DefaultWidget() { Type = "Container" };

            EnableEnd.IsPrincipal = true;
            EnableEnd.IsVisible = true;
            EnableEnd.DisplayName = "Enable End";
            EnableEnd.Tooltip = "Enable or disable end processing.";
            EnableEnd.OrderIndex = orderIndex;

            FrameworkEnd.IsPrincipal = true;
            FrameworkEnd.IsVisible = EnableEnd.Value;
            FrameworkEnd.DisplayName = "End Workflow";
            FrameworkEnd.Tooltip = "The workflow that handles end processing.";
            FrameworkEnd.OrderIndex = orderIndex++;
            FrameworkEnd.Widget = new DefaultWidget() { Type = "Container" };

            EnableEmailNotifications.IsPrincipal = true;
            EnableEmailNotifications.IsVisible = true;
            EnableEmailNotifications.DisplayName = "Enable Email Notifications";
            EnableEmailNotifications.Tooltip = "Enable or disable email notifications.";
            EnableEmailNotifications.OrderIndex = orderIndex++;


        }

        protected override void ManualRegisterDependencies()
        {
            base.ManualRegisterDependencies();
            RegisterDependency(EnableQueue, "Value", "ToggleQueue");
            RegisterDependency(EnableInitializeConfig, "Value", "ToggleFrameworkInitializeConfig");
            RegisterDependency(EnableInitializeApplications, "Value", "ToggleFrameworkInitializeApplications");
            RegisterDependency(EnableBusinessException, "Value", "ToggleFrameworkBusinessException");
            RegisterDependency(EnableSystemException, "Value", "ToggleFrameworkSystemException");
            RegisterDependency(EnableSuccessful, "Value", "ToggleFrameworkSuccessful");
            RegisterDependency(EnableEnd, "Value", "ToggleFrameworkEnd");
        }

        protected override void InitializeRules()
        {
            base.InitializeRules();
            Rule("ToggleQueue", new Action(ToggleQueue));
            Rule("ToggleFrameworkInitializeConfig", new Action(ToggleFrameworkInitializeConfig));
            Rule("ToggleFrameworkInitializeApplications", new Action(ToggleFrameworkInitializeApplications));
            Rule("ToggleFrameworkBusinessException", new Action(ToggleFrameworkBusinessException));
            Rule("ToggleFrameworkSystemException", new Action(ToggleFrameworkSystemException));
            Rule("ToggleFrameworkSuccessful", new Action(ToggleFrameworkSuccessful));
            Rule("ToggleFrameworkEnd", new Action(ToggleFrameworkEnd));
        }

        private void ToggleQueue()
        {
            QueueName.IsVisible = EnableQueue.Value;
            QueueName.IsPrincipal = EnableQueue.Value;
            QueueFolder.IsVisible = EnableQueue.Value;
            QueueFolder.IsPrincipal = EnableQueue.Value;
            EnableQueue.IsPrincipal = !QueueName.IsPrincipal;

            FrameworkGetTransaction.IsVisible = EnableQueue.Value;
            FrameworkGetTransaction.IsPrincipal = EnableQueue.Value;
            
            EnableBusinessException.IsVisible = EnableQueue.Value;
            EnableSystemException.IsVisible = EnableQueue.Value;
            EnableSuccessful.IsVisible = EnableQueue.Value;
            FrameworkBusinessException.IsVisible = EnableQueue.Value && EnableBusinessException.Value;
            FrameworkSystemException.IsVisible = EnableQueue.Value && EnableSystemException.Value;
            FrameworkSuccessful.IsVisible = EnableQueue.Value && EnableSuccessful.Value;

        }

        private void ToggleFrameworkInitializeConfig()
        {
            FrameworkInitializeConfig.IsVisible = EnableInitializeConfig.Value;
            FrameworkInitializeConfig.IsPrincipal = EnableInitializeConfig.Value;
            EnableInitializeConfig.IsPrincipal = !FrameworkInitializeConfig.IsPrincipal;
        }

        private void ToggleFrameworkInitializeApplications()
        {
            FrameworkInitializeApplications.IsVisible = EnableInitializeApplications.Value;
            FrameworkInitializeApplications.IsPrincipal = EnableInitializeApplications.Value;
            FrameworkCloseApplications.IsVisible = EnableInitializeApplications.Value;
            FrameworkCloseApplications.IsPrincipal = EnableInitializeApplications.Value;
            EnableInitializeApplications.IsPrincipal = !FrameworkInitializeApplications.IsPrincipal;
        }

        private void ToggleFrameworkBusinessException()
        {
            FrameworkBusinessException.IsVisible = EnableQueue.Value && EnableBusinessException.Value;
            FrameworkBusinessException.IsPrincipal = EnableBusinessException.Value;
            EnableBusinessException.IsPrincipal = !FrameworkBusinessException.IsPrincipal;
        }

        private void ToggleFrameworkSystemException()
        {
            FrameworkSystemException.IsVisible = EnableQueue.Value && EnableSystemException.Value;
            FrameworkSystemException.IsPrincipal = EnableSystemException.Value;
            EnableSystemException.IsPrincipal = !FrameworkSystemException.IsPrincipal;
        }

        private void ToggleFrameworkSuccessful()
        {
            FrameworkSuccessful.IsVisible = EnableQueue.Value && EnableSuccessful.Value;
            FrameworkSuccessful.IsPrincipal = EnableSuccessful.Value;
            EnableSuccessful.IsPrincipal = !FrameworkSuccessful.IsPrincipal;
        }

        private void ToggleFrameworkEnd()
        {
            FrameworkEnd.IsVisible = EnableEnd.Value;
            FrameworkEnd.IsPrincipal = EnableEnd.Value;
            EnableEnd.IsPrincipal = !FrameworkEnd.IsPrincipal;
        }
    }
}
