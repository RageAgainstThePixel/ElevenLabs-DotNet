// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;

namespace ElevenLabs.Tests
{
    internal class TestFixture_09_EnumExtensions
    {
        [Test]
        public void Test_01_ToEnumStringShouldMatchJsonStringEnumMemberNameAttribute()
        {
            static string GetEnumValueFromAttribute<T>(T enumValue) where T : Enum
            {
                var enumType = typeof(T);
                var memberInfo = enumType.GetMember(enumValue.ToString()).FirstOrDefault();
                if (memberInfo != null)
                {
                    var attribute = memberInfo.GetCustomAttributes(typeof(JsonStringEnumMemberNameAttribute), false).FirstOrDefault();
                    if (attribute != null)
                    {
                        return ((JsonStringEnumMemberNameAttribute)attribute).Name;
                    }
                }
                return null;
            }

            static void TestEnumSerialization<T>(T enumValue) where T : Enum
            {
                var valueFromAttribute = GetEnumValueFromAttribute(enumValue);
                Assert.NotNull(valueFromAttribute, $"Enum value {enumValue} does not have JsonStringEnumMemberName attribute.");

                var serialized = enumValue.ToEnumString();
                Console.WriteLine($"Serialized: {serialized}");
                Assert.IsNotEmpty(serialized);
                Assert.AreEqual(valueFromAttribute, serialized);
            }


            SortDirections[] sortDirections = Enum.GetValues<SortDirections>();
            foreach (var sortDirection in sortDirections)
            {
                TestEnumSerialization(sortDirection);
            }

            VoiceTypes[] voiceTypes = Enum.GetValues<VoiceTypes>();
            foreach (var voiceType in voiceTypes)
            {
                TestEnumSerialization(voiceType);
            }

            CategoryTypes[] categoryTypes = Enum.GetValues<CategoryTypes>();
            foreach (var categoryType in categoryTypes)
            {
                TestEnumSerialization(categoryType);
            }

            FineTuningStateTypes[] fineTuningStateTypes = Enum.GetValues<FineTuningStateTypes>();
            foreach (var fineTuningStateType in fineTuningStateTypes)
            {
                TestEnumSerialization(fineTuningStateType);
            }
        }
    }
    }
}