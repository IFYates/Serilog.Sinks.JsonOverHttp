using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.JsonOverHttp.Formatting;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Serilog.Sinks.JsonOverHttp
{
    public class JsonOverHttpSink : ILogEventSink, IDisposable
    {
        private const uint RETRY_TIMES = 10;

        private bool _disposing = false;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly DynamicJsonFormatter _formatter;
        private readonly HttpClient _client;
        private readonly ConcurrentQueue<Task> _requests = new();
        private readonly HttpMessageConfiguration _config;

        private readonly BlockingCollection<LogEvent> _events = new();
        private readonly Task _consumer;

        public JsonOverHttpSink(HttpMessageConfiguration messageConfiguration)
        {
            _formatter = new DynamicJsonFormatter(messageConfiguration);
            _client = new HttpClient();
            _config = messageConfiguration;

            _consumer = new Task(logSender, TaskCreationOptions.LongRunning);
            _consumer.Start();
        }

        public void Emit(LogEvent logEvent)
        {
            if (_disposing)
            {
                return;
            }

            _events.Add(logEvent);
        }

        private void logSender()
        {
            // TODO: batching (configurable format, count, time, size)

            try
            {
                while (_events.TryTake(out var logEvent, -1, _cancellationTokenSource.Token) && !_events.IsCompleted)
                {
                    var task = Task.Run(async () =>
                    {
                        var req = _formatter.BuildRequest(logEvent, out var uri);
                        await recoverableSend(uri, req);
                    });
                    _requests.Enqueue(task);
                    task.ContinueWith(cleanQueue);
                }
            }
            catch (OperationCanceledException)
            {
                Dispose();
            }
        }

        private async Task recoverableSend(string uri, HttpContent req)
        {
            var attempt = 0;
            while (attempt++ < RETRY_TIMES && !_cancellationTokenSource.IsCancellationRequested)
            {
                if (attempt > 1)
                {
                    // Longer wait each time
                    await Task.Delay(500 * (int)Math.Pow(2, attempt - 2));
                }

                HttpResponseMessage? resp;
                try
                {
                    resp = await sendRequest(uri, req);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (resp.IsSuccessStatusCode || _disposing)
                {
                    break;
                }

                // Retry on server issue
                if ((int)resp.StatusCode is 429 or >= 500)
                {
                    continue;
                }

                // Failure
                SelfLog.WriteLine($"Failed to send log to server: {resp.StatusCode}");
                break;
            }
        }

        private Task<HttpResponseMessage> sendRequest(string uri, HttpContent req)
        {
            if (_config.Method.Method == HttpMethod.Post.Method)
            {
                return _client.PostAsync(uri, req, _cancellationTokenSource.Token);
            }
            if (_config.Method.Method == HttpMethod.Put.Method)
            {
                return _client.PutAsync(uri, req, _cancellationTokenSource.Token);
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

        private const int DISPOSE_TIMEOUT = 5_000;
        public void Dispose()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                var sw = Stopwatch.StartNew();
                _events.CompleteAdding();
                _consumer.Wait(DISPOSE_TIMEOUT);
                var rem = Math.Max(0, DISPOSE_TIMEOUT - (int)sw.ElapsedMilliseconds);
                Task.WaitAll(_requests.ToArray(), rem);
                _cancellationTokenSource.Cancel();
                _client.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
