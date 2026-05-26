using Bogus;
using Users.Api.Models;

namespace Users.Api.Data
{
    /// <summary>
    /// Clase encargada de alimentar la base de datos con registros simulados iniciales.
    /// </summary>
    public static class UserSeeder
    {
        /// <summary>
        /// Inserta usuarios aleatorios en la base de datos si esta no contiene registros.
        /// </summary>
        /// <param name="context">Instancia del contexto de la base de datos.</param>
        public static void Seed(UsersDbContext context)
        {
            if (context.Users.Any()) return;

            var faker = new Faker<User>("es")
                .RuleFor(u => u.FullName, f => f.Name.FullName())
                .RuleFor(u => u.Username, (f, u) => f.Internet.UserName(u.FullName.Split(' ')[0]).ToLower())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FullName.Split(' ')[0]));

            var fakeUsers = faker.Generate(100);

            context.Users.AddRange(fakeUsers);
            context.SaveChanges();
        }
    }
}