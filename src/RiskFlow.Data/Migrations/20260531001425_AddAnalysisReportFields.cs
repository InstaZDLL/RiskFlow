using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RiskFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisReportFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Analyses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "Analyses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectDescription",
                table: "Analyses",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Analyses");

            migrationBuilder.DropColumn(
                name: "Organization",
                table: "Analyses");

            migrationBuilder.DropColumn(
                name: "ProjectDescription",
                table: "Analyses");
        }
    }
}
