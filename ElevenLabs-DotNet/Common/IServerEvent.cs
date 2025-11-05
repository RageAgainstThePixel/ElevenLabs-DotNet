// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace ElevenLabs
{
    public interface IWebsocketEvent
    {
        public string ToJsonString();
    }

    public interface IServerEvent : IWebsocketEvent
    {
    }

    public interface IClientEvent : IWebsocketEvent
    {
    }
}
