using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.JsonOverHttp.Formatting;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog.Sinks.JsonOverHttp
{
    public class JsonOverHttpSink : ILogEventSink, IDisposable
    {
        private bool _disposed = false;

        private readonly DynamicJsonFormatter _formatter;
        private readonly HttpClient _client;
        private readonly ConcurrentQueue<Task> _requests = new();
        private readonly HttpMessageConfiguration _config;

        // TODO: batching

        public JsonOverHttpSink(HttpMessageConfiguration messageConfiguration)
        {
            _formatter = new DynamicJsonFormatter(messageConfiguration);
            _client = new HttpClient();
            _config = messageConfiguration;
        }

        static object _lock = 1; // TEMP
        public void Emit(LogEvent logEvent)
        {
            if (_disposed)
            {
                return;
            }

            lock (_lock) // TEMP
            {
                var req = _formatter.BuildRequest(logEvent, out var uri);

                var task = new Task(async () =>
                {
                    var resp = await sendRequest(uri, req);
                    // TODO: reattempt failures
                    resp.ToString();
                    if (!resp.IsSuccessStatusCode)
                    {
                    }
                });
                _requests.Enqueue(task.ContinueWith(cleanQueue));
                task.Start();
                task.Wait(); // TEMP
            }
        }

        private Task<HttpResponseMessage> sendRequest(string uri, HttpContent req)
        {
            if (_config.Method.Method == HttpMethod.Post.Method)
            {
                return _client.PostAsync(uri, req);
            }
            if (_config.Method.Method == HttpMethod.Put.Method)
            {
                return _client.PutAsync(uri, req);
            }
            throw new NotImplementedException($"Unhandled HTTP method: {_config.Method.Method}");
        }

        private void cleanQueue(Task t)
        {
            if (_requests.TryPeek(out var task) && task.IsCompleted)
            {
                lock (_requests)
                {
                    while (_requests.TryPeek(out task) && task.IsCompleted)
                    {
                        _requests.TryDequeue(out _);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                var cts = new CancellationTokenSource(30_000);
                Task.WaitAll(_requests.ToArray(), cts.Token);
                _client.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
