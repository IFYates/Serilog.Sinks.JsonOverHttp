using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Serilog.Sinks.JsonOverHttp
{
    public class JOHObject : JOHAtom
    {
        private readonly IDictionary<string, JOHAtom> _value;

        public JOHObject(IDictionary<string, JOHAtom> value)
        {
            _value = value;
        }

        public static implicit operator JOHObject(Dictionary<string, JOHAtom> value)
        {
            return new JOHObject(value);
        }

        public override void Render(LogEvent logEvent, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, IFormatProvider formatProvider = null)
        {
            var buff = new StringBuilder();
            using var buffer = new StringWriter(buff);

            var first = true;
            foreach (var item in _value)
            {
                // Only add property if has a value
                item.Value.Render(logEvent, properties, buffer, formatProvider);
                if (buff.Length > 0)
                {
                    output.Write(first ? '{' : ',');
                    first = false;

                    output.Write($"\"{item.Key}\":");
                    output.Write(buff);
                    buff.Clear();
                }
            }
            if (!first)
            {
                output.Write('}');
            }
        }
    }

}
