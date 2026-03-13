using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CosmaAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "Id", "ColorHex", "Icon", "IsActive", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "#EF4444", "food", true, "Comida" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "#3B82F6", "transport", true, "Transporte" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "#8B5CF6", "home", true, "Renta" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "#F59E0B", "entertainment", true, "Entretenimiento" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "#10B981", "health", true, "Salud" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "#EC4899", "suscription", true, "Suscripciones" },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "#6366F1", "shopping", true, "Compras" },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "#14B8A6", "education", true, "Educación" },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "#84CC16", "services", true, "Servicios" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "#6B7280", "other", true, "Otros" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"));
        }
    }
}
