using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;

namespace GothicVampire.CodeBasedAnimators
{
    public class TextUIColorAnimator
    {
        public TMP_Text TextGraphic { get; private set; }
        public float Duration { get; private set; }
        public float TimeElapsed { get; private set; }
        public float TimeRemaining => Duration - TimeElapsed;
        public float Progress => Mathf.Clamp01(TimeElapsed / Duration);
        public bool IsRunning { get; private set; }

        public TextUIColorAnimator(TMP_Text graphic)
        {
            TextGraphic = graphic;
        }

        public void Start(Color startColor, Color targetColor, float duration, bool ignoreTimeScale = false, Action onDone = null, Action onStopped = null)
        {
            if (IsRunning) throw new Exception("Already running. Stop animator or let it finish.");
            AsyncStart(startColor, targetColor, duration, ignoreTimeScale, onDone, onStopped).Forget();
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

        private async UniTask AsyncStart(Color startColor, Color targetColor, float duration, bool ignoreTimeScale = false, Action onDone = null, Action onStopped = null)
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

                TextGraphic.color = Color.Lerp(startColor, targetColor, Progress);

                if (TimeElapsed >= Duration)
                {
                    TextGraphic.color = targetColor;
                }

                return TimeElapsed >= Duration || IsRunning == false;
            }, cancellationToken: TextGraphic.destroyCancellationToken);

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