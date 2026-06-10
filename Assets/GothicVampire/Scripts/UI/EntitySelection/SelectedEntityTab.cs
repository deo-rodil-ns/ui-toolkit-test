using GothicVampire.Player.Inputs.Entity;
using TMPro;
using UnityEngine;

namespace GothicVampire.UI.Entity
{
    public interface ISelectedEntityTabService
    {
        void OpenTab<T>(T entityData);
        void CloseTab();

        EntityType Type { get; }

    }
}
