using System;
using System.Collections.Generic;
using System.Data;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;

namespace Finance.Automations._00_Shared
{
    /// <summary>
    /// Determines whether the current time falls within a specified maintenance window.
    /// </summary>
    public class IsMaintenanceTime : CodedWorkflow
    {
        /// <summary>
        /// Checks if the current system time is within the given maintenance time range.
        /// It is recommended to provide UTC values as it normalizes timezones.
        /// Your input will still work if you don't and the robot is on the same timezone.
        /// But you run the risk that a robot eventually exists within a different timezone.
        /// </summary>
        /// <param name="start">The start time of the maintenance window.</param>s
        /// <param name="end">The end time of the maintenance window.</param>
        /// <param name="now">The current time of day. This is an overload; main use case is testing.</param>
        /// <returns>
        /// True if the current time is within the maintenance window, otherwise false.
        /// </returns>
        [Workflow]
        public bool Execute(TimeSpan start, TimeSpan end, TimeSpan now)
        {
            // Overload support for either providing or not providing 'now', currently only to support tests. 
            // I would provide a default value for it but DateTime, and derivative types like TimeSpan are not Compile-Time constants, which is a requirement 
            // of coded workflows. ROI still positive, 
            TimeSpan curr;
            if(now == null || now == new DateTime(0).TimeOfDay) curr = DateTime.Now.TimeOfDay;
            else curr = now;

            /// Determines if the current time falls within the maintenance window.
            /// If the start time is earlier than or equal to the end time (e.g., 01:00-05:00), 
            /// the current time must be between start and end.
            /// If the start time is later than the end time (e.g., 22:00-04:00), 
            /// the current time can be either after the start time or before the end time.
            var result = start <= end ? curr >= start && curr <= end : curr >= start || curr <= end;
            Log($"IsMaintenanceTime: {curr} - {result}", LogLevel.Info);
            return result;
        }
    }
}
