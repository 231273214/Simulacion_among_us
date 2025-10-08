using UnityEngine;

public class TaskStation : MonoBehaviour
{
    [HideInInspector] public bool isOccupied = false;
    private Crewmate occupant;

    // Intenta asignar un tripulante a esta estaci�n
    public bool TryOccupy(Crewmate crewmate)
    {
        if (isOccupied) return false; // si ya est� ocupada, no se puede

        isOccupied = true;
        occupant = crewmate;
        return true;
    }

    // Libera la estaci�n para que otro tripulante pueda usarla
    public void Release()
    {
        isOccupied = false;
        occupant = null;
    }

    // Comprueba si una estaci�n ya fue completada por un tripulante espec�fico
    public bool IsCompletedBy(Crewmate crewmate)
    {
        return occupant == crewmate; // por ahora simple, luego podemos expandir
    }
}

