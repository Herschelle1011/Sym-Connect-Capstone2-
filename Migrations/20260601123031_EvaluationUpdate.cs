using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class EvaluationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Members_MemberId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_MemberId",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Evaluations");

            migrationBuilder.RenameColumn(
                name: "DateSubmitted",
                table: "Evaluations",
                newName: "DateCreated");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedDate",
                table: "Groups",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Evaluations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedDate",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Evaluations");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "Evaluations",
                newName: "DateSubmitted");

            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "Evaluations",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Evaluations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_MemberId",
                table: "Evaluations",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Members_MemberId",
                table: "Evaluations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId");
        }
    }
}
