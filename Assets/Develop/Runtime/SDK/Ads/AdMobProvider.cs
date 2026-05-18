using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Config;
using Develop.Runtime.SDK.Network;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace Develop.Runtime.SDK.Ads
{
    /// <summary>
    /// Инициализирует AdMob SDK и предоставляет доступ к рекламным юнитам.
    ///
    /// При офлайн-старте подписывается на INetworkService.OnConnected
    /// и автоматически инициализируется при появлении сети.
    /// MobileAds.Initialize вызывается ровно один раз — ограничение SDK.
    /// </summary>
    public sealed class AdMobProvider : IAdsProvider, IDisposable
    {
        // ── Публичное состояние ───────────────────────────────────────────────

        public bool IsInitialized { get; private set; }

        public IAdUnit         Interstitial => _interstitial;
        public IRewardedAdUnit RewardedHammer     => _rewardedHammer;
        public IRewardedAdUnit RewardedTimeFreeze => _rewardedTimeFreeze;
        public IBannerAdUnit Banner => _banner;

        // ── Приватные поля ────────────────────────────────────────────────────

        private readonly InterstitialAdUnit _interstitial;
        private readonly RewardedAdUnit _rewardedHammer;
        private readonly RewardedAdUnit _rewardedTimeFreeze;
        private readonly BannerAdUnit      _banner;
        private readonly INetworkService    _network;

        // 0 = Initialize ещё не вызывался, 1 = вызван (SDK — singleton, повтор запрещён)
        private int _sdkInitSentFlag;

        // 0 = живой, 1 = disposed
        private int _isDisposed;

        private readonly CancellationTokenSource _disposeCts = new();
        private readonly UniTaskCompletionSource _sdkInitTcs = new();

        // ─────────────────────────────────────────────────────────────────────

        public AdMobProvider(SdkSettingsConfig settings)
        {
            _network = new NetworkService();
            _interstitial = new InterstitialAdUnit(settings.AdMobInterstitialId);
            _rewardedHammer    = new RewardedAdUnit(settings.AdMobRewardedHammerId);
            _rewardedTimeFreeze = new RewardedAdUnit(settings.AdMobRewardedTimeFreezeId);
            _banner = new BannerAdUnit(settings.AdMobBannerId);
        }

        // ── Инициализация ─────────────────────────────────────────────────────

        private async UniTask RequestConsentAsync(CancellationToken token)
        {
            var tcs = new UniTaskCompletionSource();
            await using var reg = token.Register(() => tcs.TrySetCanceled(token));

            var requestParams = new ConsentRequestParameters
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings = new ConsentDebugSettings
                {
                    DebugGeography = DebugGeography.EEA, // эмулируем EU в редакторе
                }
#endif
            };

            ConsentInformation.Update(requestParams, error =>
            {
                if (error != null)
                {
                    Debug.LogWarning($"[AdMobProvider] Consent update error: {error.Message}");
                    tcs.TrySetResult(); // не блокируем инициализацию при ошибке
                    return;
                }

                if (ConsentInformation.IsConsentFormAvailable())
                {
                    ConsentForm.Load((form, loadError) =>
                    {
                        if (loadError != null)
                        {
                            Debug.LogWarning($"[AdMobProvider] Consent form load error: {loadError.Message}");
                            tcs.TrySetResult();
                            return;
                        }

                        if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
                        {
                            form.Show(showError =>
                            {
                                if (showError != null)
                                    Debug.LogWarning($"[AdMobProvider] Consent show error: {showError.Message}");
                                tcs.TrySetResult();
                            });
                        }
                        else
                        {
                            tcs.TrySetResult();
                        }
                    });
                }
                else
                {
                    tcs.TrySetResult();
                }
            });

            await tcs.Task;
        }
        
        /// <summary>
        /// Инициализирует AdMob SDK.
        ///
        /// Сценарии:
        ///   • Онлайн-старт  — инициализируется сразу.
        ///   • Офлайн-старт  — ждёт OnConnected, затем инициализируется.
        ///                     Повторные вызовы до появления сети безопасны.
        ///   • Уже готов     — no-op.
        /// </summary>
        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (IsInitialized || IsDisposed) return;

            await UniTask.SwitchToMainThread(cancellationToken);

            using var linkedCts = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, _disposeCts.Token);

            await WaitForNetworkAsync(linkedCts.Token);

            await InitializeSdkAsync(linkedCts.Token);

            IsInitialized = true;
            Debug.Log("[AdMobProvider] Initialized");

            _interstitial.StartPreload();
            _rewardedHammer.StartPreload();
            _rewardedTimeFreeze.StartPreload();
        }

        // ── Ожидание сети ─────────────────────────────────────────────────────

        private async UniTask WaitForNetworkAsync(CancellationToken token)
        {
            if (_network.IsOnline) return;

            Debug.Log("[AdMobProvider] Offline — waiting for network...");

            var tcs = new UniTaskCompletionSource();

            void OnConnected() => tcs.TrySetResult();

            await using var reg = token.Register(() =>
            {
                _network.OnConnected -= OnConnected;
                tcs.TrySetCanceled(token);
            });

            _network.OnConnected += OnConnected;

            // Повторная проверка после подписки: сеть могла появиться
            // между первым IsOnline и подпиской на событие (race condition)
            if (_network.IsOnline)
            {
                _network.OnConnected -= OnConnected;
                return;
            }

            await tcs.Task;
            _network.OnConnected -= OnConnected;

            Debug.Log("[AdMobProvider] Network restored, proceeding");
        }

        // ── Инициализация SDK ─────────────────────────────────────────────────

        private async UniTask InitializeSdkAsync(CancellationToken token)
        {
            if (Interlocked.CompareExchange(ref _sdkInitSentFlag, 1, 0) == 0)
            {
                try
                {
                    await using var reg = token.Register(
                        () => _sdkInitTcs.TrySetCanceled(token));
                    
                    await RequestConsentAsync(token);
                    
                    await UniTask.SwitchToMainThread(token);

                    MobileAds.Initialize(status =>
                    {
                        LogAdapterStatuses(status);
                        _sdkInitTcs.TrySetResult();
                    });

                    await _sdkInitTcs.Task;
                }
                catch (Exception ex)
                {
                    _sdkInitTcs.TrySetException(ex);
                    throw;
                }
            }
            else
            {
                await _sdkInitTcs.Task.AttachExternalCancellation(token);
            }
        }

        // ── Вспомогательные методы ────────────────────────────────────────────

        private bool IsDisposed => Volatile.Read(ref _isDisposed) == 1;

        private static void LogAdapterStatuses(InitializationStatus status)
        {
            var map = status.getAdapterStatusMap();
            if (map == null) return;

            foreach (var adapter in map)
            {
                Debug.Log(
                    $"[AdMobProvider] Adapter {adapter.Key}: " +
                    $"{adapter.Value.InitializationState} — {adapter.Value.Description}");
            }
        }

        // ── Dispose ───────────────────────────────────────────────────────────

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 1) return;

            _disposeCts.Cancel();
            _disposeCts.Dispose();

            _interstitial.Dispose();
            _rewardedHammer.Dispose();
            _rewardedTimeFreeze.Dispose();
            _banner.Dispose();
        }
    }
}
