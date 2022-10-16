using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RocketLeagueTradingTools.Infrastructure.Persistence.SQLite.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemName = table.Column<string>(type: "TEXT", nullable: false),
                    OfferType = table.Column<int>(type: "INTEGER", nullable: false),
                    PriceFrom = table.Column<int>(type: "INTEGER", nullable: false),
                    PriceTo = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Certification = table.Column<string>(type: "TEXT", nullable: false),
                    Disabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BuyOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Link = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<string>(type: "TEXT", nullable: false),
                    ScrapedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Certification = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyOffers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SellOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Link = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<string>(type: "TEXT", nullable: false),
                    ScrapedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Certification = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellOffers", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Alerts",
                columns: new[] { "Id", "Certification", "Color", "Disabled", "ItemName", "OfferType", "PriceFrom", "PriceTo" },
                values: new object[] { 1, "", "None", false, "Hellfire", 1, 0, 100 });

            migrationBuilder.InsertData(
                table: "Alerts",
                columns: new[] { "Id", "Certification", "Color", "Disabled", "ItemName", "OfferType", "PriceFrom", "PriceTo" },
                values: new object[] { 2, "", "None", false, "Dueling Dragons", 1, 0, 500 });

            migrationBuilder.CreateIndex(
                name: "IX_BuyOffers_Id_SourceId_ScrapedDate_Name_Price_Color_Certification",
                table: "BuyOffers",
                columns: new[] { "Id", "SourceId", "ScrapedDate", "Name", "Price", "Color", "Certification" });

            migrationBuilder.CreateIndex(
                name: "IX_SellOffers_Id_SourceId_ScrapedDate_Name_Price_Color_Certification",
                table: "SellOffers",
                columns: new[] { "Id", "SourceId", "ScrapedDate", "Name", "Price", "Color", "Certification" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "BuyOffers");

            migrationBuilder.DropTable(
                name: "SellOffers");
        }
    }
}
