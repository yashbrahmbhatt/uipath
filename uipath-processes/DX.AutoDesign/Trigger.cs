using System;
using System.Collections.Generic;
using System.Data;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace DX.AutoDesign
{
    public class Trigger
    {
        public string Name;
        public TriggerType Type;
        public string Description;
        public string ProcessName;
    }
    
    public enum TriggerType {
        /// <summary>
        /// Scheduled start based on CRON expression
        /// </summary>
        Time,
        /// <summary>
        /// Starts based on conditions being met in a queue.
        /// </summary>
        Queue,
        /// <summary>
        /// Starts based on an event occurring in a 3rd party integration included by UiPath, or a webhook service interacting with orchestrator
        /// </summary>
        Event
    }
}