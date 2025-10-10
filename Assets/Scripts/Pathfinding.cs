using UnityEngine;
using System.Collections.Generic;

/// Implementa el algoritmo A* para encontrar el camino más corto entre dos puntos
public class Pathfinding : MonoBehaviour
{
    private GridManager gridManager;

    private void Awake()
    {
        gridManager = GetComponent<GridManager>();
    }

    /// Encuentra un camino desde startPos hasta targetPos usando A*
    /// Retorna una lista de posiciones en el mundo que forman el camino
    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = gridManager.NodeFromWorldPoint(startPos);
        Node targetNode = gridManager.NodeFromWorldPoint(targetPos);

        List<Vector3> waypoints = new List<Vector3>();

        // Si el objetivo no es accecible, no hay camino
        if (!targetNode.walkable)
        {
            return waypoints;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Encuentra el nodo con el menor fCost
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Si llegamos al objetivo, reconstruir el camino
            if (currentNode == targetNode)
            {
                waypoints = RetracePath(startNode, targetNode);
                break;
            }

            // Evaluar vecinos
            foreach (Node neighbor in gridManager.GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);

                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return waypoints;
    }

    /// Reconstruye el camino desde el nodo final hasta el inicial
    private List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        // Simplificar el camino
        List<Vector3> waypoints = SimplifyPath(path);
        waypoints.Reverse();

        return waypoints;
    }

    /// Simplifica el camino eliminando puntos intermedios innecesarios
    private List<Vector3> SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(
                path[i - 1].gridX - path[i].gridX,
                path[i - 1].gridY - path[i].gridY
            );

            if (directionNew != directionOld)
            {
                waypoints.Add(path[i - 1].worldPosition);
            }

            directionOld = directionNew;
        }

        return waypoints;
    }

    /// Calcula la distancia entre dos nodos
    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}



