using System.Threading;
using Cysharp.Threading.Tasks;

namespace Develop.Runtime.SDK.Ads
{
    /// <summary>
    /// Провайдер рекламы. Отвечает за инициализацию SDK
    /// и предоставление доступа к рекламным юнитам.
    /// </summary>
    public interface IAdsProvider
    {
        bool IsInitialized { get; }

        IAdUnit         Interstitial { get; }
        IRewardedAdUnit RewardedHammer     { get; }
        IRewardedAdUnit RewardedTimeFreeze { get; }
        IBannerAdUnit Banner     { get; }

        /// <summary>
        /// Инициализирует AdMob SDK.
        /// При офлайн-старте ждёт появления сети автоматически.
        /// Безопасно вызывать повторно.
        /// </summary>
        UniTask InitializeAsync(CancellationToken cancellationToken);
    }
}
