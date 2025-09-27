/*
 * File: IDispatcherTestable.cs
 * Project: Yash Standard Project
 * Component: CodedWorkflows Base Classes
 * 
 * Description: Interface that defines the contract for testable Dispatcher workflows.
 *              Provides dispatcher-specific testing capabilities and properties.
 * 
 * Author: Yash Team
 * Created: September 2025
 */

using System;

namespace Yash.StandardProject.CodedWorkflows.Dispatcher
{
    /// <summary>
    /// Interface that defines the contract for testable Dispatcher workflows.
    /// Extends the base ITestable interface with dispatcher-specific functionality.
    /// </summary>
    public interface IDispatcherTestable : ITestable
    {
        // Dispatcher workflows typically don't need additional properties beyond the base ITestable
        // The Execute method signature is already defined in ITestable and matches dispatcher needs
        // ConfigPath and ConfigScopes will be implemented as properties in the concrete classes
    }
}