using System;
using System.Linq;
using GothicVampire.WorldEvents.Effects;
using TMPro;
using UnityEngine;

namespace GothicVampire.WorldEvents
{
    public class WorldEventBannerView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _eventNameText;
        [SerializeField] private TextMeshProUGUI _eventDescriptionText;
        [SerializeField] private TextMeshProUGUI _eventDurationText;


        private WorldEvent _onGoingWorldEvent;

        public void Show(WorldEvent worldEvent)
        {
            gameObject.SetActive(true);
            
            _onGoingWorldEvent = worldEvent;
            _eventNameText.text = _onGoingWorldEvent.DisplayName;
            _eventDescriptionText.text = _onGoingWorldEvent.Description;
            _eventDurationText.text = _onGoingWorldEvent.Effects.First().CycleRemaining.ToString() + " Cycle(s) remaining";
        }
        
        public void Hide(WorldEvent worldEvent)
        {
            if (_onGoingWorldEvent != worldEvent) return;
            
            _onGoingWorldEvent = null;

            gameObject.SetActive(false);
        }
    }
}
