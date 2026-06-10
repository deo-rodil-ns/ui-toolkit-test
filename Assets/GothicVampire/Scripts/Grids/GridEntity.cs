using GothicVampire.Player.Inputs.Entity;
using UnityEngine;

namespace GothicVampire.Grids
{
    public class GridEntity : MonoBehaviour
    {
        [SerializeField] private EntityType _type;
        private GridCoord _gridPos;
        private Quaternion _rotation;

        public EntityType Type => _type;

        // These services are optional if you want auto-world updates
        private IBuildingService _gridService;

        private void Awake()
        {
            _gridService = Sylpheed.Core.ServiceLocator.Get<IBuildingService>();
        }

        public GridCoord GridPos
        {
            get => _gridPos;
            set
            {
                _gridPos = value;
                UpdateWorldTransform();
            }
        }

        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                UpdateWorldTransform();
            }
        }

        public void Initialize(GridCoord gridPos, Quaternion rotation)
        {
            _gridPos = gridPos;
            _rotation = rotation;
            UpdateWorldTransform();
        }

        private void UpdateWorldTransform()
        {
            if (_gridService == null || _gridService.GridToWorldPosition(_gridPos) == null)
                return;

            // Convert grid position to world center
            Vector3 worldPos = _gridService.ProjectGridToWorldPosition(
                _gridPos,
                new GridCoord(1, 1),
                _rotation
            );

            transform.position = worldPos;
            transform.rotation = _rotation;
        }
    }
}
