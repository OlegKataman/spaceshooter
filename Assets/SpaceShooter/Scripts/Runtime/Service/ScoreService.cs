using System;
using System.Collections.Generic;
using Develop.Runtime.SDK.Analytics;
using Develop.Runtime.SDK.Config;
using VContainer;

namespace SpaceShooter.Runtime.Service
{
    public sealed class ScoreService
    {
        [Inject]
        private AnalyticsFacade _analytics;

        public int Score { get; private set; }
        public event Action OnAddScore;

        public void AddScore()
        {
            Score++;
            
            _analytics.LogEvent(AnalyticsEvent.EnemyDestroyed,
                new Dictionary<string, object>
                {
                    { "score", Score }
                });
            
            OnAddScore?.Invoke();
        }
    }
}