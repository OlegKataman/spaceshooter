using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Develop.Runtime.SDK.Network
{
    /// <summary>
    /// Реализация INetworkService для Unity.
    /// Использует Application.internetReachability + периодическую проверку.
    /// </summary>
    public sealed class NetworkService : INetworkService, IDisposable
    {
        private bool _isOnline;
        private readonly CancellationTokenSource _cts;
        private readonly float _checkIntervalSeconds = 5f;
        private static readonly string PingTarget = "https://www.google.com";

        public bool IsOnline => _isOnline;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<bool> OnNetworkStateChanged;

        public NetworkService()
        {
            _cts = new CancellationTokenSource();
            
            StartNetworkMonitoring().Forget();
        }

        private async UniTaskVoid StartNetworkMonitoring()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var currentOnline = await GetCurrentReachability();

                if (currentOnline != _isOnline)
                {
                    var wasOnline = _isOnline;
                    _isOnline = currentOnline;

                    OnNetworkStateChanged?.Invoke(_isOnline);

                    switch (_isOnline)
                    {
                        case true when !wasOnline: 
                            OnConnected?.Invoke();
                            break;
                        case false when wasOnline:
                            OnDisconnected?.Invoke();
                            break;
                    }
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_checkIntervalSeconds), 
                                   cancellationToken: _cts.Token)
                             .SuppressCancellationThrow();
            }
        }

        private static async UniTask<bool> GetCurrentReachability()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return false;

            try
            {
                using var request = UnityWebRequest.Head(PingTarget);
                request.timeout = 3;
                await request.SendWebRequest();
                return request.result == UnityWebRequest.Result.Success;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            
            OnConnected = null;
            OnDisconnected = null;
            OnNetworkStateChanged = null;
        }
    }
}