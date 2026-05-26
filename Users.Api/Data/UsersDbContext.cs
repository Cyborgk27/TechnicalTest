using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using Users.Api.Models;

namespace Users.Api.Data
{
    /// <summary>
    /// Contexto de Entity Framework Core para la gestión de usuarios en SQL Server.
    /// </summary>
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Colección de datos de los usuarios en la base de datos.
        /// </summary>
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}