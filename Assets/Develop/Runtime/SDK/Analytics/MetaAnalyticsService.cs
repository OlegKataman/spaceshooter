using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Config;
using UnityEngine;

namespace Develop.Runtime.SDK.Analytics
{
    public sealed class MetaAnalyticsService : IAnalyticsService
    {
        public AnalyticsTarget Target => AnalyticsTarget.Meta;
        public bool IsInitialized { get; private set; }

        private readonly string _appId;

        public MetaAnalyticsService(SdkSettingsConfig settings)
        {
            _appId = settings.MetaAppId;
        }
        
        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: cancellationToken);
                
                // AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);

                IsInitialized = true;
                Debug.Log("[Meta] Initialized");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Meta] Initialization failed: {e}");
            }
        }
        
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[Meta] Not initialized, skipping event");
                return;
            }

            try
            {
                // var metaParams = new Dictionary<string, object>();
        
                // if (parameters != null)
                //     foreach (var p in parameters)
                //         metaParams[p.Key] = p.Value;
        
                // FB.LogAppEvent(eventName, parameters: metaParams);

                Debug.Log($"[Meta] Event: {eventName}");

                if (parameters == null) return;

                foreach (var p in parameters)
                    Debug.Log($"  {p.Key}: {p.Value}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Meta] LogEvent failed: {e}");
            }
        }
    }
}