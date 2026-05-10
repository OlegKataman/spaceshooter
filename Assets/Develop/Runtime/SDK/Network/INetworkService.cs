using System;

namespace Develop.Runtime.SDK.Network
{
    /// <summary>
    /// Предоставляет информацию о состоянии сети.
    /// </summary>
    public interface INetworkService
    {
        bool IsOnline { get; }

        /// <summary>Вызывается когда соединение появляется (offline → online).</summary>
        event Action OnConnected;
    }
}
