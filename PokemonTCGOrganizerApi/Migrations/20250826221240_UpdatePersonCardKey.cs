using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonTCGOrganizer.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonCardKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonCards",
                table: "PersonCards");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonCards",
                table: "PersonCards",
                columns: new[] { "PersonId", "PokemonCardId", "Printing", "Language" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonCards",
                table: "PersonCards");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonCards",
                table: "PersonCards",
                columns: new[] { "PersonId", "PokemonCardId", "Condition", "Language" });
        }
    }
}
