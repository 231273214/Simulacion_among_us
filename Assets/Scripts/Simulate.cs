using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Simulate : MonoBehaviour
{
    public static Simulate Instance { get; private set; }

    private List<TaskStation> allStations = new List<TaskStation>();

    void Awake()
    {
        Instance = this;
        // Buscar todas las estaciones en la escena
        allStations = Object.FindObjectsByType<TaskStation>(FindObjectsSortMode.None).ToList();
    }

    // Retorna la estación libre más cercana a una posición
    public TaskStation FindNearestAvailableTaskStation(Vector3 fromPosition)
    {
        TaskStation nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var station in allStations)
        {
            if (station.isOccupied) continue; // usamos isOccupied en lugar de IsOccupied

            float dist = Vector3.Distance(fromPosition, station.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = station;
            }
        }

        return nearest;
    }

    // Método auxiliar (opcional): obtener estaciones aleatorias libres
    public TaskStation GetRandomAvailableStation()
    {
        var available = allStations.Where(s => !s.isOccupied).ToList();
        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }
}
