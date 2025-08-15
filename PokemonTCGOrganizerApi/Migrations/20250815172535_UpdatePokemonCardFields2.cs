using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonTCGOrganizer.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePokemonCardFields2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Condition",
                table: "PokemonCards");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "PokemonCards");

            migrationBuilder.DropColumn(
                name: "Printing",
                table: "PokemonCards");

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "PersonCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "PersonCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Printing",
                table: "PersonCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Condition",
                table: "PersonCards");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "PersonCards");

            migrationBuilder.DropColumn(
                name: "Printing",
                table: "PersonCards");

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "PokemonCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "PokemonCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Printing",
                table: "PokemonCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
