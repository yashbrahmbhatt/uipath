using System;
using System.Collections.Generic;
using System.Data;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace Yash.Testing
{
    public class TestCase<TInput, TOutput>
    {
        public string Id { get; set; }
        public TInput Input { get; set; }
        public TOutput ExpectedOutput { get; set; }
        public string ExpectedExceptionMessage { get; set; } = null;
    }
}