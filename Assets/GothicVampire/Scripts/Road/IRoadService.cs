using GothicVampire.Player.Inputs.Entity;
using GothicVampire.Roads;
using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Grids
{
    public interface IRoadService
    {
        void PlaceRoad(RoadData newRoad);
        List<SelectableEntity> GetStreet(SelectableEntity entity);
        RoadHighlighter RoadHighlighter { get; }
        RoadTier GetPreplacedRoadTier { get; }
        Vector3 GridToWorldPosition(GridCoord gridPos);
        Grid2D<GridCell> GridCell { get; }
        bool IsPreplacingRoad();

        List<bool> GetRoadConnections(GridCoord coord, bool forRoadHighlights);
    }
}
