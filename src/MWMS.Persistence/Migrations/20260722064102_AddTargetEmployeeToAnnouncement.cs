using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MWMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetEmployeeToAnnouncement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetEmployeeId",
                table: "Announcements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_TargetEmployeeId",
                table: "Announcements",
                column: "TargetEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Employees_TargetEmployeeId",
                table: "Announcements",
                column: "TargetEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Employees_TargetEmployeeId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_TargetEmployeeId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "TargetEmployeeId",
                table: "Announcements");
        }
    }
}
