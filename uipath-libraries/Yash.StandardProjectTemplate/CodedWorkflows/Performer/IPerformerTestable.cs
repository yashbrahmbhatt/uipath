/*
 * File: IPerformerTestable.cs
 * Project: Yash Standard Project
 * Component: CodedWorkflows Base Classes
 * 
 * Description: Interface that defines the contract for testable Performer workflows.
 *              Provides performer-specific testing capabilities and properties.
 * 
 * Author: Yash Team
 * Created: September 2025
 */

using System;

namespace Yash.StandardProject.CodedWorkflows.Performer
{
    /// <summary>
    /// Interface that defines the contract for testable Performer workflows.
    /// Extends the base ITestable interface with performer-specific functionality.
    /// </summary>
    public interface IPerformerTestable : ITestable
    {
        /// <summary>
        /// Queue item reference for performer testing.
        /// Performer tests typically work with specific queue items.
        /// </summary>
        string QueueItemReference { get; set; }
        
        /// <summary>
        /// Transaction data for performer testing.
        /// Can contain test-specific transaction data or mock data.
        /// </summary>
        object TransactionData { get; set; }
    }
}