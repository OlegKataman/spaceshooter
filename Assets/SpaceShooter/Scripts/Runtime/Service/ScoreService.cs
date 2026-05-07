using System;
using Develop.Runtime.SDK.Analytics;
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
            
            _analytics.EnemyDestroyed(Score);
            
            OnAddScore?.Invoke();
        }
    }
}