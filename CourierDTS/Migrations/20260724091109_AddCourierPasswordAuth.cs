using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourierDTS.Migrations
{
    /// <inheritdoc />
    public partial class AddCourierPasswordAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Couriers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Couriers");
        }
    }
}
