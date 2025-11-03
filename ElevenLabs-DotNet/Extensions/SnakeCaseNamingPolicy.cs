// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;

namespace ElevenLabs.Extensions
{
    internal sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
            => StringExtensions.ToSnakeCase(name);
    }
}
