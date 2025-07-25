using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Yash.Config.Helpers
{
    public static class FileDialogHelpers
    {
        public static string? PromptForFile(
            string title = "Select a File",
            string filter = "All Files (*.*)|*.*",
            string initialDirectory = "")
        {
            using var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                Multiselect = false,
                InitialDirectory = initialDirectory
            };

            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
        }

        public static List<string>? PromptForFiles(
            string title = "Select Files",
            string filter = "All Files (*.*)|*.*",
            string initialDirectory = "")
        {
            using var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                Multiselect = true,
                InitialDirectory = initialDirectory
            };

            return dialog.ShowDialog() == DialogResult.OK ? new List<string>(dialog.FileNames) : null;
        }

        public static string? PromptForDirectory(string description = "Select a Folder",
            string initialDirectory = "")
        {
            using var dialog = new FolderBrowserDialog
            {
                UseDescriptionForTitle = true,
                Description = description,
                ShowNewFolderButton = true,
                InitialDirectory = initialDirectory
            };

            return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
        }

        public static string? PromptForSaveFile(
            string title = "Save File As",
            string filter = "All Files (*.*)|*.*",
            string defaultFileName = "",
            string initialDirectory = "")
        {
            using var dialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                FileName = defaultFileName,
                InitialDirectory = initialDirectory
            };

            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
        }

        public static bool? PromptYesNoCancel(string title, string message)
        {
            var result = System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            return result switch
            {
                MessageBoxResult.Yes => true,
                MessageBoxResult.No => false,
                _ => null,
            };
        }
    }
}
