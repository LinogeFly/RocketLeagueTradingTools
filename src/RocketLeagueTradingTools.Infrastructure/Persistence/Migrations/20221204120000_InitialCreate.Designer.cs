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
    [Migration("20221204120000_InitialCreate")]
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

                    b.HasIndex("ItemName");

                    b.HasIndex("PriceFrom");

                    b.HasIndex("PriceTo");

                    b.ToTable("Alerts", (string)null);
                });

            modelBuilder.Entity("RocketLeagueTradingTools.Infrastructure.Persistence.Models.PersistedBlacklistedTrader", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("TraderName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TradingSite")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("TradingSite", "TraderName")
                        .IsUnique();

                    b.ToTable("Blacklist", (string)null);
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

                    b.Property<int>("TradeOfferId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CreatedDate");

                    b.HasIndex("TradeOfferId");

                    b.ToTable("Notifications", (string)null);
                });

            modelBuilder.Entity("RocketLeagueTradingTools.Infrastructure.Persistence.Models.PersistedTradeOffer", b =>
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

                    b.Property<string>("ItemName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ItemType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OfferType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Price")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ScrapedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("TraderName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("TradingSite")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ItemName");

                    b.HasIndex("Price");

                    b.HasIndex("ScrapedDate");

                    b.ToTable("TradeOffers", (string)null);
                });

            modelBuilder.Entity("RocketLeagueTradingTools.Infrastructure.Persistence.Models.PersistedNotification", b =>
                {
                    b.HasOne("RocketLeagueTradingTools.Infrastructure.Persistence.Models.PersistedTradeOffer", "TradeOffer")
                        .WithMany()
                        .HasForeignKey("TradeOfferId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("TradeOffer");
                });
#pragma warning restore 612, 618
        }
    }
}