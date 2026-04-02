using SpaceShooter.Runtime.Core;
using SpaceShooter.Runtime.Extensions;
using SpaceShooter.Runtime.Service;
using TMPro;
using UnityEngine;
using VContainer;

namespace SpaceShooter.Runtime.UI
{
    public sealed class GameHudFragment : MonoBehaviour
    {
        [SerializeField] 
        private TMP_Text _scoreText, _healthText;
        
        [Inject] 
        private ScoreService _scoreService;

        private void Awake()
        {
            this.InjectIntoSceneLifetime();
        }

        private void OnEnable()
        {
            _scoreService.OnAddScore += UpdateScoreText;

            var player = FindAnyObjectByType<Player>();
            player.OnHealthChange += UpdateHealthText;
        }

        private void OnDisable()
        {
            _scoreService.OnAddScore -= UpdateScoreText;

            var player = FindAnyObjectByType<Player>();
            
            if (player != null)
                player.OnHealthChange -= UpdateHealthText;
        }

        private void UpdateScoreText()
        {
            _scoreText.text = _scoreService.Score.ToString();
        }
        
        private void UpdateHealthText()
        {
            var player = FindAnyObjectByType<Player>();

            _healthText.text = $"{player.Health} / 3";
        }
    }
}
