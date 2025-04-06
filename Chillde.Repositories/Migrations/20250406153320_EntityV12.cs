using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Packages_OfferId",
                table: "Packages");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_OfferId",
                table: "Packages",
                column: "OfferId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Packages_OfferId",
                table: "Packages");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_OfferId",
                table: "Packages",
                column: "OfferId");
        }
    }
}
