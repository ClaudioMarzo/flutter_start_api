using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlutterStart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class refactEntityLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Loans",
                newName: "LoanDate");

            migrationBuilder.AddColumn<string>(
                name: "Observations",
                table: "Loans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Loans",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Observations",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "LoanDate",
                table: "Loans",
                newName: "StartDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "Loans",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
