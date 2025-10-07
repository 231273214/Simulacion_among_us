using UnityEngine;
using System.Collections;

public class Crewmate : AgentBase
{
    public enum State { Idle, MoveToTask, PerformingTask, Flee }
    public State state = State.Idle;

    [Header("Crewmate")]
    public LayerMask agentMask;
    public float fleeDuration = 3f;
    public float taskSeekRadius = 8f;

    private TaskStation currentStation;
    private float fleeTimer = 0f;
    private Vector2 roamTarget;

    void Start()
    {
        PickNewRoamTarget();
    }

    public override void Simulate()
    {
        // simple FSM
        switch (state)
        {
            case State.Idle:
                // Buscar estación cercana y libre
                GameObject nearestStation = Simulate.Instance.FindNearestAvailableTaskStation(transform.position);

                if (nearestStation != null)
                {
                    currentStation = nearestStation;
                    state = State.MoveToTask;
                }
                else
                {
                    // Moverse aleatoriamente
                    MoveTowards(roamTarget);
                    if (Vector2.Distance(transform.position, roamTarget) < 0.3f)
                        PickNewRoamTarget();
                }
                break;

            case State.MoveToTask:
                if (currentStation == null)
                {
                    state = State.Idle;
                    break;
                }

                MoveTowards(currentStation.transform.position);

                if (Vector2.Distance(transform.position, currentStation.transform.position) < 0.7f)
                {
                    // Intentar entrar en slot
                    if (currentStation.TryEnter())
                    {
                        StopMoving();
                        StartCoroutine(PerformAtStation(currentStation));
                        state = State.PerformingTask;
                    }
                    else
                    {
                        // Estación ocupada: busca otra o espera
                        currentStation = null;
                        state = State.Idle;
                    }
                }
                break;

            case State.PerformingTask:
                // Aquí se puede chequear si vio una eliminación
                break;

            case State.Flee:
                // Huyendo de la posición del traidor
                fleeTimer -= Time.deltaTime;
                if (fleeTimer <= 0)
                    state = State.Idle;
                break;
        }
    }

    IEnumerator PerformAtStation(TaskStation station)
    {
        yield return station.PerformTask(() => {
            // tarea completada: notificar a SimulateManager
            SimulateManager.Instance.ReportTaskCompleted();
        });
        station.Leave();
        currentStation = null;
        state = State.Idle;
    }

    void PickNewRoamTarget()
    {
        Vector2 random = (Vector2)transform.position + Random.insideUnitCircle * 4f;
        roamTarget = random;
    }

    // llamado por SimulateManager cuando otro vio una eliminación
    public void OnWitnessKill(Vector2 threatPos)
    {
        // iniciar huida: moverse en sentido opuesto
        Vector2 dirAway = ((Vector2)transform.position - threatPos).normalized;
        Vector2 fleePoint = (Vector2)transform.position + dirAway * 5f;
        MoveTowards(fleePoint);
        state = State.Flee;
        fleeTimer = fleeDuration;
    }
}
