using Serilog.Events;
using Serilog.Sinks.JsonOverHttp.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Serilog.Sinks.JsonOverHttp
{
    public class JOHProperties : JOHAtom
    {
        public string[] PropertiesToExclude { get; }

        public JOHProperties(string[] propertiesToExclude)
        {
            PropertiesToExclude = propertiesToExclude ?? Array.Empty<string>();
        }

        public override void Render(LogEvent logEvent, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, IFormatProvider formatProvider = null)
        {
            var buff = new StringBuilder();
            using var buffer = new StringWriter(buff);
            var writer = new EscapedJsonWriter(output);

            var first = true;
            foreach (var prop in logEvent.Properties.Where(e => PropertiesToExclude.Contains(e.Key) != true))
            {
                output.Write(first ? '{' : ',');
                first = false;

                output.Write($"\"{prop.Key}\":");

                JOHValue.RenderValue(prop.Value, buffer, formatProvider);
                if (buff.Length > 0)
                {
                    output.Write('"');
                    writer.Write(buff.ToString());
                    output.Write('"');
                    buff.Clear();
                }
                else
                {
                    output.Write("null");
                }
            }
            if (!first)
            {
                output.Write('}');
            }
        }
    }
}
