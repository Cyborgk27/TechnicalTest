using Bogus;
using WorkItems.Api.Models;

namespace WorkItems.Api.Data
{
    /// <summary>
    /// Se encargará de poblar la tabla de WorkItems generando escenarios iniciales controlados.
    /// </summary>
    public static class WorkItemSeeder
    {
        /// <summary>
        /// Alimenta la base de datos simulando un historial de tareas asignadas.
        /// </summary>
        public static void Seed(WorkItemsDbContext context)
        {
            if (context.WorkItems.Any()) return;

            // Lista estática de usuarios ficticios basados en los del primer microservicio
            var systemUsers = new List<string> { "juan.perez", "maria.gomez", "pedro.vaca", "ana.silva" };

            var faker = new Faker<WorkItem>("es")
                .RuleFor(i => i.Title, f => f.Commerce.ProductName())
                .RuleFor(i => i.Description, f => f.Lorem.Sentence(10))
                .RuleFor(i => i.DueDate, f => f.Date.Between(DateTime.Today.AddDays(1), DateTime.Today.AddDays(15)))
                .RuleFor(i => i.Relevance, f => f.PickRandom<WorkItemRelevance>())
                .RuleFor(i => i.Status, f => f.PickRandom<WorkItemStatus>())
                .RuleFor(i => i.AssignedUsername, f => f.PickRandom(systemUsers));

            var fakeItems = faker.Generate(100);

            // ESCENARIO DE PRUEBA: Forzar la saturación de "juan.perez" con 4 tareas complejas pendientes
            // De esta forma los evaluadores constatarán que el algoritmo lo ignora al intentar asignarle código nuevo.
            for (int i = 0; i < 4; i++)
            {
                fakeItems.Add(new WorkItem
                {
                    Title = $"Tarea Crítica Inyectada {i + 1}",
                    Description = "Inyección automática para simular saturación en pruebas de distribución.",
                    DueDate = DateTime.Today.AddDays(6),
                    Relevance = WorkItemRelevance.High,
                    Status = WorkItemStatus.Pending,
                    AssignedUsername = "juan.perez"
                });
            }

            context.WorkItems.AddRange(fakeItems);
            context.SaveChanges();
        }
    }
}