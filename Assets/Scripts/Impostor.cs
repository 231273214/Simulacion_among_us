using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

/// <summary>
/// Controla el comportamiento de un impostor.
/// Hereda de CrewMate pero puede fingir tareas y eliminar tripulantes.
/// </summary>
public class Impostor : Crewmate
{
    [Header("Configuración de Impostor")]
    [SerializeField] private float killCooldown = 10f; // Tiempo entre asesinatos
    [SerializeField] private float killRange = 1.5f; // Distancia para poder matar
    [SerializeField] private float fakeTaskChance = 0.7f; // Probabilidad de fingir en lugar de hacer tarea real (0-1)
    [SerializeField] private float killAttemptInterval = 2f; // Cada cuánto tiempo intenta matar

    private float killTimer = 0f;
    private bool canKill = true;

    protected override void Start()
    {
        base.Start();

        // Cambiar color a rojo para identificar al impostor
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
        }

        // Iniciar comportamiento de asesinato
        StartCoroutine(KillBehavior());
    }

    /// <summary>
    /// Comportamiento de asesinato del impostor.
    /// Busca tripulantes cercanos y los elimina si puede.
    /// </summary>
    private IEnumerator KillBehavior()
    {
        while (!isDead)
        {
            // Actualizar el cooldown de asesinato
            if (!canKill)
            {
                killTimer += Time.deltaTime;
                if (killTimer >= killCooldown)
                {
                    canKill = true;
                    killTimer = 0f;
                }
            }

            // Intentar matar si está disponible y no está haciendo una tarea
            if (canKill && currentState != CrewmateState.DoingTask)
            {
                Crewmate victim = FindNearestCrewMate();

                if (victim != null && Vector3.Distance(transform.position, victim.transform.position) <= killRange)
                {
                    KillCrewmate(victim);
                    canKill = false;
                    killTimer = 0f;

                    // Alejarse del cuerpo después de matar
                    yield return StartCoroutine(FleeFromBody(victim.transform.position));
                }
            }

            yield return new WaitForSeconds(killAttemptInterval);
        }
    }

    /// <summary>
    /// Encuentra el tripulante vivo más cercano (que no sea impostor).
    /// </summary>
    private Crewmate FindNearestCrewMate()
    {
        Crewmate[] allCrewmates = FindObjectsOfType<Crewmate>();
        Crewmate nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Crewmate crewmate in allCrewmates)
        {
            // Ignorar a sí mismo, otros impostores y tripulantes muertos
            if (crewmate == this || crewmate is Impostor || crewmate.IsDead)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, crewmate.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = crewmate;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Mata a un tripulante.
    /// </summary>
    private void KillCrewmate(Crewmate victim)
    {
        if (victim != null && !victim.IsDead)
        {
            victim.Die();
            Debug.Log($"Impostor {gameObject.name} eliminó a {victim.gameObject.name}");
        }
    }

    /// <summary>
    /// Huye del cuerpo después de matar para parecer menos sospechoso.
    /// </summary>
    private IEnumerator FleeFromBody(Vector3 bodyPosition)
    {
        // Calcular dirección opuesta al cuerpo
        Vector3 fleeDirection = (transform.position - bodyPosition).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * 3f;

        // Calcular camino de huida
        List<Vector3> fleePath = pathfinding.FindPath(transform.position, fleeTarget);

        if (fleePath != null && fleePath.Count > 0)
        {
            // Moverse por el camino de huida
            foreach (Vector3 waypoint in fleePath)
            {
                while (Vector3.Distance(transform.position, waypoint) > waypointReachDistance)
                {
                    Vector3 direction = (waypoint - transform.position).normalized;
                    transform.position += direction * moveSpeed * Time.deltaTime;
                    yield return null;
                }
            }
        }
    }

    /// <summary>
    /// Sobrescribe el comportamiento de realizar tareas.
    /// El impostor a veces finge hacer tareas.
    /// </summary>
    protected override IEnumerator PerformTask()
    {
        if (targetStation == null)
        {
            currentState = CrewmateState.Idle;
            yield break;
        }

        isDoingTask = true;

        // Decidir si fingir o hacer la tarea real
        bool shouldFake = Random.value < fakeTaskChance;

        if (shouldFake)
        {
            // Fingir tarea: solo esperar sin activar la estación
            float fakeTaskTime = targetStation.TaskDuration * Random.Range(0.5f, 1.5f);
            yield return new WaitForSeconds(fakeTaskTime);

            // Liberar la estación manualmente (no se completó realmente)
            targetStation.ForceRelease();
        }
        else
        {
            // Hacer la tarea real (para no ser tan obvio)
            while (!targetStation.UpdateTask(Time.deltaTime))
            {
                yield return null;
            }
        }

        isDoingTask = false;
        targetStation = null;

        // Buscar otra tarea
        currentState = CrewmateState.Idle;
    }

    /// <summary>
    /// Dibuja el rango de asesinato en el editor.
    /// </summary>
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Dibujar rango de asesinato
        Gizmos.color = canKill ? Color.red : Color.gray;
        Gizmos.DrawWireSphere(transform.position, killRange);
    }
}
