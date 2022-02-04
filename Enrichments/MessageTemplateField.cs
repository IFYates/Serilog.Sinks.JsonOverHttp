using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.JsonOverHttp.Enrichers
{
    public class MessageTemplateField : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var prop = propertyFactory.CreateProperty("MessageTemplate", logEvent.MessageTemplate.Text);
            logEvent.AddPropertyIfAbsent(prop);
        }
    }
}
