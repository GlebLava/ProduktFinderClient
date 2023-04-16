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
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly SemaphoreSlim _semaphore;

        public HttpClientQueue(int maxConcurrency)
        {
            _semaphore = new SemaphoreSlim(maxConcurrency);
        }

        public async Task<HttpResponseMessage?> EnqueueAsync(HttpRequestMessage request)
        {
            _queue.Enqueue(request);
            await _semaphore.WaitAsync(); // Acquire a slot from the semaphore
            try
            {
                while (_queue.TryDequeue(out var queuedRequest))
                {
                    var response = await _httpClient.SendAsync(queuedRequest);
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
