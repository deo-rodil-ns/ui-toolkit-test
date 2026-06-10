using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace GothicVampire.CodeBasedAnimators
{
    public class MeshColorAnimator
    {
        public MeshRenderer MeshRenderer { get; private set; }
        public float Duration { get; private set; }
        public float TimeElapsed { get; private set; }
        public float TimeRemaining => Duration - TimeElapsed;
        public float Progress => Mathf.Clamp01(TimeElapsed / Duration);
        public bool IsRunning { get; private set; }

        public MeshColorAnimator(MeshRenderer renderer)
        {
            MeshRenderer = renderer;
        }

        public void Start(Color startColor, Color targetColor, float duration, bool allMaterials, bool ignoreTimeScale = false, CancellationToken? destroyCancellationToken = null, Action onDone = null, Action onStopped = null)
        {
            if (IsRunning) throw new Exception("Already running. Stop animator or let it finish.");
            AsyncStart(startColor, targetColor, duration, allMaterials, ignoreTimeScale, destroyCancellationToken, onDone, onStopped).Forget();
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

        private async UniTask AsyncStart(Color startColor, Color targetColor, float duration, bool allMaterials, bool ignoreTimeScale = false, CancellationToken? destroyCancellationToken = null, Action onDone = null, Action onStopped = null)
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

                if (allMaterials)
                {
                    for (int i = 0; i < MeshRenderer.materials.Length; i++)
                    {
                        MeshRenderer.materials[i].color = Color.Lerp(startColor, targetColor, Progress);
                    }
                }
                else
                {
                    MeshRenderer.material.color = Color.Lerp(startColor, targetColor, Progress);
                }

                if (TimeElapsed >= Duration)
                {
                    if (allMaterials)
                    {
                        for (int i = 0; i < MeshRenderer.materials.Length; i++)
                        {
                            MeshRenderer.materials[i].color = targetColor;
                        }
                    }
                    else
                    {
                        MeshRenderer.material.color = targetColor;
                    }
                }

                return TimeElapsed >= Duration || IsRunning == false;
            }, cancellationToken: destroyCancellationToken == null ? MeshRenderer.GetCancellationTokenOnDestroy() : destroyCancellationToken.Value);

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