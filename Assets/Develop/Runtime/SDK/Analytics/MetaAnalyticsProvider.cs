using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Config;
using UnityEngine;

namespace Develop.Runtime.SDK.Analytics
{
    public sealed class MetaAnalyticsProvider : IAnalyticsProvider
    {
        public AnalyticsTarget Target => AnalyticsTarget.Meta;
        public bool IsInitialized { get; private set; }

        private readonly string _appId;

        public MetaAnalyticsProvider(SdkSettingsConfig settings)
        {
            _appId = settings.MetaAppId;
        }
        
        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.NextFrame();
                
                // AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);

                IsInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Meta] Initialization failed: {e}");
            }
        }
        
        public void LogEvent(string eventName, params AnalyticsParam[] parameters)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[Meta] Not initialized, skipping event");
                return;
            }

            try
            {
                Debug.Log($"[Meta] Event: {eventName}");
                
                Dictionary<string, object> data = null;

                if (parameters != null && parameters.Length > 0)
                {
                    data = new Dictionary<string, object>();

                    foreach (var p in parameters)
                    {
                        data[p.key] = p.ToString();
                        
                        Debug.Log($"  {p.key}: {p.ToString()}");
                    }
                }

                // FB.LogAppEvent(eventName, null, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Meta] LogEvent failed: {e}");
            }
        }
    }
}