using WorkItems.Api.Dtos;
using WorkItems.Api.Intefaces;
using WorkItems.Api.Models;

namespace WorkItems.Api.Services
{
    /// <summary>
    /// Implementación del algoritmo core de asignación basado en prioridades y balanceo.
    /// </summary>
    public class DistributionService : IDistributionService
    {
        public string AllocateWorkItem(DateTime dueDate, WorkItemRelevance relevance, List<UserMetricsDto> userMetrics)
        {
            // Filtrar usando la función explícita de saturación
            var availableUsers = userMetrics
                .Where(u => !IsUserSaturated(u))
                .ToList();

            if (!availableUsers.Any())
            {
                throw new InvalidOperationException("No existen usuarios disponibles. Todos los colaboradores están saturados.");
            }

            // Regla: Menos de 3 días (Urgente)
            if ((dueDate.Date - DateTime.Today).TotalDays < 3)
            {
                return availableUsers.OrderBy(u => u.PendingCount).First().Username;
            }

            // Regla: Alta relevancia
            if (relevance == WorkItemRelevance.High)
            {
                return availableUsers.OrderBy(u => u.PendingCount).First().Username;
            }

            // Distribución estándar
            return availableUsers.OrderBy(u => u.PendingCount).First().Username;
        }
        public bool IsUserSaturated(UserMetricsDto user)
        {
            // Criterio de aceptación: Más de 3 ítems altamente relevantes pendientes
            return user.HighRelevancePendingCount > 3;
        }
    }
}