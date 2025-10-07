using UnityEngine;

public class TaskStation : MonoBehaviour
{
    private bool occupied = false;

    void Start()
    {
        // Registrar esta estación en el Simulate
        Simulate.Instance.RegisterStation(this);
    }

    void OnDestroy()
    {
        // Si se elimina, quitarla del registro
        if (Simulate.Instance != null)
            Simulate.Instance.UnregisterStation(this);
    }

    // --- IMPORTANTE ---
    public bool IsAvailable()
    {
        return !occupied;
    }

    public bool TryEnter()
    {
        if (!occupied)
        {
            occupied = true;
            return true;
        }
        return false;
    }

    public void Leave()
    {
        occupied = false;
    }
}
