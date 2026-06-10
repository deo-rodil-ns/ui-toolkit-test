using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

namespace GothicVampire.CodeBasedAnimators
{
    public class ScaleAnimator
    {
        public Transform Transform { get; private set; }
        public float Duration { get; private set; }
        public float TimeElapsed { get; private set; }
        public float TimeRemaining => Duration - TimeElapsed;
        public float Progress => Mathf.Clamp01(TimeElapsed / Duration);
        public bool IsRunning { get; private set; }

        public ScaleAnimator(Transform transform)
        {
            Transform = transform;
        }

        public void StartScale(Vector3 startScale, Vector3 targetScale, float duration, bool ignoreTimeScale = false, CancellationToken? destroyCancellationToken = null, Action onDone = null, Action onStopped = null)
        {
            if (IsRunning) throw new Exception("Already running. Stop animator or let it finish.");
            AsyncStartScale(startScale, targetScale, duration, ignoreTimeScale, destroyCancellationToken, onDone, onStopped).Forget();
        }

        public void StartBounce(Vector3 originalScale, float duration, bool ignoreTimeScale = false, CancellationToken? destroyCancellationToken = null, Action onDone = null, Action onStopped = null)
        {
            if (IsRunning) throw new Exception("Already running. Stop animator or let it finish.");
            AsyncStartBounce(originalScale, duration, ignoreTimeScale, destroyCancellationToken, onDone, onStopped).Forget();
        }

        public void Stop()
        {
            if (IsRunning == false) return;
            IsRunning = false;
        }

        private void Reset()
        {
            TimeElapsed = 0.0f;
        }

        private async UniTask AsyncStartScale(Vector3 startScale, Vector3 targetScale, float duration, bool ignoreTimeScale = false, CancellationToken? destroyCancellationToken = null, Action onDone = null, Action onStopped = null)
        {
            if (IsRunning) throw new Exception("Already running. Stop animator or let it finish.");

            Reset();

            IsRunning = true;
            Duration = duration;

            // Wait for color change time
            await UniTask.WaitUntil(() =>
            {
                if (IsRunning == false) return false;

                if (ignoreTimeScale)
                    TimeElapsed += Time.fixedDeltaTime;
                else
                    TimeElapsed += Time.deltaTime;

                Transform.localScale = Vector3.Lerp(startScale, targetScale, Progress);

                if (TimeElapsed >= Duration)
                {
                    Transform.localScale = targetScale;
                }

                return TimeElapsed >= Duration || IsRunning == false;
            }, cancellationToken: destroyCancellationToken == null ? Transform.GetCancellationTokenOnDestroy() : destroyCancellationToken.Value);

            if (IsRunning == false)
            {
                onStopped?.Invoke();
                return;
            }

            IsRunning = false;
            onDone?.Invoke();
        }

        private async UniTask AsyncStartBounce(Vector3 originalScale, float duration, bool ignoreTimeScale = false, CancellationToken? destroyCancellationToken = null, Action onDone = null, Action onStopped = null)
        {
            if (IsRunning) throw new Exception("Already running. Stop animator or let it finish.");

            Reset();

            IsRunning = true;
            Duration = duration;

            Transform.DOShakeScale(Duration);

            // Wait for color change time
            await UniTask.WaitUntil(() =>
            {
                if (IsRunning == false) return false;

                if (ignoreTimeScale)
                    TimeElapsed += Time.fixedDeltaTime;
                else
                    TimeElapsed += Time.deltaTime;

                return TimeElapsed >= Duration || IsRunning == false;
            }, cancellationToken: destroyCancellationToken == null ? Transform.GetCancellationTokenOnDestroy() : destroyCancellationToken.Value);

            Transform.DORewind();
            Transform.localScale = originalScale;

            if (IsRunning == false)
            {
                onStopped?.Invoke();
                return;
            }

            IsRunning = false;
            onDone?.Invoke();
        }
    }
}
