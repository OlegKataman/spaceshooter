using Develop.Runtime.Pool;
using UnityEngine;

namespace SpaceShooter.Runtime.Core
{
    public sealed class Shooter : MonoBehaviour
    {
        [SerializeField] 
        private Transform _spawnPoint;
        [SerializeField]
        private ObjectPool _bulletPool;

        [SerializeField]
        private float _fireRate = 0.5f;

        private void Start()
        {
            _bulletPool.Warmup(200);
            InvokeRepeating(nameof(Shoot), 0f, _fireRate);
        }

        private void Shoot()
        {
            _bulletPool.Get(_spawnPoint.position, this.transform.rotation);
        }
    }
}