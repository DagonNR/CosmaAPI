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

        var categoryNamesById = categories.ToDictionary(x => x.Id, x => x.Name);

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
            .RuleFor(x => x.Date, f =>
            {
                var date = f.Date.Between(DateTime.Today.AddMonths(-12), DateTime.Today);
                return DateOnly.FromDateTime(date);
            })
            .RuleFor(x => x.CreatedAtUtc, (f, x) =>
            {
                var localDate = x.Date.ToDateTime(TimeOnly.MinValue);
                var dateTime = new DateTime(
                    localDate.Year,
                    localDate.Month,
                    localDate.Day,
                    f.Random.Int(7, 22),
                    f.Random.Int(0, 59),
                    f.Random.Int(0, 59),
                    DateTimeKind.Local);

                return dateTime.ToUniversalTime();
            })
            .RuleFor(x => x.UpdatedAtUtc, _ => null)
            .RuleFor(x => x.CategoryId, f =>
            {
                var pick = f.PickRandom(categories);
                return pick.Id;
            })
            .RuleFor(x => x.Description, (f, x) =>
            {
                var category = categoryNamesById[x.CategoryId];
                return GetRandomDescription(f, category);
            })
            .RuleFor(x => x.Amount, (f, x) =>
            {
                var category = categoryNamesById[x.CategoryId];
                return GenerateAmount(f, category, x.Description);
            })
            .RuleFor(x => x.IsEssential, (f, x) =>
            {
                var category = categoryNamesById[x.CategoryId];
                return GenerateIsEssential(f, category, x.Description);
            })
            .RuleFor(x => x.PaymentMethod, f => f.PickRandom<PaymentMethod>());

        var expenses = expenseFaker.Generate(options.ExpensesCount);

        dbContext.Expenses.AddRange(expenses);
        await dbContext.SaveChangesAsync();
    }

    private static string GetRandomDescription(Faker f, string category)
    {
        return category switch
        {
            "Comida" => f.PickRandom("Snacks", "Restaurante", "Supermercado", "Café"),
            "Transporte" => f.PickRandom("Gasolina", "Transporte público", "Uber", "Mantenimiento del coche"),
            "Entretenimiento" => f.PickRandom("Cine", "Concierto", "Videojuegos", "Salidas"),
            "Renta" => "Alquiler",
            "Salud" => f.PickRandom("Medicamentos", "Visita al médico", "Vitaminas"),
            "Suscripciones" => f.PickRandom("Netflix", "Spotify", "Amazon Prime", "Software", "Youtube Premium", "HBO Max", "Disney+"),
            "Compras" => f.PickRandom("Ropa", "Electrónica", "Hogar", "Regalos", "Amazon", "Compras pequeñas"),
            "Educación" => f.PickRandom("Libros", "Cursos en línea", "Escuela", "Pago académico"),
            "Servicios" => f.PickRandom("Limpieza", "Luz", "Agua", "Internet", "Teléfono"),
            _ => f.PickRandom("Gasto general", "Varios", "Otros")
        };
    }

    private static decimal GenerateAmount(Faker f, string category, string description)
    {
        return (category, description) switch
        {
            // COMIDA
            ("Comida", "Snacks") =>
                WeightedAmount(f, (20m, 45m, 45), (46m, 80m, 40), (81m, 140m, 15)),

            ("Comida", "Restaurante") =>
                WeightedAmount(f, (120m, 220m, 35), (221m, 380m, 50), (381m, 650m, 15)),

            ("Comida", "Supermercado") =>
                WeightedAmount(f, (180m, 450m, 25), (451m, 1100m, 50), (1101m, 2200m, 25)),

            ("Comida", "Café") =>
                WeightedAmount(f, (45m, 75m, 50), (76m, 110m, 35), (111m, 160m, 15)),

            // TRANSPORTE
            ("Transporte", "Gasolina") =>
                WeightedAmount(f, (250m, 500m, 35), (501m, 850m, 45), (851m, 1300m, 20)),

            ("Transporte", "Transporte público") =>
                WeightedAmount(f, (12m, 15m, 65), (16m, 30m, 25), (31m, 100m, 10)),

            ("Transporte", "Uber") =>
                WeightedAmount(f, (60m, 120m, 40), (121m, 220m, 45), (221m, 380m, 15)),

            ("Transporte", "Mantenimiento del coche") =>
                WeightedAmount(f, (300m, 900m, 40), (901m, 2500m, 40), (2501m, 6000m, 20)),

            // ENTRETENIMIENTO
            ("Entretenimiento", "Cine") =>
                WeightedAmount(f, (90m, 140m, 50), (141m, 250m, 35), (251m, 420m, 15)),

            ("Entretenimiento", "Concierto") =>
                WeightedAmount(f, (500m, 1200m, 40), (1201m, 2500m, 40), (2501m, 4500m, 20)),

            ("Entretenimiento", "Videojuegos") =>
                WeightedAmount(f, (80m, 300m, 30), (301m, 900m, 40), (901m, 1600m, 30)),

            ("Entretenimiento", "Salidas") =>
                WeightedAmount(f, (120m, 300m, 35), (301m, 700m, 45), (701m, 1500m, 20)),

            // RENTA
            ("Renta", "Alquiler") =>
                WeightedAmount(f, (7500m, 11000m, 25), (11001m, 16000m, 50), (16001m, 25000m, 25)),

            // SALUD
            ("Salud", "Medicamentos") =>
                WeightedAmount(f, (80m, 220m, 45), (221m, 500m, 40), (501m, 1200m, 15)),

            ("Salud", "Visita al médico") =>
                WeightedAmount(f, (350m, 500m, 35), (501m, 900m, 45), (901m, 1500m, 20)),

            ("Salud", "Vitaminas") =>
                WeightedAmount(f, (120m, 250m, 45), (251m, 450m, 40), (451m, 800m, 15)),

            // SUSCRIPCIONES
            ("Suscripciones", "Spotify") =>
                WeightedFixed(f, (74m, 15), (139m, 50), (189m, 15), (239m, 20)),

            ("Suscripciones", "Amazon Prime") =>
                99m,

            ("Suscripciones", "Disney+") =>
                WeightedFixed(f, (49.90m, 20), (89m, 40), (99m, 40)),

            ("Suscripciones", "HBO Max") =>
                WeightedFixed(f, (149m, 45), (239m, 55)),

            ("Suscripciones", "Netflix") =>
                WeightedAmount(f, (99m, 119m, 40), (120m, 179m, 35), (180m, 249m, 25)),

            ("Suscripciones", "Youtube Premium") =>
                WeightedAmount(f, (79m, 99m, 30), (100m, 159m, 50), (160m, 220m, 20)),

            ("Suscripciones", "Software") =>
                WeightedFixed(f, (99m, 20), (184.99m, 35), (249m, 15), (374.99m, 30)),

            // COMPRAS
            ("Compras", "Ropa") =>
                WeightedAmount(f, (250m, 700m, 40), (701m, 1500m, 40), (1501m, 3000m, 20)),

            ("Compras", "Electrónica") =>
                WeightedAmount(f, (300m, 1200m, 35), (1201m, 3500m, 40), (3501m, 12000m, 25)),

            ("Compras", "Hogar") =>
                WeightedAmount(f, (100m, 500m, 35), (501m, 1500m, 45), (1501m, 3000m, 20)),

            ("Compras", "Regalos") =>
                WeightedAmount(f, (150m, 500m, 35), (501m, 1200m, 45), (1201m, 2500m, 20)),

            ("Compras", "Amazon") =>
                WeightedAmount(f, (120m, 500m, 30), (501m, 1800m, 45), (1801m, 4000m, 25)),

            ("Compras", "Compras pequeñas") =>
                WeightedAmount(f, (50m, 120m, 45), (121m, 250m, 40), (251m, 500m, 15)),

            // EDUCACIÓN
            ("Educación", "Libros") =>
                WeightedAmount(f, (120m, 250m, 40), (251m, 500m, 45), (501m, 900m, 15)),

            ("Educación", "Cursos en línea") =>
                WeightedAmount(f, (150m, 500m, 30), (501m, 1500m, 45), (1501m, 3000m, 25)),

            ("Educación", "Escuela") =>
                WeightedAmount(f, (600m, 1200m, 25), (1201m, 2500m, 45), (2501m, 4000m, 30)),

            ("Educación", "Pago académico") =>
                WeightedAmount(f, (700m, 1200m, 30), (1201m, 2200m, 45), (2201m, 4000m, 25)),

            // SERVICIOS
            ("Servicios", "Limpieza") =>
                WeightedAmount(f, (80m, 200m, 40), (201m, 450m, 45), (451m, 900m, 15)),

            ("Servicios", "Luz") =>
                WeightedAmount(f, (250m, 600m, 35), (601m, 1200m, 45), (1201m, 2200m, 20)),

            ("Servicios", "Agua") =>
                WeightedAmount(f, (100m, 250m, 45), (251m, 500m, 40), (501m, 800m, 15)),

            ("Servicios", "Internet") =>
                WeightedAmount(f, (389m, 500m, 35), (501m, 650m, 45), (651m, 850m, 20)),

            ("Servicios", "Teléfono") =>
                WeightedAmount(f, (100m, 250m, 30), (251m, 500m, 45), (501m, 800m, 25)),

            // DEFAULT
            _ =>
                WeightedAmount(f, (50m, 150m, 45), (151m, 500m, 40), (501m, 1500m, 15))
        };
    }

    private static bool GenerateIsEssential(Faker f, string category, string description)
    {
        return (category, description) switch
        {
            ("Renta", _) => f.Random.Bool(0.98f),

            ("Servicios", "Luz") => f.Random.Bool(0.98f),
            ("Servicios", "Agua") => f.Random.Bool(0.98f),
            ("Servicios", "Internet") => f.Random.Bool(0.95f),
            ("Servicios", "Teléfono") => f.Random.Bool(0.85f),
            ("Servicios", "Limpieza") => f.Random.Bool(0.40f),

            ("Comida", "Supermercado") => f.Random.Bool(0.95f),
            ("Comida", "Restaurante") => f.Random.Bool(0.30f),
            ("Comida", "Snacks") => f.Random.Bool(0.20f),
            ("Comida", "Café") => f.Random.Bool(0.15f),

            ("Transporte", "Gasolina") => f.Random.Bool(0.90f),
            ("Transporte", "Transporte público") => f.Random.Bool(0.95f),
            ("Transporte", "Uber") => f.Random.Bool(0.45f),
            ("Transporte", "Mantenimiento del coche") => f.Random.Bool(0.90f),

            ("Entretenimiento", _) => f.Random.Bool(0.05f),

            ("Salud", _) => f.Random.Bool(0.95f),

            ("Suscripciones", "Software") => f.Random.Bool(0.60f),
            ("Suscripciones", _) => f.Random.Bool(0.10f),

            ("Compras", "Hogar") => f.Random.Bool(0.65f),
            ("Compras", "Ropa") => f.Random.Bool(0.25f),
            ("Compras", "Electrónica") => f.Random.Bool(0.20f),
            ("Compras", "Regalos") => f.Random.Bool(0.05f),
            ("Compras", "Amazon") => f.Random.Bool(0.30f),
            ("Compras", "Compras pequeñas") => f.Random.Bool(0.10f),

            ("Educación", _) => f.Random.Bool(0.90f),

            _ => f.Random.Bool(0.50f)
        };
    }

    private static decimal WeightedAmount(Faker f, params (decimal Min, decimal Max, int Weight)[] ranges)
    {
        var totalWeight = ranges.Sum(r => r.Weight);
        var pick = f.Random.Int(1, totalWeight);
        var accumulated = 0;

        foreach (var range in ranges)
        {
            accumulated += range.Weight;
            if (pick <= accumulated)
            {
                return Math.Round(f.Random.Decimal(range.Min, range.Max), 2);
            }
        }

        var last = ranges[^1];
        return Math.Round(f.Random.Decimal(last.Min, last.Max), 2);
    }

    private static decimal WeightedFixed(Faker f, params (decimal Value, int Weight)[] options)
    {
        var totalWeight = options.Sum(o => o.Weight);
        var pick = f.Random.Int(1, totalWeight);
        var accumulated = 0;

        foreach (var option in options)
        {
            accumulated += option.Weight;
            if (pick <= accumulated)
            {
                return option.Value;
            }
        }

        return options[^1].Value;
    }
}