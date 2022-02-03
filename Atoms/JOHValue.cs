using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.JsonOverHttp.Formatting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Serilog.Sinks.JsonOverHttp
{
    public class JOHValue : JOHAtom
    {
        private readonly static MessageTemplateParser _parser = new();

        private MessageTemplate _template;

        public JOHValue(string template)
        {
            _template = _parser.Parse(template);
        }

        public override void Render(LogEvent logEvent, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, IFormatProvider formatProvider = null)
        {
            var writer = new EscapedJsonWriter(output)
            {
                QuoteOnOpen = true,
                EmptyIsNull = true
            };
            _template.Render(properties, writer, formatProvider);
            if (writer.HasWritten)
            {
                output.Write('"');
            }
        }

        public static void RenderValue(LogEventPropertyValue value, TextWriter output, IFormatProvider formatProvider = null)
        {
            if (value is ScalarValue sv)
            {
                output.Write(sv.Value);
            }
            else
            {
                value.Render(output, formatProvider: formatProvider);
            }
        }
    }
}
