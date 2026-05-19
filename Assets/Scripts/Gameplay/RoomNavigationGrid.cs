using System.Collections.Generic;
using ShadowDash.Enemies;
using ShadowDash.Player;
using UnityEngine;

namespace ShadowDash.Gameplay
{
    public sealed class RoomNavigationGrid : MonoBehaviour
    {
        [SerializeField] private Vector2 gridCenter = Vector2.zero;
        [SerializeField] private Vector2 gridSize = new Vector2(18f, 12f);
        [SerializeField] private float cellSize = 0.75f;
        [SerializeField] private LayerMask obstacleMask = ~0;

        private readonly List<Node> openNodes = new List<Node>();
        private readonly List<Vector2Int> neighborOffsets = new List<Vector2Int>
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        private Node[,] nodes;
        private int width;
        private int height;
        private Vector2 origin;

        private void Awake()
        {
            BuildGrid();
        }

        public bool HasLineOfSight(Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            float distance = delta.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return true;
            }

            RaycastHit2D[] hits = Physics2D.RaycastAll(from, delta / distance, distance, obstacleMask);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hitCollider = hits[i].collider;
                if (IsBlockingCollider(hitCollider))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryFindPath(Vector2 start, Vector2 goal, List<Vector2> path)
        {
            path.Clear();

            if (nodes == null || nodes.Length == 0)
            {
                return false;
            }

            Node startNode = FindNearestWalkable(WorldToGrid(start));
            Node goalNode = FindNearestWalkable(WorldToGrid(goal));
            if (startNode == null || goalNode == null)
            {
                return false;
            }

            ResetSearch();
            openNodes.Clear();

            startNode.gCost = 0f;
            startNode.hCost = Heuristic(startNode, goalNode);
            openNodes.Add(startNode);

            while (openNodes.Count > 0)
            {
                Node current = TakeBestOpenNode();
                current.closed = true;

                if (current == goalNode)
                {
                    BuildPath(goalNode, path);
                    return path.Count > 0;
                }

                for (int i = 0; i < neighborOffsets.Count; i++)
                {
                    Node neighbor = GetNeighbor(current, neighborOffsets[i]);
                    if (neighbor == null || !neighbor.walkable || neighbor.closed)
                    {
                        continue;
                    }

                    if (IsDiagonalMove(neighborOffsets[i]) && IsCornerBlocked(current, neighborOffsets[i]))
                    {
                        continue;
                    }

                    float tentativeG = current.gCost + Vector2.Distance(current.worldPosition, neighbor.worldPosition);
                    if (neighbor.opened && tentativeG >= neighbor.gCost)
                    {
                        continue;
                    }

                    neighbor.parent = current;
                    neighbor.gCost = tentativeG;
                    neighbor.hCost = Heuristic(neighbor, goalNode);

                    if (!neighbor.opened)
                    {
                        neighbor.opened = true;
                        openNodes.Add(neighbor);
                    }
                }
            }

            return false;
        }

        private void BuildGrid()
        {
            width = Mathf.Max(1, Mathf.CeilToInt(gridSize.x / cellSize));
            height = Mathf.Max(1, Mathf.CeilToInt(gridSize.y / cellSize));
            origin = gridCenter - gridSize * 0.5f;
            nodes = new Node[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2 worldPosition = GridToWorld(x, y);
                    nodes[x, y] = new Node(x, y, worldPosition, IsWalkable(worldPosition));
                }
            }
        }

