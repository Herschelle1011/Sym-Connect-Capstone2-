using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class newUpdateForPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Candidates_CandidateId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Elections_ElectionId",
                table: "Votes");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "Votes",
                newName: "positionId");

            migrationBuilder.RenameIndex(
                name: "IX_Votes_UserId_ElectionId_Position",
                table: "Votes",
                newName: "IX_Votes_UserId_ElectionId_positionId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "positions",
                newName: "PositionName");

            migrationBuilder.RenameColumn(
                name: "position",
                table: "Candidates",
                newName: "positionId");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "positions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_positionId",
                table: "Votes",
                column: "positionId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_positionId",
                table: "Candidates",
                column: "positionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_positions_positionId",
                table: "Candidates",
                column: "positionId",
                principalTable: "positions",
                principalColumn: "PositionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Candidates_CandidateId",
                table: "Votes",
                column: "CandidateId",
                principalTable: "Candidates",
                principalColumn: "CandidateId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Elections_ElectionId",
                table: "Votes",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "ElectionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_positions_positionId",
                table: "Votes",
                column: "positionId",
                principalTable: "positions",
                principalColumn: "PositionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_positions_positionId",
                table: "Candidates");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Candidates_CandidateId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Elections_ElectionId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_positions_positionId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_positionId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_positionId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "positions");

            migrationBuilder.RenameColumn(
                name: "positionId",
                table: "Votes",
                newName: "Position");

            migrationBuilder.RenameIndex(
                name: "IX_Votes_UserId_ElectionId_positionId",
                table: "Votes",
                newName: "IX_Votes_UserId_ElectionId_Position");

            migrationBuilder.RenameColumn(
                name: "PositionName",
                table: "positions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "positionId",
                table: "Candidates",
                newName: "position");

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Candidates_CandidateId",
                table: "Votes",
                column: "CandidateId",
                principalTable: "Candidates",
                principalColumn: "CandidateId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Elections_ElectionId",
                table: "Votes",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "ElectionId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
