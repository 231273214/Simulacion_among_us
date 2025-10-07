using System.Collections.Generic;
using UnityEngine;

public class Simulate : MonoBehaviour
{
    // --- Singleton ---
    public static Simulate Instance { get; private set; }

    // Lista de todas las estaciones de tareas registradas
    public List<TaskStation> taskStations = new List<TaskStation>();

    void Awake()
    {
        // Configurar Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // --- Registro de estaciones ---
    public void RegisterStation(TaskStation station)
    {
        if (!taskStations.Contains(station))
        {
            taskStations.Add(station);
        }
    }

    public void UnregisterStation(TaskStation station)
    {
        if (taskStations.Contains(station))
        {
            taskStations.Remove(station);
        }
    }

    // --- Buscar estación disponible más cercana ---
    public GameObject FindNearestAvailableTaskStation(Vector2 fromPosition)
    {
        GameObject nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (var station in taskStations)
        {
            if (station == null) continue;

            // Verificar si está disponible
            if (!station.IsAvailable()) continue;

            float distance = Vector2.Distance(fromPosition, station.transform.position);

            if (distance < nearestDistance)
            {
                nearest = station.gameObject;
                nearestDistance = distance;
            }
        }

        return nearest;
    }
}
