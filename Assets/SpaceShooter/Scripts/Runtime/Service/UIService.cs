using UnityEngine;

namespace SpaceShooter.Runtime.Service
{
    public sealed class UIService : MonoBehaviour
    {
        [SerializeField] 
        private GameObject _gameHudFragment, _gameOverFragment;
        
        public void ShowGameHudFragment()
        {
            _gameHudFragment.SetActive(true);
        }
        
        public void ShowGameOverFragment()
        {
            _gameOverFragment.SetActive(true);
        }
    }
}