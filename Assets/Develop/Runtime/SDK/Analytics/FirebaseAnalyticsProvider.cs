using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Config;
using Firebase;
using Firebase.Analytics;
using UnityEngine;

namespace Develop.Runtime.SDK.Analytics
{
    public sealed class FirebaseAnalyticsProvider : IAnalyticsProvider
    {
        public AnalyticsTarget Target => AnalyticsTarget.Firebase;
        public bool IsInitialized { get; private set; }
        
        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.NextFrame();
                
                var status = await FirebaseApp.CheckAndFixDependenciesAsync();

                if (status != DependencyStatus.Available)
                {
                    Debug.LogWarning($"[Firebase] Dependency status: {status}");
                    return;
                }

                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                FirebaseAnalytics.SetSessionTimeoutDuration(TimeSpan.FromMinutes(30));
                
                IsInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Firebase] Initialization failed: {e}");
            }
        }
        
        public void LogEvent(string eventName, params AnalyticsParam[] parameters)
        {
            if (!IsInitialized)
                return;
            
            Debug.Log($"[Firebase] Event: {eventName}");

            if (parameters == null || parameters.Length == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var data = parameters.Select(p => p.type switch
            {
                ParamType.Long   => new Parameter(p.key, p.AsLong()),
                ParamType.Double => new Parameter(p.key, p.AsDouble()),
                _                => new Parameter(p.key, p.AsString())
            }).ToArray();
            
            foreach (var p in parameters)
                Debug.Log($"  {p.key}: {p.ToString()}");

            FirebaseAnalytics.LogEvent(eventName, data);
        }
    }
}