using Sylpheed.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GothicVampire.Player.Inputs.Entity
{
    public class WorldVisualEffects : MonoBehaviour, IWorldVisualEffectsService
    {
        private List<ModelVisualInputEffects> _existingModelVisualEffects = new List<ModelVisualInputEffects>();
        private IEntitySelectorService _entitySelectorService;

        private UnityEvent _evtSetAllVisualModelsToGhost { get; } = new();
        private UnityEvent _evtSetAllVisualModelsToNormal { get; } = new();

        void Awake()
        {
            ServiceLocator.Register<IWorldVisualEffectsService>(this);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _entitySelectorService = ServiceLocator.Get<IEntitySelectorService>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void RegisterModel(ModelVisualInputEffects modelVisual)
        {
            _existingModelVisualEffects.Add(modelVisual);
            _evtSetAllVisualModelsToGhost.AddListener(modelVisual.SetToGhostState);
            _evtSetAllVisualModelsToNormal.AddListener(modelVisual.SetModelToNormal);
        }

        public void ResignModel(ModelVisualInputEffects modelVisual)
        {
            _evtSetAllVisualModelsToGhost.RemoveListener(modelVisual.SetToGhostState);
            _evtSetAllVisualModelsToNormal.RemoveListener(modelVisual.SetModelToNormal);
        }

        public void SetVisualModelsToGhost()
        {
            _evtSetAllVisualModelsToGhost.Invoke();
        }

        public void SetVisualModelsToNormal()
        {
            _evtSetAllVisualModelsToNormal.Invoke();
        }

        #region IWorldVisualEffectsService

        void IWorldVisualEffectsService.SetVisualModelsToGhost() => SetVisualModelsToGhost();
        void IWorldVisualEffectsService.SetVisualModelsToNormal() => SetVisualModelsToNormal();
        void IWorldVisualEffectsService.RegisterModel(ModelVisualInputEffects modelVisual) => RegisterModel(modelVisual);
        void IWorldVisualEffectsService.ResignModel(ModelVisualInputEffects modelVisual) => ResignModel(modelVisual);

        #endregion
    }
}