        private bool IsWalkable(Vector2 worldPosition)
        {
            float checkSize = cellSize * 0.8f;
            Collider2D[] hits = Physics2D.OverlapBoxAll(worldPosition, new Vector2(checkSize, checkSize), 0f, obstacleMask);
            for (int i = 0; i < hits.Length; i++)
            {
                if (IsBlockingCollider(hits[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsBlockingCollider(Collider2D hitCollider)
        {
            if (hitCollider == null || hitCollider.isTrigger)
            {
                return false;
            }

            return hitCollider.GetComponentInParent<PlayerHealth>() == null &&
                   hitCollider.GetComponentInParent<EnemyHealth>() == null;
        }

        private Node FindNearestWalkable(Vector2Int gridPosition)
        {
            if (IsInsideGrid(gridPosition))
            {
                Node directNode = nodes[gridPosition.x, gridPosition.y];
                if (directNode.walkable)
                {
                    return directNode;
                }
            }

            int maxRadius = Mathf.Max(width, height);
            for (int radius = 1; radius <= maxRadius; radius++)
            {
                for (int x = gridPosition.x - radius; x <= gridPosition.x + radius; x++)
                {
                    for (int y = gridPosition.y - radius; y <= gridPosition.y + radius; y++)
                    {
                        Vector2Int candidate = new Vector2Int(x, y);
                        if (!IsInsideGrid(candidate))
                        {
                            continue;
                        }

                        Node node = nodes[x, y];
                        if (node.walkable)
                        {
                            return node;
                        }
                    }
                }
            }

            return null;
        }

        private Node TakeBestOpenNode()
        {
            int bestIndex = 0;
            float bestCost = openNodes[0].FCost;
            for (int i = 1; i < openNodes.Count; i++)
            {
                float cost = openNodes[i].FCost;
                if (cost < bestCost || Mathf.Approximately(cost, bestCost) && openNodes[i].hCost < openNodes[bestIndex].hCost)
                {
                    bestIndex = i;
                    bestCost = cost;
                }
            }

            Node bestNode = openNodes[bestIndex];
            openNodes.RemoveAt(bestIndex);
            return bestNode;
        }

        private Node GetNeighbor(Node node, Vector2Int offset)
        {
            int x = node.x + offset.x;
            int y = node.y + offset.y;
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return null;
            }

            return nodes[x, y];
        }

        private bool IsDiagonalMove(Vector2Int offset)
        {
            return offset.x != 0 && offset.y != 0;
        }

        private bool IsCornerBlocked(Node node, Vector2Int diagonalOffset)
        {
            Node horizontal = GetNeighbor(node, new Vector2Int(diagonalOffset.x, 0));
            Node vertical = GetNeighbor(node, new Vector2Int(0, diagonalOffset.y));
            return horizontal == null || vertical == null || !horizontal.walkable || !vertical.walkable;
        }

        private void BuildPath(Node goalNode, List<Vector2> path)
        {
            Node current = goalNode;
            while (current != null)
            {
                path.Add(current.worldPosition);
                current = current.parent;
            }

            path.Reverse();
        }

        private void ResetSearch()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    nodes[x, y].ResetSearch();
                }
            }
        }

        private float Heuristic(Node a, Node b)
        {
            return Vector2.Distance(a.worldPosition, b.worldPosition);
        }

        private Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            Vector2 local = worldPosition - origin;
            int x = Mathf.Clamp(Mathf.FloorToInt(local.x / cellSize), 0, width - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt(local.y / cellSize), 0, height - 1);
            return new Vector2Int(x, y);
        }

        private Vector2 GridToWorld(int x, int y)
        {
            return origin + new Vector2((x + 0.5f) * cellSize, (y + 0.5f) * cellSize);
        }

        private bool IsInsideGrid(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < width &&
                   gridPosition.y >= 0 && gridPosition.y < height;
        }

        private sealed class Node
        {
            public readonly int x;
            public readonly int y;
            public readonly Vector2 worldPosition;
            public readonly bool walkable;

            public Node parent;
            public float gCost;
            public float hCost;
            public bool opened;
            public bool closed;

            public float FCost => gCost + hCost;

            public Node(int x, int y, Vector2 worldPosition, bool walkable)
            {
                this.x = x;
                this.y = y;
                this.worldPosition = worldPosition;
                this.walkable = walkable;
            }

            public void ResetSearch()
            {
                parent = null;
                gCost = 0f;
                hCost = 0f;
                opened = false;
                closed = false;
            }
        }
    }
}
