namespace WorkItems.Api.Models
{
    /// <summary>
    /// Define el nivel de importancia de un ítem de trabajo.
    /// </summary>
    public enum WorkItemRelevance
    {
        Low,
        High
    }

    /// <summary>
    /// Representa el ciclo de vida o estado actual del ítem de trabajo.
    /// </summary>
    public enum WorkItemStatus
    {
        Pending,
        Completed
    }
}