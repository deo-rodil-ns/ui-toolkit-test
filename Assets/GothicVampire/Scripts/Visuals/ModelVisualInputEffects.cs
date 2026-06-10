using NUnit.Framework;
using Sylpheed.Core;
using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Player.Inputs.Entity
{
    // ToDo: Might change _selectedColor from solid green to highlight
    public class ModelVisualInputEffects : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private EntityType _entityType;

        [Header("Model Rotation Offset")]
        [Tooltip("Used while we dont have the models, adjusts the cubes position depending on rotation")]
        [SerializeField] private Vector3 _negativeRotOffset;
        [SerializeField] private Vector3 _positiveRotOffset;
        [Header("Arrow Rotation Offset")]
        [Tooltip("Used while we dont have the models, adjusts the cubes position while relocating")]
        [SerializeField] private Vector3 _relocateOffset;
        [SerializeField] private Vector3 _relocateArrowOffset;

        [Header("Local References")]
        [SerializeField] private GameObject _model;
        [SerializeField] private GameObject _arrowIndicator;
        [SerializeField] private List<Transform> _doors;
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Renderer[] _disablingRenderers;

        [Header("Global References")]
        [SerializeField] private Material _ghostMaterial;
        [SerializeField] private Material _selectedMaterial;
        [SerializeField] private Material _relocatingMaterial;

        public List<Transform> Doors => _doors;
        
        private Material[] _defaultMaterials;

        private IWorldVisualEffectsService _worldVisualEffectsService;
        private IEntitySelectorService _entitySelectorService;
        private SelectableEntity _selectableEntity;

        private Vector3 _originalOffset;

        private void Awake()
        {
            if (_arrowIndicator != null) _arrowIndicator.SetActive(false);
        }

        private void Start()
        {
            _worldVisualEffectsService = ServiceLocator.Get<IWorldVisualEffectsService>();
            _entitySelectorService = ServiceLocator.Get<IEntitySelectorService>();

            if (_worldVisualEffectsService != null)
            {
                _worldVisualEffectsService.RegisterModel(this);
            }

            Initialize();

            _defaultMaterials = new Material[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _defaultMaterials[i] = _renderers[i].material;
            }

            _originalOffset = _model.transform.localPosition;
        }

        private void Initialize()
        {
            _selectableEntity = GetComponentInParent<SelectableEntity>();

            if (_selectableEntity == null) return;

            _selectableEntity.EvtEntitySelected.AddListener(SetToSelectedState);
            _selectableEntity.EvtEntityUnselected.AddListener(SetToNormalState);

            _selectableEntity.EvtEntityRelocating.AddListener(SetModelToRelocating);
            _selectableEntity.EvtEntityRelocated.AddListener(SetModelToNormal);

            UpdateModelOffsetPosition();
        }

        public void SetToSelectedState()
        {
            SetMaterial(_selectedMaterial);

            if (_arrowIndicator != null)
            {
                _arrowIndicator.SetActive(false);
            }
        }

        public void SetToRelocatingState()
        {
            SetMaterial(_relocatingMaterial);
            if (_arrowIndicator != null)
            {
                _arrowIndicator.SetActive(true);
            }

            SetDisablingRenderers(false);
        }

        public void SetToGhostState()
        {
            var selectedEntity = _entitySelectorService.GetSelectedEntity()?.gameObject;
            if (selectedEntity != null && selectedEntity == transform.parent?.gameObject) return;
            SetMaterial(_ghostMaterial);

            SetDisablingRenderers(false);
        }

        public void SetToNormalState()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].material = _defaultMaterials[i];
            }

            if (_arrowIndicator != null)
            {
                _arrowIndicator.SetActive(false);
            }

            SetDisablingRenderers(true);
        }

        public void SetModelToNormal()
        {
            SetToNormalState();
            UpdateModelOffsetPosition();
        }

        public void SetModelToRelocating()
        {
            SetToRelocatingState();
            UpdateModelRelocateOffsetPosition();
        }

        #region Private Methods

        private void UpdateModelRelocateOffsetPosition()
        {
            if (_selectableEntity == null) return;

            _model.transform.localPosition = _relocateOffset;
            _arrowIndicator.transform.localPosition = _relocateArrowOffset;
        }

        private void UpdateModelOffsetPosition()
        {
            if (_selectableEntity == null) return;

            // Negative
            if(_selectableEntity.transform.rotation.eulerAngles.y > 0)
            {
                _model.transform.localPosition = _negativeRotOffset;
            }
            else
            {
                _model.transform.localPosition = _positiveRotOffset;
            }
        }

        private void OnDestroy()
        {
            _worldVisualEffectsService.ResignModel(this);
        }

        private void SetMaterial(Material material)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].material = material;
            }
        }

        private void SetDisablingRenderers(bool enable)
        {
            for (int i = 0; i < _disablingRenderers.Length; i++)
            {
                _disablingRenderers[i].gameObject.SetActive(enable);
            }
        }

        #endregion
    }
}
