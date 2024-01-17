using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leng.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
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
                name: "MTGDeck",
                columns: table => new
                {
                    MTGDeckID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FormatID = table.Column<int>(type: "int", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTGDeck", x => x.MTGDeckID);
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
                name: "LengUserDeck",
                columns: table => new
                {
                    LengUserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MTGDeckID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LengUserDeck", x => new { x.LengUserID, x.MTGDeckID });
                    table.ForeignKey(
                        name: "FK_LengUserDeck_LengUser_LengUserID",
                        column: x => x.LengUserID,
                        principalTable: "LengUser",
                        principalColumn: "LengUserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LengUserDeck_MTGDeck_MTGDeckID",
                        column: x => x.MTGDeckID,
                        principalTable: "MTGDeck",
                        principalColumn: "MTGDeckID",
                        onDelete: ReferentialAction.Cascade);
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
                    name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MTGSetsID = table.Column<int>(type: "int", nullable: false),
                    asciiName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    edhrecRank = table.Column<int>(type: "int", nullable: false),
                    edhrecSaltiness = table.Column<float>(type: "real", nullable: false),
                    faceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    frameVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hasAlternativeDeckLimit = table.Column<bool>(type: "bit", nullable: true),
                    hasFoil = table.Column<bool>(type: "bit", nullable: true),
                    hasNonFoil = table.Column<bool>(type: "bit", nullable: true),
                    isAlternative = table.Column<bool>(type: "bit", nullable: true),
                    isOnlineOnly = table.Column<bool>(type: "bit", nullable: false),
                    mcmId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    number = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    originalText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    originalType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    power = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scryfallId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    side = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTGCard", x => x.MTGCardsID);
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
                    IdLengUserID = table.Column<string>(name: "Id.LengUserID", type: "nvarchar(450)", nullable: true),
                    IdMTGCardsID = table.Column<int>(name: "Id.MTGCardsID", type: "int", nullable: false),
                    count = table.Column<int>(type: "int", nullable: false),
                    countFoil = table.Column<int>(type: "int", nullable: false),
                    want = table.Column<int>(type: "int", nullable: false),
                    wantFoil = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LengUserMTGCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LengUserMTGCards_LengUser_Id.LengUserID",
                        column: x => x.IdLengUserID,
                        principalTable: "LengUser",
                        principalColumn: "LengUserID");
                    table.ForeignKey(
                        name: "FK_LengUserMTGCards_MTGCard_Id.MTGCardsID",
                        column: x => x.IdMTGCardsID,
                        principalTable: "MTGCard",
                        principalColumn: "MTGCardsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LengUserDeck_MTGDeckID",
                table: "LengUserDeck",
                column: "MTGDeckID");

            migrationBuilder.CreateIndex(
                name: "IX_LengUserMTGCards_Id.LengUserID",
                table: "LengUserMTGCards",
                column: "Id.LengUserID");

            migrationBuilder.CreateIndex(
                name: "IX_LengUserMTGCards_Id.MTGCardsID",
                table: "LengUserMTGCards",
                column: "Id.MTGCardsID");

            migrationBuilder.CreateIndex(
                name: "IX_MTGCard_MTGSetsID",
                table: "MTGCard",
                column: "MTGSetsID");

            migrationBuilder.CreateIndex(
                name: "IX_MTGCard_name_setCode_number",
                table: "MTGCard",
                columns: new[] { "name", "setCode", "number" },
                unique: true,
                filter: "[setCode] IS NOT NULL AND [number] IS NOT NULL");

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
                name: "LengUserDeck");

            migrationBuilder.DropTable(
                name: "LengUserMTGCards");

            migrationBuilder.DropTable(
                name: "MTGDeck");

            migrationBuilder.DropTable(
                name: "LengUser");

            migrationBuilder.DropTable(
                name: "MTGCard");

            migrationBuilder.DropTable(
                name: "MTGSets");

            migrationBuilder.DropTable(
                name: "MTGTranslations");
        }
    }
}
