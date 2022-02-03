using System;
using System.IO;
using System.Text;

namespace Serilog.Sinks.JsonOverHttp.Formatting
{
    /// <summary>
    /// Wraps a <see cref="TextWriter"/> instance where the written values are escaped as JSON.
    /// </summary>
    public class EscapedJsonWriter : TextWriter
    {
        private readonly TextWriter _underlying;

        public bool QuoteOnOpen { get; set; }
        public bool EmptyIsNull { get; set; }
        public bool HasWritten { get; private set; }

        public override Encoding Encoding => throw new NotImplementedException();

        public EscapedJsonWriter(TextWriter underlying)
        {
            _underlying = underlying;
        }

        public override void Write(object? value)
        {
            Write(value?.ToString());
        }
        public override void Write(string? value)
        {
            if (value != null && (!EmptyIsNull || value.Length > 0))
            {
                if (QuoteOnOpen && !HasWritten)
                {
                    _underlying.Write('"');
                }
                _underlying.Write(value
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\x0A", "\\u000A")
                    .Replace("\x0D", "\\u000D"));
                HasWritten = true;
            }
        }
    }
}
