using UnityEngine;

namespace GothicVampire.Grids
{
    public interface IBuildingService
    {
        Vector3 GridToWorldPosition(GridCoord gridPos);
        GridCoord WorldToGridPosition(Vector3 worldPos);
        Vector3 RotatedPosition(GridCoord gridPos, Quaternion rotation);
        void RelocateBuilding();
        Vector3 ProjectGridToWorldPosition(GridCoord gridPos, GridCoord size, Quaternion rotation);

        bool IsRelocatingOrPlacingBuilding();
    }
}