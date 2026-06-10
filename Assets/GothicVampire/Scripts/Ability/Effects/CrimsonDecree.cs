using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GothicVampire.Buildings;
using GothicVampire.Buildings.Effects;
using GothicVampire.Currencies;
using GothicVampire.Game;
using GothicVampire.Productions;
using Sylpheed.Extensions;
using UnityEngine;

namespace GothicVampire.Abilities.Effects
{
    [Serializable]
    public sealed class CrimsonDecree : AbilityEffect
    {
        [SerializeField] private float _bonus = 1f;
        [SerializeField] private float _duration;
        
        // TODO: Create a dedicated buff system. Remove this later
        public bool Active { get; private set; }
        // TODO: Hack for now. Only works if time scale isn't being changed.
        public float EffectTimeRemaining
        {
            get
            {
                if (!Active) return 0f;
                var expiryTime = _timeActivated + _duration;
                return expiryTime - Time.realtimeSinceStartup;
            }
        }

        private readonly Dictionary<Building, ProductionOrder> _orderMap = new();
        private BuildingManager _buildingManager;
        private ProductionManager _productionManager;
        private CancellationTokenSource _cts = new();
        private float _timeActivated;
        
        protected override void OnActivate(Ability ability, ITargetable target)
        {
            if (target is not Faction faction) throw new Exception("Invalid target");
            _buildingManager = faction.GetService<BuildingManager>() ?? throw new Exception("No BuildingManager");
            _productionManager = faction.GetService<ProductionManager>() ?? throw new Exception("No ProductionManager");

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            ActivateBuff(_cts.Token).Forget();
        }

        ~CrimsonDecree()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private async UniTaskVoid ActivateBuff(CancellationToken cancellationToken)
        {
            try
            {
                Active = true;
                _timeActivated = Time.realtimeSinceStartup;
                
                // Create orders from production buildings
                _buildingManager.Buildings.ForEach(b => CreateOrder(b));
            
                // React to building changes (add, remove, upgrade)
                _buildingManager.EvtBuildingConstructed.AddListener(OnBuildingConstructed);
                _buildingManager.EvtBuildingUpgraded.AddListener(OnBuildingUpgraded);
                _buildingManager.EvtBuildingRemoved.AddListener(OnBuildingRemoved);
                
                // TODO: Implemented as Unitask for now. Use a dedicated Buff system later.
                await UniTask.WaitForSeconds(_duration, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                Active = false;
                
                // Remove all orders
                _orderMap.Keys.ToList().ForEach(RemoveOrder);
                _orderMap.Clear();
                
                // Remove listeners
                _buildingManager?.EvtBuildingConstructed.RemoveListener(OnBuildingConstructed);
                _buildingManager?.EvtBuildingUpgraded.RemoveListener(OnBuildingUpgraded);
                _buildingManager?.EvtBuildingRemoved.RemoveListener(OnBuildingRemoved);
            }
        }
        
        private ProductionOrder CreateOrder(Building building)
        {
            if (building.Effect is not AddProduction effect) return null;

            var order = effect.Order.Clone();
            order.InputBase = new List<Currency>();
            order.OutputBase = order.OutputBase.Select(c => c.WithValue(_bonus)).ToList();
            
            _productionManager.AddOrder(order);
            _orderMap.Add(building, order);
            
            return order;
        }

        private void RemoveOrder(Building building)
        {
            if (!_orderMap.Remove(building, out var order)) return;
            _productionManager.RemoveOrder(order);
            _orderMap.Remove(building);
        }
        
        private void OnBuildingConstructed(Building building) => CreateOrder(building);
        private void OnBuildingRemoved(Building building) => RemoveOrder(building);
        private void OnBuildingUpgraded(Building building)
        {
            // Replace order for the building
            RemoveOrder(building);
            CreateOrder(building);
        }
    }
}