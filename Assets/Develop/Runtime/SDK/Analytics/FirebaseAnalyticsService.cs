using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Config;
using UnityEngine;

namespace Develop.Runtime.SDK.Analytics
{
    public sealed class FirebaseAnalyticsService : IAnalyticsService
    {
        public AnalyticsTarget Target => AnalyticsTarget.Firebase;
        public bool IsInitialized { get; private set; }
        
        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: cancellationToken);
                
                /*
                var status = await FirebaseApp.CheckAndFixDependenciesAsync();

                if (status != DependencyStatus.Available)
                {
                    Debug.LogError($"[Firebase] Dependency check failed: {status}");
                    return;
                }

                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                FirebaseAnalytics.SetSessionTimeoutDuration(TimeSpan.FromMinutes(30));*/
                
                IsInitialized = true;
                Debug.Log("[Firebase] Initialized");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Firebase] Initialization failed: {e}");
            }
        }
        
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[Firebase] Not initialized, skipping event");
                return;
            }

            try
            {
                /*
                if (parameters == null)
                {
                    FirebaseAnalytics.LogEvent(eventName);
                }
                else
                {
                    // Firebase SDK очікує Parameter[] — конвертуємо словник
                    var firebaseParams = parameters
                        .Select(p => new Parameter(p.Key, p.Value.ToString()))
                        .ToArray();

                    FirebaseAnalytics.LogEvent(eventName, firebaseParams);
                }*/
            }
            catch (Exception e)
            {
                Debug.LogError($"[Firebase] LogEvent failed: {e}");
            }
        }
    }
}