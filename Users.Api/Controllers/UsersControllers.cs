using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Users.API.Data;
using Users.API.Models;

namespace Users.API.Controllers
{
    /// <summary>
    /// Controlador encargado de exponer los servicios del microservicio de usuarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersDbContext _context;

        public UsersController(UsersDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene el listado completo de usuarios registrados en el sistema.
        /// </summary>
        /// <returns>Una lista de usuarios.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<User>))]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        /// <summary>
        /// Obtiene un usuario específico utilizando su nombre de usuario (Username).
        /// </summary>
        /// <param name="username">Nombre de usuario único de referencia.</param>
        /// <returns>El usuario solicitado o un estado 404 si no existe.</returns>
        [HttpGet("{username}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByUsername(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username.ToLower().Trim());

            if (user == null)
            {
                return NotFound(new { Message = $"El usuario '{username}' no fue encontrado." });
            }

            return Ok(user);
        }
    }
}