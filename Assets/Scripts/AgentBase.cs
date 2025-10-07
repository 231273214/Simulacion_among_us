using UnityEngine;

public abstract class AgentBase : MonoBehaviour
{
    [Header("Agent Base")]
    public float moveSpeed = 2f;
    public float visionRadius = 4f;
    public float safeDistance = 2f; // para comprobar si alguien mas esta cerca en una eliminaci�n
    public Rigidbody2D rb;

    protected virtual void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    // Cada agente implementa su logica por frame aqui
    public abstract void Simulate();

    // utilitarios
    protected Collider2D[] SenseNearby(float radius, LayerMask mask)
    {
        return Physics2D.OverlapCircleAll(transform.position, radius, mask);
    }

    protected void MoveTowards(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
    }

    protected void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
    }
}
