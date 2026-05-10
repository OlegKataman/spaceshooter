using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Develop.Runtime.SDK.Ads
{
    /// <summary>
    /// Управляет жизненным циклом interstitial-объявления AdMob.
    /// Логика ретраев и предзагрузки наследуется из BaseAdUnit.
    /// </summary>
    public sealed class InterstitialAdUnit : BaseAdUnit<InterstitialAd>
    {
        public InterstitialAdUnit(string adUnitId) : base(adUnitId) { }

        // ── Загрузка ──────────────────────────────────────────────────────────

        protected override async UniTask<(InterstitialAd ad, string errorMessage)> LoadAdAsync(
            CancellationToken token)
        {
            var tcs = new UniTaskCompletionSource<(InterstitialAd, string)>();

            await using var reg = token.Register(() => tcs.TrySetCanceled(token));

            InterstitialAd.Load(AdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    tcs.TrySetResult((null, error.GetMessage()));
                    return;
                }

                LogAdSource(ad);
                tcs.TrySetResult((ad, null));
            });

            return await tcs.Task;
        }

        protected override bool CanShow(InterstitialAd ad) => ad.CanShowAd();

        protected override void DestroyAd(InterstitialAd ad) => ad.Destroy();

        // ── Показ ─────────────────────────────────────────────────────────────

        protected override async UniTask ShowAdAndWaitAsync(
            InterstitialAd ad, CancellationToken token)
        {
            var showTcs = new UniTaskCompletionSource();

            void CleanupAndDestroy()
            {
                ad.OnAdFullScreenContentClosed -= OnClosed;
                ad.OnAdFullScreenContentFailed -= OnFailed;
                ad.OnAdFullScreenContentOpened -= OnOpened;
                ad.Destroy();
            }

            void OnOpened() => Debug.Log("[Interstitial] Opened");
            void OnClosed() { CleanupAndDestroy(); showTcs.TrySetResult(); }
            void OnFailed(AdError error)
            {
                Debug.LogWarning($"[Interstitial] Show failed: {error.GetMessage()}");
                CleanupAndDestroy();
                showTcs.TrySetResult();
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
                Debug.LogError($"[Interstitial] Show exception: {e}");
                CleanupAndDestroy();
                showTcs.TrySetResult();
                return;
            }

#if UNITY_EDITOR
            await WaitForEditorCloseAsync(showTcs);
#else
            var wasCancelled = await showTcs.Task
                .AttachExternalCancellation(token)
                .SuppressCancellationThrow();

            if (wasCancelled)
                Debug.Log("[Interstitial] Show wait cancelled");
#endif
        }

#if UNITY_EDITOR
        private static async UniTask WaitForEditorCloseAsync(UniTaskCompletionSource showTcs)
        {
            var escapeTcs = new UniTaskCompletionSource();

            UniTask.Void(async () =>
            {
                while (!escapeTcs.Task.Status.IsCompleted())
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        escapeTcs.TrySetResult();
                        break;
                    }
                }
            });

            await UniTask.WhenAny(showTcs.Task, escapeTcs.Task);
            escapeTcs.TrySetResult();
        }
#endif

        // ── Вспомогательные методы ────────────────────────────────────────────

        private static void LogAdSource(InterstitialAd ad)
        {
            try
            {
                var info = ad.GetResponseInfo()?.GetLoadedAdapterResponseInfo();
                Debug.Log($"[Interstitial] Loaded via {info?.AdSourceName ?? "unknown"}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Interstitial] Failed to log ad source: {e.Message}");
            }
        }
    }
}
