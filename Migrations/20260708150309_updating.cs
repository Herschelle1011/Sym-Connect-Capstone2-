using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class updating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationDetail_EvaluationQuestions_EvaluationQuestionQuestionId",
                table: "EvaluationDetail");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationDetail_EvaluationQuestionQuestionId",
                table: "EvaluationDetail");

            migrationBuilder.DropColumn(
                name: "EvaluationQuestionQuestionId",
                table: "EvaluationDetail");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "EvaluationDetail");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationDetail_QuestionId",
                table: "EvaluationDetail",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationDetail_EvaluationQuestions_QuestionId",
                table: "EvaluationDetail",
                column: "QuestionId",
                principalTable: "EvaluationQuestions",
                principalColumn: "QuestionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationDetail_EvaluationQuestions_QuestionId",
                table: "EvaluationDetail");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationDetail_QuestionId",
                table: "EvaluationDetail");

            migrationBuilder.AddColumn<int>(
                name: "EvaluationQuestionQuestionId",
                table: "EvaluationDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "EvaluationDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationDetail_EvaluationQuestionQuestionId",
                table: "EvaluationDetail",
                column: "EvaluationQuestionQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationDetail_EvaluationQuestions_EvaluationQuestionQuestionId",
                table: "EvaluationDetail",
                column: "EvaluationQuestionQuestionId",
                principalTable: "EvaluationQuestions",
                principalColumn: "QuestionId");
        }
    }
}
