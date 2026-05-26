using WorkItems.Api.Models;

namespace WorkItems.Api.Dtos
{
    /// <summary>
    /// Request DTO para la creación estructurada de una nueva tarea.
    /// </summary>
    public class CreateWorkItemRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public WorkItemRelevance Relevance { get; set; }
    }
}
