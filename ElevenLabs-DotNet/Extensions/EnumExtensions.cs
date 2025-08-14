// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json;

namespace ElevenLabs.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Serialize an enum according to the JsonStringEnumMemberName annotation, if available.
        /// 
        /// see https://stackoverflow.com/a/10418943
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="enumValue">The enum value to serialize</param>
        /// <returns>
        /// The value of the <see cref="JsonStringEnumMemberName"/> if present; otherwise, the result of <c>enumValue.ToString()</c>.
        /// </returns>
        public static string ToEnumString<T>(this T enumValue) where T : Enum
        {
            var jsonString = JsonSerializer.Serialize(enumValue, ElevenLabsClient.JsonSerializationOptions);
            return jsonString.Trim('"'); // Trim the quotes added by JsonSerializer
        }
    }
}