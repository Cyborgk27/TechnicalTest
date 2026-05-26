using WorkItems.Api.Dtos;
using WorkItems.Api.Models;

namespace WorkItems.Api.Intefaces
{
    /// <summary>
    /// Define las operaciones de caso de uso para la gestión, consulta y asignación de ítems de trabajo.
    /// </summary>
    public interface IWorkItemApplicationService
    {
        /// <summary>
        /// Coordina la obtención de usuarios, cálculo de métricas y asignación final del ítem.
        /// </summary>
        Task<WorkItem> AllocateAndCreateAsync(CreateWorkItemRequest request);

        /// <summary>
        /// Obtiene los ítems pendientes de un colaborador específico, ordenados bajo los criterios del negocio.
        /// </summary>
        Task<IEnumerable<WorkItem>> GetPendingByUserAsync(string username);

        /// <summary>
        /// Transiciona el estado de una tarea a completada liberando la carga laboral del colaborador.
        /// </summary>
        Task<WorkItem> CompleteWorkItemAsync(int id);
    }
}
