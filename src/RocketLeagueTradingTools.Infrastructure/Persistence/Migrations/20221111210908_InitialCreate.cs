﻿using System;
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
                    ItemName = table.Column<string>(type: "TEXT", nullable: false),
                    OfferType = table.Column<string>(type: "TEXT", nullable: false),
                    PriceFrom = table.Column<int>(type: "INTEGER", nullable: false),
                    PriceTo = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemType = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Certification = table.Column<string>(type: "TEXT", nullable: false),
                    Enabled = table.Column<string>(type: "TEXT", nullable: false)
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
                    ItemType = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Certification = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyOffers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TradeItemName = table.Column<string>(type: "TEXT", nullable: false),
                    TradeItemType = table.Column<string>(type: "TEXT", nullable: false),
                    TradeItemColor = table.Column<string>(type: "TEXT", nullable: false),
                    TradeItemCertification = table.Column<string>(type: "TEXT", nullable: false),
                    TradeOfferPrice = table.Column<int>(type: "INTEGER", nullable: false),
                    TradeOfferSourceId = table.Column<string>(type: "TEXT", nullable: false),
                    TradeOfferLink = table.Column<string>(type: "TEXT", nullable: false),
                    TradeOfferScrapedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SeenDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
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
                    ItemType = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Certification = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellOffers", x => x.Id);
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
                name: "IX_BuyOffers_Name",
                table: "BuyOffers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BuyOffers_Price",
                table: "BuyOffers",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_BuyOffers_ScrapedDate",
                table: "BuyOffers",
                column: "ScrapedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedDate",
                table: "Notifications",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TradeOfferScrapedDate",
                table: "Notifications",
                column: "TradeOfferScrapedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SellOffers_Name",
                table: "SellOffers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_SellOffers_Price",
                table: "SellOffers",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_SellOffers_ScrapedDate",
                table: "SellOffers",
                column: "ScrapedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "BuyOffers");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "SellOffers");
        }
    }
}