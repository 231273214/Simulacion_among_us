using UnityEngine;
using System.Collections.Generic;

public class SimulateManager : MonoBehaviour
{
    public static SimulateManager Instance { get; private set; }

    [Header("Settings")]
    public List<AgentBase> agents = new List<AgentBase>();
    public List<TaskStation> stations = new List<TaskStation>();
    public int totalTasks = 10;
    private int tasksCompleted = 0;
    public int numTraitors = 1;

    
    public LayerMask crewmateLayerMask;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
        var agCopy = new List<AgentBase>(agents);
        foreach (var a in agCopy)
        {
            if (a == null) continue;
            a.Simulate();
        }

        
        CheckVictoryConditions();
    }

    public void RegisterAgent(AgentBase a)
    {
        if (!agents.Contains(a)) agents.Add(a);
    }

    public void UnregisterAgent(AgentBase a)
    {
        if (agents.Contains(a)) agents.Remove(a);
    }

    public void ReportTaskCompleted()
    {
        tasksCompleted++;
        Debug.Log($"Tarea completada: {tasksCompleted}/{totalTasks}");
    }

    // Reporte por Traitor.Kill()
    public void ReportKill(Vector2 pos, Crewmate victim)
    {
        Debug.Log($"Tripulante eliminado en {pos}");
        // notificar testigos: buscar tripulantes que puedan ver el evento (distancia/vision)
        float witnessRadius = 6f;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(pos, witnessRadius);
        foreach (var col in nearby)
        {
            Crewmate c = col.GetComponent<Crewmate>();
            if (c != null)
            {
                // c.OnWitnessKill(pos);
            }
        }

        // remover del registro
        // if (victim != null) UnregisterAgent(victim);

        // evaluar si traidores ganan
    }

    void CheckVictoryConditions()
    {
        // tripulantes ganan completando totalTasks
        if (tasksCompleted >= totalTasks)
        {
            Debug.Log("Tripulantes GANAN: tareas completadas");
            Time.timeScale = 0f;
        }

        // traidores ganan si quedan <= numTraitors de agentes tripulantes
        int crewCount = 0;
        foreach (var a in agents)
        {
            if (a is Crewmate) crewCount++;
        }
        if (crewCount <= numTraitors)
        {
            Debug.Log("Traidores GANAN");
            Time.timeScale = 0f;
        }
    }
}
