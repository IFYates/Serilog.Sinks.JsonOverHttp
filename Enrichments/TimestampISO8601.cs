using Serilog.Core;
using Serilog.Events;

namespace Serilog.Sinks.JsonOverHttp.Enrichers
{
    public class TimestampISO8601 : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var prop = propertyFactory.CreateProperty("TimestampISO8601", logEvent.Timestamp.UtcDateTime.ToString("o"));
            logEvent.AddPropertyIfAbsent(prop);
        }
    }
}
