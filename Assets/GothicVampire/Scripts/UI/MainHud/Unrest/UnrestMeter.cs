using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GothicVampire.Game;
using GothicVampire.Unrest;
using GothicVampire.Utils.UI;
using Sylpheed.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.MainHud
{
    public class UnrestMeter : MonoBehaviour
    {
        [SerializeField] private Image _fillCurrent;
        [SerializeField] private Image _fillCurrentInverse;
        [SerializeField] private Image _fillIncrease;
        [SerializeField] private Image _fillDecrease;
        [SerializeField] private GameObject _decreaseContainer;
        [SerializeField] private UnrestSnapshotTooltip _tooltip;
        [SerializeField] private TMP_Text _netText;
        
        [Header("Misc")]
        [SerializeField] private float _fillAnimDuration = 0.5f;
        [SerializeField] private float _netAnimDuration = 0.5f;
        [SerializeField] private Color _netPositiveColor;
        [SerializeField] private Color _netNegativeColor;
        [SerializeField] private float _fillPulseDuration = 1f;
        
        private UnrestActor _unrestActor;
        private UnrestSnapshot _queuedPredictedSnapshot;
        private bool _resolvedSnapshotAnimating;
        private bool _pendingSnapshotQueued;

        private void Awake()
        {
            _netText.gameObject.SetActive(false);
            
            _fillCurrent.fillAmount = 0f;
            _fillCurrentInverse.fillAmount = 1f;
            
            _fillIncrease.fillAmount = 0f;
            _fillDecrease.fillAmount = 1f;
            _decreaseContainer.SetActive(false);
        }

        private void Start()
        {
            _unrestActor = World.Current.Player.GetService<UnrestActor>();
            _unrestActor.EvtPredicted.AddListener(OnUnrestPredicted);
            _unrestActor.EvtValueChanged.AddListener(OnUnrestValueUpdated);

            _fillCurrent.fillAmount = _unrestActor.Normalized;
            _fillCurrentInverse.fillAmount = 1f - _unrestActor.Normalized;
            
            _tooltip.Initialize(_unrestActor);
            
            // TODO: Doesn't work with gradient
            // StartCoroutine(AnimatePulse(_fillIncrease, _fillAnimDuration));
            // StartCoroutine(AnimatePulse(_fillDecrease, _fillAnimDuration));
        }

        private void OnDestroy()
        {
            _unrestActor?.EvtPredicted.RemoveListener(OnUnrestPredicted);
            _unrestActor?.EvtValueChanged.RemoveListener(OnUnrestValueUpdated);
        }

        private void OnUnrestPredicted(UnrestSnapshot snapshot)
        {
            _decreaseContainer.SetActive(snapshot.Delta < 0);
            
            // _fillIncrease.AnimateFill(snapshot.Normalized, _fillAnimDuration);
            // _fillDecrease.AnimateFill(1f - snapshot.Normalized, _fillAnimDuration);
            
            QueuePredictionMeterAnimation().Forget();
        }
        
        private void OnUnrestValueUpdated(UnrestActor.ValueChangedArgs args)
        {
            _fillCurrent.AnimateFill(args.Normalized, _fillAnimDuration);
            _fillCurrentInverse.AnimateFill(1f - args.Normalized, _fillAnimDuration);
            
            AnimateNetText(_netText, args.Delta, args.Normalized, _netAnimDuration);

            // Set busy animation
            _resolvedSnapshotAnimating = true;
            UniTask.Delay(TimeSpan.FromSeconds(_fillAnimDuration)).ContinueWith(() =>
            {
                _resolvedSnapshotAnimating = false;
            }).Forget();
        }

        private async UniTaskVoid QueuePredictionMeterAnimation()
        {
            if (_pendingSnapshotQueued) return;
            _pendingSnapshotQueued = true;
            
            await UniTask.WaitWhile(() => _resolvedSnapshotAnimating);
            var snapshot = _unrestActor.LastPredicted;
            _fillIncrease.AnimateFill(snapshot.Normalized, _fillAnimDuration);
            _fillDecrease.AnimateFill(1f - snapshot.Normalized, _fillAnimDuration);
            
            _pendingSnapshotQueued = false;
        }

        private void AnimateNetText(TMP_Text text, float value, float startNormalizedPos, float duration)
        {
            if (Mathf.Approximately(value, 0f)) return;
            
            text.gameObject.SetActive(true);
            text.text = value.ToStringWithPrefix("N0");
            
            // Text
            var textParent = text.transform.parent as RectTransform ?? throw new Exception();
            var containerHeight = textParent.rect.height;
            text.rectTransform.anchoredPosition = new Vector2(text.rectTransform.anchoredPosition.x, containerHeight * startNormalizedPos);
            // var toYPos = text.rectTransform.localPosition.y + 50f;
            // text.rectTransform.DOLocalMoveY(toYPos, duration);
            
            // Color fade
            text.color = value > 0 ? _netPositiveColor : _netNegativeColor;
            DOTween.To(
                () => text.color,
                x => text.color = x,
                new Color(text.color.r, text.color.g, text.color.b, 0f),
                duration);
        }

        private static IEnumerator AnimatePulse(Image image, float duration)
        {
            // TODO: Doesn't work with gradient
            while (true)
            {
                // Fade to invisible
                yield return DOTween.ToAlpha(
                        () => image.color,
                        x => image.color = x,
                        0f,
                        duration / 2f)
                    .WaitForCompletion();
            
                // Fade to opaque
                yield return DOTween.ToAlpha(
                        () => image.color,
                        x => image.color = x,
                        1f,
                        duration / 2f)
                    .WaitForCompletion();
            }
        }
    }
}