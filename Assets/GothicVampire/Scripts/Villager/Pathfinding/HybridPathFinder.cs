using System.Collections.Generic;
using UnityEngine;

namespace GothicVampire.Grids
{
    public struct PathResult
    {
        public List<object> Path;
        public float Cost;
    }

    public static class HybridPathfinder
    {
        public static PathResult FindPath(GridSystem grid, GridCoord startCell, GridCoord goalCell)
        {
            object startNode = startCell; // Start in center
            object goalNode = FindNearestWalkable(grid, goalCell);   // Goal is center

            var openSet = new List<HybridPathNode>();
            var closedSet = new HashSet<object>();

            openSet.Add(new HybridPathNode
            {
                Node = startNode,
                G = 0,
                H = Heuristic(startNode, goalNode)
            });

            while (openSet.Count > 0)
            {
                openSet.Sort((a, b) => a.F.CompareTo(b.F));
                HybridPathNode current = openSet[0];

                if (NodesEqual(current.Node, goalNode))
                {
                    return new PathResult
                    {
                        Path = ReconstructPath(current),
                        Cost = current.G,
                    };
                }

                openSet.RemoveAt(0);
                closedSet.Add(current.Node);

                foreach (var neighbor in grid.GetHybridNeighbors(current.Node))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    float tentativeG = current.G + GetNodeCost(grid, neighbor);

                    // Check if neighbor is already in openSet
                    HybridPathNode existing =
                        openSet.Find(n => NodesEqual(n.Node, neighbor));

                    if (existing == null)
                    {
                        openSet.Add(new HybridPathNode
                        {
                            Node = neighbor,
                            Parent = current,
                            G = tentativeG,
                            H = Heuristic(neighbor, goalNode)
                        });
                    }
                    else if (tentativeG < existing.G)
                    {
                        existing.G = tentativeG;
                        existing.Parent = current;
                    }
                }
            }

            // No valid path
            return new PathResult { };
        }

        // Temporary, Wait until we have access to "Door" positions for buildings.
        public static GridCoord FindNearestWalkable(GridSystem grid, GridCoord target)
        {
            // If target itself is walkable, return immediately
            if (grid.Grid.InBounds(target))
            {
                var c = grid.Grid.Get(target);
                if (IsWalkable(grid,target))
                    return target;
            }

            // BFS-style outward search
            Queue<GridCoord> queue = new Queue<GridCoord>();
            HashSet<GridCoord> visited = new HashSet<GridCoord>();

            queue.Enqueue(target);
            visited.Add(target);

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (queue.Count > 0)
            {
                GridCoord cur = queue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    GridCoord next = new GridCoord(cur.x + dx[i], cur.y + dy[i]);

                    if (!grid.Grid.InBounds(next) || visited.Contains(next))
                        continue;

                    visited.Add(next);
                    queue.Enqueue(next);

                    var cell = grid.Grid.Get(next);
                    if (IsWalkable(grid, next))
                        return next;
                }
            }

            // If nothing is walkable (very rare)
            return target;
        }

        public static bool IsWalkable(GridSystem grid, GridCoord target)
        {
            var cell = grid.Grid.Get(target);

            if(cell == null) return false;

            if (cell.Occupant != null) {
                if (cell.OccupantEntity == Player.Inputs.Entity.EntityType.Road)
                    return true;
                else
                    return false;
            }

            return true;
        }

        private static float Heuristic(object a, object b)
        {
            GridCoord ca = (a is GridCoord gcA) ? gcA : ((EdgeNode)a).Cell;
            GridCoord cb = (b is GridCoord gcB) ? gcB : ((EdgeNode)b).Cell;

            return Mathf.Abs(ca.x - cb.x) + Mathf.Abs(ca.y - cb.y);
        }

        private static float GetNodeCost(GridSystem grid, object node)
        {
            if (node is GridCoord center)
            {
                // Center nodes = cheapest
                var cell = grid.Grid.Get(center);
                // Roads reduce cost
                if (cell.OccupantEntity == Player.Inputs.Entity.EntityType.Road)
                    return 0.05f;
                else
                {
                    return 1f;
                }
            }

            if (node is EdgeNode edge)
            {
                var cell = grid.Grid.Get(edge.Cell);

                float cost = 2f; // higher so A* uses edges only when needed

                // Roads reduce cost
                if (cell.OccupantEntity == Player.Inputs.Entity.EntityType.Road)
                    cost = 0.15f;

                // Adjacent obstacle penalty (edge hugging)
                cost += cell.AdjacentObstacleCount * 0.5f;

                return cost;
            }

            return 1f;
        }

        //   NODE COMPARISON
        private static bool NodesEqual(object a, object b)
        {
            if (a is GridCoord g1 && b is GridCoord g2)
                return g1.Equals(g2);

            if (a is EdgeNode e1 && b is EdgeNode e2)
                return e1.Equals(e2);

            return false;
        }

        //   PATH RECONSTRUCTION
        private static List<object> ReconstructPath(HybridPathNode end)
        {
            List<object> path = new();

            HybridPathNode cur = end;
            while (cur != null)
            {
                path.Add(cur.Node);
                cur = cur.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}
