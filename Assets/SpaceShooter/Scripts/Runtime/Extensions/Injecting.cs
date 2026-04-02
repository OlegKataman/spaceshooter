using SpaceShooter.Runtime.Scope;
using UnityEngine;
using VContainer.Unity;

namespace SpaceShooter.Runtime.Extensions
{
    public static class Injecting
    {
        public static void InjectIntoSceneLifetime(this GameObject gameObject)
        {
            var context = Object.FindAnyObjectByType<SceneLifetime>();
            var container = context.Container;
            
            container.InjectGameObject(gameObject);
        }

        public static void InjectIntoSceneLifetime(this object obj)
        {
            var context = Object.FindAnyObjectByType<SceneLifetime>();
            var container = context.Container;
            
            container.Inject(obj);
        }

        public static GameObject InstantiateIntoSceneLifetime(this GameObject gameObject, Vector3 position, Quaternion rotation)
        {
            var context = Object.FindAnyObjectByType<SceneLifetime>();
            var container = context.Container;

            return container.Instantiate(gameObject, position, rotation);
        }
    }
}