/*
 * File: IReporterTestable.cs
 * Project: Yash Standard Project
 * Component: CodedWorkflows Base Classes
 * 
 * Description: Interface that defines the contract for testable Reporter workflows.
 *              Provides reporter-specific testing capabilities and properties.
 * 
 * Author: Yash Team
 * Created: September 2025
 */

using System;

namespace Yash.StandardProject.CodedWorkflows.Reporter
{
    /// <summary>
    /// Interface that defines the contract for testable Reporter workflows.
    /// Extends the base ITestable interface with reporter-specific functionality.
    /// </summary>
    public interface IReporterTestable : ITestable
    {
        /// <summary>
        /// From date for report data filtering in tests.
        /// </summary>
        DateTime FromDate { get; set; }

        /// <summary>
        /// To date for report data filtering in tests.
        /// </summary>
        DateTime ToDate { get; set; }

        /// <summary>
        /// CRON schedule for report execution in tests.
        /// </summary>
        string Cron { get; set; }
    }
}