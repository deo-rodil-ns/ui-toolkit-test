using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Buildings;
using GothicVampire.Game;
using UnityEngine;

namespace GothicVampire.UI.BuildingProgression
{
    public class UnlockableBuildingListView : MonoBehaviour
    {
        [SerializeField] private UnlockableBuildingElement _template;
        [SerializeField] private GameObject _hasUnlockableIndicator;

        private readonly List<UnlockableBuildingElement> _elements = new();

        private void Awake()
        {
            _template.gameObject.SetActive(false);
            _hasUnlockableIndicator.SetActive(false);
        }

        public void Show(BuildingTier tier, Faction faction)
        {
            var buildingManager = faction.GetService<BuildingManager>();
            
            // Clear previous
            _elements.ForEach(e => Destroy(e.gameObject));
            _elements.Clear();

            // Create elements
            foreach (var unlockable in tier.UnlockablesOnBuild)
            {
                // Only list this unlockable if it is a building tier.
                var buildingTier = unlockable as BuildingTier;
                if (buildingTier == null) continue;
                
                var element = Instantiate(_template, _template.transform.parent);
                _elements.Add(element);
                element.Show(buildingTier);
                element.gameObject.SetActive(true);
            }
            
            // Indicator
            _hasUnlockableIndicator.SetActive(tier.UnlockablesOnBuild.Any());
        }
    }
}