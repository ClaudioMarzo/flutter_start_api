using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlutterStart.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSbnBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sbn",
                table: "Books",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sbn",
                table: "Books");
        }
    }
}
