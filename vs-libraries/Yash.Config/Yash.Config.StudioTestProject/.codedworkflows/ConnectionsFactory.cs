using UiPath.CodedWorkflows;
using System;

namespace Yash.Config.StudioTestProject
{
    public class ExcelFactory
    {
        public ExcelFactory(ICodedWorkflowsServiceContainer resolver)
        {
        }
    }

    public class O365MailFactory
    {
        public UiPath.MicrosoftOffice365.Activities.Api.MailConnection My_Workspace_Microsoft_Outlook_365__2 { get; set; }

        public O365MailFactory(ICodedWorkflowsServiceContainer resolver)
        {
            My_Workspace_Microsoft_Outlook_365__2 = new UiPath.MicrosoftOffice365.Activities.Api.MailConnection("be652697-7357-4ec2-bc53-d72235334358", resolver);
        }
    }

    public class OneDriveFactory
    {
        public OneDriveFactory(ICodedWorkflowsServiceContainer resolver)
        {
        }
    }
}