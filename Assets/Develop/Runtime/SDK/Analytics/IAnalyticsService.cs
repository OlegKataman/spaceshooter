using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Config;

namespace Develop.Runtime.SDK.Analytics
{
    public interface IAnalyticsService
    {
        AnalyticsTarget Target { get; }
        bool IsInitialized { get; }

        UniTask InitializeAsync(CancellationToken cancellationToken);
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
    }
}