using Microsoft.AspNetCore.Mvc;
using WorkItems.Api.Dtos;
using WorkItems.Api.Intefaces;
using WorkItems.Api.Models;

namespace WorkItems.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkItemsController : ControllerBase
    {
        private readonly IWorkItemApplicationService _workItemAppService;

        public WorkItemsController(IWorkItemApplicationService workItemAppService)
        {
            _workItemAppService = workItemAppService;
        }

        /// <summary>
        /// Crea un nuevo ítem de trabajo, validando sus parámetros y aplicando las prioridades de entrega y relevancia.
        /// </summary>
        [HttpPost("allocate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkItem))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> CreateAndAllocate([FromBody] CreateWorkItemRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { Error = "El título del ítem de trabajo es mandatorio." });
            }

            try
            {
                var result = await _workItemAppService.AllocateAndCreateAsync(request);
                return Ok(new { Message = "Ítem distribuido y guardado con éxito.", Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = "Datos de entrada inválidos.", Details = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = "Regla de Distribución Incumplida.", Details = ex.Message });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Error = "Falla de comunicación en la arquitectura de microservicios.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Consulta la lista de pendientes de un usuario específico de forma ordenada.
        /// </summary>
        [HttpGet("user/{username}/pending")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkItem>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPendingByUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new { Error = "El nombre de usuario es requerido para la consulta." });
            }

            var pendingItems = await _workItemAppService.GetPendingByUserAsync(username);
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
            try
            {
                var item = await _workItemAppService.CompleteWorkItemAsync(id);
                return Ok(new { Message = "Ítem de trabajo completado con éxito. Carga liberada.", Item = item });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}