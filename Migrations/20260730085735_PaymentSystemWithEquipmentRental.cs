using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakerspaceFablabPlatform.Migrations
{
    /// <inheritdoc />
    public partial class PaymentSystemWithEquipmentRental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentRentals_Equipments_EquipmentId",
                table: "EquipmentRentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_EquipmentRentals_EquipmentRentalId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Users_UserId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRentals_EquipmentId_ReleasedAt",
                table: "EquipmentRentals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_UserId",
                table: "Payment");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "Payments");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_EquipmentRentalId",
                table: "Payments",
                newName: "IX_Payments_EquipmentRentalId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RentedAt",
                table: "EquipmentRentals",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReleasedAt",
                table: "EquipmentRentals",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaidAt",
                table: "EquipmentRentals",
                type: "timestamp",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPaid",
                table: "EquipmentRentals",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsOverdue",
                table: "EquipmentRentals",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpectedReturnAt",
                table: "EquipmentRentals",
                type: "timestamp",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Payments",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "RentalFee",
                table: "Payments",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentNumber",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "PaidAmount",
                table: "Payments",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "LateFee",
                table: "Payments",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "Payments",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_EquipmentId",
                table: "EquipmentRentals",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_ExpectedReturnAt",
                table: "EquipmentRentals",
                column: "ExpectedReturnAt");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_IsOverdue",
                table: "EquipmentRentals",
                column: "IsOverdue");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_IsPaid",
                table: "EquipmentRentals",
                column: "IsPaid");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_PaymentId",
                table: "EquipmentRentals",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_ReleasedAt",
                table: "EquipmentRentals",
                column: "ReleasedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_RentedAt",
                table: "EquipmentRentals",
                column: "RentedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_UserId_ReleasedAt",
                table: "EquipmentRentals",
                columns: new[] { "UserId", "ReleasedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_UserId_RentedAt",
                table: "EquipmentRentals",
                columns: new[] { "UserId", "RentedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedAt",
                table: "Payments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentNumber_Unique",
                table: "Payments",
                column: "PaymentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StatusDate",
                table: "Payments",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StatusPaidAt",
                table: "Payments",
                columns: new[] { "Status", "PaidAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserDate",
                table: "Payments",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserStatus",
                table: "Payments",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_LateFee_NonNegative",
                table: "Payments",
                sql: "\"LateFee\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PaidAmount_NonNegative",
                table: "Payments",
                sql: "\"PaidAmount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PaidAtAfterCreated",
                table: "Payments",
                sql: "\"PaidAt\" IS NULL OR \"PaidAt\" >= \"CreatedAt\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RefundedAtAfterCreated",
                table: "Payments",
                sql: "\"RefundedAt\" IS NULL OR \"RefundedAt\" >= \"CreatedAt\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RentalFee_NonNegative",
                table: "Payments",
                sql: "\"RentalFee\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TotalAmount_NonNegative",
                table: "Payments",
                sql: "\"TotalAmount\" >= 0");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentRentals_Equipments_EquipmentId",
                table: "EquipmentRentals",
                column: "EquipmentId",
                principalTable: "Equipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_EquipmentRentals_EquipmentRentalId",
                table: "Payments",
                column: "EquipmentRentalId",
                principalTable: "EquipmentRentals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquipmentRentals_Equipments_EquipmentId",
                table: "EquipmentRentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_EquipmentRentals_EquipmentRentalId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Users_UserId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRentals_EquipmentId",
                table: "EquipmentRentals");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRentals_ExpectedReturnAt",
                table: "EquipmentRentals");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRentals_IsOverdue",
                table: "EquipmentRentals");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRentals_IsPaid",
                table: "EquipmentRentals");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRentals_PaymentId",
                table: "EquipmentRentals");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRentals_ReleasedAt",
                table: "EquipmentRentals");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRentals_RentedAt",
                table: "EquipmentRentals");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRentals_UserId_ReleasedAt",
                table: "EquipmentRentals");

            migrationBuilder.DropIndex(
                name: "IX_EquipmentRentals_UserId_RentedAt",
                table: "EquipmentRentals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CreatedAt",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentNumber_Unique",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StatusDate",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StatusPaidAt",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_UserDate",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_UserStatus",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LateFee_NonNegative",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PaidAmount_NonNegative",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PaidAtAfterCreated",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RefundedAtAfterCreated",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RentalFee_NonNegative",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TotalAmount_NonNegative",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Payments");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payment");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_EquipmentRentalId",
                table: "Payment",
                newName: "IX_Payment_EquipmentRentalId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RentedAt",
                table: "EquipmentRentals",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReleasedAt",
                table: "EquipmentRentals",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaidAt",
                table: "EquipmentRentals",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPaid",
                table: "EquipmentRentals",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsOverdue",
                table: "EquipmentRentals",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpectedReturnAt",
                table: "EquipmentRentals",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Payment",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Payment",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<decimal>(
                name: "RentalFee",
                table: "Payment",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentNumber",
                table: "Payment",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentMethod",
                table: "Payment",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "PaidAmount",
                table: "Payment",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "LateFee",
                table: "Payment",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "Payment",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Payment",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRentals_EquipmentId_ReleasedAt",
                table: "EquipmentRentals",
                columns: new[] { "EquipmentId", "ReleasedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payment_UserId",
                table: "Payment",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EquipmentRentals_Equipments_EquipmentId",
                table: "EquipmentRentals",
                column: "EquipmentId",
                principalTable: "Equipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_EquipmentRentals_EquipmentRentalId",
                table: "Payment",
                column: "EquipmentRentalId",
                principalTable: "EquipmentRentals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Users_UserId",
                table: "Payment",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
