namespace CosmaAPI.data.seed;

public static class CategorySeedData
{
    public static readonly Guid FoodId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid TransportId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid RentId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid EntertainmentId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid HealthId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid SuscriptionsId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid ShoppingId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid EducationId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static readonly Guid ServicesId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    public static readonly Guid OtherId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    public static object[] GetSeedData()
    {
        return new object[]
        {
            new
            {
                Id = FoodId,
                Name = "Comida",
                ColorHex = "#EF4444",
                Icon = "food",
                IsActive = true
            },
            new
            {
                Id = TransportId,
                Name = "Transporte",
                ColorHex = "#3B82F6",
                Icon = "transport",
                IsActive = true
            },
            new
            {
                Id = RentId,
                Name = "Renta",
                ColorHex = "#8B5CF6",
                Icon = "home",
                IsActive = true
            },
            new
            {
                Id = EntertainmentId,
                Name = "Entretenimiento",
                ColorHex = "#F59E0B",
                Icon = "entertainment",
                IsActive = true
            },
            new
            {
                Id = HealthId,
                Name = "Salud",
                ColorHex = "#10B981",
                Icon = "health",
                IsActive = true
            },
            new
            {
                Id = SuscriptionsId,
                Name = "Suscripciones",
                ColorHex = "#EC4899",
                Icon = "suscription",
                IsActive = true
            },
            new
            {
                Id = ShoppingId,
                Name = "Compras",
                ColorHex = "#6366F1",
                Icon = "shopping",
                IsActive = true
            },
            new
            {
                Id = EducationId,
                Name = "Educación",
                ColorHex = "#14B8A6",
                Icon = "education",
                IsActive = true
            },
            new
            {
                Id = ServicesId,
                Name = "Servicios",
                ColorHex = "#84CC16",
                Icon = "services",
                IsActive = true
            },
            new
            {
                Id = OtherId,
                Name = "Otros",
                ColorHex = "#6B7280",
                Icon = "other",
                IsActive = true
            },
        };
    }
}