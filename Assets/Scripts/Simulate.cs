using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Simulate : MonoBehaviour
{
    public static Simulate Instance { get; private set; }

    private List<TaskStation> allStations = new List<TaskStation>();

    void Awake()
    {
        Instance = this;
        allStations = FindObjectsOfType<TaskStation>().ToList();
    }


    /// Encuentra la estación libre más cercana dentro del radio de búsqueda
    public TaskStation FindNearestAvailableTaskStation(Vector3 fromPosition, float searchRadius)
    {
        TaskStation nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var station in allStations)
        {
            if (station == null || station.IsOccupied) continue;

            float dist = Vector3.Distance(fromPosition, station.transform.position);
            if (dist < minDist && dist <= searchRadius)
            {
                minDist = dist;
                nearest = station;
            }
        }

        return nearest;
    }

    /// Devuelve una estación aleatoria libre 
    public TaskStation GetRandomAvailableStation()
    {
        var available = allStations.Where(s => !s.IsOccupied).ToList();
        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }
}

