using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Unrest;
using Sylpheed.Extensions;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class UnrestSnapshotTooltip : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private TMP_Text _deltaText;
        [SerializeField] private Color _zeroDeltaColor;
        [SerializeField] private Color _positiveDeltaColor;
        [SerializeField] private Color _negativeDeltaColor;
        
        [Header("Breakdown")]
        [SerializeField] private UnrestSnapshotCategoryElement _activeEffectTemplate;
        [SerializeField] private UnrestSnapshotCategoryElement _nextCycleTemplate;
        [SerializeField] private GameObject _activeEffectsContainer;
        [SerializeField] private GameObject _nextCycleContainer;
        [SerializeField] private GameObject _emptyState;

        private UnrestActor _unrestActor;
        private readonly List<UnrestSnapshotCategoryElement> _nextCycleElements = new();
        private readonly List<UnrestSnapshotCategoryElement> _activeEffectElements = new();
        private bool _shouldRefresh;

        private void Awake()
        {
            _nextCycleTemplate.gameObject.SetActive(false);
            _activeEffectTemplate.gameObject.SetActive(false);
            _nextCycleContainer.SetActive(false);
            _activeEffectsContainer.SetActive(false);
            _emptyState.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (!_unrestActor) return;
            
            Refresh();
            
            _unrestActor.EvtResolved.AddListener(OnUnrestResolved);
            _unrestActor.EvtPredicted.AddListener(OnUnrestPredicted);
        }

        private void OnDisable()
        {
            if (!_unrestActor) return;
            
            _unrestActor?.EvtResolved.RemoveListener(OnUnrestResolved);
            _unrestActor?.EvtPredicted.RemoveListener(OnUnrestPredicted);
        }

        public void Initialize(UnrestActor unrestActor)
        {
            _unrestActor = unrestActor;
            Refresh();
        }

        private void Refresh()
        {
            _valueText.text = _unrestActor.LastResolved.Value.ToString("N0");
            
            _deltaText.text = _unrestActor.LastPredicted.UnclampedDelta.ToStringWithPrefix("N0");
            _deltaText.color = _unrestActor.LastPredicted.UnclampedDelta switch
            {
                > 0 => _positiveDeltaColor,
                < 0 => _negativeDeltaColor,
                _ => _zeroDeltaColor
            };
            
            CreateNextCycleElements();
            CreateActiveEffectElements();
            _emptyState.SetActive(!_unrestActor.LastPredicted.AllCategories.Any());
        }

        private void LateUpdate()
        {
            if (!_shouldRefresh) return;
            _shouldRefresh = false;
            Refresh();
        }

        private void OnUnrestResolved(UnrestSnapshot snapshot) => _shouldRefresh = true;
        private void OnUnrestPredicted(UnrestSnapshot snapshot) => _shouldRefresh = true;

        private void CreateNextCycleElements()
        {
            _nextCycleElements.ForEach(e => Destroy(e.gameObject));
            _nextCycleElements.Clear();
            
            _nextCycleContainer.SetActive(_unrestActor.LastPredicted.PostResolvedCategories.Any());
            
            foreach (var categorySnapshot in _unrestActor.LastPredicted.PostResolvedCategories)
            {
                var element =  Instantiate(_nextCycleTemplate, _nextCycleTemplate.transform.parent);
                _nextCycleElements.Add(element);
                element.Initialize(categorySnapshot);
                element.gameObject.SetActive(true);
            }
        }
        
        private void CreateActiveEffectElements()
        {
            _activeEffectElements.ForEach(e => Destroy(e.gameObject));
            _activeEffectElements.Clear();
            
            _activeEffectsContainer.SetActive(_unrestActor.LastPredicted.PreResolvedCategories.Any());
            
            foreach (var categorySnapshot in _unrestActor.LastPredicted.PreResolvedCategories)
            {
                var element =  Instantiate(_activeEffectTemplate, _activeEffectTemplate.transform.parent);
                _activeEffectElements.Add(element);
                element.Initialize(categorySnapshot);
                element.gameObject.SetActive(true);
            }
        }
    }
}