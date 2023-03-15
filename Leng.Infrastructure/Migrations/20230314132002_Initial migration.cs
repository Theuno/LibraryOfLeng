using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leng.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LengUser",
                columns: table => new
                {
                    LengUserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    aduuid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LengUser", x => x.LengUserID);
                });

            migrationBuilder.CreateTable(
                name: "MTGTranslations",
                columns: table => new
                {
                    MTGTranslationsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    French = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    German = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Italian = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Spanish = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTGTranslations", x => x.MTGTranslationsID);
                });

            migrationBuilder.CreateTable(
                name: "MTGSets",
                columns: table => new
                {
                    MTGSetsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    baseSetSize = table.Column<int>(type: "int", nullable: false),
                    block = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    setCode = table.Column<string>(type: "varchar(8)", nullable: false),
                    isFoilOnly = table.Column<bool>(type: "bit", nullable: true),
                    isForeignOnly = table.Column<bool>(type: "bit", nullable: true),
                    isNonFoilOnly = table.Column<bool>(type: "bit", nullable: true),
                    isOnlineOnly = table.Column<bool>(type: "bit", nullable: false),
                    isPartialPreview = table.Column<bool>(type: "bit", nullable: false),
                    mcmId = table.Column<int>(type: "int", nullable: true),
                    mcmIdExtras = table.Column<int>(type: "int", nullable: true),
                    mcmName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mtgoCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    releaseDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tokenSetCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    totalSetSize = table.Column<int>(type: "int", nullable: true),
                    translationsMTGTranslationsID = table.Column<int>(type: "int", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTGSets", x => x.MTGSetsID);
                    table.ForeignKey(
                        name: "FK_MTGSets_MTGTranslations_translationsMTGTranslationsID",
                        column: x => x.translationsMTGTranslationsID,
                        principalTable: "MTGTranslations",
                        principalColumn: "MTGTranslationsID");
                });

            migrationBuilder.CreateTable(
                name: "MTGCard",
                columns: table => new
                {
                    MTGCardsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    artist = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    setCode = table.Column<string>(type: "varchar(8)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MTGSetsID = table.Column<int>(type: "int", nullable: false),
                    asciiName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    edhrecRank = table.Column<int>(type: "int", nullable: false),
                    edhrecSaltiness = table.Column<float>(type: "real", nullable: false),
                    hasFoil = table.Column<bool>(type: "bit", nullable: true),
                    hasNonFoil = table.Column<bool>(type: "bit", nullable: true),
                    isOnlineOnly = table.Column<bool>(type: "bit", nullable: false),
                    mcmId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    number = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    originalText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    originalType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    power = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scryfallId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LengUserID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTGCard", x => x.MTGCardsID);
                    table.ForeignKey(
                        name: "FK_MTGCard_LengUser_LengUserID",
                        column: x => x.LengUserID,
                        principalTable: "LengUser",
                        principalColumn: "LengUserID");
                    table.ForeignKey(
                        name: "FK_MTGCard_MTGSets_MTGSetsID",
                        column: x => x.MTGSetsID,
                        principalTable: "MTGSets",
                        principalColumn: "MTGSetsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LengUserMTGCards",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LengUserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MTGCardsID = table.Column<int>(type: "int", nullable: false),
                    count = table.Column<int>(type: "int", nullable: false),
                    countFoil = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LengUserMTGCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LengUserMTGCards_LengUser_LengUserID",
                        column: x => x.LengUserID,
                        principalTable: "LengUser",
                        principalColumn: "LengUserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LengUserMTGCards_MTGCard_MTGCardsID",
                        column: x => x.MTGCardsID,
                        principalTable: "MTGCard",
                        principalColumn: "MTGCardsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LengUserMTGCards_LengUserID",
                table: "LengUserMTGCards",
                column: "LengUserID");

            migrationBuilder.CreateIndex(
                name: "IX_LengUserMTGCards_MTGCardsID",
                table: "LengUserMTGCards",
                column: "MTGCardsID");

            migrationBuilder.CreateIndex(
                name: "IX_MTGCard_LengUserID",
                table: "MTGCard",
                column: "LengUserID");

            migrationBuilder.CreateIndex(
                name: "IX_MTGCard_MTGSetsID",
                table: "MTGCard",
                column: "MTGSetsID");

            migrationBuilder.CreateIndex(
                name: "IX_MTGCard_name_setCode_number",
                table: "MTGCard",
                columns: new[] { "name", "setCode", "number" },
                unique: true,
                filter: "[name] IS NOT NULL AND [setCode] IS NOT NULL AND [number] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MTGSets_setCode",
                table: "MTGSets",
                column: "setCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MTGSets_translationsMTGTranslationsID",
                table: "MTGSets",
                column: "translationsMTGTranslationsID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LengUserMTGCards");

            migrationBuilder.DropTable(
                name: "MTGCard");

            migrationBuilder.DropTable(
                name: "LengUser");

            migrationBuilder.DropTable(
                name: "MTGSets");

            migrationBuilder.DropTable(
                name: "MTGTranslations");
        }
    }
}
