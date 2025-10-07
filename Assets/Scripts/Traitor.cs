using TMPro;
using UnityEngine;

public class Traitor : AgentBase
{
    public enum State { Roam, Chase, AttemptKill }
    public State state = State.Roam;

    [Header("Traitor")]
    public LayerMask crewmateMask;
    public float killRange = 0.6f;
    public float killCooldown = 4f;
    private float killTimer = 0f;

    private Crewmate target;

    void Start()
    {
        
    }

    public override void Simulate()
    {
        killTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Roam:
                // buscar tripulantes en vision
                Crewmate c = FindNearestCrewmateInRadius(visionRadius);
                if (c != null)
                {
                    target = c;
                    state = State.Chase;
                }
                else
                {
                    
                    rb.linearVelocity = new Vector2(Mathf.PerlinNoise(Time.time, 0) - 0.5f, Mathf.PerlinNoise(0, Time.time) - 0.5f) * moveSpeed;
                }
                break;

            case State.Chase:
                if (target == null)
                {
                    state = State.Roam;
                    break;
                }
                MoveTowards(target.transform.position);
                float dist = Vector2.Distance(transform.position, target.transform.position);
                if (dist <= killRange && killTimer <= 0f)
                {
                    // verificar que no haya otros agentes cerca del objetivo
                    Collider2D[] near = Physics2D.OverlapCircleAll(target.transform.position, safeDistance, crewmateMask | (1 << gameObject.layer));
                    int others = 0;
                    foreach (var col in near)
                    {
                        if (col.gameObject == target.gameObject) continue;
                        if (col.GetComponent<Crewmate>() != null) others++;
                    }

                    if (others == 0)
                    {
                        // matar
                        Kill(target);
                        killTimer = killCooldown;
                        state = State.Roam;
                    }
                    else
                    {
                        // demasiada gente, abortar por ahora
                        state = State.Roam;
                    }
                }
                break;

            case State.AttemptKill:
                break;
        }

    }

    Crewmate FindNearestCrewmateInRadius(float r)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, r, crewmateMask);
        float best = float.MaxValue;
        Crewmate bestC = null;
        foreach (var c in cols)
        {
            Crewmate cr = c.GetComponent<Crewmate>();
            if (cr == null) continue;
            float d = Vector2.Distance(transform.position, cr.transform.position);
            if (d < best)
            {
                best = d;
                bestC = cr;
            }
        }
        return bestC;
    }

    void Kill(Crewmate victim)
    {
        // notifica al manager y destruye al tripulante (o desactiva)
        SimulateManager.Instance.ReportKill(transform.position, victim);
        // eliminar fisicamente 
        Destroy(victim.gameObject);
    }
}
