using WorkItems.Api.Dtos;
using WorkItems.Api.Models;

namespace WorkItems.Api.Services
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
    }
}