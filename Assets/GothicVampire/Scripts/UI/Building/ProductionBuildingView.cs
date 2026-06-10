using System;
using System.Collections.Generic;
using System.Linq;
using GothicVampire.Buildings;
using GothicVampire.Buildings.Effects;
using GothicVampire.Currencies;
using GothicVampire.Grids;
using GothicVampire.UI.Currencies;
using Sylpheed.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GothicVampire.UI.Buildings
{
    public class ProductionBuildingView : MonoBehaviour
    {
        [Header("Subviews")] 
        [SerializeField] private SimpleBuildingView _simpleBuildingView;
        [SerializeField] private BuildingJobAssignmentView _jobAssignmentView;
        [SerializeField] private GameObject _buffsPanel;
        
        [Header("Production")]
        [SerializeField] private CurrencyListView _productionInput;
        [SerializeField] private CurrencyListView _productionOutput;
        [SerializeField] private CurrencyListView _upkeep;

        public SimpleBuildingView SimpleBuildingView => _simpleBuildingView;
        private Building _building;
        private AddProduction _effect;

        public void Show(Building building)
        {
            _building = building;
            _effect = building.Effect as AddProduction;
            
            // Disable irrelevant views
            _buffsPanel.SetActive(false); // TODO: Temporarily disabled
            if (_upkeep) _upkeep.gameObject.SetActive(false);
            
            _simpleBuildingView.Show(_building);
            _jobAssignmentView.Show(_building.JobAssignment);
            UpdateProduction(_effect);
            
            _building.EvtTierUpdated?.AddListener(OnTierUpdated);
            _effect?.EvtProjectionUpdated.AddListener(UpdateProduction);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _building?.EvtTierUpdated?.RemoveListener(OnTierUpdated);
            _building = null;
            
            _simpleBuildingView.Hide();
            _jobAssignmentView.Hide();
            
            gameObject.SetActive(false);
        }

        private void UpdateProduction(AddProduction effect)
        {
            if (!_building) return; // TODO: Hack. Seems like an event is still firing when building is destroyed.
            if (!_building.Construction.Ready) return;

            var input = effect?.InputProjection ?? new List<Currency>();
            var output = effect?.OutputProjection ?? new List<Currency>();
            var inputPerJob = effect?.InputPerJob ?? new List<Currency>();
            var outputPerJob = effect?.OutputPerJob ?? new List<Currency>();
            
            if (inputPerJob.Any()) _productionInput.Show(input);
            else _productionInput.Hide();
            
            if (outputPerJob.Any()) _productionOutput.Show(output);
            else _productionOutput.Hide();
        }
        
        private void OnTierUpdated(Building building)
        {
            _effect?.EvtProjectionUpdated.RemoveListener(UpdateProduction);
            _effect = building.Effect as AddProduction;
            _effect?.EvtProjectionUpdated.AddListener(UpdateProduction);
            UpdateProduction(_effect);
        }
    }
}