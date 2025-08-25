using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace employer_employee.Migrations
{
    /// <inheritdoc />
    public partial class fifteen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SettledInMonth",
                table: "SalaryCarryForwards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SettledInYear",
                table: "SalaryCarryForwards",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Employers",
                keyColumn: "EmployerId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$FAeCU58EZ9czFQT/oaWin.mtnp1eOZFfUib/ktHwqpl5.KKEd0ExK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SettledInMonth",
                table: "SalaryCarryForwards");

            migrationBuilder.DropColumn(
                name: "SettledInYear",
                table: "SalaryCarryForwards");

            migrationBuilder.UpdateData(
                table: "Employers",
                keyColumn: "EmployerId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$xrofWM43O52gUy15unPeRe1lpc9cPavFED2lUDe6Xvoe5RM8fKZy.");
        }
    }
}
