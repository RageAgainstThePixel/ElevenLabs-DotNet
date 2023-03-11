// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json.Serialization;

namespace ElevenLabs.User
{
    public sealed class NextInvoice
    {
        [JsonInclude]
        [JsonPropertyName("amount_due_cents")]
        public double AmountDueCents { get; }

        [JsonInclude]
        [JsonPropertyName("next_payment_attempt_unix")]
        public int NextPaymentAttemptUnix { get; }

        [JsonIgnore]
        public DateTime NextPaymentAttempt => DateTimeOffset.FromUnixTimeSeconds(NextPaymentAttemptUnix).DateTime;
    }
}
