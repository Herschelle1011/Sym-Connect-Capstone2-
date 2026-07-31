using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class smallAdjustmentMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroupDetails_MemberId",
                table: "GroupDetails");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Members",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_GroupDetails_MemberId",
                table: "GroupDetails",
                column: "MemberId",
                unique: true,
                filter: "[MemberId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroupDetails_MemberId",
                table: "GroupDetails");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Members",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupDetails_MemberId",
                table: "GroupDetails",
                column: "MemberId");
        }
    }
}
