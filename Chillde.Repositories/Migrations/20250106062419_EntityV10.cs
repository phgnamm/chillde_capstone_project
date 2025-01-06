using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WardName",
                table: "ShippingAddresses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ProvinceName",
                table: "ShippingAddresses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DistrictName",
                table: "ShippingAddresses",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<float[]>(
                name: "EmbeddingVector",
                table: "Services",
                type: "real[]",
                nullable: false,
                defaultValue: new float[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmbeddingVector",
                table: "Services");

            migrationBuilder.AlterColumn<string>(
                name: "WardName",
                table: "ShippingAddresses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProvinceName",
                table: "ShippingAddresses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DistrictName",
                table: "ShippingAddresses",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
