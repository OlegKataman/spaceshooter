using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Develop.Runtime.SDK.Ads
{
    /// <summary>
    /// Базовый класс рекламного юнита.
    /// Инкапсулирует предзагрузку с экспоненциальными ретраями,
    /// атомарную замену объявления и Dispose.
    ///
    /// Дочерние классы реализуют только загрузку конкретного типа рекламы
    /// и показ с ожиданием закрытия.
    /// </summary>
    public abstract class FullscreenAdUnit<TAd> : IAdUnit where TAd : class
    {
        // ── Конфигурация ──────────────────────────────────────────────────────

        private const int   MaxLoadRetries           = 10;
        private const float InitialRetryDelaySeconds = 5f;
        private const float MaxRetryDelaySeconds     = 60f;
        private const float LoadLoopTimeoutSeconds = 300f;
        private const float ShowTimeoutSeconds     = 10f;

        // ── Публичное состояние ───────────────────────────────────────────────

        public bool IsReady
        {
            get
            {
                var ad = Volatile.Read(ref _ad);
                return ad != null && CanShow(ad);
            }
        }

        // ── Приватные поля ────────────────────────────────────────────────────

        // Текущее загруженное объявление; заменяется атомарно
        private TAd _ad;

        // 0 = свободно, 1 = загрузка идёт
        private int _loadingFlag;

        // 0 = живой, 1 = disposed
        private int _isDisposed;

        private readonly CancellationTokenSource _disposeCts = new();

        // ── Конструктор ───────────────────────────────────────────────────────

        protected readonly string AdUnitId;

        protected FullscreenAdUnit(string adUnitId)
        {
            AdUnitId = adUnitId;
        }

        // ── Абстрактные методы ────────────────────────────────────────────────

        /// <summary>Загружает объявление и возвращает его, либо null при ошибке.</summary>
        protected abstract UniTask<(TAd ad, string errorMessage)> LoadAdAsync(
            CancellationToken token);

        /// <summary>Проверяет, можно ли показать уже загруженное объявление.</summary>
        protected abstract bool CanShow(TAd ad);

        /// <summary>Показывает объявление и ожидает закрытия.</summary>
        protected abstract UniTask ShowAdAndWaitAsync(TAd ad, CancellationToken token);

        /// <summary>Уничтожает объявление и освобождает его ресурсы.</summary>
        protected abstract void DestroyAd(TAd ad);

        // ── Предзагрузка ──────────────────────────────────────────────────────

        /// <summary>
        /// Запускает фоновую предзагрузку.
        /// Повторный вызов во время активной загрузки — no-op.
        /// </summary>
        internal void StartPreload() => LoadAsync().Forget();

        private async UniTaskVoid LoadAsync()
        {
            if (IsDisposed) return;

            if (Interlocked.CompareExchange(ref _loadingFlag, 1, 0) != 0) return;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_disposeCts.Token);
            try
            {
                await RunLoadLoopAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[{GetType().Name}] Preload cancelled");
            }
            finally
            {
                Volatile.Write(ref _loadingFlag, 0);
            }
        }

        private async UniTask RunLoadLoopAsync(CancellationToken token)
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(LoadLoopTimeoutSeconds));

            var retryCount = 0;
            var delaySec = InitialRetryDelaySeconds;

            while (retryCount < MaxLoadRetries && !timeoutCts.Token.IsCancellationRequested)
            {
                var (ad, errorMessage) = await LoadAdAsync(timeoutCts.Token);

                if (ad != null)
                {
                    var oldAd = Interlocked.Exchange(ref _ad, ad);
                    if (oldAd != null) DestroyAd(oldAd);
                    return;
                }

                Debug.LogWarning(
                    $"[{GetType().Name}] Load failed: {errorMessage}. " +
                    $"Retry {retryCount + 1}/{MaxLoadRetries}");

                await UniTask.Delay(TimeSpan.FromSeconds(delaySec), cancellationToken: timeoutCts.Token);
                delaySec = Math.Min(delaySec * 2f, MaxRetryDelaySeconds);
                retryCount++;
            }

            Debug.LogError($"[{GetType().Name}] Max load retries or timeout reached.");
        }

        // ── Показ ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Ожидает готовности рекламы и атомарно забирает объявление для показа.
        /// Возвращает null если реклама не готова в течение ShowTimeoutSeconds
        /// или была отменена.
        /// </summary>
        protected async UniTask<TAd> TakeAdForShowAsync(CancellationToken token)
        {
            var waitReady = UniTask.WaitUntil(() => IsReady, cancellationToken: token)
                .SuppressCancellationThrow();

            var timeout = UniTask.Delay(TimeSpan.FromSeconds(ShowTimeoutSeconds), 
                    cancellationToken: token)
                .SuppressCancellationThrow();

            var (winnerIndex, _, cancelled) = await UniTask.WhenAny(waitReady, timeout);

            if (cancelled || winnerIndex != 0 || !IsReady)
            {
                Debug.LogWarning($"[{GetType().Name}] Show {(cancelled ? "cancelled" : "timeout")}");
                return null;
            }

            // Атомарный захват рекламы
            var ad = Interlocked.Exchange(ref _ad, null);
            if (ad == null || !CanShow(ad))
            {
                Debug.LogWarning($"[{GetType().Name}] Ad lost before show");
                if (ad != null) DestroyAd(ad);
                StartPreload();
                return null;
            }

            return ad;
        }

        /// <summary>
        /// Базовый ShowAsync — используется в InterstitialAdUnit.
        /// RewardedAdUnit переопределяет его со своей сигнатурой.
        /// </summary>
        public virtual async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            if (IsDisposed)
            {
                Debug.LogWarning($"[{GetType().Name}] Disposed, cannot show");
                return;
            }

            await UniTask.SwitchToMainThread(cancellationToken);

            using var linkedCts = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken, _disposeCts.Token);

            var ad = await TakeAdForShowAsync(linkedCts.Token);
            if (ad == null) return;

            await ShowAdAndWaitAsync(ad, linkedCts.Token);

            StartPreload();
        }

        // ── Вспомогательные методы ────────────────────────────────────────────
        
        /// <summary>
        /// Возвращает текущее объявление (безопасно для наследников)
        /// </summary>
        protected TAd GetCurrentAd()
        {
            return Volatile.Read(ref _ad);
        }

        protected bool IsDisposed => Volatile.Read(ref _isDisposed) == 1;

        protected CancellationToken DisposeToken => _disposeCts.Token;

        // ── Dispose ───────────────────────────────────────────────────────────

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 1) return;

            _disposeCts.Cancel();
            _disposeCts.Dispose();

            var ad = Interlocked.Exchange(ref _ad, null);
            if (ad != null) DestroyAd(ad);
        }
    }
}
