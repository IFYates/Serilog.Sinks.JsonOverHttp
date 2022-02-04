using System.Collections.Generic;
using System.Net.Http;

namespace Serilog.Sinks.JsonOverHttp
{
    public class HttpMessageConfiguration
    {
        public string UriTemplate { get; set; } = string.Empty;
        public HttpMethod Method { get; set; } = HttpMethod.Post;

        public IDictionary<string, string>? Headers { get; set; }
        public JOHObject? Body { get; set; }

        /// <summary>
        /// Parse the content body before sending to ensure valid JSON.
        /// </summary>
        public bool ValidateBody { get; set; }
        /// <summary>
        /// Send formatted (indented) JSON.
        /// </summary>
        public bool FormatBody { get; set; }
    }
}
