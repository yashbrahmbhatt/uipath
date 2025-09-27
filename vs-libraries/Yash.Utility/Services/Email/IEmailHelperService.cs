using System;
using System.Collections.Generic;
using System.Data;
using UiPath.Core;
using UiPath.Robot.Activities.Api;
using Yash.Utility.Services.Email.Models;

namespace Yash.Utility.Services.Email
{
    /// <summary>
    /// Interface for email helper service that provides email template and formatting utilities
    /// </summary>
    public interface IEmailHelperService
    {
        /// <summary>
        /// Generates a diagnostic dictionary with environment, exception, and transaction information
        /// </summary>
        /// <param name="ex">Optional exception to include</param>
        /// <param name="transaction">Optional queue item transaction</param>
        /// <param name="jobInfo">Optional running job information</param>
        /// <returns>Dictionary containing diagnostic information</returns>
        Dictionary<string, object> GenerateDiagnosticDictionary(Exception? ex = null, QueueItem? transaction = null, IRunningJobInformation? jobInfo = null);

        /// <summary>
        /// Dumps email templates to specified folder
        /// </summary>
        /// <param name="folderPath">Path to folder where templates will be saved</param>
        void DumpEmailTemplates(string folderPath);

        /// <summary>
        /// Prepares an email template by replacing placeholders with data values
        /// </summary>
        /// <param name="bodyTemplate">HTML email template</param>
        /// <param name="data">Data dictionary for placeholder replacement</param>
        /// <returns>Tuple containing prepared subject and body</returns>
        (string body, string subject) PrepareEmailTemplate(string bodyTemplate, Dictionary<string, object> data);

        /// <summary>
        /// Extracts subject from HTML title tag
        /// </summary>
        /// <param name="htmlBody">HTML content</param>
        /// <returns>Extracted subject</returns>
        string ExtractSubjectFromHtml(string htmlBody);
    }
}