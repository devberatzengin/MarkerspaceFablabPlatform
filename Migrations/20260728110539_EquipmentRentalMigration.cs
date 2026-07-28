using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakerspaceFablabPlatform.Migrations
{
    /// <inheritdoc />
    public partial class EquipmentRentalMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Users_UsingById",
                table: "Equipments");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_UsingById",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "UsingById",
                table: "Equipments");

            migrationBuilder.CreateTable(
                name: "EquipmentRentals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RentedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpectedReturnAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentRentals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentRentals_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentRentals_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_EquipmentId_ReleasedAt",
                table: "EquipmentRentals",
                columns: new[] { "EquipmentId", "ReleasedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_UserId",
                table: "EquipmentRentals",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentRentals");

            migrationBuilder.AddColumn<Guid>(
                name: "UsingById",
                table: "Equipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_UsingById",
                table: "Equipments",
                column: "UsingById");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Users_UsingById",
                table: "Equipments",
                column: "UsingById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
