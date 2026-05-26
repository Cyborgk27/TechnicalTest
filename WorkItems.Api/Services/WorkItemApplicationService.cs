using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WorkItems.Api.Data;
using WorkItems.Api.Dtos;
using WorkItems.Api.Models;

namespace WorkItems.Api.Services
{
    /// <summary>
    /// Servicio que actúa como fachada para orquestar la lógica de negocio de WorkItems.
    /// </summary>
    public class WorkItemApplicationService : IWorkItemApplicationService
    {
        private readonly WorkItemsDbContext _context;
        private readonly IDistributionService _distributionService;
        private readonly IHttpClientFactory _httpClientFactory;

        // Simulación de los usuarios conocidos del sistema para redundancia
        private readonly List<string> _fallbackUsernames = new() { "juan.perez", "maria.gomez", "pedro.vaca", "ana.silva" };

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
            List<string> externalUsernames = new();

            // 1. Obtener usuarios desde el microservicio externo
            var httpClient = _httpClientFactory.CreateClient("UsersServiceClient");
            var httpResponse = await httpClient.GetAsync("api/Users");

            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error de comunicación externa: {httpResponse.StatusCode}");
            }

            var contentStream = await httpResponse.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var usersFromService = await JsonSerializer.DeserializeAsync<List<ExternalUserDto>>(contentStream, options);

            if (usersFromService != null && usersFromService.Any())
            {
                externalUsernames = usersFromService.Select(u => u.Username).ToList();
            }
            else
            {
                externalUsernames = _fallbackUsernames;
            }

            // 2. Calcular métricas actuales
            var userMetrics = await _context.WorkItems
                .Where(i => i.Status == WorkItemStatus.Pending)
                .GroupBy(i => i.AssignedUsername)
                .Select(g => new UserMetricsDto
                {
                    Username = g.Key,
                    PendingCount = g.Count(),
                    HighRelevancePendingCount = g.Count(i => i.Relevance == WorkItemRelevance.High)
                }).ToListAsync();

            // Asegurar mapeo completo de usuarios con 0 tareas
            foreach (var username in externalUsernames)
            {
                if (!userMetrics.Any(m => m.Username == username))
                {
                    userMetrics.Add(new UserMetricsDto { Username = username, PendingCount = 0, HighRelevancePendingCount = 0 });
                }
            }

            // 3. Ejecutar algoritmo puro de asignación
            string assignedUser = _distributionService.AllocateWorkItem(request.DueDate, request.Relevance, userMetrics);

            // 4. Persistir la nueva entidad
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

            return newWorkItem;
        }
    }
}