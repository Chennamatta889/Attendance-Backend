using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace employer_employee.Migrations
{
    /// <inheritdoc />
    public partial class third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employers_EmployerId",
                table: "Employees");

            migrationBuilder.AlterColumn<int>(
                name: "EmployerId",
                table: "Employees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "Employers",
                columns: new[] { "EmployerId", "Email", "Name", "PasswordHash" },
                values: new object[] { 1, "admin@company.com", "Admin", "Srik@NTh" });

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employers_EmployerId",
                table: "Employees",
                column: "EmployerId",
                principalTable: "Employers",
                principalColumn: "EmployerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employers_EmployerId",
                table: "Employees");

            migrationBuilder.DeleteData(
                table: "Employers",
                keyColumn: "EmployerId",
                keyValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "EmployerId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employers_EmployerId",
                table: "Employees",
                column: "EmployerId",
                principalTable: "Employers",
                principalColumn: "EmployerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
