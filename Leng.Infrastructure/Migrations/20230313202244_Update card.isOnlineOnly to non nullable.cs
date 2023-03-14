using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leng.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatecardisOnlineOnlytononnullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "isOnlineOnly",
                table: "MTGCard",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "isOnlineOnly",
                table: "MTGCard",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }
    }
}
