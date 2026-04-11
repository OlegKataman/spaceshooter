using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Config;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Develop.Runtime.SDK.Ads
{
    public sealed class AdMobAdsService : IAdsService, IDisposable
    {
        public bool IsInitialized { get; private set; }
        public bool IsInterstitialReady => _interstitialAd != null && _interstitialAd.CanShowAd();

        private readonly string _interstitialId;
        private InterstitialAd _interstitialAd;

        public AdMobAdsService(SdkSettingsConfig settings)
        {
            _interstitialId = settings.AdMobInterstitialId;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            var initSource = new UniTaskCompletionSource();
            
            MobileAds.Initialize(status =>
            {
                LogAdapterStatuses(status);
                initSource.TrySetResult();
            });

            await initSource.Task;

            IsInitialized = true;
            Debug.Log("[AdMob Mediation] Initialized");

            await LoadInterstitialAsync(cancellationToken);
        }
        
        public async UniTask ShowInterstitialAsync(CancellationToken cancellationToken)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[AdMob Mediation] Not initialized");
                return;
            }

            await UniTask.WaitUntil(() => IsInterstitialReady, cancellationToken: cancellationToken)
                .TimeoutWithoutException(TimeSpan.FromSeconds(10));

            if (!IsInterstitialReady)
            {
                Debug.LogWarning("[AdMob Mediation] Interstitial not ready");
                return;
            }
            
            var ad = Interlocked.Exchange(ref _interstitialAd, null);
            if (ad == null || !ad.CanShowAd())
            {
                Debug.LogWarning("[AdMob Mediation] Interstitial not ready");
                return;
            }

            var tcs = new UniTaskCompletionSource();
            
            void Cleanup()
            {
                ad.OnAdFullScreenContentClosed -= OnClosed;
                ad.OnAdFullScreenContentFailed -= OnFailed;
                ad.OnAdFullScreenContentOpened -= OnOpened;
            }
            
            void OnOpened()
            {
                Debug.Log("[AdMob Mediation] Interstitial opened");
            }

            void OnClosed()
            {
                Cleanup();
                tcs.TrySetResult();
            }

            void OnFailed(AdError error)
            {
                Debug.LogWarning($"[AdMob Mediation] Show failed: {error.GetMessage()}");
                Cleanup();
                tcs.TrySetResult();
            }
            
            ad.OnAdFullScreenContentClosed += OnClosed;
            ad.OnAdFullScreenContentFailed += OnFailed;
            ad.OnAdFullScreenContentOpened += OnOpened;

            try
            {
                ad.Show();
            }
            catch (Exception e)
            {
                Debug.LogError($"[AdMob Mediation] Show exception: {e}");
                Cleanup();
                ad.Destroy();
                LoadInterstitialAsync(cancellationToken).Forget();
                return;
            }

#if UNITY_EDITOR
            var editorClose = UniTask.WaitUntil(
                () => UnityEngine.Input.GetKeyDown(KeyCode.Escape),
                cancellationToken: cancellationToken
            );
            await UniTask.WhenAny(tcs.Task.AttachExternalCancellation(cancellationToken), editorClose);
#else
            await tcs.Task.AttachExternalCancellation(cancellationToken);
#endif
            ad.Destroy();
            LoadInterstitialAsync(cancellationToken).Forget();
            
            await UniTask.SwitchToMainThread();
        }

        private async UniTask LoadInterstitialAsync(CancellationToken cancellationToken)
        {
            var delay = TimeSpan.FromSeconds(5);
    
            while (!cancellationToken.IsCancellationRequested)
            {
                var loadSource = new UniTaskCompletionSource<bool>();

                InterstitialAd.Load(_interstitialId, new AdRequest(), (ad, error) =>
                {
                    if (error != null)
                    {
                        Debug.LogWarning($"[AdMob Mediation] Load failed: {error.GetMessage()}");
                        loadSource.TrySetResult(false);
                        return;
                    }

                    _interstitialAd?.Destroy();
                    _interstitialAd = ad;

                    Debug.Log($"[AdMob Mediation] Interstitial loaded " +
                              $"via {ad.GetResponseInfo().GetLoadedAdapterResponseInfo().AdSourceName}");

                    loadSource.TrySetResult(true);
                });

                var success = await loadSource.Task.AttachExternalCancellation(cancellationToken);
                if (success) return;
        
                Debug.Log($"[AdMob Mediation] Retrying in {delay.TotalSeconds}s...");
                await UniTask.Delay(delay, cancellationToken: cancellationToken);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
            }
        }
        
        private void LogAdapterStatuses(InitializationStatus status)
        {
            foreach (var adapter in status.getAdapterStatusMap())
            {
                var state = adapter.Value.InitializationState;
                Debug.Log($"[AdMob Mediation] Adapter {adapter.Key}: {state} " +
                          $"— {adapter.Value.Description}");
            }
        }

        void IDisposable.Dispose()
        {
            _interstitialAd?.Destroy();
        }
    }
}