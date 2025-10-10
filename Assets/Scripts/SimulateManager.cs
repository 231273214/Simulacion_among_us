using UnityEngine;
using System.Collections.Generic;

/// Gestor principal de la simulación
/// Controla la creación de tripulantes, impostores y estaciones
/// Verifica condiciones de victoria
public class SimulationManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject mapPrefab; 
    [SerializeField] private GameObject crewMatePrefab;
    [SerializeField] private GameObject impostorPrefab;
    [SerializeField] private GameObject taskStationPrefab;

    [Header("Configuración de Spawn")]
    [SerializeField] private int numberOfCrewMates = 8;
    [SerializeField] private int numberOfImpostors = 2;
    [SerializeField] private int numberOfTaskStations = 6;
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-20, -20);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(20, 20);
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Referencias")]
    [SerializeField] private Transform crewMatesParent;
    [SerializeField] private Transform impostorsParent;
    [SerializeField] private Transform stationsParent;

    private List<Crewmate> allCrewMates = new List<Crewmate>();
    private List<Impostor> allImpostors = new List<Impostor>();
    private List<TaskStation> allStations = new List<TaskStation>();

    private bool gameEnded = false;
    private string winnerMessage = "";

    // Eventos públicos para el estado del juego
    public bool GameEnded => gameEnded;
    public string WinnerMessage => winnerMessage;

    private void Start()
    {
        InitializeSimulation();
    }

    private void Update()
    {
        // Verificar condiciones de victoria solo si el juego no ha terminado
        if (!gameEnded)
        {
            CheckWinConditions();
        }
    }

    /// Inicializa la simulación creando todos los elementos
    private void InitializeSimulation()
    {
        // Reiniciar estado del juego
        gameEnded = false;
        winnerMessage = "";

        // Crear contenedores si no existen
        if (crewMatesParent == null)
        {
            crewMatesParent = new GameObject("CrewMates").transform;
        }
        if (impostorsParent == null)
        {
            impostorsParent = new GameObject("Impostors").transform;
        }
        if (stationsParent == null)
        {
            stationsParent = new GameObject("TaskStations").transform;
        }

        // Crear estaciones de tarea
        SpawnTaskStations();

        // Crear tripulantes
        SpawnCrewMates();

        // Crear impostores
        SpawnImpostors();

        Debug.Log($"Simulación iniciada: {numberOfCrewMates} tripulantes, {numberOfImpostors} impostores, {numberOfTaskStations} estaciones");
    }

    /// Verifica las condiciones de victoria.
    private void CheckWinConditions()
    {
        // Contar tripulantes vivos
        int aliveCrewMates = 0;
        foreach (Crewmate cm in allCrewMates)
        {
            if (!cm.IsDead)
            {
                aliveCrewMates++;
            }
        }

        // Condición 1: Si todos los tripulantes están muertos, ganan los impostores
        if (aliveCrewMates == 0)
        {
            EndGame("¡LOS IMPOSTORES GANAN! Todos los tripulantes han sido eliminados.");
            return;
        }

        // Condición 2: Si todas las tareas están completadas, ganan los tripulantes
        int completedTasks = 0;
        foreach (TaskStation station in allStations)
        {
            if (station.IsCompleted)
            {
                completedTasks++;
            }
        }

        if (completedTasks == allStations.Count)
        {
            EndGame("¡LOS TRIPULANTES GANAN! Todas las tareas han sido completadas.");
            return;
        }
    }

    /// Termina el juego con un mensaje de victoria
    private void EndGame(string message)
    {
        gameEnded = true;
        winnerMessage = message;

        Debug.Log("==========================================");
        Debug.Log(message);
        Debug.Log("==========================================");

        // Detener todos los tripulantes e impostores
        foreach (Crewmate cm in allCrewMates)
        {
            if (cm != null)
            {
                cm.enabled = false;
            }
        }

        foreach (Impostor imp in allImpostors)
        {
            if (imp != null)
            {
                imp.enabled = false;
            }
        }
    }

    /// <summary>
    /// Crea las estaciones de tarea en posiciones aleatorias válidas.
    /// </summary>
    private void SpawnTaskStations()
    {
        for (int i = 0; i < numberOfTaskStations; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();

            GameObject stationObj = Instantiate(taskStationPrefab, spawnPosition, Quaternion.identity, stationsParent);
            stationObj.name = $"TaskStation_{i + 1}";

            TaskStation station = stationObj.GetComponent<TaskStation>();
            if (station != null)
            {
                allStations.Add(station);
            }
        }
    }

    /// <summary>
    /// Crea los tripulantes en posiciones aleatorias válidas.
    /// </summary>
    private void SpawnCrewMates()
    {
        for (int i = 0; i < numberOfCrewMates; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();

            GameObject crewMateObj = Instantiate(crewMatePrefab, spawnPosition, Quaternion.identity, crewMatesParent);
            crewMateObj.name = $"CrewMate_{i + 1}";

            Crewmate crewMate = crewMateObj.GetComponent<Crewmate>();
            if (crewMate != null)
            {
                allCrewMates.Add(crewMate);
            }
        }
    }

    /// <summary>
    /// Crea los impostores en posiciones aleatorias válidas.
    /// </summary>
    private void SpawnImpostors()
    {
        for (int i = 0; i < numberOfImpostors; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();

            GameObject impostorObj = Instantiate(impostorPrefab, spawnPosition, Quaternion.identity, impostorsParent);
            impostorObj.name = $"Impostor_{i + 1}";

            Impostor impostor = impostorObj.GetComponent<Impostor>();
            if (impostor != null)
            {
                allImpostors.Add(impostor);
            }
        }
    }

    /// <summary>
    /// Encuentra una posición válida para hacer spawn (sin obstáculos).
    /// </summary>
    private Vector3 GetValidSpawnPosition()
    {
        Vector3 position;
        int maxAttempts = 50;
        int attempts = 0;

        do
        {
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            position = new Vector3(x, y, 0);
            attempts++;

            // Si no se encuentra posición válida después de muchos intentos, devolver la última
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("No se pudo encontrar una posición de spawn válida después de muchos intentos");
                break;
            }

        } while (Physics2D.OverlapCircle(position, 0.5f, obstacleLayer) != null);

        return position;
    }

    /// <summary>
    /// Obtiene estadísticas de la simulación.
    /// </summary>
    public SimulationStats GetStats()
    {
        int aliveCrewMates = 0;
        int deadCrewMates = 0;
        int busyStations = 0;
        int completedTasks = 0;

        foreach (Crewmate cm in allCrewMates)
        {
            if (cm.IsDead)
                deadCrewMates++;
            else
                aliveCrewMates++;
        }

        foreach (TaskStation station in allStations)
        {
            if (station.IsOccupied)
                busyStations++;
            if (station.IsCompleted)
                completedTasks++;
        }

        return new SimulationStats
        {
            totalCrewMates = numberOfCrewMates,
            aliveCrewMates = aliveCrewMates,
            deadCrewMates = deadCrewMates,
            totalImpostors = numberOfImpostors,
            totalStations = numberOfTaskStations,
            busyStations = busyStations,
            completedTasks = completedTasks
        };
    }

    /// <summary>
    /// Reinicia la simulación destruyendo todos los objetos y recreándolos.
    /// </summary>
    public void ResetSimulation()
    {
        // Destruir todos los objetos existentes
        foreach (Transform child in crewMatesParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in impostorsParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in stationsParent)
        {
            Destroy(child.gameObject);
        }

        // Limpiar listas
        allCrewMates.Clear();
        allImpostors.Clear();
        allStations.Clear();

        // Reiniciar
        InitializeSimulation();
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujar área de spawn
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(
            (spawnAreaMin.x + spawnAreaMax.x) / 2,
            (spawnAreaMin.y + spawnAreaMax.y) / 2,
            0
        );
        Vector3 size = new Vector3(
            spawnAreaMax.x - spawnAreaMin.x,
            spawnAreaMax.y - spawnAreaMin.y,
            0
        );
        Gizmos.DrawWireCube(center, size);
    }
}

/// <summary>
/// Estructura para almacenar estadísticas de la simulación.
/// </summary>
[System.Serializable]
public struct SimulationStats
{
    public int totalCrewMates;
    public int aliveCrewMates;
    public int deadCrewMates;
    public int totalImpostors;
    public int totalStations;
    public int busyStations;
    public int completedTasks;
}