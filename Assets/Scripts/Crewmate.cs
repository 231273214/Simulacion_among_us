using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crewmate : MonoBehaviour
{
    [Header("Movimiento y búsqueda")]
    public float moveSpeed = 3f;
    public float baseSearchRadius = 4f;
    public float maxSearchRadius = 10f;
    public float searchIncreaseRate = 1f; // cuanto crece el radio por segundo
    public float avoidDistance = 0.5f;
    public LayerMask wallLayer;
    public LayerMask stationLayer;

    [Header("Tareas")]
    public int totalTasks = 4;
    public float taskDuration = 2f;

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private TaskStation currentStation;
    private int completedTasks = 0;
    private float currentSearchRadius;
    private bool doingTask = false;
    private HashSet<TaskStation> completedStations = new HashSet<TaskStation>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        PickRandomDirection();
        currentSearchRadius = baseSearchRadius;
    }

    void FixedUpdate()
    {
        if (doingTask || completedTasks >= totalTasks)
            return;

        SearchForStations();
        MoveAndAvoidWalls();
    }

    void SearchForStations()
    {
        // Buscar estaciones dentro del radio
        Collider2D[] stations = Physics2D.OverlapCircleAll(transform.position, currentSearchRadius, stationLayer);
        TaskStation nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var col in stations)
        {
            TaskStation station = col.GetComponent<TaskStation>();
            if (station != null && !station.isOccupied && !completedStations.Contains(station))
            {
                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = station;
                }
            }
        }

        if (nearest != null)
        {
            // Reinicia radio de búsqueda
            currentSearchRadius = baseSearchRadius;
            // Dirígete hacia la estación
            moveDir = ((Vector2)nearest.transform.position - rb.position).normalized;

            // Si está suficientemente cerca, inicia tarea
            if (Vector2.Distance(transform.position, nearest.transform.position) < 0.5f)
            {
                StartCoroutine(DoTask(nearest));
            }
        }
        else
        {
            // No encontró estaciones: aumenta radio y sigue explorando
            currentSearchRadius = Mathf.Min(currentSearchRadius + searchIncreaseRate * Time.fixedDeltaTime, maxSearchRadius);
        }
    }

    IEnumerator DoTask(TaskStation station)
    {
        doingTask = true;
        currentStation = station;
        station.isOccupied = true;

        yield return new WaitForSeconds(taskDuration);

        station.isOccupied = false;
        completedTasks++;
        completedStations.Add(station);
        currentStation = null;
        doingTask = false;

        // Elige nueva dirección aleatoria para continuar explorando
        PickRandomDirection();
    }

    void MoveAndAvoidWalls()
    {
        // Detección de pared
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, avoidDistance, wallLayer);
        if (hit.collider != null)
        {
            // Gira para evitar
            float turnAngle = Random.value > 0.5f ? 90f : -90f;
            moveDir = Quaternion.Euler(0, 0, turnAngle) * moveDir;
        }

        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
    }

    void PickRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        moveDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, currentSearchRadius);
    }
}




