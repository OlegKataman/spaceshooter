using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Ads;
using Develop.Runtime.SDK.Analytics;
using Develop.Runtime.SDK.Config;
using SpaceShooter.Runtime.Extensions;
using SpaceShooter.Runtime.Service;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SpaceShooter.Runtime.Core
{
    public sealed class Game : IAsyncStartable,  IInitializable
    {
        [Inject]
        private AnalyticsFacade _analytics;
        [Inject]
        private AdsFacade _ads;
        [Inject] 
        private AssetService _assetService;
        [Inject] 
        private UIService _uiService;

        private CancellationTokenSource _cancellationTokenSource = new();
        private bool _isGameOver;
        
        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellationToken)
        {
            await UniTask.WhenAll(
                _analytics.InitializeAsync(cancellationToken),
                _ads.InitializeAsync(cancellationToken)
            );
            
            Debug.Log("[System] Analytics and Ads services have been initialized.");
        }
        
        void IInitializable.Initialize()
        {
            DoLoad().Forget();
            return;
            
            async UniTask DoLoad()
            {
                var prefab = await _assetService.LoadAsync<GameObject>("Ship");
                var instance = Object.Instantiate(prefab, new Vector3(0, -1.85f, 0), Quaternion.identity);
                
                instance.InjectIntoSceneLifetime();
                
                _uiService.ShowGameHudFragment();
            }
        }

        public void GameOver()
        {
            if (_isGameOver)
                return;
            _isGameOver = true;
            
            Do().Forget();
            return;
            
            async UniTask Do()
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                
                _analytics.LogEvent(AnalyticsEvent.GameOver);

                await _ads.ShowGameOverAdAsync(_cancellationTokenSource.Token);
                
                _uiService.ShowGameOverFragment();
            }
        }
    }
}