using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UiPath.CodedWorkflows;

namespace Yash.Utility.Services.Misc
{
    public class MiscHelperService : Base.BaseService, IMiscHelperService
    {
        private readonly Action<string, TraceEventType>? _log;

        /// <summary>
        /// Creates a new MiscHelperService instance
        /// </summary>
        /// <param name="log">Optional logging action</param>
        public MiscHelperService(Action<string, TraceEventType>? log = null)
        {
            _log = log;
        }

        private void Log(string message, TraceEventType level = TraceEventType.Information)
        {
            _log?.Invoke($"[MiscHelperService] {message}", level);
        }
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
        public bool IsMaintenanceTime(TimeSpan start, TimeSpan end, TimeSpan? now = null)
        {
            // Overload support for either providing or not providing 'now', currently only to support tests. 
            // I would provide a default value for it but DateTime, and derivative types like TimeSpan are not Compile-Time constants, which is a requirement 
            // of coded workflows. ROI still positive, 
            TimeSpan curr;
            if (now == null || now == new DateTime(0).TimeOfDay) curr = DateTime.Now.TimeOfDay;
            else curr = now.Value;

            /// Determines if the current time falls within the maintenance window.
            /// If the start time is earlier than or equal to the end time (e.g., 01:00-05:00), 
            /// the current time must be between start and end.
            /// If the start time is later than the end time (e.g., 22:00-04:00), 
            /// the current time can be either after the start time or before the end time.
            var result = start <= end ? curr >= start && curr <= end : curr >= start || curr <= end;
            Log($"IsMaintenanceTime: {curr} - {result}", TraceEventType.Information);
            return result;
        }

        public void ResetFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
            Directory.CreateDirectory(folderPath);
        }

        public string TakeScreenshot(string folder, string file, string prefix)
        {
            // Validate folder parameter
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentException("Folder path cannot be null or empty", nameof(folder));

            // Sanitize prefix to remove invalid filename characters
            if (!string.IsNullOrEmpty(prefix))
            {
                var invalidChars = Path.GetInvalidFileNameChars();
                foreach (var invalidChar in invalidChars)
                {
                    prefix = prefix.Replace(invalidChar, '_');
                }
                // Also handle additional problematic characters
                prefix = prefix.Replace("<", "_").Replace(">", "_").Replace(":", "_")
                              .Replace("|", "_").Replace("?", "_").Replace("*", "_")
                              .Replace("\"", "_").Replace("\\", "_");

                // Truncate prefix if too long to avoid path length issues
                if (prefix.Length > 50)
                {
                    prefix = prefix.Substring(0, 50);
                }
            }

            // Create filename if not provided
            if (string.IsNullOrEmpty(file))
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd HHmmss");
                string filename;
                if (!string.IsNullOrEmpty(prefix))
                {
                    filename = $"{prefix}_Screenshot_{Environment.MachineName}_{Environment.UserDomainName}_{Environment.UserName}_{timestamp}.png";
                }
                else
                {
                    filename = $"_{Environment.MachineName}_{Environment.UserDomainName}_{Environment.UserName}_{timestamp}.png";
                }
                file = Path.Combine(folder, filename);
            }
            else
            {
                // If file path is provided, extract folder from it
                var fileInfo = new FileInfo(file);
                folder = fileInfo.Directory?.FullName ?? folder;
            }

            // Ensure directory exists
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // Take screenshot
            var screenBounds = Screen.PrimaryScreen.Bounds;
            using (var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(0, 0, 0, 0, screenBounds.Size);
                }
                bitmap.Save(file);
            }

            Log($"Screenshot saved at {file}", TraceEventType.Information);
            return file;
        }
    }
}
