using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FoodDelivery.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMealTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MealTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealTypes_Name",
                table: "MealTypes",
                column: "Name",
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO "MealTypes" ("Name") VALUES
                ('Breakfast'),
                ('Lunch'),
                ('Dinner'),
                ('Appetizers'),
                ('Dessert');
                """);

            migrationBuilder.AddColumn<int>(
                name: "MealTypeId",
                table: "Meals",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Meals"
                SET "MealTypeId" = (SELECT "Id" FROM "MealTypes" WHERE "Name" = 'Lunch' LIMIT 1);
                """);

            migrationBuilder.AlterColumn<int>(
                name: "MealTypeId",
                table: "Meals",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meals_MealTypeId",
                table: "Meals",
                column: "MealTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meals_MealTypes_MealTypeId",
                table: "Meals",
                column: "MealTypeId",
                principalTable: "MealTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meals_MealTypes_MealTypeId",
                table: "Meals");

            migrationBuilder.DropTable(
                name: "MealTypes");

            migrationBuilder.DropIndex(
                name: "IX_Meals_MealTypeId",
                table: "Meals");

            migrationBuilder.DropColumn(
                name: "MealTypeId",
                table: "Meals");
        }
    }
}
