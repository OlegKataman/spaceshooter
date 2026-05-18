using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Develop.Runtime.SDK.Ads
{
    public sealed class BannerAdUnit : IBannerAdUnit
    {
        private BannerView _banner;
        private readonly string _adUnitId;
        private readonly AdPosition _position;
        private readonly AdSize _adSize;
        
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public bool IsReady => _banner != null;

        public BannerAdUnit(string adUnitId,
            AdPosition position = AdPosition.Bottom,
            AdSize adSize = null)
        {
            _adUnitId = adUnitId;
            _position = position;
            _adSize = adSize ?? AdSize.Banner;
        }

        private async UniTask LoadAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;

            var tcs = new UniTaskCompletionSource<BannerView>();

            var bannerView = new BannerView(_adUnitId, _adSize, _position);

            bannerView.OnBannerAdLoaded += () =>
            {
                bannerView.Hide();
                tcs.TrySetResult(bannerView);
                Debug.Log("[Banner] Loaded");
            };

            bannerView.OnBannerAdLoadFailed += error =>
            {
                bannerView.Destroy();
                tcs.TrySetResult(null);
                Debug.Log($"[Banner] Load failed: {error.GetMessage()}");
            };

            bannerView.LoadAd(new AdRequest());

            var result = await tcs.Task;
            if (result != null)
                _banner = result;
        }

        public async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            if (_cancellationTokenSource.IsCancellationRequested) return;

            if (_banner == null)
            {
                await LoadAsync(cancellationToken);
            }

            _banner?.Show();
        }

        public void Hide() => _banner?.Hide();

        public void Destroy()
        {
            _banner?.Destroy();
            _banner = null;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            
            _banner?.Destroy();
            _banner = null;
        }
    }
}