using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualBasic;
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

namespace Yash.StandardProject._99_Reporter.Logic
{
    /// <summary>
    /// A workflow class that adds calculated columns to a given <see cref="DataTable"/>.
    /// The columns include "TimeSaved" and "ExecutionTime", which are populated based on the status of each row.
    /// </summary>
    public class ReporterLogicAddCalculatedColumns : CodedWorkflow
    {
        /// <summary>
        /// Adds calculated columns "TimeSaved" and "ExecutionTime" to the provided <see cref="DataTable"/>.
        /// The "ExecutionTime" is calculated as the difference in seconds between the "Started (absolute)" and "Ended (absolute)" columns.
        /// The "TimeSaved" is assigned based on the row's status:
        /// - For "Successful" status, it uses the <paramref name="TimeSaved_Success"/>.
        /// - For "Failed" status, it checks the "Exception" column to determine whether to use <paramref name="TimeSaved_BusEx"/> or <paramref name="TimeSaved_SysEx"/>.
        /// </summary>
        /// <param name="OriginalTable">The <see cref="DataTable"/> to which calculated columns will be added.</param>
        /// <param name="TimeSaved_Success">The value to assign to "TimeSaved" for rows with a "Successful" status.</param>
        /// <param name="TimeSaved_BusEx">The value to assign to "TimeSaved" for rows with a "Failed" status and "BusinessException" in the "Exception" column.</param>
        /// <param name="TimeSaved_SysEx">The value to assign to "TimeSaved" for rows with a "Failed" status and "SystemException" in the "Exception" column.</param>
        /// <returns>The updated <see cref="DataTable"/> with the newly calculated columns.</returns>
        [Workflow]
        public DataTable Execute(DataTable OriginalTable, double TimeSaved_Success, double TimeSaved_BusEx, double TimeSaved_SysEx)
        {
            OriginalTable.Columns.Add(new DataColumn("TimeSaved", typeof(double)));
            OriginalTable.Columns.Add(new DataColumn("ExecutionTime", typeof(double)));

            foreach (DataRow row in OriginalTable.Rows)
            {
                // Skip rows that do not have a "Successful" or "Failed" status
                if (!new List<string>(new string[] { QueueItemStatus.Successful.ToString(), QueueItemStatus.Failed.ToString() }).Contains(row["Status"].ToString()))
                    continue;

                // Calculate execution time based on start and end times
                row["ExecutionTime"] = DateAndTime.DateDiff(DateInterval.Second, DateTime.Parse(row["Started (absolute)"].ToString()), DateTime.Parse(row["Ended (absolute)"].ToString()));

                // Assign TimeSaved based on the status and exception type
                if (row["Status"].ToString() != "Successful")
                {
                    if (row["Exception"].ToString() == "BusinessException")
                        row["TimeSaved"] = TimeSaved_BusEx;
                    else
                        row["TimeSaved"] = TimeSaved_SysEx;
                }
                else
                {
                    row["TimeSaved"] = TimeSaved_Success;
                }
            }

            return OriginalTable;
        }
    }

}