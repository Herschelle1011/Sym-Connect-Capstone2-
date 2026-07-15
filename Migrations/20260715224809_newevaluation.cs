using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class newevaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "Evaluations",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "EvaluationSubmissions",
                columns: table => new
                {
                    SubmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationSubmissions", x => x.SubmissionId);
                    table.ForeignKey(
                        name: "FK_EvaluationSubmissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationSubmissions_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "EvaluationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationResponses",
                columns: table => new
                {
                    ResponseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    EvaluationDetailId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvaluationSubmissionSubmissionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationResponses", x => x.ResponseId);
                    table.ForeignKey(
                        name: "FK_EvaluationResponses_EvaluationDetail_EvaluationDetailId",
                        column: x => x.EvaluationDetailId,
                        principalTable: "EvaluationDetail",
                        principalColumn: "EvaluationDetailId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EvaluationResponses_EvaluationSubmissions_EvaluationSubmissionSubmissionId",
                        column: x => x.EvaluationSubmissionSubmissionId,
                        principalTable: "EvaluationSubmissions",
                        principalColumn: "SubmissionId");
                    table.ForeignKey(
                        name: "FK_EvaluationResponses_EvaluationSubmissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "EvaluationSubmissions",
                        principalColumn: "SubmissionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationResponses_EvaluationDetailId",
                table: "EvaluationResponses",
                column: "EvaluationDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationResponses_EvaluationSubmissionSubmissionId",
                table: "EvaluationResponses",
                column: "EvaluationSubmissionSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationResponses_SubmissionId",
                table: "EvaluationResponses",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationSubmissions_EvaluationId",
                table: "EvaluationSubmissions",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationSubmissions_UserId",
                table: "EvaluationSubmissions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluationResponses");

            migrationBuilder.DropTable(
                name: "EvaluationSubmissions");

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "Evaluations",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
