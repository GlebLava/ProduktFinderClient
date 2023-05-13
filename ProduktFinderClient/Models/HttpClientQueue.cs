using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ProduktFinderClient.DataTypes
{
    public class HttpClientQueue
    {
        private readonly ConcurrentQueue<HttpRequestMessage> _queue = new ConcurrentQueue<HttpRequestMessage>();
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore;

        public HttpClientQueue(int maxConcurrency)
        {
            _semaphore = new SemaphoreSlim(maxConcurrency);

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler);
        }

        public async Task<HttpResponseMessage?> EnqueueAsync(HttpRequestMessage request)
        {
            return await EnqueueAsync(request, CancellationToken.None);
        }

        public async Task<HttpResponseMessage?> EnqueueAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _queue.Enqueue(request);
            await _semaphore.WaitAsync(); // Acquire a slot from the semaphore
            try
            {
                while (_queue.TryDequeue(out var queuedRequest))
                {
                    var response = await _httpClient.SendAsync(queuedRequest, cancellationToken);
                    return response;
                }
            }
            finally
            {
                _semaphore.Release(); // Release the slot back to the semaphore
            }

            return null;
        }
    }
}
