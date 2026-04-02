using System.Threading;
using Cysharp.Threading.Tasks;

namespace Develop.Runtime.SDK.Ads
{
    public interface IAdsService
    {
        bool IsInitialized { get; }
        bool IsInterstitialReady { get; }

        UniTask InitializeAsync(CancellationToken cancellationToken);
        UniTask ShowInterstitialAsync(CancellationToken cancellationToken);
    }
}