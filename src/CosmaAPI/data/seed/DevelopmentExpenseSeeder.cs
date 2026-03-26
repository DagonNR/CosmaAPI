using Bogus;
using CosmaAPI.data;
using CosmaAPI.data.seed;
using CosmaAPI.entities;
using CosmaAPI.entities.enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CosmaAPI.Data.Seed;

public static class DevelopmentExpenseSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        DevelopmentSeedingOptions options)
    {
        const string demoEmail = "demo@cosmaapi.com";
        const string demoPassword = "password123";

        var passwordHasher = new PasswordHasher<User>();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == demoEmail);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Demo User",
                Email = demoEmail,
                CreatedAtUtc = DateTime.UtcNow
            };

            user.PasswordHash = passwordHasher.HashPassword(user, demoPassword);

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var categories = await dbContext.Categories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync();

        if (categories.Count == 0)
        {
            throw new InvalidOperationException("Categorias activas no encontradas. Asegúrate de ejecutar el seeder de categorías antes de este.");
        }

        if (options.ResetExisting)
        {
            var existingExpenses = await dbContext.Expenses
                .Where(x => x.UserId == user.Id)
                .ToListAsync();

            if (existingExpenses.Count > 0)
            {
                dbContext.Expenses.RemoveRange(existingExpenses);
                await dbContext.SaveChangesAsync();
            }
        }
        else
        {
            var existingCount = await dbContext.Expenses
                .CountAsync(x => x.UserId == user.Id);

            if (existingCount >= options.ExpensesCount)
            {
                return;
            }
        }

        var expenseFaker = new Faker<Expense>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.UserId, _ => user.Id)
            .RuleFor(x => x.CreatedAtUtc, f => f.Date.RecentOffset(30).UtcDateTime)
            .RuleFor(x => x.UpdatedAtUtc, _ => null)
            .RuleFor(x => x.Date, f =>
            {
                var date = f.Date.Between(DateTime.Today.AddMonths(-12), DateTime.Today);
                return DateOnly.FromDateTime(date);
            })
            .RuleFor(x => x.CategoryId, f =>
            {
                var pick = f.PickRandom(categories);
                return pick.Id;
            })
            .RuleFor(x => x.IsEssential, f => f.Random.Bool(0.65f))
            .RuleFor(x => x.PaymentMethod, f => f.PickRandom<PaymentMethod>())
            .RuleFor(x => x.Description, (f, x) =>
            {
                var category = categories.First(c => c.Id == x.CategoryId).Name;

                return category switch
                {
                    "Comida" => f.PickRandom("Snacks", "Restaurante", "Supermercado", "Café"),
                    "Transporte" => f.PickRandom("Gasolina", "Transporte público", "Uber", "Mantenimiento del coche"),
                    "Entretenimiento" => f.PickRandom("Cine", "Concierto", "Videojuegos", "Salidas"),
                    "Renta" => f.PickRandom("Alquiler"),
                    "Salud" => f.PickRandom("Medicamentos", "Visita al médico", "Vitaminas"),
                    "Suscripciones" => f.PickRandom("Netflix", "Spotify", "Amazon Prime", "Software", "Youtube Premium", "HBO Max", "Disney+"),
                    "Compras" => f.PickRandom("Ropa", "Electrónica", "Hogar", "Regalos", "Amazon", "Callejeras"),
                    "Educación" => f.PickRandom("Libros", "Cursos en línea", "Escuela", "Materias reprobadas"),
                    "Servicios" => f.PickRandom("Limpieza", "Luz", "Agua", "Internet", "Teléfono"),
                    _ => f.PickRandom("Gasto general", "Varios", "Otros")
                };
            })
            .RuleFor(x => x.Amount, (f, x) =>
            {
                var category = categories.First(c => c.Id == x.CategoryId).Name;

                return category switch
                {
                    "Renta" => f.Random.Decimal(3500, 9000),
                    "Servicios" => f.Random.Decimal(150, 1800),
                    "Comida" => f.Random.Decimal(40, 700),
                    "Transporte" => f.Random.Decimal(20, 500),
                    "Entretenimiento" => f.Random.Decimal(80, 1200),
                    "Salud" => f.Random.Decimal(60, 2000),
                    "Suscripciones" => f.Random.Decimal(99, 400),
                    "Compras" => f.Random.Decimal(100, 3000),
                    "Educación" => f.Random.Decimal(120, 2500),
                    _ => f.Random.Decimal(50, 1500)
                };
            });

        var expenses = expenseFaker.Generate(options.ExpensesCount);

        dbContext.Expenses.AddRange(expenses);
        await dbContext.SaveChangesAsync();
    }
}