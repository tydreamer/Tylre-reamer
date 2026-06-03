using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodDelivery.API.Migrations;

/// <inheritdoc />
public partial class DropMealIsAvailableColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsAvailable",
            table: "Meals");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsAvailable",
            table: "Meals",
            type: "boolean",
            nullable: false,
            defaultValue: true);
    }
}
