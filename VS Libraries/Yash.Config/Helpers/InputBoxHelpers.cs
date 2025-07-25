using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Config.Helpers
{
    public static class InputBoxHelpers
    {
        public static string PromptForText(string title, string promptText, string defaultValue = "")
        {
            return Interaction.InputBox(promptText, title, defaultValue);
        }
    }
}
