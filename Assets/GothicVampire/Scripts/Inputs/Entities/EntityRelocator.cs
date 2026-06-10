using GothicVampire.Grids;
using Sylpheed.Core;
using UnityEngine;

namespace GothicVampire.Player.Inputs.Entity
{
    public class EntityRelocator : MonoBehaviour, IEntityRelocatorService
    {
        private SelectableEntity _curSelectedEntity;
        private IBuildingService _gridPlacementSystem;
        private IEntitySelectorService _entitySelectorService;
        private IWorldVisualEffectsService _worldVisualEffectsService;

        void Awake()
        {
            ServiceLocator.Register<IEntityRelocatorService>(this);
        }

        private void Start()
        {
            _gridPlacementSystem = ServiceLocator.Get<IBuildingService>();
            _entitySelectorService = ServiceLocator.Get<IEntitySelectorService>();
            _worldVisualEffectsService = ServiceLocator.Get<IWorldVisualEffectsService>();
        }

        public void RelocationComplete()
        {
            _worldVisualEffectsService.SetVisualModelsToNormal();
        }

        #region IEntityRelocatorService
        void IEntityRelocatorService.RelocationComplete() => RelocationComplete();
        #endregion
    }
}
