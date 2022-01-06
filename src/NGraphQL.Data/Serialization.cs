using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NGraphQL.Data {
  public static class Serialization {

    public static JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions() {
      DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
      PropertyNameCaseInsensitive = true,
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      MaxDepth = 50,
      IncludeFields = true,
      NumberHandling = JsonNumberHandling.AllowReadingFromString,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
      Converters = {
        new JsonStringEnumConverter()
      },
      AllowTrailingCommas = true,
      ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static async Task<T> DeserializeFromJsonStreamAsync<T>(this Stream stream, CancellationToken? cancellationToken = null) =>
      (T)await JsonSerializer.DeserializeAsync(stream, typeof(T), DefaultSerializerOptions, cancellationToken.HasValue ? cancellationToken.Value : CancellationToken.None);

    public static async Task<T> DeserializeFromJsonAsync<T>(this string json, CancellationToken? cancellationToken = null) {
      using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json))) {
        return await stream.DeserializeFromJsonStreamAsync<T>(cancellationToken);
      }
    }

    public static async Task SerializeToJsonStreamAsync<T>(this Stream stream, T data, CancellationToken? cancellationToken = null) =>
      await JsonSerializer.SerializeAsync(stream, data, typeof(T), DefaultSerializerOptions, cancellationToken.HasValue ? cancellationToken.Value : CancellationToken.None);

    public static async Task<string> SerializeToJsonAsync<T>(this T data, CancellationToken? cancellationToken = null) {
      using (var stream = new MemoryStream()) {
        await stream.SerializeToJsonStreamAsync<T>(data, cancellationToken);
        stream.Position = 0;
        return Encoding.UTF8.GetString(stream.ToArray());
      }
    }

    [Obsolete]
    public static string SerializeToJson<T>(this T data, CancellationToken? cancellationToken = null) {
      return JsonSerializer.Serialize(data, DefaultSerializerOptions);
    }

    public static T ToObject<T>(this JsonElement element) {
      var json = element.GetRawText();
      return JsonSerializer.Deserialize<T>(json);
    }
    public static T ToObject<T>(this JsonDocument document) {
      var json = document.RootElement.GetRawText();
      return JsonSerializer.Deserialize<T>(json);
    }
  }
}
