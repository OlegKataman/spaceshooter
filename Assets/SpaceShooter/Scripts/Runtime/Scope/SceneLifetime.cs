using SpaceShooter.Runtime.Core;
using SpaceShooter.Runtime.Service;
using VContainer;
using VContainer.Unity;

namespace SpaceShooter.Runtime.Scope
{
    public sealed class SceneLifetime : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ScoreService>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<Game>().AsSelf();
        }
    }
}