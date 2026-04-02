using UnityEngine;

namespace SpaceShooter.Runtime.Core
{
    public sealed class Bullet : MonoBehaviour
    {
        [SerializeField] 
        private float _speed;
        
        private void Update()
        {
            transform.position += transform.up * _speed * Time.deltaTime;
        }
    }
}