﻿// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System.Threading.Tasks;

namespace ElevenLabs.Tests
{
    internal class TestFixture_01_UserEndpoint : AbstractTestFixture
    {
        [Test]
        public async Task Test_01_GetUserInfo()
        {
            Assert.NotNull(ElevenLabsClient.UserEndpoint);
            var result = await ElevenLabsClient.UserEndpoint.GetUserInfoAsync();
            Assert.NotNull(result);
        }

        [Test]
        public async Task Test_02_GetSubscriptionInfo()
        {
            Assert.NotNull(ElevenLabsClient.UserEndpoint);
            var result = await ElevenLabsClient.UserEndpoint.GetSubscriptionInfoAsync();
            Assert.NotNull(result);
        }
    }
}