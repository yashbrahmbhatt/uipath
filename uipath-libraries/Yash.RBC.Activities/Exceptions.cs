using System;
using System.Collections.Generic;
using System.Data;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;
using UiPath.UIAutomationNext.API.Contracts;
using UiPath.UIAutomationNext.API.Models;
using UiPath.UIAutomationNext.Enums;
using Yash.RBC.Activities.ObjectRepository;

namespace Yash.RBC.Activities
{
    public static class Exceptions
    {
        public static class Application
        {
            public static class Initialize
            {
                public const string CouldNotVerifyLogin = "Could not verify successful login into RBC with the provided credentials.";
            }
        }
        public static class Transactions
        {
            public static class Download
            {
                public const string Timeout_Download = "File did not download in timeout specified";
            }
        }
    }
}