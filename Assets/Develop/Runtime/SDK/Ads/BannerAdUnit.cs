using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Develop.Runtime.SDK.Ads
{
    /// <summary>
    /// Управляет баннерной рекламой AdMob.
    /// </summary>
    public sealed class BannerAdUnit : BaseAdUnit<BannerView>, IBannerAdUnit
    {
        private BannerView _activeBanner;
        private readonly AdPosition _position;
        private readonly AdSize _adSize;

        public BannerAdUnit(string adUnitId, 
                           AdPosition position = AdPosition.Bottom, 
                           AdSize adSize = null) 
            : base(adUnitId)
        {
            _position = position;
            _adSize = adSize ?? AdSize.Banner;
        }

        // ── Загрузка ──────────────────────────────────────────────────────────
        protected override async UniTask<(BannerView ad, string errorMessage)> LoadAdAsync(CancellationToken token)
        {
            var tcs = new UniTaskCompletionSource<(BannerView, string)>();
            await using var reg = token.Register(() => tcs.TrySetCanceled(token));

            var bannerView = new BannerView(AdUnitId, _adSize, _position);

            bannerView.OnBannerAdLoaded += () =>
            {
                LogAdSource(bannerView);
                tcs.TrySetResult((bannerView, null));
            };

            bannerView.OnBannerAdLoadFailed += (error) =>
            {
                DestroyAd(bannerView); // уничтожаем при ошибке загрузки
                tcs.TrySetResult((null, error.GetMessage()));
            };

            bannerView.LoadAd(new AdRequest());

            return await tcs.Task;
        }

        protected override bool CanShow(BannerView ad) => ad != null;

        protected override void DestroyAd(BannerView ad) => ad?.Destroy();

        // ── Публичные методы для баннера ─────────────────────────────────────
        public new async UniTask ShowAsync(CancellationToken cancellationToken = default)
        {
            if (IsDisposed) return;

            await UniTask.SwitchToMainThread(cancellationToken);

            var banner = await TakeAdForShowAsync(cancellationToken);
            if (banner == null) return;
            
            _activeBanner = banner;
            banner.Show();

            // После показа сразу предзагружаем следующий
            StartPreload();
        }

        /// <summary>
        /// Скрывает баннер (не уничтожает его)
        /// </summary>
        public void Hide() => _activeBanner?.Hide();

        // ── Не используется для баннера ───────────────────────────────────────
        protected override UniTask ShowAdAndWaitAsync(BannerView ad, CancellationToken token)
        {
            return UniTask.CompletedTask;
        }

        // ── Вспомогательные ───────────────────────────────────────────────────
        private static void LogAdSource(BannerView banner)
        {
            try
            {
                var info = banner.GetResponseInfo()?.GetLoadedAdapterResponseInfo();
                Debug.Log($"[Banner] Loaded via {info?.AdSourceName ?? "unknown"}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Banner] Failed to log ad source: {e.Message}");
            }
        }
    }
}