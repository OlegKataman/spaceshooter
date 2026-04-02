using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Develop.Runtime.SDK.Ads
{
    public sealed class AdsFacade
    {
        [Inject]
        private IAdsService _service;

        public UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            return _service.InitializeAsync(cancellationToken);
        }

        public UniTask ShowGameOverAdAsync(CancellationToken cancellationToken)
        {
            return _service.ShowInterstitialAsync(cancellationToken);
        }
    }
}