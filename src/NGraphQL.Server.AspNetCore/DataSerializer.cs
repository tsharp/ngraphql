using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NGraphQL.Server.AspNetCore {
  internal static class DataSerializer {

    public static readonly JsonSerializerOptions options = new JsonSerializerOptions() {
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

    public static async Task<T> DeserializeFromJsonStreamAsync<T>(this Stream stream, CancellationToken cancellationToken) =>
      (T)await JsonSerializer.DeserializeAsync(stream, typeof(T), options, cancellationToken);

    public static async Task SerializeToJsonStreamAsync<T>(this Stream stream, T data, CancellationToken cancellationToken) =>
      await JsonSerializer.SerializeAsync(stream, data, typeof(T), options, cancellationToken);
  }
}
