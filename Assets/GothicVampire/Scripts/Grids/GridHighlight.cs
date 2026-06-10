using UnityEngine;

namespace GothicVampire.Grids
{
    /// <summary>
    /// Temporary holds visual cues for highlight which is buildable and which is not.
    /// </summary>
    public class GridHighlight : MonoBehaviour
    {
        [SerializeField]private GameObject _greenHighlight, _redHighlight;

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

        public void Start()
        {

        }

        #region Public Methods

        public void DisableHighlights()
        {
            _greenHighlight.SetActive(false);
            _redHighlight.SetActive(false);
        }

        public void HighlightGreen(bool isOccupied)
        {
            _greenHighlight.SetActive(!isOccupied);
            _redHighlight.SetActive(isOccupied);
        }

        #endregion
    }
}
