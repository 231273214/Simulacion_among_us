using UnityEngine;

/// Representa una estación de tareas que puede ser utilizada por tripulantes.
/// Controla si está ocupada, el progreso de la tarea y su visualización
public class TaskStation : MonoBehaviour
{
    [Header("Configuración de la Tarea")]
    [SerializeField] private float taskDuration = 3f;
    [SerializeField] private Color freeColor = Color.green;
    [SerializeField] private Color occupiedColor = Color.yellow;
    [SerializeField] private Color completedColor = Color.cyan;

    private bool isOccupied = false;
    private bool isCompleted = false;
    private float taskProgress = 0f;
    private Crewmate currentUser = null;
    private SpriteRenderer spriteRenderer;

    // Propiedades públicas
    public bool IsOccupied => isOccupied;
    public bool IsCompleted => isCompleted;
    public float TaskDuration => taskDuration;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = freeColor;
        }
    }

    public bool TryOccupy(Crewmate user)
    {
        if (isOccupied || isCompleted)
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

    public bool UpdateTask(float deltaTime)
    {
        if (!isOccupied || isCompleted)
        {
            return false;
        }

        taskProgress += deltaTime;

        if (taskProgress >= taskDuration)
        {
            CompleteTask();
            return true;
        }

        return false;
    }

    private void CompleteTask()
    {
        isOccupied = false;
        isCompleted = true;
        currentUser = null;
        taskProgress = taskDuration;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = completedColor;
        }

        Debug.Log($"Tarea completada en {gameObject.name}");
    }

    public void ForceRelease()
    {
        if (isOccupied && !isCompleted)
        {
            isOccupied = false;
            currentUser = null;
            taskProgress = 0f;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = freeColor;
            }
        }
    }

    public void ResetStation()
    {
        isOccupied = false;
        isCompleted = false;
        currentUser = null;
        taskProgress = 0f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = freeColor;
        }
    }

    private void OnDrawGizmos()
    {
        if (isCompleted)
        {
            Gizmos.color = Color.cyan;
        }
        else if (isOccupied)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.green;
        }

        Gizmos.DrawWireSphere(transform.position, 1f);
    }

    public float GetTaskProgress()
    {
        return taskProgress / taskDuration;
    }
}

