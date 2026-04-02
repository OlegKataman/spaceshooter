using Develop.Runtime.Pool;
using UnityEngine;

namespace SpaceShooter.Runtime.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class AsteroidMover : MonoBehaviour
    {
        [SerializeField] private float _minSpeed = 2f;
        [SerializeField] private float _maxSpeed = 6f;
        [SerializeField] private float _maxDriftAngle = 15f; // лёгкий дрейф влево/вправо
        [SerializeField] private float _rotationSpeed = 30f;

        private Rigidbody2D _rb;
        private ObjectPool _ownerPool;
        private Camera _camera;
        private float _despawnY;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _camera = Camera.main;
        }

        public void Initialize(ObjectPool ownerPool = null)
        {
            _ownerPool = ownerPool;

            // Нижний край экрана + небольшой запас
            _despawnY = _camera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 1f;

            // Направление — вниз с небольшим случайным дрейфом
            float angle = Random.Range(-_maxDriftAngle, _maxDriftAngle);
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
            float speed = Random.Range(_minSpeed, _maxSpeed);

            _rb.linearVelocity = direction * speed;
            _rb.angularVelocity = Random.Range(-_rotationSpeed, _rotationSpeed);
        }

        private void Update()
        {
            // Возврат в пул при выходе за нижний край
            if (transform.position.y < _despawnY)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;

                if (_ownerPool != null)
                    _ownerPool.Release(gameObject);
                else
                    gameObject.SetActive(false);
            }
        }
    }
}