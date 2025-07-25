using UiPath.CodedWorkflows;
using System;

namespace Yash.Utility.Activities
{
    public class ConnectionsManager
    {
        public GoogleDocsFactory GoogleDocs { get; set; }
        public DriveFactory Drive { get; set; }
        public GmailFactory Gmail { get; set; }
        public GoogleSheetsFactory GoogleSheets { get; set; }

        public ConnectionsManager(ICodedWorkflowsServiceContainer resolver)
        {
            GoogleDocs = new GoogleDocsFactory(resolver);
            Drive = new DriveFactory(resolver);
            Gmail = new GmailFactory(resolver);
            GoogleSheets = new GoogleSheetsFactory(resolver);
        }
    }
}