using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakerspaceFablabPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentLevelToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "EquipmentLevel",
                table: "Users",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EquipmentLevel",
                table: "Users");
        }
    }
}
