using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonTCGOrganizer.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonCards",
                table: "PersonCards");

            migrationBuilder.DropIndex(
                name: "IX_PersonCards_PersonId",
                table: "PersonCards");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonCards",
                table: "PersonCards",
                columns: new[] { "PersonId", "PokemonCardId", "Condition", "Language" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonCards_PokemonCardId",
                table: "PersonCards",
                column: "PokemonCardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonCards",
                table: "PersonCards");

            migrationBuilder.DropIndex(
                name: "IX_PersonCards_PokemonCardId",
                table: "PersonCards");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonCards",
                table: "PersonCards",
                columns: new[] { "PokemonCardId", "PersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonCards_PersonId",
                table: "PersonCards",
                column: "PersonId");
        }
    }
}
