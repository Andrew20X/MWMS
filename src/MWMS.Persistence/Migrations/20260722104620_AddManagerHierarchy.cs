using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MWMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovedByHRId",
                table: "OvertimeRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByManagerId",
                table: "OvertimeRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HRApprovalDate",
                table: "OvertimeRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManagerApprovalDate",
                table: "OvertimeRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByHRId",
                table: "LeaveRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByManagerId",
                table: "LeaveRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HRApprovalDate",
                table: "LeaveRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManagerApprovalDate",
                table: "LeaveRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ManagerId",
                table: "Employees",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_ManagerId",
                table: "Employees",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_ManagerId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ManagerId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ApprovedByHRId",
                table: "OvertimeRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedByManagerId",
                table: "OvertimeRequests");

            migrationBuilder.DropColumn(
                name: "HRApprovalDate",
                table: "OvertimeRequests");

            migrationBuilder.DropColumn(
                name: "ManagerApprovalDate",
                table: "OvertimeRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedByHRId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedByManagerId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "HRApprovalDate",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ManagerApprovalDate",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Employees");
        }
    }
}
