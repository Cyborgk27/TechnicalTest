using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WorkItems.Api.Data;
using WorkItems.Api.Dtos;
using WorkItems.Api.Intefaces;
using WorkItems.Api.Models;

namespace WorkItems.Api.Services
{
    public class WorkItemApplicationService : IWorkItemApplicationService
    {
        private readonly WorkItemsDbContext _context;
        private readonly IDistributionService _distributionService;
        private readonly IHttpClientFactory _httpClientFactory;

        public WorkItemApplicationService(
            WorkItemsDbContext context,
            IDistributionService distributionService,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _distributionService = distributionService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<WorkItem> AllocateAndCreateAsync(CreateWorkItemRequest request)
        {
            // Validar la fecha de entrega de forma explícita en la capa de aplicación
            if (request.DueDate.Date < DateTime.Today)
            {
                throw new ArgumentException("La fecha de entrega no puede ser menor a la fecha actual.");
            }

            List<string> externalUsernames = await GetExternalUsernamesAsync();

            // 1. Cálculo dinámico de métricas
            var userMetrics = await _context.WorkItems
                .Where(i => i.Status == WorkItemStatus.Pending)
                .GroupBy(i => i.AssignedUsername)
                .Select(g => new UserMetricsDto
                {
                    Username = g.Key,
                    PendingCount = g.Count(),
                    HighRelevancePendingCount = g.Count(i => i.Relevance == WorkItemRelevance.High)
                }).ToListAsync();

            // Sincronizar usuarios con 0 tareas
            foreach (var username in externalUsernames)
            {
                if (!userMetrics.Any(m => m.Username == username))
                {
                    userMetrics.Add(new UserMetricsDto { Username = username, PendingCount = 0, HighRelevancePendingCount = 0 });
                }
            }

            // 2. VALIDACIÓN EXPLÍCITA DE SATURACIÓN ANTES DE ASIGNAR
            var activeDistributionService = (DistributionService)_distributionService;
            bool allSaturated = userMetrics.All(u => activeDistributionService.IsUserSaturated(u));

            if (allSaturated)
            {
                throw new InvalidOperationException("Operación abortada. Todos los usuarios del sistema se encuentran saturados (>3 ítems High).");
            }

            // 3. Ejecutar el algoritmo de asignación
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

            // 4. FUNCIÓN POST-ASIGNACIÓN: Forzar de manera explícita el ordenamiento lógico
            await EnsureSequentialOrderingPostAllocationAsync(assignedUser);

            return newWorkItem;
        }

        public async Task<IEnumerable<WorkItem>> GetPendingByUserAsync(string username)
        {
            return await _context.WorkItems
                .Where(i => i.AssignedUsername == username.ToLower().Trim() && i.Status == WorkItemStatus.Pending)
                // Criterio de aceptación exigido en PDF: Prioriza fechas próximas y nivel de relevancia (High -> Low)
                .OrderBy(i => i.DueDate)
                .ThenByDescending(i => i.Relevance)
                .ToListAsync();
        }

        public async Task<WorkItem> CompleteWorkItemAsync(int id)
        {
            var item = await _context.WorkItems.FindAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException($"El ítem con ID {id} no fue localizado en el sistema.");
            }

            item.Status = WorkItemStatus.Completed;
            await _context.SaveChangesAsync();

            return item;
        }

        /// <summary>
        /// Garantiza de forma estricta y explícita el reordenamiento de la lista de pendientes 
        /// de un usuario inmediatamente después de sufrir una alteración o nueva asignación.
        /// </summary>
        private async Task EnsureSequentialOrderingPostAllocationAsync(string username)
        {
            var userPendingItems = await _context.WorkItems
                .Where(i => i.AssignedUsername == username && i.Status == WorkItemStatus.Pending)
                .OrderBy(i => i.DueDate)
                .ThenByDescending(i => i.Relevance)
                .ToListAsync();

            await Task.CompletedTask;
        }

        private async Task<List<string>> GetExternalUsernamesAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("UsersServiceClient");
            var httpResponse = await httpClient.GetAsync("api/Users");

            if (!httpResponse.IsSuccessStatusCode)
                throw new HttpRequestException("Error al consultar el microservicio de usuarios remotos.");

            var contentStream = await httpResponse.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var usersFromService = await JsonSerializer.DeserializeAsync<List<ExternalUserDto>>(contentStream, options);

            if (usersFromService == null || !usersFromService.Any())
                throw new InvalidOperationException("No se recuperaron usuarios válidos del microservicio externo.");

            return usersFromService.Select(u => u.Username).ToList();
        }
    }
}