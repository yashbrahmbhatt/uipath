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

namespace Finance.Automations._00_Shared
{
    public class TakeScreenshot : CodedWorkflow
    {
        [Workflow]
        public string Execute(string folder, string file, string prefix)
        {
            if(string.IsNullOrEmpty(file)) file = Path.Combine(folder, $"{prefix}_{Environment.MachineName}_{Environment.UserDomainName}_{Environment.UserName}_{DateTime.Now.ToString("yyyyMMdd HHmmss")}.png");
            else folder = new FileInfo(file).Directory.FullName;
            
            if(!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            
            var screenBounds = Screen.PrimaryScreen.Bounds;
            var ss = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);
            var graphic = Graphics.FromImage(ss);
            graphic.CopyFromScreen(new Point(0,0), new Point(screenBounds.X, screenBounds.Y), new Size(new Point(screenBounds.X, screenBounds.Y)));
            graphic.Dispose();
            ss.Save(file);
            ss.Dispose();
            Log($"Screenshot saved at {file}", LogLevel.Info);
            return file;
            // To start using services, use IntelliSense (CTRL + Space) to discover the available services:
            // e.g. system.GetAsset(...)

            // For accessing UI Elements from Object Repository, you can use the Descriptors class e.g:
            // var screen = uiAutomation.Open(Descriptors.MyApp.FirstScreen);
            // screen.Click(Descriptors.MyApp.FirstScreen.SettingsButton);
        }
    }
}