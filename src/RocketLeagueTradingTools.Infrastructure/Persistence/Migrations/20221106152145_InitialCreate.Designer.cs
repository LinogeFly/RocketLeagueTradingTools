﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RocketLeagueTradingTools.Infrastructure.Persistence;

#nullable disable

namespace RocketLeagueTradingTools.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(PersistenceDbContext))]
    [Migration("20221106152145_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.9");

            modelBuilder.Entity("RocketLeagueTradingTools.Infrastructure.Persistence.Models.PersistedAlert", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Certification")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Enabled")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ItemName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ItemType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OfferType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PriceFrom")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PriceTo")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Id", "ItemName", "Enabled", "OfferType", "PriceFrom", "PriceTo", "ItemType", "Color", "Certification");

                    b.ToTable("Alerts", (string)null);
                });

            modelBuilder.Entity("RocketLeagueTradingTools.Infrastructure.Persistence.Models.PersistedBuyOffer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Certification")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ItemType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Price")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ScrapedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("SourceId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id", "Name", "ScrapedDate", "Price", "ItemType", "Color", "Certification");

                    b.ToTable("BuyOffers", (string)null);
                });

            modelBuilder.Entity("RocketLeagueTradingTools.Infrastructure.Persistence.Models.PersistedNotification", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("SeenDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("TradeItemCertification")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TradeItemColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TradeItemName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TradeItemType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TradeOfferLink")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("TradeOfferPrice")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TradeOfferScrapedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("TradeOfferSourceId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id", "CreatedDate", "TradeOfferScrapedDate");

                    b.ToTable("Notifications", (string)null);
                });

            modelBuilder.Entity("RocketLeagueTradingTools.Infrastructure.Persistence.Models.PersistedSellOffer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Certification")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ItemType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Price")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ScrapedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("SourceId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Id", "Name", "ScrapedDate", "Price", "ItemType", "Color", "Certification");

                    b.ToTable("SellOffers", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}