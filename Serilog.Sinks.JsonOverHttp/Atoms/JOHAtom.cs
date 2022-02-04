using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;

namespace Serilog.Sinks.JsonOverHttp
{
    public abstract class JOHAtom
    {
        public static implicit operator JOHAtom(string value)
        {
            return new JOHValue(value);
        }

        public static implicit operator JOHAtom(Dictionary<string, JOHAtom> value)
        {
            return new JOHObject(value);
        }

        public abstract void Render(LogEvent logEvent, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, IFormatProvider formatProvider = null);
    }
}
