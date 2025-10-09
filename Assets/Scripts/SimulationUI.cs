using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra estad�sticas de la simulaci�n en pantalla.
/// Opcional: puede usarse con UI de Unity o solo con OnGUI para debugging r�pido.
/// </summary>
public class SimulationUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SimulationManager simulationManager;

    [Header("Configuraci�n UI")]
    [SerializeField] private bool showStats = true;
    [SerializeField] private Vector2 statsPosition = new Vector2(10, 10);
    [SerializeField] private int fontSize = 16;

    private GUIStyle statsStyle;

    private void Start()
    {
        if (simulationManager == null)
        {
            simulationManager = FindObjectOfType<SimulationManager>();
        }

        // Configurar estilo para el texto
        statsStyle = new GUIStyle();
        statsStyle.fontSize = fontSize;
        statsStyle.normal.textColor = Color.white;
        statsStyle.alignment = TextAnchor.UpperLeft;
    }

    private void OnGUI()
    {
        if (!showStats || simulationManager == null)
            return;

        SimulationStats stats = simulationManager.GetStats();

        // Crear el texto de estad�sticas
        string statsText = $"=== SIMULACI�N AMONG US ===\n\n" +
                          $"TRIPULANTES:\n" +
                          $"  Total: {stats.totalCrewMates}\n" +
                          $"  Vivos: {stats.aliveCrewMates}\n" +
                          $"  Muertos: {stats.deadCrewMates}\n\n" +
                          $"IMPOSTORES: {stats.totalImpostors}\n\n" +
                          $"ESTACIONES:\n" +
                          $"  Total: {stats.totalStations}\n" +
                          $"  Ocupadas: {stats.busyStations}\n" +
                          $"  Libres: {stats.totalStations - stats.busyStations}\n\n" +
                          $"Presiona R para reiniciar";

        // Dibujar fondo semi-transparente
        GUI.Box(new Rect(statsPosition.x - 5, statsPosition.y - 5, 300, 280), "");

        // Dibujar estad�sticas
        GUI.Label(new Rect(statsPosition.x, statsPosition.y, 290, 270), statsText, statsStyle);
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
