using System;

namespace Yash.Utility.Services.Misc
{
    /// <summary>
    /// Interface for miscellaneous helper service providing utility functions
    /// </summary>
    public interface IMiscHelperService
    {
        /// <summary>
        /// Checks if the current system time is within the given maintenance time range
        /// </summary>
        /// <param name="start">The start time of the maintenance window</param>
        /// <param name="end">The end time of the maintenance window</param>
        /// <param name="now">The current time of day (optional, for testing)</param>
        /// <returns>True if the current time is within the maintenance window, otherwise false</returns>
        bool IsMaintenanceTime(TimeSpan start, TimeSpan end, TimeSpan? now = null);

        /// <summary>
        /// Resets a folder by deleting it if it exists and creating a new empty one
        /// </summary>
        /// <param name="folderPath">Path to the folder to reset</param>
        void ResetFolder(string folderPath);

        /// <summary>
        /// Takes a screenshot and saves it to the specified location
        /// </summary>
        /// <param name="folder">Folder where the screenshot will be saved</param>
        /// <param name="file">Full file path (optional, will be generated if not provided)</param>
        /// <param name="prefix">Prefix for the generated filename</param>
        /// <returns>Full path to the saved screenshot</returns>
        string TakeScreenshot(string folder, string file, string prefix);
    }
}