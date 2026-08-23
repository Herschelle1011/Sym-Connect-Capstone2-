using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class EventForLeaders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_AspNetUsers_CreatedByUserId",
                table: "Events");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedByUserId",
                table: "Events",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_ApprovedByUserId",
                table: "Events",
                column: "ApprovedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Members_ApprovedByUserId",
                table: "Events",
                column: "ApprovedByUserId",
                principalTable: "Members",
                principalColumn: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Members_CreatedByUserId",
                table: "Events",
                column: "CreatedByUserId",
                principalTable: "Members",
                principalColumn: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Members_ApprovedByUserId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Members_CreatedByUserId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ApprovedByUserId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "Events",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_AspNetUsers_CreatedByUserId",
                table: "Events",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
