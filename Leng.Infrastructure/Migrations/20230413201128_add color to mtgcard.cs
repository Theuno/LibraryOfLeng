using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leng.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addcolortomtgcard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MTGColor",
                columns: table => new
                {
                    MTGColorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTGColor", x => x.MTGColorID);
                });

            migrationBuilder.CreateTable(
                name: "MTGCards_MTGColor",
                columns: table => new
                {
                    CardsMTGCardsID = table.Column<int>(type: "int", nullable: false),
                    ColorsMTGColorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTGCards_MTGColor", x => new { x.CardsMTGCardsID, x.ColorsMTGColorID });
                    table.ForeignKey(
                        name: "FK_MTGCards_MTGColor_MTGCard_CardsMTGCardsID",
                        column: x => x.CardsMTGCardsID,
                        principalTable: "MTGCard",
                        principalColumn: "MTGCardsID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MTGCards_MTGColor_MTGColor_ColorsMTGColorID",
                        column: x => x.ColorsMTGColorID,
                        principalTable: "MTGColor",
                        principalColumn: "MTGColorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MTGCards_MTGColor_ColorsMTGColorID",
                table: "MTGCards_MTGColor",
                column: "ColorsMTGColorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MTGCards_MTGColor");

            migrationBuilder.DropTable(
                name: "MTGColor");
        }
    }
}
