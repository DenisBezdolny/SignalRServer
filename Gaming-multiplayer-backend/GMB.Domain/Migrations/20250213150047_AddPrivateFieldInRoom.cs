using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMB.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateFieldInRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Rooms",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Rooms");
        }
    }
}
