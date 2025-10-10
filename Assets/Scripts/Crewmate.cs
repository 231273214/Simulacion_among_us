using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// Controla el comportamiento de un tripulante
/// Busca estaciones, se mueve hacia ellas, realiza tareas y repite el ciclo
public class Crewmate : MonoBehaviour
{
    [Header("Configuraci�n de Movimiento")]
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float waypointReachDistance = 0.1f;

    [Header("Configuraci�n de Tareas")]
    [SerializeField] protected float waitTimeWhenAllBusy = 2f; // Tiempo de espera si todas las estaciones est�n ocupadas

    protected Pathfinding pathfinding;
    protected List<Vector3> currentPath;
    protected int currentWaypointIndex = 0;
    protected TaskStation targetStation = null;
    protected bool isDoingTask = false;
    protected bool isDead = false;

    // Estados del tripulante
    public enum CrewmateState
    {
        Idle,           // Sin hacer nada
        MovingToTask,   // Movi�ndose hacia una estaci�n
        DoingTask,      // Realizando una tarea
        Waiting         // Esperando que se libere una estaci�n
    }

    protected CrewmateState currentState = CrewmateState.Idle;

    protected virtual void Start()
    {
        pathfinding = FindObjectOfType<Pathfinding>();

        if (pathfinding == null)
        {
            Debug.LogError("No se encontr� Pathfinding en la escena!");
            return;
        }

        // Comienza buscando una tarea
        StartCoroutine(BehaviorLoop());
    }

    /// Bucle principal del comportamiento del tripulante.
    protected virtual IEnumerator BehaviorLoop()
    {
        while (!isDead)
        {
            switch (currentState)
            {
                case CrewmateState.Idle:
                    yield return StartCoroutine(SearchForTask());
                    break;

                case CrewmateState.MovingToTask:
                    yield return StartCoroutine(MoveAlongPath());
                    break;

                case CrewmateState.DoingTask:
                    yield return StartCoroutine(PerformTask());
                    break;

                case CrewmateState.Waiting:
                    yield return new WaitForSeconds(waitTimeWhenAllBusy);
                    currentState = CrewmateState.Idle;
                    break;
            }

            yield return null;
        }
    }

    /// Busca una estaci�n de tarea disponible.
    protected virtual IEnumerator SearchForTask()
    {
        TaskStation[] allStations = FindObjectsOfType<TaskStation>();
        TaskStation closestFreeStation = null;
        float closestDistance = Mathf.Infinity;

        // Encuentra la estaci�n libre m�s cercana
        foreach (TaskStation station in allStations)
        {
            if (!station.IsOccupied)
            {
                float distance = Vector3.Distance(transform.position, station.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFreeStation = station;
                }
            }
        }

        // Si se encontr� una estaci�n libre, calcular el camino
        if (closestFreeStation != null)
        {
            targetStation = closestFreeStation;
            currentPath = pathfinding.FindPath(transform.position, targetStation.transform.position);

            if (currentPath != null && currentPath.Count > 0)
            {
                currentWaypointIndex = 0;
                currentState = CrewmateState.MovingToTask;
            }
            else
            {
                // No se pudo calcular un camino, esperar
                currentState = CrewmateState.Waiting;
            }
        }
        else
        {
            // Todas las estaciones est�n ocupadas, esperar
            currentState = CrewmateState.Waiting;
        }

        yield return null;
    }

    /// Se mueve siguiendo el camino calculado por A*
    protected virtual IEnumerator MoveAlongPath()
    {
        while (currentWaypointIndex < currentPath.Count)
        {
            Vector3 targetWaypoint = currentPath[currentWaypointIndex];

            // Mover hacia el waypoint
            while (Vector3.Distance(transform.position, targetWaypoint) > waypointReachDistance)
            {
                Vector3 direction = (targetWaypoint - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
                yield return null;
            }

            currentWaypointIndex++;
        }

        // Lleg� a la estaci�n, intentar ocuparla
        if (targetStation != null && targetStation.TryOccupy(this))
        {
            currentState = CrewmateState.DoingTask;
        }
        else
        {
            // La estaci�n fue ocupada por otro tripulante, buscar otra
            targetStation = null;
            currentState = CrewmateState.Idle;
        }

        yield return null;
    }

    /// Realiza la tarea en la estaci�n.
    protected virtual IEnumerator PerformTask()
    {
        if (targetStation == null)
        {
            currentState = CrewmateState.Idle;
            yield break;
        }

        isDoingTask = true;

        // Esperar a que se complete la tarea
        while (!targetStation.UpdateTask(Time.deltaTime))
        {
            yield return null;
        }

        isDoingTask = false;
        targetStation = null;

        // Buscar otra tarea
        currentState = CrewmateState.Idle;
    }

    /// Marca al tripulante como muerto 
    public virtual void Die()
    {
        isDead = true;

        // Liberar la estaci�n si estaba us�ndola
        if (targetStation != null && isDoingTask)
        {
            targetStation.ForceRelease();
        }

        // Cambiar apariencia
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Gris transl�cido
        }

        StopAllCoroutines();
    }

    /// Dibuja el camino actual en el editor
    protected virtual void OnDrawGizmos()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
        }
    }

    public bool IsDead => isDead;
    public CrewmateState CurrentState => currentState;
}

