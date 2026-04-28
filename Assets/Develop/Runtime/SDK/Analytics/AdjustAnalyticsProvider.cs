using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Config;
using UnityEngine;

namespace Develop.Runtime.SDK.Analytics
{
    public sealed class AdjustAnalyticsProvider : IAnalyticsProvider
    {
        public AnalyticsTarget Target => AnalyticsTarget.Adjust;
        public bool IsInitialized { get; private set; }
        
        private readonly string _appToken;

        public AdjustAnalyticsProvider(SdkSettingsConfig settings)
        {
            _appToken = settings.AdjustAppToken;
        }
        
        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.NextFrame();
                
                /*
                var config = new AdjustConfig(_appToken, AdjustEnvironment.Production);
                config.logLevel = AdjustLogLevel.Suppress;
                config.allowIdfaReading = false;
                
                Adjust.start(config);*/

                IsInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Adjust] Initialization failed: {e}");
            }
        }
        
        public void LogEvent(string eventName, params AnalyticsParam[] parameters)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[Adjust] Not initialized, skipping event");
                return;
            }

            try
            {
                Debug.Log($"[Adjust] Event: {eventName}");
                
                /*
                var ev = new AdjustEvent(eventName);

                if (parameters != null)
                {
                    foreach (var p in parameters)
                        ev.addCallbackParameter(p.Key, p.Value?.ToString());
                }

                Adjust.trackEvent(ev);*/

                if (parameters == null) return;

                foreach (var p in parameters)
                    Debug.Log($"  {p.key}: {p.ToString()}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Adjust] LogEvent failed: {e}");
            }
        }
    }
}