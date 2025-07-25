using System;
using System.Collections.Generic;
using System.Data;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace DX.AutoDesign
{
    public class Process
    {
        public string Name;
        public ProcessType Type;
        public string Description;
        public List<Step> Steps;
    }

    public enum ProcessType
    {
        /// <summary>
        /// A sequence based workflow that initializes settings, initializes applications (if required),
        /// does the necessary steps to retrieve units of work, and dispatches them to a queue.
        /// Exception handling is handled in a try catch around everything except for the setting initialization
        /// and sends an email to an asset of users based on an HTML template.
        /// This shouldn't be scaled to multiple robots so since it is best practice to have only a single source of truth
        /// for dispatching units of work.
        /// </summary>
        Dispatcher,
        /// <summary>
        /// A sequence based workflow that initializes settings, initializes applications (if required),
        /// retrieves a single transaction, does the necessary steps in a unit of work, updates the transaction,
        /// and propagates it to any downstream queues (if required).
        /// This type is scalable across multiple robots.
        /// This type can support persistance (pausing the automation for human in the loop)
        /// Exception handling is handled in a try catch around everything except for the setting initialization
        /// and sends an email to an asset of users based on an HTML template.
        /// </summary>
        SingleTransactionPerformer,
        /// <summary>
        /// A state-machine based workflow with Initialize, Get Transaction Item, Process Transaction, and End states.
        /// Initializes settings, initializes applications (if required),
        /// retrieves a single transaction, does the necessary steps in a unit of work, updates the transaction,
        /// and propagates it to any downstream queues (if required), then loops back to retrieving a single transaction
        /// or reinitializing applications depending on the outcome (success, business exception, system exception)
        /// until no more transactions are left in the queue. 
        /// This type is scalable across multiple robots.
        /// This type cannot support persistance (pausing the automation for human in the loop)
        /// Exception handling is handled in a try catch around the Process Transaction state
        /// and sends an email to an asset of users based on an HTML template.
        /// </summary>
        MultiTransactionPerformer,
        /// <summary>
        /// A sequence based workflow that initializes settings, retrieves a single transaction, 
        /// runs the classifier, creates a classification validation task (if required), and then for each document,
        /// runs the extractor, creates an extraction validation task (if required), updates the transaction,
        /// and propagates it to any downstream queues (if required).
        /// This type is scalable across multiple robots.
        /// This type can support persistance (pausing the automation for human in the loop)
        /// Exception handling is handled in a try catch around everything except for the setting initialization
        /// and sends an email to an asset of users based on an HTML template.
        /// </summary>
        DocumentUnderstanding,

    }
}