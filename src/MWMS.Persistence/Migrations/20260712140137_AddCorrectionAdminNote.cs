using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MWMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorrectionAdminNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "CorrectionRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "CorrectionRequests");
        }
    }
}
