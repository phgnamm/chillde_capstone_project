using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV29 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTimeCreationVoucher",
                table: "Orders",
                newName: "DateTimeCreateVoucher");
            migrationBuilder.AlterColumn<Guid>(
               name: "ArtisanId",
               table: "Feedbacks",
               type: "uuid",
               nullable: true,
               oldClrType: typeof(Guid),
               oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTimeCreateVoucher",
                table: "Orders",
                newName: "DateTimeCreationVoucher");
            migrationBuilder.AlterColumn<Guid>(
               name: "ArtisanId",
               table: "Feedbacks",
               type: "uuid",
               nullable: false,
               defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
               oldClrType: typeof(Guid),
               oldType: "uuid",
               oldNullable: true);
        }
    }
}
