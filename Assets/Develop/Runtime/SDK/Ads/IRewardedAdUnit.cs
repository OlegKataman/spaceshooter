using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;

namespace Develop.Runtime.SDK.Ads
{
    /// <summary>
    /// Интерфейс rewarded-рекламы.
    /// ShowAsync возвращает Reward если игрок досмотрел ролик до конца,
    /// либо null если закрыл раньше, показ упал или был отменён.
    /// </summary>
    public interface IRewardedAdUnit : IAdUnit
    {
        new UniTask<Reward> ShowAsync(CancellationToken cancellationToken);
        
        event Action<Reward> OnRewarded;
    }
}
