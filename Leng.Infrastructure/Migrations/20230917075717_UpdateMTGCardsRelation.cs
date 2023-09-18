using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leng.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMTGCardsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LengUserMTGCards_LengUser_Id.LengUserID",
                table: "LengUserMTGCards");

            migrationBuilder.DropForeignKey(
                name: "FK_LengUserMTGCards_MTGCard_Id.MTGCardsID",
                table: "LengUserMTGCards");

            migrationBuilder.DropIndex(
                name: "IX_LengUserMTGCards_Id.LengUserID",
                table: "LengUserMTGCards");

            migrationBuilder.DropColumn(
                name: "Id.LengUserID",
                table: "LengUserMTGCards");

            migrationBuilder.RenameColumn(
                name: "Id.MTGCardsID",
                table: "LengUserMTGCards",
                newName: "MTGCardsId");

            migrationBuilder.RenameIndex(
                name: "IX_LengUserMTGCards_Id.MTGCardsID",
                table: "LengUserMTGCards",
                newName: "IX_LengUserMTGCards_MTGCardsId");

            migrationBuilder.AddColumn<string>(
                name: "LengUserId",
                table: "LengUserMTGCards",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LengUserMTGCards_LengUserId",
                table: "LengUserMTGCards",
                column: "LengUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LengUserMTGCards_LengUser_LengUserId",
                table: "LengUserMTGCards",
                column: "LengUserId",
                principalTable: "LengUser",
                principalColumn: "LengUserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LengUserMTGCards_MTGCard_MTGCardsId",
                table: "LengUserMTGCards",
                column: "MTGCardsId",
                principalTable: "MTGCard",
                principalColumn: "MTGCardsID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LengUserMTGCards_LengUser_LengUserId",
                table: "LengUserMTGCards");

            migrationBuilder.DropForeignKey(
                name: "FK_LengUserMTGCards_MTGCard_MTGCardsId",
                table: "LengUserMTGCards");

            migrationBuilder.DropIndex(
                name: "IX_LengUserMTGCards_LengUserId",
                table: "LengUserMTGCards");

            migrationBuilder.DropColumn(
                name: "LengUserId",
                table: "LengUserMTGCards");

            migrationBuilder.RenameColumn(
                name: "MTGCardsId",
                table: "LengUserMTGCards",
                newName: "Id.MTGCardsID");

            migrationBuilder.RenameIndex(
                name: "IX_LengUserMTGCards_MTGCardsId",
                table: "LengUserMTGCards",
                newName: "IX_LengUserMTGCards_Id.MTGCardsID");

            migrationBuilder.AddColumn<string>(
                name: "Id.LengUserID",
                table: "LengUserMTGCards",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LengUserMTGCards_Id.LengUserID",
                table: "LengUserMTGCards",
                column: "Id.LengUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_LengUserMTGCards_LengUser_Id.LengUserID",
                table: "LengUserMTGCards",
                column: "Id.LengUserID",
                principalTable: "LengUser",
                principalColumn: "LengUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_LengUserMTGCards_MTGCard_Id.MTGCardsID",
                table: "LengUserMTGCards",
                column: "Id.MTGCardsID",
                principalTable: "MTGCard",
                principalColumn: "MTGCardsID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
