using GothicVampire.Buildings;
using GothicVampire.Game;
using Sylpheed.Core;
using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.UI.MainHud
{
    public class BuildingConstructionHud : MonoBehaviour
    {
        #region Inspector
        [Header("References")]
        [SerializeField] private BuildingConstructionButton _template;
        #endregion

        private BuildingManager _buildingManager;

        private readonly List<BuildingConstructionButton> _elements = new();

        private void Awake()
        {
            _template.gameObject.SetActive(false);
        }

        private void Start()
        {
            _buildingManager = ServiceLocator.Get<World>().Player.GetService<BuildingManager>();

            CreateElements();
        }

        private void CreateElements()
        {
            _elements.ForEach(e => Destroy(e.gameObject));
            _elements.Clear();
            
            foreach (var data in _buildingManager.Settings.PurchasableBuildings)
            {
                var element = Instantiate(_template, _template.transform.parent);
                element.Initialize(data, _buildingManager.Faction);
                element.gameObject.SetActive(true);
                element.name = $"Element - {data.DisplayName}";
                _elements.Add(element);
            }
        }
    }
}
