using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;
using System.Linq;

namespace Serilog.Sinks.JsonOverHttp.Enrichers
{
    public class MessageTemplateField : ILogEventEnricher
    {
        public bool OnlyIfDifferent { get; }

        public MessageTemplateField(bool onlyIfDifferent = false)
        {
            OnlyIfDifferent=onlyIfDifferent;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (!OnlyIfDifferent || logEvent.MessageTemplate.Tokens.Count() > 1 || logEvent.MessageTemplate.Tokens.First() is PropertyToken)
            {
                var prop = propertyFactory.CreateProperty("MessageTemplate", logEvent.MessageTemplate.Text);
                logEvent.AddPropertyIfAbsent(prop);
            }
        }
    }
}
