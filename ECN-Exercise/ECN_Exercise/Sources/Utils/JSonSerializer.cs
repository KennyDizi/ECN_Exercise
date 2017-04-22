using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ECN_Exercise.Sources.Utils
{
    public class JSonSerializer : ISerializer
    {
        public JSonSerializer() { }

        public T Deserialize<T>(string data, string url = null)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(data, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Cannot deserialize url res: {url} with error: {exception.Message}");
                return default(T);
            }
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, new IsoDateTimeConverter
            {
                DateTimeFormat = @"yyyy-MM-dd\THH:mm:ss.FFFFFFF\Z",
                DateTimeStyles = System.Globalization.DateTimeStyles.AdjustToUniversal
            });
        }

        public T DeserializeFromJsonStream<T>(Stream jsonStream, string url = null)
        {
            try
            {
                if (jsonStream == null)
#if DEBUG
                    throw new ArgumentNullException(nameof(jsonStream));
#else
                    return default(T);
#endif
                if (jsonStream.CanRead == false)
#if DEBUG
                    throw new ArgumentException($"Json stream must be support read {nameof(jsonStream)}");
#else
                    return default(T);
#endif
                T deserializeObject;                

                using (var sr = new StreamReader(jsonStream))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
                        deserializeObject = serializer.Deserialize<T>(jsonTextReader);
                    }
                }
                return deserializeObject;
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Cannot deserialize url res: {url} with error: {exception.Message}");
                return default(T);
            }
        }
    }
}