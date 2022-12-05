using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RocketLeagueTradingTools.Infrastructure.Persistence.Migrations
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
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OfferType = table.Column<string>(type: "TEXT", nullable: false),
                    ItemName = table.Column<string>(type: "TEXT", nullable: false),
                    ItemType = table.Column<string>(type: "TEXT", nullable: false),
                    PriceFrom = table.Column<int>(type: "INTEGER", nullable: false),
                    PriceTo = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Certification = table.Column<string>(type: "TEXT", nullable: false),
                    Enabled = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Blacklist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TradingSite = table.Column<string>(type: "TEXT", nullable: false),
                    TraderName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklist", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Link = table.Column<string>(type: "TEXT", nullable: false),
                    ScrapedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OfferType = table.Column<string>(type: "TEXT", nullable: false),
                    ItemName = table.Column<string>(type: "TEXT", nullable: false),
                    ItemType = table.Column<string>(type: "TEXT", nullable: false),
                    Price = table.Column<int>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Certification = table.Column<string>(type: "TEXT", nullable: false),
                    TradingSite = table.Column<string>(type: "TEXT", nullable: false),
                    TraderName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeOffers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TradeOfferId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SeenDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_TradeOffers_TradeOfferId",
                        column: x => x.TradeOfferId,
                        principalTable: "TradeOffers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ItemName",
                table: "Alerts",
                column: "ItemName");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_PriceFrom",
                table: "Alerts",
                column: "PriceFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_PriceTo",
                table: "Alerts",
                column: "PriceTo");

            migrationBuilder.CreateIndex(
                name: "IX_Blacklist_TradingSite_TraderName",
                table: "Blacklist",
                columns: new[] { "TradingSite", "TraderName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedDate",
                table: "Notifications",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TradeOfferId",
                table: "Notifications",
                column: "TradeOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_ItemName",
                table: "TradeOffers",
                column: "ItemName");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_Price",
                table: "TradeOffers",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_ScrapedDate",
                table: "TradeOffers",
                column: "ScrapedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "Blacklist");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "TradeOffers");
        }
    }
}
