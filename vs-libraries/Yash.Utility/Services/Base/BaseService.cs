using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yash.Utility.Services.Base
{
    public abstract class BaseService
    {
        private readonly IServiceProvider? _serviceProvider;
        private readonly Action<string, TraceEventType>? _logAction = null;
        private readonly TraceEventType _minLogLevel = TraceEventType.Information;
        internal void Log(string msg, TraceEventType level = TraceEventType.Information)
        {
            if (level <= _minLogLevel)
                _logAction?.Invoke($"[ConfigService] {msg}", level);
        }
        public BaseService(IServiceProvider? serviceProvider = null, Action<string, TraceEventType>? log = null, TraceEventType? minLogLevel = TraceEventType.Information)
        {
            _serviceProvider = serviceProvider;
            _logAction = log;
            _minLogLevel = minLogLevel ?? TraceEventType.Information;
        }
    }
}
