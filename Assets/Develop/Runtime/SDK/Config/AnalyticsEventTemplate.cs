using System;
using System.Collections.Generic;
using UnityEngine;

namespace Develop.Runtime.SDK.Config
{
    [CreateAssetMenu(menuName = "Analytics/Event Template")]
    public sealed class AnalyticsEventTemplate : ScriptableObject
    {
        [field: SerializeField]
        public List<EventBinding> Events { get; private set; } = new();
    }

    [Serializable]
    public sealed class EventBinding
    {
        [field: SerializeField] public AnalyticsEvent EventType { get; private set; }
        [field: SerializeField] public string Key { get; private set; }
        [field: SerializeField] public List<AnalyticsTarget> Targets { get; private set; } = new();
    }

    public enum AnalyticsEvent
    {
        EnemyDestroyed,
        GameOver
    }

    public enum AnalyticsTarget
    {
        Firebase,
        Adjust,
        Meta
    }
}