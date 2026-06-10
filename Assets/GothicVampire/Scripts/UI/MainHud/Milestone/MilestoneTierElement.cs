using System;
using System.Linq;
using GothicVampire.Technologies;
using Sylpheed.Extensions;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class MilestoneTierElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _tierText;
        [SerializeField] private MilestoneOrb _bigOrb;
        [SerializeField] private MilestoneOrb _smallOrb;
        
        public Technology Milestone { get; private set; }
        
        private MilestoneOrb _currentOrb;
        private TechnologyManager _technologyManager;

        public void Initialize(Technology milestone)
        {
            Milestone = milestone;
            _technologyManager = milestone.Faction.GetService<TechnologyManager>();
            
            _bigOrb.Initialize(milestone);
            _smallOrb.Initialize(milestone);

            Refresh();
        }

        private void OnEnable()
        {
            _technologyManager?.EvtMilestoneCompleted.AddListener(OnMilestoneCompleted);
        }
        
        private void OnDisable()
        {
            _technologyManager?.EvtMilestoneCompleted.RemoveListener(OnMilestoneCompleted);
        }

        private void Refresh()
        {
            // Tier
            var milestoneIndex = _technologyManager.Milestones.IndexOf(Milestone);
            if (milestoneIndex < 0) throw new Exception($"{Milestone.Data.DisplayName} is not  part of the milestone.");
            _tierText.text = $"T{milestoneIndex + 1}";
            
            // Change orb based on if it's the current milestone or not
            var isLastMilestone = _technologyManager.Milestones.LastOrDefault() == Milestone;
            _currentOrb = _technologyManager.ActiveMilestone == Milestone || (isLastMilestone && Milestone is { Unlocked: true })
                ? _bigOrb 
                : _smallOrb;
            _bigOrb.gameObject.SetActive(_currentOrb == _bigOrb);
            _smallOrb.gameObject.SetActive(_currentOrb == _smallOrb);
        }

        private void OnMilestoneCompleted(Technology technology) => Refresh();
    }
}