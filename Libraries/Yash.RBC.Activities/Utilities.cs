using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UiPath.CodedWorkflows;

namespace Yash.RBC.Activities
{
    public enum AccountTypes
    {
        All,
        ChequingAndSavings,
        Credit
    }

    public enum TransactionScopes
    {
        SinceLastDownload,
        All
    }
    public static class Utilities
    {
        public static void LogThings(string message, Delegate log, Delegate function = null, params object[] args)
        {
            // Get the method information from the delegate
            if (function != null)
            {
                MethodInfo methodInfo = function.Method;
                // Get parameter information
                ParameterInfo[] parameters = methodInfo.GetParameters();
                // Log the method name
                log.DynamicInvoke(message, LogLevel.Info, new Dictionary<string, object>());
                // Log the parameters and their values
                for (int i = 0; i < parameters.Length; i++)
                {
                    string paramName = parameters[i].Name;
                    object paramValue = args.Length > i ? args[i] : "No Value";
                    log.DynamicInvoke($"Parameter: {paramName}, Value: {paramValue}", LogLevel.Trace, new Dictionary<string, object>());
                }
            }
            else
            {
                log.DynamicInvoke(message + string.Join(", ", args.Select(a => JsonConvert.SerializeObject(a, Formatting.Indented))), LogLevel.Info, new Dictionary<string, object>());
            }
        }



        public static string GetTransactionScope(TransactionScopes scope)
        {
            if (scope == TransactionScopes.All)
                return "All Transactions on File";
            if (scope == TransactionScopes.SinceLastDownload)
                return "Only New Transactions Since Last Download";
            throw new Exception("Invalid Transaction Scope");
        }

        public static string GetAccountTypes(AccountTypes type)
        {
            if (type == AccountTypes.All)
                return "All Chequing, Savings & Credit Card Accounts";
            if (type == AccountTypes.ChequingAndSavings)
                return "All Chequing & Savings Accounts";
            if (type == AccountTypes.Credit)
                return "All Credit Card Accounts";
            throw new Exception("Invalid Account Type");
        }
    }
}