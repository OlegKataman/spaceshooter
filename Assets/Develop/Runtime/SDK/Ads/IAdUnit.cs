using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Develop.Runtime.SDK.Ads
{
    /// <summary>
    /// Общий интерфейс единицы рекламного формата.
    /// Каждый формат (interstitial, rewarded, banner) реализует его отдельно.
    /// </summary>
    public interface IAdUnit : IDisposable
    {
        bool IsReady { get; }

        /// <summary>Показывает рекламу и ожидает её закрытия.</summary>
        UniTask ShowAsync(CancellationToken cancellationToken);
    }
}
