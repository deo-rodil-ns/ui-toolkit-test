using UnityEngine;

namespace GothicVampire.Player.Inputs.Entity
{
    public interface IWorldVisualEffectsService
    {
        void SetVisualModelsToGhost();
        void SetVisualModelsToNormal();

        void RegisterModel(ModelVisualInputEffects modelVisual);
        void ResignModel(ModelVisualInputEffects modelVisual);
    }
}
