using System;
using GothicVampire.Technologies;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class MilestonePrerequisiteElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private Color _completedColor;
        [SerializeField] private Color _inProgressColor;
        
        public Prerequisite Prerequisite { get; private set; }
        
        public void Initialize(Prerequisite prerequisite)
        {
            Prerequisite = prerequisite;
            Refresh();
            
            prerequisite.EvtProgressUpdated.AddListener(OnProgressUpdated);
        }
        
        private void OnDestroy()
        {
            Prerequisite?.EvtProgressUpdated.RemoveListener(OnProgressUpdated);
        }

        private void Refresh()
        {
            _descriptionText.text = Prerequisite.Description;
            _progressText.text = Prerequisite.HasProgression ? $"{Prerequisite.CurrentProgress} / {Prerequisite.MaxProgress}" : string.Empty;
            _progressText.color = Prerequisite.Satisfied ? _completedColor : _inProgressColor;
        }

        private void OnProgressUpdated(Prerequisite prerequisite) => Refresh();
    }
}