using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackageFeatures_Packages_PackageId",
                table: "PackageFeatures");

            migrationBuilder.AddForeignKey(
                name: "FK_PackageFeatures_Packages_PackageId",
                table: "PackageFeatures",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PackageFeatures_Packages_PackageId",
                table: "PackageFeatures");

            migrationBuilder.AddForeignKey(
                name: "FK_PackageFeatures_Packages_PackageId",
                table: "PackageFeatures",
                column: "PackageId",
                principalTable: "Packages",
                principalColumn: "Id");
        }
    }
}
