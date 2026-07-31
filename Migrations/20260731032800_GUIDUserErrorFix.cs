using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class GUIDUserErrorFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Members_MemberId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_MemberId_ElectionId_Position",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Votes");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Votes",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_UserId_ElectionId_Position",
                table: "Votes",
                columns: new[] { "UserId", "ElectionId", "Position" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_AspNetUsers_UserId",
                table: "Votes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_AspNetUsers_UserId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_UserId_ElectionId_Position",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Votes");

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "Votes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_MemberId_ElectionId_Position",
                table: "Votes",
                columns: new[] { "MemberId", "ElectionId", "Position" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Members_MemberId",
                table: "Votes",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
