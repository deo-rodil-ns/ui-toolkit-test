using GothicVampire.Grids;
using Sylpheed.Core;
using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Roads
{
    /// <summary>
    /// Handles assigning the correct material (and optionally rotation) to a road tile
    /// based on which directions it connects to. Works both for preview (highlight) and placed roads.
    /// </summary>
    public class RoadMaterialSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MeshRenderer _meshRenderer;

        [Header("Settings")]
        [Tooltip("When true, the system checks road connections from the active road highlighter instead of placed roads.")]
        [SerializeField] private bool _isHighlightMaterial;

        [Tooltip("The current road tier which defines the set of possible materials.")]
        private RoadTier _curRoadTier;
        private RoadData _data;

        private IRoadService _roadService;

        [Header("Road Connections")]
        [SerializeField] private bool _south;
        [SerializeField] private bool _north, _east, _west;

        private Grid2D<GridCell> _grid;

        // ───────────────────────────────────────────────────────────────
        // Initialization
        // ───────────────────────────────────────────────────────────────
        private void Start()
        {
            if (_data != null)
            {
                _curRoadTier = _data.Tiers[0];
            }
        }

        private void Reset()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
        private void Update()
        {
            SetConnectionMask();
        }

        /// <summary>
        /// Assigns the tier configuration for this road (determines which materials to use).
        /// </summary>
        public void Initialize(RoadTier tier)
        {
            _curRoadTier = tier;

            _roadService = ServiceLocator.Get<IRoadService>();

            if (_roadService != null)
            {
                _grid = _roadService.GridCell;
            }
        }

        /// <summary>
        /// Called when the road grid updates or is placed.
        /// Determines the connection mask and applies the appropriate material.
        /// </summary>
        public void UpdateRoadMaterial(GridCoord coord)
        {
            if (_curRoadTier == null || _meshRenderer == null)
                return;

            SetConnections(coord);
            SetConnectionMask();
        }

        /// <summary>
        /// Checks all 4 directions and sets which sides are connected to another road.
        /// </summary>
        private void SetConnections(GridCoord coord)
        {
            var connections = _roadService.GetRoadConnections(coord, _isHighlightMaterial);

            _west  = connections[0];
            _north = connections[1];
            _east  = connections[2];
            _south = connections[3];
        }

        /// <summary>
        /// Chooses the proper material from the RoadTier
        /// based on which sides are connected.
        /// </summary>
        private void SetConnectionMask()
        {
            if (_curRoadTier == null || _meshRenderer == null)
                return;

            RoadConnection selectedConnection = RoadConnection.NorthSouth; // default fallback

            // 4-way intersection (Cross)
            if (_north && _south && _east && _west)
            {
                selectedConnection = RoadConnection.Cross; // pick one representative
            }
            // 3-way intersections
            else if (_north && _south && _east && !_west)
            {
                selectedConnection = RoadConnection.NorthSouthEast;
            }
            else if (_north && _south && _west && !_east)
            {
                selectedConnection = RoadConnection.NorthSouthWest;
            }
            else if (_east && _west && _north && !_south)
            {
                selectedConnection = RoadConnection.EastWestNorth;
            }
            else if (_east && _west && _south && !_north)
            {
                selectedConnection = RoadConnection.EastWestSouth;
            }
            // 2-way straights
            else if (_north && _south && !_east && !_west ||
                 (_north || _south) && !_east && !_west)
            {
                selectedConnection = RoadConnection.NorthSouth;
            }
            else if (_east && _west && !_north && !_south ||
                (_east || _west) && !_north && !_south)
            {
                selectedConnection = RoadConnection.EastWest;
            }
            // 2-way corners
            else if (_north && _east && !_south && !_west)
            {
                selectedConnection = RoadConnection.NorthEast;
            }
            else if (_north && _west && !_south && !_east)
            {
                selectedConnection = RoadConnection.NorthWest;
            }
            else if (_south && _east && !_north && !_west)
            {
                selectedConnection = RoadConnection.SouthEast;
            }
            else if (_south && _west && !_north && !_east)
            {
                selectedConnection = RoadConnection.SouthWest;
            }
            else
            {
                // Default or isolated tile (can use NorthSouth)
                selectedConnection = RoadConnection.NorthSouth;
            }

            // Fetch corresponding material from tier
            Material targetMaterial = _curRoadTier.GetMaterial(selectedConnection);

            if (targetMaterial != null)
            {
                _meshRenderer.sharedMaterial = targetMaterial;
            }
            else
            {
                Debug.LogWarning($"[RoadMaterialSystem] No material found for connection {selectedConnection} in {name}");
            }
        }


        public void ResetConnection()
        {
            _north = false;
            _south = false;
            _east = false;
            _west = false;
        }
    }
}
