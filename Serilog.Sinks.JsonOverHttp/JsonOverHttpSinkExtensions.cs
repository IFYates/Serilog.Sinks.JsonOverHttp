using Serilog.Configuration;
using Serilog.Events;

namespace Serilog.Sinks.JsonOverHttp
{
    public static class JsonOverHttpSinkExtensions
    {
        public static LoggerConfiguration JsonOverHttp(this LoggerSinkConfiguration loggerConfiguration, HttpMessageConfiguration messageConfiguration, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
        {
            return loggerConfiguration.Sink(new JsonOverHttpSink(messageConfiguration), restrictedToMinimumLevel);
        }
    }

}
