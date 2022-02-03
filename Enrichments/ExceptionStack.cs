using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.JsonOverHttp.Enrichers
{
    public class ExceptionStack : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var prop = propertyFactory.CreateProperty("ExceptionMessage", logEvent.Exception?.Message);
            logEvent.AddPropertyIfAbsent(prop);

            prop = propertyFactory.CreateProperty("ExceptionStack", logEvent.Exception?.StackTrace);
            logEvent.AddPropertyIfAbsent(prop);
        }
    }
}
