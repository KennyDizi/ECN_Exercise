using System.IO;

namespace ECN_Exercise.Sources.Utils
{
    public interface ISerializer
    {
        T Deserialize<T>(string data, string url = null);
        T DeserializeFromJsonStream<T>(Stream jsonStream, string url = null);

        string Serialize<T>(T obj);
    }
}