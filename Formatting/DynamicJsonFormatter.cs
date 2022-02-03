using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Parsing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Serilog.Sinks.JsonOverHttp.Formatting
{
    public class DynamicJsonFormatter
    {
        private readonly static MessageTemplateParser PARSER = new();

        private readonly HttpMessageConfiguration _config;
        private readonly MessageTemplate _uriBuilder;
        private readonly Dictionary<string, MessageTemplate> _headers;

        public DynamicJsonFormatter(HttpMessageConfiguration config)
        {
            _config = config;

            _uriBuilder = PARSER.Parse(_config.UriTemplate);
            _headers = _config.Headers?.ToDictionary(h => h.Key, h => PARSER.Parse(h.Value)) ?? new();
        }

        public HttpContent BuildRequest(LogEvent logEvent, out string uri)
        {
            // Prepare required information
            var baseProps = OutputProperties.GetOutputProperties(logEvent);
            var properties = new Dictionary<string, LogEventPropertyValue>(baseProps)
            {
                ["MessageTemplate"] = new ScalarValue(logEvent.MessageTemplate)
            };

            uri = _uriBuilder.Render(properties);

            var payload = new StringBuilder();
            using var writer = new StringWriter(payload);
            _config.Body?.Render(logEvent, properties, writer);

            var content = payload.ToString();
            if (_config.ValidateBody || _config.FormatBody)
            {
                using var doc = JsonDocument.Parse(content);
                using var mem = new MemoryStream();
                using var jsonWriter = new Utf8JsonWriter(mem, new JsonWriterOptions() { Indented = true });
                doc.RootElement.WriteTo(jsonWriter);
                jsonWriter.Flush();
                content = Encoding.UTF8.GetString(mem.ToArray());
            }

            var req = new StringContent(content, Encoding.UTF8, "application/json");

            // Headers
            if (_headers.Count > 0)
            {
                foreach (var header in _headers)
                {
                    req.Headers.Add(header.Key, header.Value.Render(properties));
                }
            }

            return req;
        }
    }
}
