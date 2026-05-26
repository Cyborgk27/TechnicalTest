using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkItems.Api.Data;
using WorkItems.Api.Dtos;
using WorkItems.Api.Models;
using WorkItems.Api.Services;

namespace WorkItems.Api.Controllers
{
    /// <summary>
    /// API encargada del control y distribución automatizada de las órdenes de trabajo.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkItemsController : ControllerBase
    {
        private readonly WorkItemsDbContext _context;
        private readonly IDistributionService _distributionService;

        // Usuarios del sistema que operan de forma externa
        private readonly List<string> _externalUsernames = new() { "juan.perez", "maria.gomez", "pedro.vaca", "ana.silva" };

        public WorkItemsController(WorkItemsDbContext context, IDistributionService distributionService)
        {
            _context = context;
            _distributionService = distributionService;
        }

        /// <summary>
        /// Crea un nuevo ítem de trabajo y ejecuta el algoritmo para asignarlo al usuario ideal.
        /// </summary>
        [HttpPost("allocate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkItem))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAndAllocate([FromBody] CreateWorkItemRequest request)
        {
            // 1. Calcular métricas en tiempo real desde la base de datos SQL Server
            var userMetrics = await _context.WorkItems
                .Where(i => i.Status == WorkItemStatus.Pending)
                .GroupBy(i => i.AssignedUsername)
                .Select(g => new UserMetricsDto
                {
                    Username = g.Key,
                    PendingCount = g.Count(),
                    HighRelevancePendingCount = g.Count(i => i.Relevance == WorkItemRelevance.High)
                }).ToListAsync();

            // Asegurar que todos los usuarios conocidos estén mapeados aunque tengan 0 tareas
            foreach (var username in _externalUsernames)
            {
                if (!userMetrics.Any(m => m.Username == username))
                {
                    userMetrics.Add(new UserMetricsDto { Username = username, PendingCount = 0, HighRelevancePendingCount = 0 });
                }
            }

            try
            {
                // 2. Correr algoritmo de asignación
                string assignedUser = _distributionService.AllocateWorkItem(request.DueDate, request.Relevance, userMetrics);

                var newWorkItem = new WorkItem
                {
                    Title = request.Title,
                    Description = request.Description,
                    DueDate = request.DueDate,
                    Relevance = request.Relevance,
                    Status = WorkItemStatus.Pending,
                    AssignedUsername = assignedUser
                };

                _context.WorkItems.Add(newWorkItem);
                await _context.SaveChangesAsync();

                return Ok(new { Message = $"Ítem asignado a exitosamente a: {assignedUser}", Data = newWorkItem });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Consulta la lista de pendientes de un usuario específico de forma ordenada.
        /// </summary>
        [HttpGet("user/{username}/pending")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkItem>))]
        public async Task<IActionResult> GetPendingByUser(string username)
        {
            // Regla: Ordenar la lista de pendientes por usuario después de cada asignación (ordenado por fecha de entrega y relevancia)
            var pendingItems = await _context.WorkItems
                .Where(i => i.AssignedUsername == username.ToLower().Trim() && i.Status == WorkItemStatus.Pending)
                .OrderBy(i => i.DueDate)
                .ThenByDescending(i => i.Relevance)
                .ToListAsync();

            return Ok(pendingItems);
        }

        /// <summary>
        /// Marca un ítem de trabajo existente como completado, liberando carga del colaborador.
        /// </summary>
        [HttpPut("{id}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteWorkItem(int id)
        {
            var item = await _context.WorkItems.FindAsync(id);
            if (item == null) return NotFound(new { Message = "El ítem solicitado no existe." });

            item.Status = WorkItemStatus.Completed;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Ítem de trabajo completado con éxito.", Item = item });
        }
    }
}
