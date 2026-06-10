using UnityEngine;

namespace GothicVampire.Player.Inputs.Entity
{
    public class RoadVisualInputEffects : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private GameObject _selectedHighlight;

        private SelectableEntity _selectableEntity;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _selectableEntity = GetComponent<SelectableEntity>();

            _selectableEntity.EvtEntitySelected.AddListener(SetToSelectedState);
            _selectableEntity.EvtEntityUnselected.AddListener(SetToNormalState);
        }

        private void SetToSelectedState()
        {
            if(_selectedHighlight == null) return;

            _selectedHighlight.SetActive(true);
        }

        private void SetToNormalState()
        {
            if( _selectedHighlight == null) return;

            _selectedHighlight.SetActive(false);
        }
    }
}
