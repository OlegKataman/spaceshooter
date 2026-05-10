using System.Threading;
using Cysharp.Threading.Tasks;

namespace Develop.Runtime.SDK.Ads
{
    public interface IBannerAdUnit : IAdUnit
    {
        new UniTask ShowAsync(CancellationToken cancellationToken = default);
        void Hide();
    }
}