using System;
using System.Collections.Generic;
using GothicVampire.Game;
using Sylpheed.Core;
using UnityEngine;

namespace GothicVampire.WorldEvents
{
    public class WorldEventsView : MonoBehaviour
    {
        [SerializeField] private WorldEventEntryElement _template;

        private readonly List<WorldEventEntryElement> _elements = new();
        private Faction _faction;
        private WorldEventManager _worldEventManager;
        
        private void Awake()
        {
            _template.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _faction = ServiceLocator.Get<World>()?.Player ?? throw new Exception("World not yet initialized");
            _worldEventManager = _faction.GetService<WorldEventManager>();
            
            Refresh();
        }

        private void OnDisable()
        {
            
        }
        
        private void Refresh()
        {
            // Clear previous
            _elements.ForEach(e => Destroy(e.gameObject));
            _elements.Clear();
            
            // Display all the next tiers that can be unlocked
            foreach (var entryData in _worldEventManager.WorldEventEntryDatas)
            {
                var element = Instantiate(_template, _template.transform.parent);
                _elements.Add(element);
                element.Show(entryData);
                element.gameObject.SetActive(true);
            }
        }
    }
}
