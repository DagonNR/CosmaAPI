using CosmaAPI.data;
using CosmaAPI.entities;
using CosmaAPI.services.implementations;
using CosmaAPI.services.interfaces;
using CosmaAPI.auth;
using CosmaAPI.middleware;
using CosmaAPI.data.seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CosmaAPI.Services.Implementations;
using Microsoft.OpenApi;
using CosmaAPI.Data.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CosmaAPI",
        Version = "v1",
        Description = "Personal expense tracking API."
    });

    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste only the JWT token here"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new("bearer", document)] = new List<string>()
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.Configure<DevelopmentSeedingOptions>(
    builder.Configuration.GetSection(DevelopmentSeedingOptions.SectionName));
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var seedingOptions = configuration
        .GetSection(DevelopmentSeedingOptions.SectionName)
        .Get<DevelopmentSeedingOptions>() ?? new DevelopmentSeedingOptions();

    if (seedingOptions.Enabled)
    {
        await DevelopmentExpenseSeeder.SeedAsync(dbContext, seedingOptions);
    }
}

app.Run();