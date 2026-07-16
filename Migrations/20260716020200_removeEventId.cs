using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class removeEventId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationResponses_EvaluationSubmissions_EvaluationSubmissionSubmissionId",
                table: "EvaluationResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Events_EventId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_EventId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationResponses_EvaluationSubmissionSubmissionId",
                table: "EvaluationResponses");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "EvaluationSubmissionSubmissionId",
                table: "EvaluationResponses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EvaluationSubmissionSubmissionId",
                table: "EvaluationResponses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_EventId",
                table: "Evaluations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationResponses_EvaluationSubmissionSubmissionId",
                table: "EvaluationResponses",
                column: "EvaluationSubmissionSubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationResponses_EvaluationSubmissions_EvaluationSubmissionSubmissionId",
                table: "EvaluationResponses",
                column: "EvaluationSubmissionSubmissionId",
                principalTable: "EvaluationSubmissions",
                principalColumn: "SubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Events_EventId",
                table: "Evaluations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId");
        }
    }
}
