using System.Collections.Generic;
using UnityEngine;

namespace Develop.Runtime.Pool
{
    public sealed class ObjectPool : MonoBehaviour
    {
        private readonly Queue<GameObject> _pool = new();
        private Transform _parent;
        
        public void Warmup(int count)
        {
            _pool.Clear();
            _parent = new GameObject(this.gameObject.name).transform;
            
            for (var i = 0; i < count; i++)
            {
                _pool.Enqueue(CreateInstance());
            }
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            var instance = _pool.Count > 0 ? _pool.Dequeue() : CreateInstance();

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);

            return instance;
        }

        public void Release(GameObject instance)
        {
            instance.SetActive(false);
            _pool.Enqueue(instance);
        }

        private GameObject CreateInstance()
        {
            var instance = Instantiate(this.gameObject, _parent);
            instance.SetActive(false);
            return instance;
        }
    }
}