// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ElevenLabs.Extensions
{
    internal static class QueryParamsExtensions
    {
        public static Dictionary<string, string> ToQueryParams(this object @object)
        {
            var parameters = new Dictionary<string, string>();
            var json = JsonSerializer.Serialize(@object, ElevenLabsClient.JsonSerializationOptions);
            using var doc = JsonDocument.Parse(json);

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.Array:
                    {
                        // Flatten arrays as comma-separated values
                        var array = string.Join(",", property.Value.EnumerateArray().Select(e => e.GetString()));

                        if (!string.IsNullOrWhiteSpace(array))
                        {
                            parameters.Add(property.Name, array);
                        }

                        continue;
                    }
                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                    {
                        continue; // ignored
                    }
                    default:
                    {
                        var value = property.Value.ToString();

                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            parameters.Add(property.Name, value);
                        }

                        continue;
                    }
                }
            }

            return parameters;
        }
    }
}
