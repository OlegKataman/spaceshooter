using Develop.Runtime.Pool;
using UnityEngine;

namespace SpaceShooter.Runtime.Core
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private ObjectPool _asteroid;
        [SerializeField] private ObjectPool _asteroidX;

        [Header("Spawn Settings")]
        [SerializeField] private float _spawnInterval = 1.5f;
        [SerializeField] private float _spawnOffsetY = 1f; // отступ за верхним краем экрана

        private float _timer;
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Start()
        {
            _asteroid.Warmup(200);
            _asteroidX.Warmup(100);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _spawnInterval)
            {
                _timer = 0f;
                SpawnRandomAsteroid();
            }

            if (_spawnInterval <= 0.1f)
                _spawnInterval = 0.1f;
            else
                _spawnInterval -= 0.0004f;
        }

        private void SpawnRandomAsteroid()
        {
            // Случайный X по ширине экрана
            float minX = _camera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
            float maxX = _camera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
            float spawnX = Random.Range(minX, maxX);

            // Спавним за верхним краем экрана
            float spawnY = _camera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y + _spawnOffsetY;

            Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

            // Выбираем случайный пул (30% шанс на крупный)
            ObjectPool pool = Random.value < 0.3f ? _asteroidX : _asteroid;
            GameObject asteroid = pool.Get(spawnPos, Quaternion.identity);

            // Инициализируем компонент движения
            if (asteroid.TryGetComponent<AsteroidMover>(out var mover))
                mover.Initialize();
        }
    }
}