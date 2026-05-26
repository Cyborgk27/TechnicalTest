using WorkItems.Api.Dtos;
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
            // Regla: Filtrar usuarios saturados (Más de 3 ítems pendientes altamente relevantes)
            var availableUsers = userMetrics
                .Where(u => u.HighRelevancePendingCount <= 3)
                .ToList();

            if (!availableUsers.Any())
            {
                throw new InvalidOperationException("No existen usuarios disponibles. Todos los colaboradores están saturados.");
            }

            // Regla: Próxima a vencer (menos de 3 días desde la fecha actual)
            bool isUrgent = (dueDate.Date - DateTime.Today).TotalDays < 3;

            if (isUrgent)
            {
                // Se asigna al usuario con menos ítems pendientes independientemente de la relevancia
                var targetUser = availableUsers
                    .OrderBy(u => u.PendingCount)
                    .First();

                return targetUser.Username;
            }

            // Regla: Los ítems relevantes se asignan primero a quienes tienen menor lista de pendientes
            if (relevance == WorkItemRelevance.High)
            {
                var targetUser = availableUsers
                    .OrderBy(u => u.PendingCount)
                    .First();

                return targetUser.Username;
            }
            else
            {
                // Distribución estándar por balanceo de carga para ítems normales
                var targetUser = availableUsers
                    .OrderBy(u => u.PendingCount)
                    .First();

                return targetUser.Username;
            }
        }
    }
}