using Develop.Runtime.Pool;
using SpaceShooter.Runtime.Extensions;
using SpaceShooter.Runtime.Service;
using UnityEngine;
using VContainer;

namespace SpaceShooter.Runtime.Core
{
    public sealed class Enemy : MonoBehaviour
    {
        [SerializeField] 
        private ObjectPool _pool;

        [SerializeField] 
        private LayerMask _bulletLayerMask;
        
        [Inject]
        private ScoreService _scoreService;

        private void Awake()
        {
            this.gameObject.InjectIntoSceneLifetime();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & _bulletLayerMask) != 0)
                DestroyEnemy();
        }

        private void DestroyEnemy()
        {
            _scoreService.AddScore();

            _pool.Release(this.gameObject);
        }
    }
}