using UiPath.CodedWorkflows;
using System;

namespace Yash.StandardProject
{
    public class GoogleDocsFactory
    {
        public GoogleDocsFactory(ICodedWorkflowsServiceContainer resolver)
        {
        }
    }

    public class DriveFactory
    {
        public UiPath.GSuite.Activities.Api.DriveConnection Shared_eyashb_gmail_com__2 { get; set; }

        public DriveFactory(ICodedWorkflowsServiceContainer resolver)
        {
            Shared_eyashb_gmail_com__2 = new UiPath.GSuite.Activities.Api.DriveConnection("74a62e37-adbb-4457-ac82-559550bd6613", resolver);
        }
    }

    public class GmailFactory
    {
        public UiPath.GSuite.Activities.Api.GmailConnection Shared_yash_brahmbhatt_ybdev_one__2 { get; set; }

        public GmailFactory(ICodedWorkflowsServiceContainer resolver)
        {
            Shared_yash_brahmbhatt_ybdev_one__2 = new UiPath.GSuite.Activities.Api.GmailConnection("850cb90b-6379-4a1f-8e84-356e5a91bc03", resolver);
        }
    }

    public class GoogleSheetsFactory
    {
        public UiPath.GSuite.Activities.Api.SheetsConnection Shared_yash_brahmbhatt_ybdev_one { get; set; }

        public GoogleSheetsFactory(ICodedWorkflowsServiceContainer resolver)
        {
            Shared_yash_brahmbhatt_ybdev_one = new UiPath.GSuite.Activities.Api.SheetsConnection("d77e052b-a14e-4789-983b-e751e9176894", resolver);
        }
    }
}