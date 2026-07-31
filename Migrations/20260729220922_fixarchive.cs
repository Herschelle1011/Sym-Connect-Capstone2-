using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class fixarchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartyLists_Elections_ElectionId",
                table: "PartyLists");

            migrationBuilder.AlterColumn<int>(
                name: "ElectionId",
                table: "PartyLists",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedDate",
                table: "PartyLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isArchived",
                table: "PartyLists",
                type: "bit",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PartyLists_Elections_ElectionId",
                table: "PartyLists",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "ElectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartyLists_Elections_ElectionId",
                table: "PartyLists");

            migrationBuilder.DropColumn(
                name: "ArchivedDate",
                table: "PartyLists");

            migrationBuilder.DropColumn(
                name: "isArchived",
                table: "PartyLists");

            migrationBuilder.AlterColumn<int>(
                name: "ElectionId",
                table: "PartyLists",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PartyLists_Elections_ElectionId",
                table: "PartyLists",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "ElectionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
