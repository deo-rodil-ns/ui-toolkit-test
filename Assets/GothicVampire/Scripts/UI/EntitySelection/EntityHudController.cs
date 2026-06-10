using GothicVampire.Buildings;
using GothicVampire.Buildings.Effects;
using GothicVampire.Grids;
using GothicVampire.Player.Inputs.Entity;
using Sylpheed.Core;
using System;
using GothicVampire.UI.Buildings;
using UnityEngine;
using UnityEngine.Events;
using GothicVampire.Roads;
using GothicVampire.Villagers;
using UnityEngine.UI;

namespace GothicVampire.UI.Entity
{
    public class EntityHudController : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private SimpleBuildingView _simpleBuildingView;
        [SerializeField] private ProductionBuildingView _productionBuildingView;
        [SerializeField] private VillagerBuildingView _villagerBuildingView;
        [SerializeField] private RoadView _roadView;
        [SerializeField] private VillagerInfoView _villagerInfoView;
        
        private ISelectedEntityTabService _curSelectedTabService;
        private IEntitySelectorService _entitySelector;

        private Action _onDeselected;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _entitySelector = ServiceLocator.Get<IEntitySelectorService>() 
                              ?? throw new Exception("EntitySelectorService not found");
            _entitySelector.EvtEntitySelected.AddListener(OnEntitySelected);
            _entitySelector.EvtEntityUnSelected.AddListener(OnEntityDeselected);
        }
        public void OnDestroy()
        {
            _entitySelector?.EvtEntitySelected.RemoveListener(OnEntitySelected);
            _entitySelector?.EvtEntityUnSelected.RemoveListener(OnEntityDeselected);
        }

        private void OnEntitySelected(SelectableEntity entity)
        {
            switch (entity.Type)
            {
                case EntityType.None:
                    break;
                case EntityType.Building:
                    HandleSelectedBuilding(entity);
                    break;
                case EntityType.Terrain:
                    break;
                case EntityType.Road:
                    HandleSelectedRoad(entity);
                    break;
                case EntityType.Villager:
                    //Note: Removed for now while design for villager info UI not complete.
                    //HandleSelectedVillager(entity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleSelectedRoad(SelectableEntity entity)
        {
            var road = entity.GetComponent<Road>();

            if (!road) throw new System.Exception($"A Building/Road does not exist in entity received {entity.gameObject.name}");

            var roadEffect = road.Effect;

            _roadView.Show(roadEffect);
            _onDeselected = () => _roadView.Hide();
        }

        private void HandleSelectedBuilding(SelectableEntity entity)
        {
            var building = entity.GetComponent<Building>();

            if(building != null)
            {
                Button closeButton = null;
                switch (building.Data.HudType)
                {
                    case BuildingHudType.Basic:
                        _simpleBuildingView.Show(building);
                        _onDeselected = () => _simpleBuildingView.Hide();
                        closeButton = _simpleBuildingView.CloseButton;
                        break;
                    case BuildingHudType.Production:
                        _productionBuildingView.Show(building);
                        _onDeselected = () => _productionBuildingView.Hide();
                        closeButton = _productionBuildingView.SimpleBuildingView.CloseButton;
                        break;
                    case BuildingHudType.VillagerHousing:
                        _villagerBuildingView.Show(building);
                        _onDeselected = () => _villagerBuildingView.Hide();
                        closeButton = _villagerBuildingView.SimpleBuildingView.CloseButton;
                        break;
                    default:
                        _simpleBuildingView.Show(building);
                        _onDeselected = () => _simpleBuildingView.Hide();
                        closeButton = _simpleBuildingView.CloseButton;
                        break;
                }

                if (closeButton != null)
                {
                    closeButton.onClick.RemoveAllListeners();
                    closeButton.onClick.AddListener(_onDeselected.Invoke);
                    closeButton.onClick.AddListener(() => _entitySelector.UnselectEntity(entity));
                }
            }
        }

        private void HandleSelectedVillager(SelectableEntity entity)
        {
            var villager = entity.GetComponent<Villager>();
            
            if (!villager) throw new System.Exception($"A Villager does not exist in entity received {entity.gameObject.name}");
            
            _villagerInfoView.Show(villager);
            _onDeselected = () => _villagerInfoView.Hide();
        }
        
        private void OnEntityDeselected()
        {
            _onDeselected?.Invoke();
            _onDeselected = null;
        }
    }
}
