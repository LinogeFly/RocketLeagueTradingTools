﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RocketLeagueTradingTools.Infrastructure.Persistence.SQLite;

#nullable disable

namespace RocketLeagueTradingTools.Infrastructure.Persistence.SQLite.Migrations
{
    [DbContext(typeof(SQLiteDbContext))]
    partial class SQLiteDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.9");

            modelBuilder.Entity("RocketLeagueTradingTools.Infrastructure.Persistence.SQLite.PersistedTypes.PersistedAlert", b =>
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

                    b.Property<bool>("Disabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ItemName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("OfferType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PriceFrom")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PriceTo")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Alerts", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Certification = "",
                            Color = "None",
                            Disabled = false,
                            ItemName = "Hellfire",
                            OfferType = 1,
                            PriceFrom = 0,
                            PriceTo = 100
                        },
                        new
                        {
                            Id = 2,
                            Certification = "",
                            Color = "None",
                            Disabled = false,
                            ItemName = "Dueling Dragons",
                            OfferType = 1,
                            PriceFrom = 0,
                            PriceTo = 500
                        });
                });

            modelBuilder.Entity("RocketLeagueTradingTools.Infrastructure.Persistence.SQLite.PersistedTypes.PersistedBuyOffer", b =>
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

                    b.HasIndex("Id", "SourceId", "ScrapedDate", "Name", "Price", "Color", "Certification");

                    b.ToTable("BuyOffers", (string)null);
                });

            modelBuilder.Entity("RocketLeagueTradingTools.Infrastructure.Persistence.SQLite.PersistedTypes.PersistedSellOffer", b =>
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

                    b.HasIndex("Id", "SourceId", "ScrapedDate", "Name", "Price", "Color", "Certification");

                    b.ToTable("SellOffers", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
