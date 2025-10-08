using UnityEngine;

public class TaskStation : MonoBehaviour
{
    [HideInInspector] public bool isOccupied = false;
    private Crewmate occupant;

    // Intenta asignar un tripulante a esta estación
    public bool TryOccupy(Crewmate crewmate)
    {
        if (isOccupied) return false; // si ya está ocupada, no se puede

        isOccupied = true;
        occupant = crewmate;
        return true;
    }

    // Libera la estación para que otro tripulante pueda usarla
    public void Release()
    {
        isOccupied = false;
        occupant = null;
    }

    // Comprueba si una estación ya fue completada por un tripulante específico
    public bool IsCompletedBy(Crewmate crewmate)
    {
        return occupant == crewmate; // por ahora simple, luego podemos expandir
    }
}

