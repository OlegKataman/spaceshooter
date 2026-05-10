using Cysharp.Threading.Tasks;
using Develop.Runtime.SDK.Ads;
using GoogleMobileAds.Api;
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
        [Inject] 
        private IAdsProvider _ads;

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

        public void RewardButtonClick()
        {
            DoAsync().Forget();
            return;
            
            async UniTask DoAsync()
            {
                _ads.Rewarded.OnRewarded += GiveReward;
                
                await _ads.Rewarded.ShowAsync(destroyCancellationToken);
                
                _ads.Rewarded.OnRewarded -= GiveReward;
            }
        }

        private void GiveReward(Reward reward)
        {
            Debug.Log($"Give reward {reward.Type} {reward.Amount}");
        }
    }
}
