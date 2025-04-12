using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chillde.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EntityV16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliveryReminderSend",
                table: "Orders",
                newName: "DeliveryReminderSent");
            migrationBuilder.RenameColumn(
                name: "ReminderSent",
                table: "Orders",
                newName: "DeadlineMissed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliveryReminderSend",
                table: "Orders",
                newName: "DeliveryReminderSent");
            migrationBuilder.RenameColumn(
               name: "ReminderSent",
               table: "Orders",
               newName: "DeadlineMissed");
        }
    }
}
