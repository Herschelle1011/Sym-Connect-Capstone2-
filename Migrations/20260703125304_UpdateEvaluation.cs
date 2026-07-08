using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_AspNetUsers_SubmittedByUserId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_SubmittedByUserId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "Activities");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Evaluations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenAt",
                table: "Evaluations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmittedById",
                table: "Activities",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EvaluationQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    answerType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationQuestions", x => x.QuestionId);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationDetail",
                columns: table => new
                {
                    EvaluationDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    evaluationQuestionQuestionId = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchiveDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationDetail", x => x.EvaluationDetailId);
                    table.ForeignKey(
                        name: "FK_EvaluationDetail_EvaluationQuestions_evaluationQuestionQuestionId",
                        column: x => x.evaluationQuestionQuestionId,
                        principalTable: "EvaluationQuestions",
                        principalColumn: "QuestionId");
                    table.ForeignKey(
                        name: "FK_EvaluationDetail_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "EvaluationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SubmittedById",
                table: "Activities",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationDetail_EvaluationId",
                table: "EvaluationDetail",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationDetail_evaluationQuestionQuestionId",
                table: "EvaluationDetail",
                column: "evaluationQuestionQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_AspNetUsers_SubmittedById",
                table: "Activities",
                column: "SubmittedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_AspNetUsers_SubmittedById",
                table: "Activities");

            migrationBuilder.DropTable(
                name: "EvaluationDetail");

            migrationBuilder.DropTable(
                name: "EvaluationQuestions");

            migrationBuilder.DropIndex(
                name: "IX_Activities_SubmittedById",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "OpenAt",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "SubmittedById",
                table: "Activities");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Evaluations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubmittedByUserId",
                table: "Activities",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_SubmittedByUserId",
                table: "Activities",
                column: "SubmittedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_AspNetUsers_SubmittedByUserId",
                table: "Activities",
                column: "SubmittedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
