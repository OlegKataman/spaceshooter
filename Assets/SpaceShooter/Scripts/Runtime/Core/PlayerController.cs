using UnityEngine;

namespace SpaceShooter.Runtime.Core
{
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _acceleration = 10f;
        [SerializeField] private float _drag = 5f;

        [Header("Tilt")]
        [SerializeField] private float _tiltAmount = 15f;     // максимальный угол
        [SerializeField] private float _tiltSpeed = 10f;      // скорость поворота
        [SerializeField] private float _tiltBoost = 2f;       // усиление при резких свайпах

        private Vector2 _startTouch;
        private float _velocity;
        private float _prevVelocity;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _startTouch = Input.mousePosition;
            }

            if (Input.GetMouseButton(0))
            {
                var delta = (Vector2)Input.mousePosition - _startTouch;
                float targetVelocity = delta.x * (_speed / 100f);

                _velocity = Mathf.Lerp(_velocity, targetVelocity, _acceleration * Time.deltaTime);
            }
            else
            {
                _velocity = Mathf.Lerp(_velocity, 0, _drag * Time.deltaTime);
            }

            transform.position += Vector3.right * _velocity * Time.deltaTime;

            float velocityChange = Time.deltaTime > 0f
                ? (_velocity - _prevVelocity) / Time.deltaTime
                : 0f;

            _prevVelocity = _velocity;

            float tiltFromSpeed = -_velocity * _tiltAmount;
            float tiltFromSwipe = -velocityChange * _tiltBoost;

            float targetTilt = Mathf.Clamp(tiltFromSpeed + tiltFromSwipe, -_tiltAmount, _tiltAmount);

            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetTilt);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                _tiltSpeed * Time.deltaTime);
        }
    }
}