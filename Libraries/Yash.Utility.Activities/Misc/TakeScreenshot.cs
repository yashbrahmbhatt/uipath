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
    public class TakeScreenshot : CodedWorkflow
    {
        [Workflow]
        public string Execute(string in_str_FolderPath, string in_str_FilePath, string in_str_Prefix)
        {
            if(string.IsNullOrEmpty(in_str_FilePath)) in_str_FilePath = Path.Combine(in_str_FolderPath, $"{in_str_Prefix}_{Environment.MachineName}_{Environment.UserDomainName}_{Environment.UserName}_{DateTime.Now.ToString("yyyyMMdd HHmmss")}.png");
            else in_str_FolderPath = new FileInfo(in_str_FilePath).Directory.FullName;
            
            if(!Directory.Exists(in_str_FolderPath)) Directory.CreateDirectory(in_str_FolderPath);
            
            var screenBounds = Screen.PrimaryScreen.Bounds;
            var ss = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);
            var graphic = Graphics.FromImage(ss);
            graphic.CopyFromScreen(new Point(0,0), new Point(screenBounds.X, screenBounds.Y), new Size(new Point(screenBounds.X, screenBounds.Y)));
            graphic.Dispose();
            ss.Save(in_str_FilePath);
            ss.Dispose();
            Log($"Screenshot saved at {in_str_FilePath}", LogLevel.Info);
            return in_str_FilePath;
        }
    }
}