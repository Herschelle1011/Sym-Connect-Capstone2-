using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberDateApplied : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PreferredGroupId",
                table: "Members",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateApplied",
                table: "Members",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_PreferredGroupId",
                table: "Members",
                column: "PreferredGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Groups_PreferredGroupId",
                table: "Members",
                column: "PreferredGroupId",
                principalTable: "Groups",
                principalColumn: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Groups_PreferredGroupId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_PreferredGroupId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "DateApplied",
                table: "Members");

            migrationBuilder.AlterColumn<int>(
                name: "PreferredGroupId",
                table: "Members",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
