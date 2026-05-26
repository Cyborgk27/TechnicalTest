using System.ComponentModel.DataAnnotations;

namespace Users.API.Models
{
    /// <summary>
    /// Representa la entidad de usuario dentro del sistema de distribución.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre de usuario único (ej. juan.perez) utilizado como referencia externa.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo del colaborador.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico institucional.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
    }
}