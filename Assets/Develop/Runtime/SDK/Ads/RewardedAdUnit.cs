using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Develop.Runtime.SDK.Ads
{
    /// <summary>
    /// Управляет жизненным циклом rewarded-объявления AdMob.
    /// Логика ретраев и предзагрузки наследуется из BaseAdUnit.
    ///
    /// ShowAsync возвращает Reward если игрок досмотрел ролик до конца,
    /// либо null если закрыл раньше, показ упал или был отменён.
    /// </summary>
    public sealed class RewardedAdUnit : BaseAdUnit<RewardedAd>, IRewardedAdUnit
    {
        public RewardedAdUnit(string adUnitId) : base(adUnitId) { }
        
        public event Action<Reward> OnRewarded;

        // ── Загрузка ──────────────────────────────────────────────────────────

        protected override async UniTask<(RewardedAd ad, string errorMessage)> LoadAdAsync(
            CancellationToken token)
        {
            var tcs = new UniTaskCompletionSource<(RewardedAd, string)>();

            await using var reg = token.Register(() => tcs.TrySetCanceled(token));

            RewardedAd.Load(AdUnitId, new AdRequest(), (ad, error) =>
            {
                if (error != null)
                {
                    tcs.TrySetResult((null, error.GetMessage()));
                    return;
                }

                LogAdSource(ad);
                tcs.TrySetResult((ad, null));
            });

            return await tcs.Task;
        }

        protected override bool CanShow(RewardedAd ad) => ad.CanShowAd();

        protected override void DestroyAd(RewardedAd ad) => ad.Destroy();

        // ── Показ (базовый UniTask — требуется интерфейсом IAdUnit) ───────────

        /// <summary>
        /// Показывает rewarded-рекламу без возврата награды.
        /// Используйте ShowAsync(CancellationToken) с Reward если нужен результат.
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken cancellationToken)
            => await ((IRewardedAdUnit)this).ShowAsync(cancellationToken);

        // ── Показ с возвратом Reward ──────────────────────────────────────────

        /// <summary>
        /// Показывает rewarded-рекламу и возвращает Reward если игрок досмотрел до конца.
        /// Возвращает null если ролик закрыт раньше, упал или отменён.
        /// </summary>
        async UniTask<Reward> IRewardedAdUnit.ShowAsync(CancellationToken cancellationToken)
        {
            if (IsDisposed)
            {
                Debug.LogWarning("[Rewarded] Disposed, cannot show");
                return null;
            }

            await UniTask.SwitchToMainThread(cancellationToken);

            using var linkedCts = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, DisposeToken);

            var ad = await TakeAdForShowAsync(linkedCts.Token);
            if (ad == null) return null;

            var reward = await ShowRewardedAndWaitAsync(ad, linkedCts.Token);

            StartPreload();

            return reward;
        }

        // ── Показ и ожидание закрытия ─────────────────────────────────────────

        protected override async UniTask ShowAdAndWaitAsync(RewardedAd ad, CancellationToken token)
            => await ShowRewardedAndWaitAsync(ad, token);

        private async UniTask<Reward> ShowRewardedAndWaitAsync(
            RewardedAd ad, CancellationToken token)
        {
            var showTcs  = new UniTaskCompletionSource();
            Reward earnedReward = null;

            void CleanupAndDestroy()
            {
                ad.OnAdFullScreenContentClosed -= OnClosed;
                ad.OnAdFullScreenContentFailed -= OnFailed;
                ad.OnAdFullScreenContentOpened -= OnOpened;
                ad.Destroy();
            }

            void OnOpened() => Debug.Log("[Rewarded] Opened");

            void OnClosed()
            {
                CleanupAndDestroy();
                showTcs.TrySetResult();
            }

            void OnFailed(AdError error)
            {
                Debug.LogWarning($"[Rewarded] Show failed: {error.GetMessage()}");
                CleanupAndDestroy();
                showTcs.TrySetResult();
            }

            ad.OnAdFullScreenContentClosed += OnClosed;
            ad.OnAdFullScreenContentFailed += OnFailed;
            ad.OnAdFullScreenContentOpened += OnOpened;

            try
            {
                // Callback вызывается только если игрок досмотрел до конца
                ad.Show(reward =>
                {
                    earnedReward = reward;
                    OnRewarded?.Invoke(reward);
                    Debug.Log($"[Rewarded] Earned: {reward.Type} x{reward.Amount}");
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"[Rewarded] Show exception: {e}");
                CleanupAndDestroy();
                showTcs.TrySetResult();
                return null;
            }

#if UNITY_EDITOR
            await WaitForEditorCloseAsync(showTcs);
            // В редакторе эмулируем выдачу награды — иначе не протестировать флоу
            earnedReward ??= new Reward { Type = "editor_mock", Amount = 1 };
#else
            var wasCancelled = await showTcs.Task
                .AttachExternalCancellation(token)
                .SuppressCancellationThrow();

            if (wasCancelled)
                Debug.Log("[Rewarded] Show wait cancelled");
#endif

            return earnedReward;
        }

#if UNITY_EDITOR
        private static async UniTask WaitForEditorCloseAsync(UniTaskCompletionSource showTcs)
        {
            var escapeTcs = new UniTaskCompletionSource();

            UniTask.Void(async () =>
            {
                while (!escapeTcs.Task.Status.IsCompleted())
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        escapeTcs.TrySetResult();
                        break;
                    }
                }
            });

            await UniTask.WhenAny(showTcs.Task, escapeTcs.Task);
            escapeTcs.TrySetResult();
        }
#endif

        // ── Вспомогательные методы ────────────────────────────────────────────

        private static void LogAdSource(RewardedAd ad)
        {
            try
            {
                var info = ad.GetResponseInfo()?.GetLoadedAdapterResponseInfo();
                Debug.Log($"[Rewarded] Loaded via {info?.AdSourceName ?? "unknown"}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Rewarded] Failed to log ad source: {e.Message}");
            }
        }
    }
}
