using System.Collections.Generic;
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
            if (_configs.Count == 0)
            {
                Debug.LogWarning($"[AnalyticsEventSender] Config not assigned on {gameObject.name}");
                return;
            }

            foreach (var config in _configs)
                SendConfig(config);
        }

        private void SendConfig(AnalyticsEventConfig config)
        {
            var parameters = config.Parameters;

            if (parameters.Count == 0)
            {
                _facade.LogEvent(config.EventType);
                return;
            }

            var analyticsParams = new AnalyticsParam[parameters.Count];

            for (var i = 0; i < parameters.Count; i++)
                analyticsParams[i] = parameters[i].ToParam();

            _facade.LogEvent(config.EventType, analyticsParams);
        }
    }
}