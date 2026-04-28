// AnalyticsEventConfig.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Develop.Runtime.SDK.Analytics;

namespace Develop.Runtime.SDK.Config
{
    [CreateAssetMenu(menuName = "Analytics/Event Config")]
    public sealed class AnalyticsEventConfig : ScriptableObject
    {
        [field: SerializeField] public AnalyticsEvent EventType { get; private set; }
        [field: SerializeField] public List<SerializedAnalyticsParam> Parameters { get; private set; } = new();
    }

    [Serializable]
    public sealed class SerializedAnalyticsParam
    {
        public string key;
        public string value;
        public SerializedParamType type;

        public AnalyticsParam ToParam() => type switch
        {
            SerializedParamType.Int    => int.TryParse(value, out var i)       ? AnalyticsParam.Of(key, i)      : AnalyticsParam.Of(key, value),
            SerializedParamType.Double => double.TryParse(value, out var d)    ? AnalyticsParam.Of(key, d)      : AnalyticsParam.Of(key, value),
            SerializedParamType.Bool   => bool.TryParse(value, out var b)      ? AnalyticsParam.Of(key, b)      : AnalyticsParam.Of(key, value),
            _                          => AnalyticsParam.Of(key, value)
        };
    }

    public enum SerializedParamType
    {
        String,
        Int,
        Double,
        Bool
    }
}