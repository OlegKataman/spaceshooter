using Develop.Runtime.SDK.Ads;
using Develop.Runtime.SDK.Analytics;
using Develop.Runtime.SDK.Config;
using SpaceShooter.Runtime.Service;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SpaceShooter.Runtime.Scope
{
    public sealed class ProjectLifetime : LifetimeScope
    {
        [SerializeField] 
        private SdkSettingsConfig _sdkSettings;
        [SerializeField] 
        private AnalyticsEventTemplate _template;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<AssetService>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<UIService>();
            
            builder.RegisterInstance(_sdkSettings).As<SdkSettingsConfig>();
            
            // Analytics
            builder.Register<IAnalyticsService, FirebaseAnalyticsService>(Lifetime.Singleton);
            builder.Register<IAnalyticsService, AdjustAnalyticsService>(Lifetime.Singleton);
            builder.Register<IAnalyticsService, MetaAnalyticsService>(Lifetime.Singleton);
            
            builder.RegisterInstance(_template).As<AnalyticsEventTemplate>();
            builder.Register<AnalyticsFacade>(Lifetime.Singleton);

            // Ads
            builder.Register<IAdsService, AdMobMediationService>(Lifetime.Singleton);
            builder.Register<AdsFacade>(Lifetime.Singleton);
        }
    }
}