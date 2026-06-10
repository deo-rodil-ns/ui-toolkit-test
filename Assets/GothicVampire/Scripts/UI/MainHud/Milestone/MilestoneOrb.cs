using System;
using GothicVampire.Technologies;
using Sylpheed.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.MainHud
{
    public class MilestoneOrb : MonoBehaviour
    {
        [SerializeField] private Image[] _fills;

        public Technology Milestone { get; private set; }

        public void Initialize(Technology technology)
        {
            Milestone = technology;
            Refresh();
            
            technology.Prerequisites.EvtProgressUpdated.AddListener(OnProgressUpdated);
        }
        
        private void OnDestroy()
        {
            Milestone?.Prerequisites.EvtProgressUpdated.RemoveListener(OnProgressUpdated);
        }

        private void Refresh()
        {
            // Dynamic fill amount only applies to active milestone
            float fillAmount;
            if (Milestone.Unlocked) fillAmount = 1f;
            else if (!Milestone.IsMilestoneActiveOrComplete) fillAmount = 0f;
            else fillAmount = Milestone.Prerequisites.Progress;
            
            _fills.ForEach(f => f.fillAmount = fillAmount);
        }
        
        private void OnProgressUpdated(PrerequisiteGroup prerequisite) => Refresh();
    }
}