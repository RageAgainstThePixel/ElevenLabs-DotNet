﻿// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ElevenLabs.Proxy
{
    /// <inheritdoc />
    public abstract class AbstractAuthenticationFilter : IAuthenticationFilter
    {
        /// <inheritdoc />
        public virtual void ValidateAuthentication(IHeaderDictionary request) { }

        /// <inheritdoc />
        public abstract Task ValidateAuthenticationAsync(IHeaderDictionary request);
    }
}
