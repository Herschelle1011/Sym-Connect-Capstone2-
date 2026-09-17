using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sym_Connect__Capstone2_.Migrations
{
    /// <inheritdoc />
    public partial class updatedEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "EvaluationQuestions");

            migrationBuilder.DropColumn(
                name: "ArchiveDate",
                table: "EvaluationDetail");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "EvaluationDetail");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "EvaluationDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "EvaluationQuestions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchiveDate",
                table: "EvaluationDetail",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "EvaluationDetail",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "EvaluationDetail",
                type: "datetime2",
                nullable: true);
        }
    }
}
