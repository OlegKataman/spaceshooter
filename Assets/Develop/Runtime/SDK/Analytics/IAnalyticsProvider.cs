using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Config;

namespace Develop.Runtime.SDK.Analytics
{
    public enum ParamType { Long, Double, String }

    public readonly struct AnalyticsParam
    {
        public readonly string key;
        public readonly ParamType type;
        
        private readonly long   _longValue;
        private readonly double _doubleValue;
        private readonly string _stringValue;

        private AnalyticsParam(string key, long value)
        {
            this.key    = key;
            type        = ParamType.Long;
            _longValue  = value;
            _doubleValue = 0;
            _stringValue = null;
        }

        private AnalyticsParam(string key, double value)
        {
            this.key     = key;
            type         = ParamType.Double;
            _doubleValue = value;
            _longValue   = 0;
            _stringValue = null;
        }

        private AnalyticsParam(string key, string value)
        {
            this.key     = key;
            type         = ParamType.String;
            _stringValue = value;
            _longValue   = 0;
            _doubleValue = 0;
        }
        
        public static AnalyticsParam Of(string key, int    v) => new(key, (long)v);
        public static AnalyticsParam Of(string key, long   v) => new(key, v);
        public static AnalyticsParam Of(string key, float  v) => new(key, (double)v);
        public static AnalyticsParam Of(string key, double v) => new(key, v);
        public static AnalyticsParam Of(string key, bool   v) => new(key, v ? 1L : 0L);
        public static AnalyticsParam Of(string key, string v) => new(key, v);
        
        public long   AsLong()   => _longValue;
        public double AsDouble() => _doubleValue;
        public string AsString() => _stringValue;

        public override string ToString() => type switch
        {
            ParamType.Long   => _longValue.ToString(),
            ParamType.Double => _doubleValue.ToString("G"),
            _                => _stringValue ?? string.Empty
        };
    }
    
    public interface IAnalyticsProvider
    {
        AnalyticsTarget Target { get; }
        bool IsInitialized { get; }

        UniTask InitializeAsync(CancellationToken cancellationToken);
        void LogEvent(string eventName, params AnalyticsParam[] parameters);
    }
}