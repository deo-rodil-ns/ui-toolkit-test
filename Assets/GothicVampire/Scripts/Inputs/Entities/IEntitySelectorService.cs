using System.Collections.Generic;
using UnityEngine.Events;

namespace GothicVampire.Player.Inputs.Entity
{
    public interface IEntitySelectorService
    {
        UnityEvent<SelectableEntity> EvtEntitySelected { get; }
        UnityEvent EvtDoubleClickedUpdate { get; }
        UnityEvent EvtEntityUnSelected { get; }
        SelectableEntity GetSelectedEntity();
        IReadOnlyList<SelectableEntity> GetSelectedEntities { get; }
        void RegisterEntity(SelectableEntity entity);
        void ResignEntity(SelectableEntity entity);
        void SelectEntity(SelectableEntity entity);
        void UnselectEntity(SelectableEntity entity);
        void DoubleClicked(SelectableEntity entity);
        void UnselectCurrentEntity();
    }
}
