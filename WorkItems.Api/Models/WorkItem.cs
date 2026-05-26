using System.ComponentModel.DataAnnotations;

namespace WorkItems.Api.Models
{
    /// <summary>
    /// Entidad de dominio que representa un ítem de trabajo dentro del sistema.
    /// </summary>
    public class WorkItem
    {
        /// <summary>
        /// Identificador único de la tarea.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Título descriptivo de la asignación.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada de las actividades a realizar.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Fecha estipulada para la entrega del trabajo.
        /// </summary>
        [Required]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Nivel de relevancia (Low / High).
        /// </summary>
        [Required]
        public WorkItemRelevance Relevance { get; set; }

        /// <summary>
        /// Estado actual de la tarea (Pending / Completed).
        /// </summary>
        [Required]
        public WorkItemStatus Status { get; set; }

        /// <summary>
        /// Referencia única (Username) del usuario al que fue asignado el ítem.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string AssignedUsername { get; set; } = string.Empty;
    }
}