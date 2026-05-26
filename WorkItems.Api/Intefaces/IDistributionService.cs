using WorkItems.Api.Dtos;
using WorkItems.Api.Models;

namespace WorkItems.Api.Intefaces
{
    /// <summary>
    /// Define las operaciones del motor encargado de distribuir tareas bajo reglas de negocio.
    /// </summary>
    public interface IDistributionService
    {
        /// <summary>
        /// Evalúa las reglas de negocio distribuyendo un ítem al usuario óptimo.
        /// </summary>
        string AllocateWorkItem(DateTime dueDate, WorkItemRelevance relevance, List<UserMetricsDto> userMetrics);

        /// <summary>
        /// Determina de forma pública si las métricas de un usuario superan el umbral de saturación.
        /// </summary>
        bool IsUserSaturated(UserMetricsDto user);
    }
}