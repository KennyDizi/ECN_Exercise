using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ECN_Exercise.Sources.Utils
{
    public interface IXModernHttpClientService
    {
        ISerializer Serializer { get; }
        Uri BaseAddress { get; set; }
        long MaxResponseContentBufferSize { get; set; }
        HttpRequestHeaders DefaultRequestHeaders { get; }
        TimeSpan Timeout { get; set; }

        #region Task

        Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption,
            CancellationToken cancellationToken);

        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content,
            CancellationToken cancellationToken);

        Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken);

        Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content,
            CancellationToken cancellationToken);

        Task GetTaskAsync<T>(string requestUri, Func<T, Task> processorSuccess, Func<Task> processorError = null) where T : class;
        Task GetTaskAsyncCallback<T>(string requestUri, Func<T, object, Task> processorSuccess, Func<Task> processorError = null, object callbackObject = null) where T : class;

        Task PostTaskAsync<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues, Func<T, Task> processorSuccess, Func<Task> processorError = null) where T : class;
        Task PostTaskAsync<T>(string requestUri, HttpContent content, Func<T, Task> processorSuccess, Func<Task> processorError = null) where T : class;
        Task PostTaskAsyncCallback<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues, Func<T, object, Task> processorSuccess, Func<Task> processorError = null, object callbackObject = null) where T : class;
        Task PostTaskAsyncCallback<T>(string requestUri, HttpContent content, Func<T, object, Task> processorSuccess, Func<Task> processorError = null, object callbackObject = null) where T : class;

        Task DeleteTaskAsync<T>(string requestUri, Func<T, Task> processorSuccess, Func<Task> processorError = null) where T : class;
        Task DeleteTaskAsyncCallback<T>(string requestUri, Func<T, object, Task> processorSuccess, Func<Task> processorError = null, object callbackObject = null) where T : class;

        Task PutTaskAsync<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues, Func<T, Task> processorSuccess, Func<Task> processorError = null) where T : class;
        Task PutTaskAsync<T>(string requestUri, HttpContent content, Func<T, Task> processorSuccess, Func<Task> processorError = null) where T : class;
        Task PutTaskAsyncCallback<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues, Func<T, object, Task> processorSuccess, Func<Task> processorError = null, object callbackObject = null) where T : class;
        Task PutTaskAsyncCallback<T>(string requestUri, HttpContent content, Func<T, object, Task> processorSuccess, Func<Task> processorError = null, object callbackObject = null) where T : class;

        #endregion
    }
}