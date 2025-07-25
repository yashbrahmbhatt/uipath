using System;
using TraceEventType = System.Diagnostics.TraceEventType;

namespace Yash.Config.Helpers
{
    public interface ILogger
    {
        void Log(string message, TraceEventType type = TraceEventType.Information);
    }

    public class DelegateLogger : ILogger
    {
        private readonly Action<string, TraceEventType>? _logAction;
        private readonly string _prefix;

        public DelegateLogger(Action<string, TraceEventType>? logAction = null, string prefix = "")
        {
            _logAction = logAction;
            _prefix = prefix;
        }

        public void Log(string message, TraceEventType type = TraceEventType.Information)
        {
            _logAction?.Invoke($"{_prefix}{message}", type);
        }
    }

    public abstract class LoggableWorkflow
    {
        protected ILogger Logger { get; }

        protected LoggableWorkflow(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected void Log(string message, TraceEventType type = TraceEventType.Verbose)
        {
            Logger.Log(message, type);
        }
    }

}
