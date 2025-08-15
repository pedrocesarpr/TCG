using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonTCGOrganizer.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePokemonCardFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "PokemonCards");

            migrationBuilder.DropColumn(
                name: "TradeQuantity",
                table: "PokemonCards");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "PokemonCards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TradeQuantity",
                table: "PokemonCards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
