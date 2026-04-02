using System.Collections.Generic;
using System.Linq;
using Develop.Runtime.SDK.Analytics;
using UnityEngine;
using VContainer;

namespace Develop.Runtime.SDK.Config
{
    public sealed class AnalyticsEventSender : MonoBehaviour
    {
        [SerializeField] 
        private List<AnalyticsEventConfig> _configs = new();

        [Inject]
        private AnalyticsFacade _facade;
        
        public void Send()
        {
            if (!_configs.Any())
            {
                Debug.LogWarning($"[AnalyticsEventSender] Config not assigned on {gameObject.name}");
                return;
            }

            foreach (var config in _configs)
            {
                _facade.LogEvent(config.EventType, BuildParameters(config.Parameters));
            }
        }

        private Dictionary<string, object> BuildParameters(List<AnalyticsParam> parameters)
        {
            if (!parameters.Any())
                return null;

            var result = new Dictionary<string, object>();

            foreach (var p in parameters)
                result[p.key] = p.value;

            return result;
        }
    }
}