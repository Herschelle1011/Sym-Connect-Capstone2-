using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEvaluationPolished : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationDetail_EvaluationQuestions_evaluationQuestionQuestionId",
                table: "EvaluationDetail");

            migrationBuilder.RenameColumn(
                name: "answerType",
                table: "EvaluationQuestions",
                newName: "AnswerType");

            migrationBuilder.RenameColumn(
                name: "evaluationQuestionQuestionId",
                table: "EvaluationDetail",
                newName: "EvaluationQuestionQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_EvaluationDetail_evaluationQuestionQuestionId",
                table: "EvaluationDetail",
                newName: "IX_EvaluationDetail_EvaluationQuestionQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationDetail_EvaluationQuestions_EvaluationQuestionQuestionId",
                table: "EvaluationDetail",
                column: "EvaluationQuestionQuestionId",
                principalTable: "EvaluationQuestions",
                principalColumn: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationDetail_EvaluationQuestions_EvaluationQuestionQuestionId",
                table: "EvaluationDetail");

            migrationBuilder.RenameColumn(
                name: "AnswerType",
                table: "EvaluationQuestions",
                newName: "answerType");

            migrationBuilder.RenameColumn(
                name: "EvaluationQuestionQuestionId",
                table: "EvaluationDetail",
                newName: "evaluationQuestionQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_EvaluationDetail_EvaluationQuestionQuestionId",
                table: "EvaluationDetail",
                newName: "IX_EvaluationDetail_evaluationQuestionQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationDetail_EvaluationQuestions_evaluationQuestionQuestionId",
                table: "EvaluationDetail",
                column: "evaluationQuestionQuestionId",
                principalTable: "EvaluationQuestions",
                principalColumn: "QuestionId");
        }
    }
}
