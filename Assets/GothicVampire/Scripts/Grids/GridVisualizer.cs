using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GothicVampire.Grids
{
    /// <summary>
    /// Displays visible grid cells in the world around the player's mouse cursor.
    /// The further the grid cell is from the center, the lower its color alpha becomes.
    /// </summary>
    [RequireComponent(typeof(GridSystem))]
    public class GridVisualizer : MonoBehaviour
    {
        [SerializeField] private bool _showVisuals = false;
        [SerializeField] private Transform _poolParent;
        [Header("Display Settings")]
        [Tooltip("Prefab for a single grid cell highlight (e.g., a transparent plane).")]
        [SerializeField] private Transform _cellHighlightPrefab;

        [Tooltip("How many cells (in each direction) to show around the cursor.")]
        [SerializeField] private int _visibleRadius = 5;

        [Tooltip("Base color for grid highlights (center tile uses this alpha).")]
        [SerializeField] private Color _baseColor = new Color(0f, 1f, 0f, 0.25f);

        [Tooltip("Minimum alpha at max distance from center.")]
        [Range(0f, 1f)]
        [SerializeField] private float _minAlpha = 0.05f;

        private GridSystem _gridSystem;
        private readonly List<Transform> _pool = new();
        private readonly List<Renderer> _renderers = new();

        private void Start()
        {
            _gridSystem = GetComponent<GridSystem>();

            int poolSize = (_visibleRadius * 2 + 1) * (_visibleRadius * 2 + 1);
            for (int i = 0; i < poolSize; i++)
            {
                var tile = Instantiate(_cellHighlightPrefab, _poolParent);
                var renderer = tile.GetComponentInChildren<Renderer>();

                if (renderer != null)
                {
                    renderer.material = new Material(renderer.material); // make instance
                    renderer.material.color = _baseColor;
                    _renderers.Add(renderer);
                }
                else
                {
                    _renderers.Add(null);
                }

                tile.gameObject.SetActive(false);
                _pool.Add(tile);
            }
        }

        private void Update()
        {
            if (Camera.main == null || _gridSystem.Grid == null || !_showVisuals)
                return;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (_gridSystem.TryGetCoordFromRay(ray, out var centerCoord, out _))
            {
                ShowTilesAround(centerCoord);
            }
            else
            {
                HideAll();
            }
        }

        #region Public Methods

        public void ShowVisuals(bool newState)
        {
            _showVisuals = newState;
            SetTilesVisibility(_showVisuals);
        }

        #endregion

        #region Private Methods

        private void ShowTilesAround(GridCoord center)
        {
            int poolIndex = 0;

            for (int x = -_visibleRadius; x <= _visibleRadius; x++)
            {
                for (int y = -_visibleRadius; y <= _visibleRadius; y++)
                {
                    if (poolIndex >= _pool.Count)
                        return;

                    var coord = new GridCoord(center.x + x, center.y + y);
                    if (_gridSystem.Grid.InBounds(coord))
                    {
                        float distance = Mathf.Sqrt(x * x + y * y);
                        float t = Mathf.InverseLerp(0, _visibleRadius, distance);
                        float alpha = Mathf.Lerp(_baseColor.a, _minAlpha, t);

                        var tile = _pool[poolIndex];
                        var renderer = _renderers[poolIndex];

                        tile.position = _gridSystem.Grid.GetWorldCenter(coord);
                        tile.localScale = Vector3.one * _gridSystem.Grid.CellSize * 0.98f;

                        if (renderer != null)
                        {
                            Color c = _baseColor;
                            c.a = alpha;
                            renderer.material.color = c;
                        }

                        tile.gameObject.SetActive(true);
                    }
                    else
                    {
                        _pool[poolIndex].gameObject.SetActive(false);
                    }

                    poolIndex++;
                }
            }
        }

        private void SetTilesVisibility(bool newVisibility)
        {
            foreach (var tile in _pool)
            {
                tile.gameObject.SetActive(newVisibility);
            }
        }

        private void HideAll()
        {
            foreach (var tile in _pool)
                tile.gameObject.SetActive(false);
        }

        #endregion
    }
}
