using GothicVampire.Grids;
using Sylpheed.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GothicVampire.Roads
{
    public class RoadHighlight : MonoBehaviour
    {
        #region Events

        #endregion

        [SerializeField] private GameObject _greenHighlight, _redHighlight;
        [SerializeField] private GameObject _roadMaterial;
        [SerializeField] private RoadMaterialSystem _roadMaterialSystem;

        public RoadMaterialSystem RoadMaterialSystem => _roadMaterialSystem;

        public Vector3 position
        {
            get
            {
                return this.transform.position;
            }
            set
            {
                this.transform.position = value;
            }
        }

        private void Start()
        {

        }

        private void OnDestroy()
        {

        }

        #region Public Methods

        public void DisableHighlights()
        {
            _greenHighlight.SetActive(false);
            _redHighlight.SetActive(false);
            _roadMaterial.SetActive(false);
        }

        public void HighlightGreen(bool isOccupied)
        {
            _greenHighlight.SetActive(!isOccupied);
            _redHighlight.SetActive(isOccupied);

            if (!_roadMaterial.activeSelf)
            {
                _roadMaterial.SetActive(true);
            }
        }

        #endregion
    }
}
