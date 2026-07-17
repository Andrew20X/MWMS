using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MWMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveAdminMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminMessage",
                table: "LeaveRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminMessage",
                table: "LeaveRequests");
        }
    }
}
