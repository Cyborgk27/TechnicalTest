using WorkItems.Api.Dtos;
using WorkItems.Api.Models;

namespace WorkItems.Api.Services
{
    /// <summary>
    /// Define las operaciones de caso de uso para la gestión y asignación de ítems de trabajo.
    /// </summary>
    public interface IWorkItemApplicationService
    {
        /// <summary>
        /// Coordina la obtención de usuarios, cálculo de métricas y asignación final del ítem.
        /// </summary>
        Task<WorkItem> AllocateAndCreateAsync(CreateWorkItemRequest request);
    }
}
