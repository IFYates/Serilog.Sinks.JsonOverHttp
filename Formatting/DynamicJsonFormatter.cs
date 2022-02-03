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
            var data = OutputProperties.GetOutputProperties(logEvent);

            // Resolve payload
            var payload = new StringBuilder();
            using var writer = new StringWriter(payload);
            _config.Body?.Render(logEvent, data, writer);
            var content = payload.ToString();

            // Parse as necessary
            if (_config.ValidateBody || _config.FormatBody)
            {
                using var doc = JsonDocument.Parse(content);
                if (_config.FormatBody)
                {
                    using var mem = new MemoryStream();
                    using var jsonWriter = new Utf8JsonWriter(mem, new JsonWriterOptions() { Indented = true });
                    doc.RootElement.WriteTo(jsonWriter);
                    jsonWriter.Flush();
                    content = Encoding.UTF8.GetString(mem.ToArray());
                }
            }

            var req = new StringContent(content, Encoding.UTF8, "application/json");

            // URI and Headers
            uri = _uriBuilder.Render(data);
            foreach (var header in _headers)
            {
                req.Headers.Add(header.Key, header.Value.Render(data));
            }

            return req;
        }
    }
}
