using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class VoteElectionForMembersDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vote_Candidates_CandidateId",
                table: "Vote");

            migrationBuilder.DropForeignKey(
                name: "FK_Vote_Elections_ElectionId",
                table: "Vote");

            migrationBuilder.DropForeignKey(
                name: "FK_Vote_Members_MemberId",
                table: "Vote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vote",
                table: "Vote");

            migrationBuilder.RenameTable(
                name: "Vote",
                newName: "Votes");

            migrationBuilder.RenameIndex(
                name: "IX_Vote_MemberId_ElectionId_Position",
                table: "Votes",
                newName: "IX_Votes_MemberId_ElectionId_Position");

            migrationBuilder.RenameIndex(
                name: "IX_Vote_ElectionId",
                table: "Votes",
                newName: "IX_Votes_ElectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Vote_CandidateId",
                table: "Votes",
                newName: "IX_Votes_CandidateId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Votes",
                table: "Votes",
                column: "VoteId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Members_MemberId",
                table: "Votes",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Candidates_CandidateId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Elections_ElectionId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Members_MemberId",
                table: "Votes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Votes",
                table: "Votes");

            migrationBuilder.RenameTable(
                name: "Votes",
                newName: "Vote");

            migrationBuilder.RenameIndex(
                name: "IX_Votes_MemberId_ElectionId_Position",
                table: "Vote",
                newName: "IX_Vote_MemberId_ElectionId_Position");

            migrationBuilder.RenameIndex(
                name: "IX_Votes_ElectionId",
                table: "Vote",
                newName: "IX_Vote_ElectionId");

            migrationBuilder.RenameIndex(
                name: "IX_Votes_CandidateId",
                table: "Vote",
                newName: "IX_Vote_CandidateId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vote",
                table: "Vote",
                column: "VoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_Candidates_CandidateId",
                table: "Vote",
                column: "CandidateId",
                principalTable: "Candidates",
                principalColumn: "CandidateId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_Elections_ElectionId",
                table: "Vote",
                column: "ElectionId",
                principalTable: "Elections",
                principalColumn: "ElectionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_Members_MemberId",
                table: "Vote",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "MemberId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
