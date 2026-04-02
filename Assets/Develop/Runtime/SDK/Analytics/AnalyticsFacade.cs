using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Config;
using UnityEngine;

namespace Develop.Runtime.SDK.Analytics
{
    public sealed class AnalyticsFacade
    {
        private readonly Dictionary<AnalyticsTarget, IAnalyticsService> _services;
        private readonly Dictionary<AnalyticsEvent, EventBinding> _bindings;

        public AnalyticsFacade(IEnumerable<IAnalyticsService> services, AnalyticsEventTemplate template)
        {
            _services = services.ToDictionary(s => s.Target);
            _bindings = template.Events.ToDictionary(e => e.EventType);
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            await UniTask.WhenAll(
                _services.Values.Select(s => s.InitializeAsync(cancellationToken))
            );

            Debug.Log("[Analytics] All services initialized");
        }
        
        public void LogEvent(AnalyticsEvent eventType, Dictionary<string, object> parameters = null)
        {
            if (!_bindings.TryGetValue(eventType, out var binding))
            {
                Debug.LogWarning($"[Analytics] No binding found for event: {eventType}");
                return;
            }

            foreach (var target in binding.Targets)
            {
                if (_services.TryGetValue(target, out var service))
                    service.LogEvent(binding.Key, parameters);
                else
                    Debug.LogWarning($"[Analytics] No service registered for target: {target}");
            }
        }
    }
}