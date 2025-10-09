using UnityEngine;

/// <summary>
/// Representa una estaci�n de tareas que puede ser utilizada por tripulantes.
/// Controla si est� ocupada, el progreso de la tarea y su visualizaci�n.
/// </summary>
public class TaskStation : MonoBehaviour
{
    [Header("Configuraci�n de la Tarea")]
    [SerializeField] private float taskDuration = 3f; // Tiempo que toma completar la tarea (segundos)
    [SerializeField] private Color freeColor = Color.green;
    [SerializeField] private Color occupiedColor = Color.yellow;

    private bool isOccupied = false;
    private float taskProgress = 0f;
    private Crewmate currentUser = null;
    private SpriteRenderer spriteRenderer;

    public bool IsOccupied => isOccupied;
    public float TaskDuration => taskDuration;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = freeColor;
        }
    }

    /// <summary>
    /// Intenta ocupar la estaci�n. Retorna true si se ocup� exitosamente.
    /// </summary>
    public bool TryOccupy(Crewmate user)
    {
        if (isOccupied)
        {
            return false;
        }

        isOccupied = true;
        currentUser = user;
        taskProgress = 0f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = occupiedColor;
        }

        return true;
    }

    /// <summary>
    /// Actualiza el progreso de la tarea. Retorna true cuando se completa.
    /// </summary>
    public bool UpdateTask(float deltaTime)
    {
        if (!isOccupied)
        {
            return false;
        }

        taskProgress += deltaTime;

        // La tarea se completa cuando el progreso alcanza la duraci�n
        if (taskProgress >= taskDuration)
        {
            CompleteTask();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Completa la tarea y libera la estaci�n.
    /// </summary>
    private void CompleteTask()
    {
        isOccupied = false;
        currentUser = null;
        taskProgress = 0f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = freeColor;
        }
    }

    /// <summary>
    /// Fuerza la liberaci�n de la estaci�n (�til si el tripulante se va antes de terminar).
    /// </summary>
    public void ForceRelease()
    {
        if (isOccupied)
        {
            CompleteTask();
        }
    }

    /// <summary>
    /// Dibuja el �rea de interacci�n de la estaci�n.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }

    /// <summary>
    /// Obtiene el progreso actual de la tarea (0 a 1).
    /// </summary>
    public float GetTaskProgress()
    {
        return taskProgress / taskDuration;
    }
}


