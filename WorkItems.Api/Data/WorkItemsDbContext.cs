using Microsoft.EntityFrameworkCore;
using WorkItems.Api.Models;

namespace WorkItems.Api.Data
{
    /// <summary>
    /// Contexto de Entity Framework Core para la administración de ítems de trabajo en SQL Server.
    /// </summary>
    public class WorkItemsDbContext : DbContext
    {
        public WorkItemsDbContext(DbContextOptions<WorkItemsDbContext> options) : base(options)
        {
        }

        public DbSet<WorkItem> WorkItems { get; set; }
    }
}
