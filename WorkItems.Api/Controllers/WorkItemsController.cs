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
        private readonly IWorkItemApplicationService _workItemAppService;

        public WorkItemsController(WorkItemsDbContext context, IWorkItemApplicationService workItemAppService)
        {
            _context = context;
            _workItemAppService = workItemAppService;
        }

        /// <summary>
        /// Crea un nuevo ítem de trabajo y delega la asignación automática al servicio de aplicación.
        /// </summary>
        [HttpPost("allocate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkItem))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAndAllocate([FromBody] CreateWorkItemRequest request)
        {
            try
            {
                var result = await _workItemAppService.AllocateAndCreateAsync(request);
                return Ok(new { Message = $"Ítem asignado exitosamente a: {result.AssignedUsername}", Data = result });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Error = "El microservicio de usuarios no se encuentra disponible o respondió con error.", Details = ex.Message });
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