using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Game;
using GothicVampire.Technologies;
using Sylpheed.Core;
using Sylpheed.Extensions;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class MilestoneHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text _milestoneLevelText;
        [SerializeField] private MilestoneTierElement _milestoneTemplate;

        [Header("Subviews")] 
        [SerializeField] private MilestonePrerequisiteListView _prerequisiteListView;
        [SerializeField] private MilestoneUnlockListView _unlockListView;
        [SerializeField] private MilestoneRewardListView _rewardListView;

        [Header("Empty State")] 
        [SerializeField] private GameObject _emptyDescriptionState;
        [SerializeField] private GameObject _activeDescriptionState;
        
        private TechnologyManager _technologyManager;
        private readonly List<MilestoneTierElement> _milestoneElements = new();
        
        private void Awake()
        {
            gameObject.SetActive(false);
            _milestoneTemplate.gameObject.SetActive(false);
        }
        
        private void OnEnable()
        {
            _technologyManager = ServiceLocator.Get<World>().Player.GetService<TechnologyManager>();
            Refresh();
            
            _technologyManager?.EvtMilestoneCompleted.AddListener(OnMilestoneCompleted);
        }

        private void OnDisable()
        {
            Clear();
            
            _technologyManager?.EvtMilestoneCompleted.RemoveListener(OnMilestoneCompleted);
        }

        private void Refresh()
        {
            // Determine tier level
            var tierLv = 0;
            if (_technologyManager.ActiveMilestone != null) tierLv = _technologyManager.Milestones.IndexOf(_technologyManager.ActiveMilestone) + 1;
            else if (_technologyManager.Milestones.Any()) tierLv = _technologyManager.Milestones.Count;
            else tierLv = 0;
            _milestoneLevelText.text = $"{tierLv}";
            
            CreateElements();
            _prerequisiteListView.Show(_technologyManager.ActiveMilestone);
            _unlockListView.Show(_technologyManager.ActiveMilestone);
            _rewardListView.Show(_technologyManager.ActiveMilestone);
            
            // Handle empty state
            _emptyDescriptionState.SetActive(_technologyManager.ActiveMilestone == null);
            _activeDescriptionState.SetActive(_technologyManager.ActiveMilestone != null);
        }

        private void Clear()
        {
            _milestoneElements.ForEach(e => Destroy(e.gameObject));
            _milestoneElements.Clear();
        }

        private void CreateElements()
        {
            _milestoneElements.ForEach(e => Destroy(e.gameObject));
            _milestoneElements.Clear();

            foreach (var milestone in _technologyManager.Milestones)
            {
                var element = Instantiate(_milestoneTemplate, _milestoneTemplate.transform.parent);
                _milestoneElements.Add(element);
                element.Initialize(milestone);
                element.gameObject.SetActive(true);
            }
        }
        
        private void OnMilestoneCompleted(Technology technology) => Refresh();

        public void ToggleVisibility()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }
}