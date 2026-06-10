using GothicVampire.CodeBasedAnimators;
using UnityEngine;

namespace GothicVampire.Buildings
{
    [RequireComponent(typeof(Building))]
    public class BuildingVisualEffects : MonoBehaviour
    {
        #region Inspector
        [SerializeField] private float _duration = 0.4f;
        #endregion

        private Building _building;

        private void Awake()
        {
            _building = GetComponent<Building>();
        }

        public void Bounce()
        {
            ScaleAnimator scaleAnimator = new(_building.Model.transform);
            scaleAnimator.StartBounce(_building.Model.transform.localScale, _duration, false, this.destroyCancellationToken);
        }
    }
}
