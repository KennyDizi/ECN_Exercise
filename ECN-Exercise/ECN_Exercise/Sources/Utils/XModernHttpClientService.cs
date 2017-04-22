using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ModernHttpClient;
using Plugin.Connectivity;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace ECN_Exercise.Sources.Utils
{
    public class XModernHttpClientService : IXModernHttpClientService
    {
        /// <summary>
        /// miliseconds a request timeout
        /// </summary>
        private const int AppTimeOut = 20000;

        private JSonSerializer _serializer;

        public ISerializer Serializer
            => LazyInitializer.EnsureInitialized(ref _serializer, () => new JSonSerializer());

        private HttpClient BaseClient { get; }

        public Uri BaseAddress
        {
            get { return BaseClient.BaseAddress; }
            set { BaseClient.BaseAddress = value; }
        }

        public long MaxResponseContentBufferSize
        {
            get { return BaseClient.MaxResponseContentBufferSize; }
            set { BaseClient.MaxResponseContentBufferSize = value; }
        }

        public HttpRequestHeaders DefaultRequestHeaders => BaseClient.DefaultRequestHeaders;

        public TimeSpan Timeout
        {
            get { return BaseClient.Timeout; }
            set { BaseClient.Timeout = value; }
        }

        #region constructor

        private XModernHttpClientService()
        {
            var handler = new NativeMessageHandler(throwOnCaptiveNetwork: true, customSSLVerification: true);
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip |
                                                 DecompressionMethods.Deflate;
            }

            var httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(AppTimeOut)
            };

            if (httpClient.DefaultRequestHeaders != null)
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type",
                    "application/json; charset=utf-8; multipart/form-data; application/octet-stream");
            }

            BaseClient = httpClient;
        }

        private static readonly Lazy<XModernHttpClientService> Lazy =
            new Lazy<XModernHttpClientService>(() => new XModernHttpClientService());

        public static IXModernHttpClientService Instance => Lazy.Value;

        #endregion

        #region origin method

        public Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            return !CrossConnectivity.Current.IsConnected
                ? null
                : BaseClient.GetAsync(requestUri, completionOption, cancellationToken);
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content,
            CancellationToken cancellationToken)
        {
            return !CrossConnectivity.Current.IsConnected
                ? null
                : BaseClient.PostAsync(requestUri, content, cancellationToken);
        }

        public Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken)
        {
            return !CrossConnectivity.Current.IsConnected
                ? null
                : BaseClient.DeleteAsync(requestUri, cancellationToken);
        }

        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content,
            CancellationToken cancellationToken)
        {
            return !CrossConnectivity.Current.IsConnected
                ? null
                : BaseClient.PutAsync(requestUri, content, cancellationToken);
        }

        #endregion

        #region Task

        #region get

        /// <summary>
        /// get with call back is a task
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="processorSuccess"></param>
        /// <param name="processorError"></param>
        /// <returns></returns>
        public async Task GetTaskAsync<T>(string requestUri, Func<T, Task> processorSuccess,
            Func<Task> processorError = null)
            where T : class
        {
            Debug.WriteLine($"Get method with url: {requestUri}");
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var responseMessage = await GetAsync(requestUri, HttpCompletionOption.ResponseContentRead, cts.Token);
                    if (responseMessage == null)
                    {
                        if (processorError != null) await processorError();
                    }
                    else
                    {
                        responseMessage = responseMessage.EnsureSuccessStatusCode();
                        var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                        var result = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            await processorSuccess(result);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        if (processorError != null) await processorError();
                        var token = cts.Token;
                        Debug.WriteLine(
                            $"A real cancellation, triggered by the caller, token is: {token}, message: {ex.Message}, url: {requestUri}");
                    }
                    else
                    {
                        if (processorError != null) await processorError();
                        Debug.WriteLine($"A web request timeout, message: {ex.Message}, url: {requestUri}");
                    }

                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    if (processorError != null) await processorError();
                    Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
                    cts.Cancel();
                }
            }
        }

        /// <summary>
        /// get with callback object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="processorSuccess"></param>
        /// <param name="processorError"></param>
        /// <param name="callbackObject"></param>
        /// <returns></returns>
        public async Task GetTaskAsyncCallback<T>(string requestUri, Func<T, object, Task> processorSuccess,
            Func<Task> processorError = null,
            object callbackObject = null) where T : class
        {
            Debug.WriteLine($"Get method with url: {requestUri}");
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var responseMessage = await GetAsync(requestUri, HttpCompletionOption.ResponseContentRead, cts.Token);
                    if (responseMessage == null)
                    {
                        if (processorError != null) await processorError();
                    }
                    else
                    {
                        responseMessage = responseMessage.EnsureSuccessStatusCode();
                        var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                        var result = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            await processorSuccess(result, callbackObject);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        if (processorError != null) await processorError();
                        var token = cts.Token;
                        Debug.WriteLine(
                            $"A real cancellation, triggered by the caller, token is: {token}, message: {ex.Message}, url: {requestUri}");
                    }
                    else
                    {
                        if (processorError != null) await processorError();
                        Debug.WriteLine($"A web request timeout, message: {ex.Message}, url: {requestUri}");
                    }

                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    if (processorError != null) await processorError();
                    Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
                    cts.Cancel();
                }
            }
        }

        #endregion

        #region post

        public async Task PostTaskAsync<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, Task> processorSuccess, Func<Task> processorError = null) where T : class
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var stringContent = KeyValuePairToStringContent(keyvalues);
                    var responseMessage = await PostAsync(requestUri, stringContent, cts.Token);
                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                    var result = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                    if (result == null)
                    {
                        if (processorError != null) await processorError();
                    }
                    else
                    {
                        if(processorSuccess != null) await processorSuccess(result);
                    }
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    Debug.WriteLine(
                        taskCanceledException.CancellationToken == cts.Token
                            ? $"A real cancellation, triggered by the caller, token is: {cts.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                            : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
            }
        }

        public async Task PostTaskAsync<T>(string requestUri, HttpContent content, Func<T, Task> processorSuccess,
            Func<Task> processorError = null) where T : class
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var responseMessage = await PostAsync(requestUri, content, cts.Token);
                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                    var result = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                    if (result == null)
                    {
                        if (processorError != null) await processorError();
                    }
                    else
                    {
                        if (processorSuccess != null) await processorSuccess(result);
                    }
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    Debug.WriteLine(
                        taskCanceledException.CancellationToken == cts.Token
                            ? $"A real cancellation, triggered by the caller, token is: {cts.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                            : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
            }
        }

        public async Task PostTaskAsyncCallback<T>(string requestUri,
            IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, object, Task> processorSuccess, Func<Task> processorError = null, object callbackObject = null)
            where T : class
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var stringContent = KeyValuePairToStringContent(keyvalues);
                    var responseMessage = await PostAsync(requestUri, stringContent, cts.Token);
                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                    var seObj = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);
                    if (seObj == null)
                    {
                        Debug.WriteLine($"================Can't parse json to object: {nameof(T)}");
                        if (processorError != null) await processorError();
                        cts.Cancel();
                    }
                    else
                    {
                        await processorSuccess(seObj, callbackObject);
                    }
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    Debug.WriteLine(
                        taskCanceledException.CancellationToken == cts.Token
                            ? $"A real cancellation, triggered by the caller, token is: {cts.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                            : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Your request will be terminal with error: {exception.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
            }
        }

        public async Task PostTaskAsyncCallback<T>(string requestUri, HttpContent content,
            Func<T, object, Task> processorSuccess,
            Func<Task> processorError = null, object callbackObject = null) where T : class
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var responseMessage = await PostAsync(requestUri, content, cts.Token);
                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                    var seObj = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);
                    if (seObj == null)
                    {
                        Debug.WriteLine($"================Can't parse json to object: {nameof(T)}");
                        if (processorError != null) await processorError();
                        cts.Cancel();
                    }
                    else
                    {
                        await processorSuccess(seObj, callbackObject);
                    }
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    Debug.WriteLine(
                        taskCanceledException.CancellationToken == cts.Token
                            ? $"A real cancellation, triggered by the caller, token is: {cts.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                            : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Your request will be terminal with error: {exception.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
            }
        }

        #endregion

        #region delete

        /// <summary>
        /// delele task
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="processorSuccess"></param>
        /// <param name="processorError"></param>
        /// <returns></returns>
        public async Task DeleteTaskAsync<T>(string requestUri, Func<T, Task> processorSuccess,
            Func<Task> processorError = null) where T : class
        {
            Debug.WriteLine($"Delete method with url: {requestUri}");
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var responseMessage = await DeleteAsync(requestUri, cts.Token);
                    if (responseMessage == null)
                    {
                        if (processorError != null) await processorError();
                    }
                    else
                    {
                        responseMessage = responseMessage.EnsureSuccessStatusCode();
                        var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                        var result = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            await processorSuccess(result);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        if (processorError != null) await processorError();
                        var token = cts.Token;
                        Debug.WriteLine(
                            $"A real cancellation, triggered by the caller, token is: {token}, message: {ex.Message}, url: {requestUri}");
                    }
                    else
                    {
                        if (processorError != null) await processorError();
                        Debug.WriteLine($"A web request timeout, message: {ex.Message}, url: {requestUri}");
                    }

                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    if (processorError != null) await processorError();
                    Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
                    cts.Cancel();
                }
            }
        }

        /// <summary>
        /// delete task with call back
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="processorSuccess"></param>
        /// <param name="processorError"></param>
        /// <param name="callbackObject"></param>
        /// <returns></returns>
        public async Task DeleteTaskAsyncCallback<T>(string requestUri, Func<T, object, Task> processorSuccess,
            Func<Task> processorError = null,
            object callbackObject = null) where T : class
        {
            Debug.WriteLine($"Delete method with url: {requestUri}");
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var responseMessage = await DeleteAsync(requestUri, cts.Token);
                    if (responseMessage == null)
                    {
                        if (processorError != null) await processorError();
                    }
                    else
                    {
                        responseMessage = responseMessage.EnsureSuccessStatusCode();
                        var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                        var result = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                        if (result == null)
                        {
                            if (processorError != null) await processorError();
                        }
                        else
                        {
                            await processorSuccess(result, callbackObject);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        if (processorError != null) await processorError();
                        var token = cts.Token;
                        Debug.WriteLine(
                            $"A real cancellation, triggered by the caller, token is: {token}, message: {ex.Message}, url: {requestUri}");
                    }
                    else
                    {
                        if (processorError != null) await processorError();
                        Debug.WriteLine($"A web request timeout, message: {ex.Message}, url: {requestUri}");
                    }

                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    if (processorError != null) await processorError();
                    Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
                    cts.Cancel();
                }
            }
        }

        #endregion

        #region put

        public async Task PutTaskAsync<T>(string requestUri, IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, Task> processorSuccess, Func<Task> processorError = null) where T : class
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var stringContent = KeyValuePairToStringContent(keyvalues);
                    var responseMessage = await PutAsync(requestUri, stringContent, cts.Token);
                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                    var result = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                    if (result == null)
                    {
                        if (processorError != null) await processorError();
                    }
                    else
                    {
                        await processorSuccess(result);
                    }
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    Debug.WriteLine(
                        taskCanceledException.CancellationToken == cts.Token
                            ? $"A real cancellation, triggered by the caller, token is: {cts.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                            : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
            }
        }

        public async Task PutTaskAsync<T>(string requestUri, HttpContent content, Func<T, Task> processorSuccess,
            Func<Task> processorError = null) where T : class
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var responseMessage = await PutAsync(requestUri, content, cts.Token);
                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                    var result = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);

                    if (result == null)
                    {
                        if (processorError != null) await processorError();
                    }
                    else
                    {
                        await processorSuccess(result);
                    }
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    Debug.WriteLine(
                        taskCanceledException.CancellationToken == cts.Token
                            ? $"A real cancellation, triggered by the caller, token is: {cts.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                            : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Server die with error: {exception.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
            }
        }

        public async Task PutTaskAsyncCallback<T>(string requestUri,
            IEnumerable<KeyValuePair<string, string>> keyvalues,
            Func<T, object, Task> processorSuccess, Func<Task> processorError = null, object callbackObject = null)
            where T : class
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var stringContent = KeyValuePairToStringContent(keyvalues);
                    var responseMessage = await PutAsync(requestUri, stringContent, cts.Token);
                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                    var seObj = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);
                    if (seObj == null)
                    {
                        Debug.WriteLine($"================Can't parse json to object: {nameof(T)}");
                        if (processorError != null) await processorError();
                        cts.Cancel();
                    }
                    else
                    {
                        await processorSuccess(seObj, callbackObject);
                    }
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    Debug.WriteLine(
                        taskCanceledException.CancellationToken == cts.Token
                            ? $"A real cancellation, triggered by the caller, token is: {cts.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                            : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Your request will be terminal with error: {exception.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
            }
        }

        public async Task PutTaskAsyncCallback<T>(string requestUri, HttpContent content,
            Func<T, object, Task> processorSuccess,
            Func<Task> processorError = null, object callbackObject = null) where T : class
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                if (processorError != null) await processorError();
            }
            else
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(AppTimeOut);
                try
                {
                    var responseMessage = await PutAsync(requestUri, content, cts.Token);
                    responseMessage = responseMessage.EnsureSuccessStatusCode();
                    var jsonStream = await responseMessage.Content.ReadAsStreamAsync();
                    var seObj = Instance.Serializer.DeserializeFromJsonStream<T>(jsonStream, requestUri);
                    if (seObj == null)
                    {
                        Debug.WriteLine($"================Can't parse json to object: {nameof(T)}");
                        if (processorError != null) await processorError();
                        cts.Cancel();
                    }
                    else
                    {
                        await processorSuccess(seObj, callbackObject);
                    }
                }
                catch (TaskCanceledException taskCanceledException)
                {
                    Debug.WriteLine(
                        taskCanceledException.CancellationToken == cts.Token
                            ? $"A real cancellation, triggered by the caller, token is: {cts.Token}, message: {taskCanceledException.Message}, url: {requestUri}"
                            : $"A web request timeout, message: {taskCanceledException.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"Your request will be terminal with error: {exception.Message}, url: {requestUri}");
                    if (processorError != null) await processorError();
                    cts.Cancel();
                }
            }
        }

        #endregion

        #endregion

        #region converter

        private static StringContent KeyValuePairToStringContent(IEnumerable<KeyValuePair<string, string>> inputEnumerator)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append('{');
            foreach (var current in inputEnumerator)
            {
                if (stringBuilder.Length > 1)
                {
                    stringBuilder.Append(',');
                }

                stringBuilder.Append("\"" + current.Key + "\"");
                stringBuilder.Append(':');
                stringBuilder.Append("\"" + current.Value + "\"");
            }
            stringBuilder.Append('}');

            var queryString = new StringContent(stringBuilder.ToString(), Encoding.UTF8, "application/json");
            return queryString;
        }

        private StringContent ObjectToStringContent<T>(T objectToSend, string url = null)
        {
            var json = JsonConvert.SerializeObject(objectToSend);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return content;
        }

        #endregion
    }
}