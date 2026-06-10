using TMPro;
using UnityEngine;

namespace GothicVampire.WorldEvents
{
    public class WorldEventEntryElement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _eventNameText;
        [SerializeField] private TextMeshProUGUI _categoryText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _cycleSinceNumberText;
        
        public void Show(WorldEventResult entryData)
        {
            _eventNameText.text = entryData.EventName;
            _categoryText.text = entryData.Category;
            _descriptionText.text = entryData.Description;
            
            _cycleSinceNumberText.text = entryData.Cycle.ToString();
        }
    }
}
