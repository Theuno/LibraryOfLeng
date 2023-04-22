using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leng.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddfacenamecolorindexonLengUserMTGCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "colors",
                table: "MTGCard",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "faceName",
                table: "MTGCard",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "colors",
                table: "MTGCard");

            migrationBuilder.DropColumn(
                name: "faceName",
                table: "MTGCard");
        }
    }
}
