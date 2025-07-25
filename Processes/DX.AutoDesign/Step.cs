using System;
using System.Collections.Generic;
using System.Data;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace DX.AutoDesign
{
    public class Step
    {
        public string Name;
        public string Description;
        public StepType Type;
        /// <summary>
        /// A dictionary of the next steps from this particular step.
        /// The key should be a short description of the condition or logic to determine what the next step is. 
        /// If only 1 downstream step is needed, then the short description can just be "Next"
        /// The value should be the name of the downstream step.
        /// </summary>
        public Dictionary<string, string> DownstreamSteps;
        /// <summary>
        /// A list of the names of the possible upstream steps.
        /// </summary>
        public List<string> UpstreamSteps;
    }
    
    public enum StepType{
        WebGUIInteraction,
        SOAPAPIInteraction,
        RESTAPIInteraction,
        DatabaseInteraction,
        MainframeInteraction,
        UiPathPlatformInteraction,
        DataManipulation,
        Decision,
        HumanInTheLoopTask,
    }
}