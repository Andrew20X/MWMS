using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MWMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryDeductions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinkedAttendanceId",
                table: "LeaveRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AbsenceResolutionStatus",
                table: "Attendances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeadlineForLeaveRequest",
                table: "Attendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnexcused",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "SalaryDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    RelatedAttendanceId = table.Column<int>(type: "int", nullable: false),
                    DeductionAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppliedOnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryDeductions_Attendances_RelatedAttendanceId",
                        column: x => x.RelatedAttendanceId,
                        principalTable: "Attendances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalaryDeductions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LinkedAttendanceId",
                table: "LeaveRequests",
                column: "LinkedAttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryDeductions_EmployeeId",
                table: "SalaryDeductions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryDeductions_RelatedAttendanceId",
                table: "SalaryDeductions",
                column: "RelatedAttendanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_Attendances_LinkedAttendanceId",
                table: "LeaveRequests",
                column: "LinkedAttendanceId",
                principalTable: "Attendances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_Attendances_LinkedAttendanceId",
                table: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "SalaryDeductions");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_LinkedAttendanceId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "LinkedAttendanceId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "AbsenceResolutionStatus",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "DeadlineForLeaveRequest",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "IsUnexcused",
                table: "Attendances");
        }
    }
}
