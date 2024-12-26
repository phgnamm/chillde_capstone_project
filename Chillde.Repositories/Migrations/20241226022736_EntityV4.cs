using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Accounts",
                newName: "StoreDescription");

            migrationBuilder.AddColumn<string>(
                name: "Banner",
                table: "Accounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StoreAddress",
                table: "Accounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SuccessDeliveryRate",
                table: "Accounts",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Banner",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "StoreAddress",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SuccessDeliveryRate",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "StoreDescription",
                table: "Accounts",
                newName: "Address");
        }
    }
}
