using Serilog.Core;
using Serilog.Events;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Serilog.Sinks.JsonOverHttp.Enrichers
{
    public class StackTraceField : ILogEventEnricher
    {
        private readonly int _skipFrames;
        private readonly bool _reverseOrder;
        private readonly int _showFrames;

        public StackTraceField(int skipFrames, bool reverseOrder, int showFrames)
        {
            _skipFrames = skipFrames;
            _reverseOrder = reverseOrder;
            _showFrames = showFrames;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var stack = (IEnumerable<StackFrame>)new StackTrace(_skipFrames).GetFrames();
            if (_reverseOrder)
            {
                stack = stack.Reverse();
            }
            stack = stack.Take(_showFrames);

            // TODO
        }
    }
}
