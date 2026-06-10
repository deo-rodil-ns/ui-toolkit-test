using System;
using GothicVampire.Buildings;
using GothicVampire.Game;
using GothicVampire.Technologies;
using Sylpheed.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GothicVampire.UI.BuildingProgression
{
    public class BuildingProgressionView : MonoBehaviour
    {
        [SerializeField] private BuildingProgressionElement _template;

        private readonly List<BuildingProgressionElement> _elements = new();
        private TechnologyManager _technologyManager;
        private BuildingManager _buildingManager;
        private Faction _faction;

        private void Awake()
        {
            _template.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _faction = ServiceLocator.Get<World>()?.Player ?? throw new Exception("World not yet initialized");
            _buildingManager = _faction.GetService<BuildingManager>();
            _technologyManager = _faction.GetService<TechnologyManager>();

            gameObject.SetActive(true);
            Refresh();

            _technologyManager.EvtUpdated.AddListener(Refresh);
        }

        private void OnDisable()
        {
            _technologyManager?.EvtUpdated.RemoveListener(Refresh);
        }

        private void Refresh()
        {
            // Clear previous
            _elements.ForEach(e => Destroy(e.gameObject));
            _elements.Clear();

            // Display all the next tiers that can be unlocked
            foreach (var tier in _buildingManager.GetNextLockedTiers(true).OrderBy(d => d.DisplayName))
            {
                var element = Instantiate(_template, _template.transform.parent);
                _elements.Add(element);
                element.Show(tier, _faction);
                element.gameObject.SetActive(true);
            }
        }
    }
}