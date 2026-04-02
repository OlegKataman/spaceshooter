using System;
using System.Collections.Generic;
using UnityEngine;

namespace Develop.Runtime.SDK.Config
{
    [CreateAssetMenu(menuName = "Analytics/Event Config")]
    public sealed class AnalyticsEventConfig : ScriptableObject
    {
        [field: SerializeField] public AnalyticsEvent EventType { get; private set; }
        [field: SerializeField] public List<AnalyticsParam> Parameters { get; private set; } = new();
    }

    [Serializable]
    public class AnalyticsParam
    {
        public string key;
        public string value;
    }
}