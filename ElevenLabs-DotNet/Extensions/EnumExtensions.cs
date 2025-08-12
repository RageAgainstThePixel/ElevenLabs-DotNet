// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Runtime.Serialization;

namespace ElevenLabs.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Serialize an enum according to the EnumMemberAttribute annotation, if available.
        /// 
        /// see https://stackoverflow.com/a/10418943
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="enumValue">The enum value to serialize</param>
        /// <returns></returns>
        public static string ToEnumString<T>(this T enumValue) where T : Enum
        {
            var enumType = typeof(T);
            var name = Enum.GetName(enumType, enumValue);
            var enumMemberAttribute = ((EnumMemberAttribute[])enumType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).SingleOrDefault();
            return enumMemberAttribute?.Value ?? enumValue.ToString();
        }
    }
}