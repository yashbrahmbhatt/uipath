using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using UiPath.Core;
using UiPath.Core.Activities.Storage;
using UiPath.Excel;
using UiPath.Excel.Activities;
using UiPath.Excel.Activities.API;
using UiPath.Excel.Activities.API.Models;
using UiPath.GSuite.Activities.Api;
using UiPath.Orchestrator.Client.Models;
using UiPath.Testing;
using UiPath.Testing.Activities.Api.Models;
using UiPath.Testing.Activities.Models;
using UiPath.Testing.Activities.TestData;
using UiPath.Testing.Activities.TestDataQueues.Enums;
using UiPath.Testing.Enums;

namespace Yash.Utility.Activities.Misc
{
    public static class EnumUtilities
    {
        public static string GetEnumLabel<TEnum>(TEnum value) where TEnum : Enum
        {
            FieldInfo field = typeof(TEnum).GetField(value.ToString());
            if (field == null)
                return value.ToString();

            var descriptionAttr = field.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttr != null ? descriptionAttr.Description : value.ToString();
        }
    }
}