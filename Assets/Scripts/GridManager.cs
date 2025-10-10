using UnityEngine;
using System.Collections.Generic;

/// Gestiona la grilla lógica del mapa, detectando zonas caminables y obstáculos.
/// Se usa como base para el pathfinding
public class GridManager : MonoBehaviour
{
    [Header("Configuración de la Grilla")]
    [SerializeField] private Vector2 gridWorldSize = new Vector2(50, 50); // Tamaño del mapa en unidades de mundo
    [SerializeField] private float nodeRadius = 0.5f; // Radio de cada nodo (determina el tamaño de la grilla)
    [SerializeField] private LayerMask unwalkableMask; // Capa de objetos que bloquean el paso (paredes, obstáculos)

    private Node[,] grid; // Matriz bidimensional de nodos
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    public int GridSizeX => gridSizeX;
    public int GridSizeY => gridSizeY;

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    /// Crea la grilla verificando qué posiciones son caminables.
    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius)
                                                      + Vector3.up * (y * nodeDiameter + nodeRadius);

                // Verifica si hay un collider en esta posición (obstáculo)
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.9f, unwalkableMask);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    /// Obtiene el nodo más cercano a una posición del mundo
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    /// Obtiene los nodos vecinos de un nodo dado (8 direcciones)
    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // El nodo central no es vecino de sí mismo

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }

    /// Dibuja la grilla en el editor para debugging
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = n.walkable ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}

/// Representa un nodo individual en la grilla.
/// Contiene información sobre si es caminable y sus costos para el algoritmo A*
public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    // Costos para A*
    public int gCost; // Distancia desde el nodo inicial
    public int hCost; // Distancia heurística hasta el nodo objetivo
    public Node parent; // Nodo padre en el camino

    public int fCost => gCost + hCost; // Costo total

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
}

