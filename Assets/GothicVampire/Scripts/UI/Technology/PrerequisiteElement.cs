using GothicVampire.Technologies;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.Technologies
{
    public class PrerequisiteElement : MonoBehaviour
    {
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private Color _completedColor;
        [SerializeField] private Color _inProgressColor;
        
        public PrerequisiteElementData Data { get; private set; }
        
        public void Initialize(PrerequisiteElementData data)
        {
            Data = data;
            Refresh();
            
            Data?.EvtUpdated.AddListener(OnProgressUpdated);
        }
        
        private void OnDestroy()
        {
            Data?.EvtUpdated.RemoveListener(OnProgressUpdated);
        }

        private void Refresh()
        {
            _descriptionText.text = Data.Description;
            _progressText.text = Data.HasProgression ? $"{Data.CurrentProgress} / {Data.MaxProgress}" : string.Empty;
            _progressText.color = Data.Satisfied ? _completedColor : _inProgressColor;
        }

        private void OnProgressUpdated(PrerequisiteElementData data) => Refresh();
    }
}