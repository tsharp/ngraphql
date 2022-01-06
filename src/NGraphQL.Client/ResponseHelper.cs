using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NGraphQL.Data;
using NGraphQL.Utilities;

namespace NGraphQL.Client {

  public static class ResponseHelper {

    public static void EnsureNoErrors(this ServerResponse response) {
      if (response.Errors == null || response.Errors.Count == 0)
        return;
      var errText = response.GetErrorsAsText();
      var msg = "Request failed.";
      if (!string.IsNullOrWhiteSpace(errText))
        msg += " Error(s):" + Environment.NewLine + errText;
      throw new Exception(msg);
    }

    public static string GetErrorsAsText(this ServerResponse response) {
      if (response.Errors == null || response.Errors.Count == 0)
        return string.Empty;
      var text = string.Join(Environment.NewLine, response.Errors);
      return text;
    }

    internal static JsonElement? GetDataJElement(this ServerResponse response) {
      // read 'data' object as JObject 
      if (!response.TopFields.TryGetValue("data", out var data)) {
        return null;
      }

      return data;
    }

    public static T GetTopField<T>(this ServerResponse response, string name) {
      var dataJElement = response.GetDataJElement();

      if (!dataJElement.HasValue) {
        throw new Exception("'data' element was not returned by the request. See errors in response.");
      }

      if (!dataJElement.Value.TryGetProperty(name, out var jElement)) {
        throw new Exception($"Field '{name}' not found in response.");
      }

      var type = typeof(T);
      var nullable = ReflectionHelper.CheckNullable(ref type);

      if (jElement.ValueKind == JsonValueKind.Null) {
        if (!nullable) {
          throw new Exception($"Field '{name}': cannot convert null value to type {typeof(T)}.");
        }

        return (T)(object)null;
      }

      return jElement.ToObject<T>();
    }

    public static TEnum ToEnum<TEnum>(object value) {
      throw new NotImplementedException();
    //  var enumType = typeof(TEnum);
    //  if (!enumType.IsEnum)
    //    throw new Exception($"Invalid type argument '{enumType}', expected enum.");
    //  var handler = KnownEnumTypes.GetEnumHandler(enumType);
    //  if (handler.IsFlagSet) {
    //    if (!(value is IList<string> stringList))
    //      stringList = ((IList)value).OfType<string>().ToList();
    //    return (TEnum)handler.ConvertStringListToFlagsEnumValue(stringList);
    //  } else
    //    return (TEnum)handler.ConvertStringToEnumValue((string)value);
    }
  }
}
