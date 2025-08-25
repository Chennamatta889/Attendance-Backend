using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace employer_employee.Migrations
{
    /// <inheritdoc />
    public partial class fourth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Advances");

            migrationBuilder.RenameColumn(
                name: "Salary",
                table: "Employees",
                newName: "MonthlySalary");

            migrationBuilder.RenameColumn(
                name: "JoiningDate",
                table: "Employees",
                newName: "DateOfJoining");

            migrationBuilder.AddColumn<decimal>(
                name: "FixedSalary",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Advances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNo",
                table: "Advances",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FixedSalary",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Advances");

            migrationBuilder.DropColumn(
                name: "ReferenceNo",
                table: "Advances");

            migrationBuilder.RenameColumn(
                name: "MonthlySalary",
                table: "Employees",
                newName: "Salary");

            migrationBuilder.RenameColumn(
                name: "DateOfJoining",
                table: "Employees",
                newName: "JoiningDate");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Advances",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
