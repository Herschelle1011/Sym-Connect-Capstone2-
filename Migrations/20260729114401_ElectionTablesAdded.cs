using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class ElectionTablesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_PartyList_PartyId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_PartyList_Elections_ElectionId",
                table: "PartyList");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartyList",
                table: "PartyList");

            migrationBuilder.RenameTable(
                name: "PartyList",
                newName: "PartyLists");

            migrationBuilder.RenameColumn(
                name: "CloseDate",
                table: "Elections",
                newName: "EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_PartyList_ElectionId",
                table: "PartyLists",
                newName: "IX_PartyLists_ElectionId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedDate",
                table: "Elections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Elections",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Elections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartyListDescription",
                table: "PartyLists",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartyLists",
                table: "PartyLists",
                column: "PartyListId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_PartyLists_PartyId",
                table: "Candidates",
                column: "PartyId",
                principalTable: "PartyLists",
                principalColumn: "PartyListId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartyLists_Elections_ElectionId",
                table: "PartyLists",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "ElectionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_PartyLists_PartyId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_PartyLists_Elections_ElectionId",
                table: "PartyLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartyLists",
                table: "PartyLists");

            migrationBuilder.DropColumn(
                name: "ArchivedDate",
                table: "Elections");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Elections");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Elections");

            migrationBuilder.DropColumn(
                name: "PartyListDescription",
                table: "PartyLists");

            migrationBuilder.RenameTable(
                name: "PartyLists",
                newName: "PartyList");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Elections",
                newName: "CloseDate");

            migrationBuilder.RenameIndex(
                name: "IX_PartyLists_ElectionId",
                table: "PartyList",
                newName: "IX_PartyList_ElectionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartyList",
                table: "PartyList",
                column: "PartyListId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_PartyList_PartyId",
                table: "Candidates",
                column: "PartyId",
                principalTable: "PartyList",
                principalColumn: "PartyListId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartyList_Elections_ElectionId",
                table: "PartyList",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "ElectionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
