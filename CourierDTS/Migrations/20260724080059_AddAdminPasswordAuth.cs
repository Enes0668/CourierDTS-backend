using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourierDTS.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPasswordAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Admins",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "Name", "PasswordHash", "Phone" },
                values: new object[] { 1, "admin", "100000./YV4z10YUt6MWaUGPjWgIA==.5Ee9u4gYbUfz0F6+HvABCrh90mKDY8awIRsX3taIIAE=", "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Admins");
        }
    }
}
