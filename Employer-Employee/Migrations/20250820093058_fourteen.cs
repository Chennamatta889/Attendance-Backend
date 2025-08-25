using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace employer_employee.Migrations
{
    /// <inheritdoc />
    public partial class fourteen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CarriedForwardBalance",
                table: "SalarySettlements",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAmount",
                table: "SalaryCarryForwards",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Employers",
                keyColumn: "EmployerId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$xrofWM43O52gUy15unPeRe1lpc9cPavFED2lUDe6Xvoe5RM8fKZy.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarriedForwardBalance",
                table: "SalarySettlements");

            migrationBuilder.DropColumn(
                name: "BalanceAmount",
                table: "SalaryCarryForwards");

            migrationBuilder.UpdateData(
                table: "Employers",
                keyColumn: "EmployerId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$bhY2HwcU2wpl7cBAIs2XjeWhto/w.qfE5CPukpEVmuxlfosbMh0Ja");
        }
    }
}
