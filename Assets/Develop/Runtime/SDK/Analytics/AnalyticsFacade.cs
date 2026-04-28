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
        private readonly Dictionary<AnalyticsTarget, IAnalyticsProvider> _providers;
        private readonly Dictionary<AnalyticsEvent, EventBinding> _bindings;

        public AnalyticsFacade(IEnumerable<IAnalyticsProvider> services, AnalyticsEventTemplate template)
        {
            _providers = services.ToDictionary(s => s.Target);
            _bindings = template.Events.ToDictionary(e => e.EventType);
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            await UniTask.WhenAll(
                _providers.Values.Select(s => s.InitializeAsync(cancellationToken))
            );

            Debug.Log("[Analytics] Ready");
        }
        
        public void LevelStart(int levelIndex)
        {
            LogEvent(
                AnalyticsEvent.LevelStart,
                AnalyticsParam.Of("level_index", levelIndex));
        }

        public void LevelComplete(int levelIndex, float timeSec, int moves = 0)
        {
            LogEvent(
                AnalyticsEvent.LevelComplete,
                AnalyticsParam.Of("level_index", levelIndex),
                AnalyticsParam.Of("time_sec", timeSec),
                AnalyticsParam.Of("moves", moves));
        }

        public void LevelFail(int levelIndex, string reason = "unknown")
        {
            LogEvent(
                AnalyticsEvent.LevelFail,
                AnalyticsParam.Of("level_index", levelIndex),
                AnalyticsParam.Of("reason", reason));
        }

        public void Retry(int levelIndex, int retryCount)
        {
            LogEvent(
                AnalyticsEvent.LevelRetry,
                AnalyticsParam.Of("level_index", levelIndex),
                AnalyticsParam.Of("retry_count", retryCount));
        }

        public void AdWatch(string placement, string type)
        {
            LogEvent(
                AnalyticsEvent.AdWatched,
                AnalyticsParam.Of("placement", placement),
                AnalyticsParam.Of("type", type));
        }

        public void Revenue(double value, string currency)
        {
            LogEvent(
                AnalyticsEvent.Purchase,
                AnalyticsParam.Of("value", value),
                AnalyticsParam.Of("currency", currency));
        }

        public void LogEvent(
            AnalyticsEvent eventType,
            params AnalyticsParam[] parameters)
        {
            if (!_bindings.TryGetValue(eventType, out var binding))
                return;

            foreach (var target in binding.Targets)
            {
                if (_providers.TryGetValue(target, out var provider))
                    provider.LogEvent(binding.Key, parameters);
            }
        }
    }
}