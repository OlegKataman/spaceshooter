using System;
using UnityEngine;
using VContainer;

namespace SpaceShooter.Runtime.Core
{
    public sealed class Player : MonoBehaviour
    {
        [SerializeField] 
        private LayerMask _enemyLayerMask;
        
        public int Health { get; private set; } = 3;
        public event Action OnHealthChange;

        [Inject] 
        private Game _game;
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (((1 << other.gameObject.layer) & _enemyLayerMask) != 0)
                Damage();
        }
        
        private void Damage()
        {
            Health--;
            
            if (Health <= 0)
                _game.GameOver();
            
            OnHealthChange?.Invoke();
        }
    }
}