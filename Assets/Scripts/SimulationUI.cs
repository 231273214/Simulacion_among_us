using UnityEngine;

/// Muestra estadísticas de la simulación en pantalla
public class SimulationUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SimulationManager simulationManager;

    [Header("Configuración UI")]
    [SerializeField] private bool showStats = true;
    [SerializeField] private Vector2 statsPosition = new Vector2(10, 10);
    [SerializeField] private int fontSize = 16;

    private GUIStyle statsStyle;
    private GUIStyle victoryStyle;

    private void Start()
    {
        if (simulationManager == null)
        {
            simulationManager = FindObjectOfType<SimulationManager>();
        }

        // Configurar estilo para el texto de estadísticas
        statsStyle = new GUIStyle();
        statsStyle.fontSize = fontSize;
        statsStyle.normal.textColor = Color.white;
        statsStyle.alignment = TextAnchor.UpperLeft;

        // Configurar estilo para el mensaje de victoria
        victoryStyle = new GUIStyle();
        victoryStyle.fontSize = 28;
        victoryStyle.fontStyle = FontStyle.Bold;
        victoryStyle.normal.textColor = Color.yellow;
        victoryStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void OnGUI()
    {
        if (simulationManager == null)
            return;

        // Si el juego terminó, mostrar mensaje de victoria grande
        if (simulationManager.GameEnded)
        {
            DrawVictoryScreen();
        }

        // Siempre mostrar las estadísticas si está habilitado
        if (showStats)
        {
            DrawStats();
        }
    }

    /// Dibuja las estadísticas de la simulación
    private void DrawStats()
    {
        SimulationStats stats = simulationManager.GetStats();

        // Calcular porcentaje de tareas completadas
        float taskProgress = stats.totalStations > 0 ?
            ((float)stats.completedTasks / stats.totalStations) * 100f : 0f;

        // Crear el texto de estadísticas
        string statsText = $"=== SIMULACIÓN AMONG US ===\n\n" +
                          $"TRIPULANTES:\n" +
                          $"  Total: {stats.totalCrewMates}\n" +
                          $"  Vivos: {stats.aliveCrewMates}\n" +
                          $"  Muertos: {stats.deadCrewMates}\n\n" +
                          $"IMPOSTORES: {stats.totalImpostors}\n\n" +
                          $"TAREAS:\n" +
                          $"  Total: {stats.totalStations}\n" +
                          $"  Completadas: {stats.completedTasks}\n" +
                          $"  En progreso: {stats.busyStations}\n" +
                          $"  Libres: {stats.totalStations - stats.busyStations - stats.completedTasks}\n" +
                          $"  Progreso: {taskProgress:F1}%\n\n" +
                          $"Presiona R para reiniciar";

        // Dibujar fondo semi-transparente
        GUI.Box(new Rect(statsPosition.x - 5, statsPosition.y - 5, 320, 320), "");

        // Dibujar estadísticas
        GUI.Label(new Rect(statsPosition.x, statsPosition.y, 310, 310), statsText, statsStyle);
    }
    /// Dibuja la pantalla de victoria cuando termina el juego
    private void DrawVictoryScreen()
    {
        // Obtener el centro de la pantalla
        float screenCenterX = Screen.width / 2f;
        float screenCenterY = Screen.height / 2f;

        // Tamaño del cuadro de victoria
        float boxWidth = 600f;
        float boxHeight = 200f;

        // Dibujar fondo oscuro semi-transparente en toda la pantalla
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
        GUI.color = Color.white;

        // Dibujar cuadro de victoria
        GUI.Box(new Rect(screenCenterX - boxWidth / 2, screenCenterY - boxHeight / 2, boxWidth, boxHeight), "");

        // Dibujar mensaje de victoria
        GUI.Label(
            new Rect(screenCenterX - boxWidth / 2, screenCenterY - 40, boxWidth, 80),
            simulationManager.WinnerMessage,
            victoryStyle
        );

        // Dibujar instrucción de reinicio
        GUIStyle restartStyle = new GUIStyle(statsStyle);
        restartStyle.fontSize = 18;
        restartStyle.alignment = TextAnchor.MiddleCenter;
        restartStyle.normal.textColor = Color.white;

        GUI.Label(
            new Rect(screenCenterX - boxWidth / 2, screenCenterY + 40, boxWidth, 40),
            "Presiona R para reiniciar la simulación",
            restartStyle
        );
    }

    private void Update()
    {
        // Reiniciar con la tecla R
        if (Input.GetKeyDown(KeyCode.R) && simulationManager != null)
        {
            simulationManager.ResetSimulation();
        }
    }
}