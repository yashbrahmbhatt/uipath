using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using UiPath.CodedWorkflows;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Orchestrator.Client.Models;

namespace Yash.Utility.Activities.Misc
{
    /// <summary>
    /// A coded workflow activity that captures a screenshot of the primary screen and saves it to a specified file path.
    /// </summary>
    public class TakeScreenshot : CodedWorkflow
    {
        /// <summary>
        /// Captures a screenshot of the primary screen and saves it to the specified folder or file path.
        /// </summary>
        /// <param name="in_str_FolderPath">The folder path where the screenshot should be saved. Used only if <paramref name="in_str_FilePath"/> is null or empty.</param>
        /// <param name="in_str_FilePath">Optional full file path for the screenshot. If not provided, a file path is auto-generated using the folder path and a naming convention.</param>
        /// <param name="in_str_Prefix">A prefix to include in the auto-generated file name (if <paramref name="in_str_FilePath"/> is not provided).</param>
        /// <returns>The full path of the saved screenshot file.</returns>
        [Workflow]
        public string Execute(string in_str_FolderPath, string in_str_FilePath, string in_str_Prefix)
        {
            if (string.IsNullOrEmpty(in_str_FilePath))
            {
                in_str_FilePath = Path.Combine(
                    in_str_FolderPath,
                    $"{in_str_Prefix}_{Environment.MachineName}_{Environment.UserDomainName}_{Environment.UserName}_{DateTime.Now:yyyyMMdd HHmmss}.png");
            }
            else
            {
                in_str_FolderPath = new FileInfo(in_str_FilePath).Directory.FullName;
            }

            if (!Directory.Exists(in_str_FolderPath))
            {
                Directory.CreateDirectory(in_str_FolderPath);
            }

            var screenBounds = Screen.PrimaryScreen.Bounds;
            var ss = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);
            var graphic = Graphics.FromImage(ss);
            graphic.CopyFromScreen(
                new Point(0, 0),
                new Point(screenBounds.X, screenBounds.Y),
                new Size(new Point(screenBounds.X, screenBounds.Y)));
            graphic.Dispose();

            ss.Save(in_str_FilePath);
            ss.Dispose();

            Log($"Screenshot saved at {in_str_FilePath}", LogLevel.Info);
            return in_str_FilePath;
        }
    }
}