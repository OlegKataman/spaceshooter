using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Config;
using UnityEngine;

namespace Develop.Runtime.SDK.Analytics
{
    public sealed class AdjustAnalyticsService : IAnalyticsService
    {
        public AnalyticsTarget Target => AnalyticsTarget.Adjust;
        public bool IsInitialized { get; private set; }
        
        private readonly string _appToken;

        public AdjustAnalyticsService(SdkSettingsConfig settings)
        {
            _appToken = settings.AdjustAppToken;
        }
        
        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: cancellationToken);
                
                /*
                var adjustConfig = new AdjustConfig(_appToken, AdjustEnvironment.Production);
                adjustConfig.logLevel = AdjustLogLevel.Suppress;
                adjustConfig.allowIdfaReading = false;
                
                Adjust.start(adjustConfig);*/

                IsInitialized = true;
                Debug.Log("[Adjust] Initialized");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Adjust] Initialization failed: {e}");
            }
        }
        
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[Adjust] Not initialized, skipping event");
                return;
            }

            try
            {
                /*
                var adjustEvent = new AdjustEvent(eventName);
        
                if (parameters != null)
                     foreach (var p in parameters)
                         adjustEvent.addCallbackParameter(p.Key, p.Value.ToString());
        
                 Adjust.trackEvent(adjustEvent);*/

                Debug.Log($"[Adjust] Event: {eventName}");

                if (parameters == null) return;

                foreach (var p in parameters)
                    Debug.Log($"  {p.Key}: {p.Value}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Adjust] LogEvent failed: {e}");
            }
        }
    }
}