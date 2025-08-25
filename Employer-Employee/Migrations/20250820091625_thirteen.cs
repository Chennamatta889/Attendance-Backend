using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace employer_employee.Migrations
{
    /// <inheritdoc />
    public partial class thirteen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employers",
                keyColumn: "EmployerId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$bhY2HwcU2wpl7cBAIs2XjeWhto/w.qfE5CPukpEVmuxlfosbMh0Ja");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Employers",
                keyColumn: "EmployerId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$kPkwVKPiU1ZxJa13JP3VIOQrdZXO5KahbNs.xWGYbDA17PRvZqNbm");
        }
    }
}
