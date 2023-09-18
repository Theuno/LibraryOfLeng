﻿// <auto-generated />
using System;
using Leng.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Leng.Infrastructure.Migrations
{
    [DbContext(typeof(LengDbContext))]
    [Migration("20230917075717_UpdateMTGCardsRelation")]
    partial class UpdateMTGCardsRelation
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Leng.Domain.Models.LengUser", b =>
                {
                    b.Property<string>("LengUserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("aduuid")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("LengUserID");

                    b.ToTable("LengUser");
                });

            modelBuilder.Entity("Leng.Domain.Models.LengUserDeck", b =>
                {
                    b.Property<string>("LengUserID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("MTGDeckID")
                        .HasColumnType("int");

                    b.HasKey("LengUserID", "MTGDeckID");

                    b.HasIndex("MTGDeckID");

                    b.ToTable("LengUserDeck");
                });

            modelBuilder.Entity("Leng.Domain.Models.LengUserMTGCards", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LengUserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("MTGCardsId")
                        .HasColumnType("int");

                    b.Property<int>("count")
                        .HasColumnType("int");

                    b.Property<int>("countFoil")
                        .HasColumnType("int");

                    b.Property<int>("want")
                        .HasColumnType("int");

                    b.Property<int>("wantFoil")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("LengUserId");

                    b.HasIndex("MTGCardsId");

                    b.ToTable("LengUserMTGCards");
                });

            modelBuilder.Entity("Leng.Domain.Models.MTGCards", b =>
                {
                    b.Property<int>("MTGCardsID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MTGCardsID"));

                    b.Property<int>("MTGSetsID")
                        .HasColumnType("int");

                    b.Property<string>("artist")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("asciiName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("color")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("edhrecRank")
                        .HasColumnType("int");

                    b.Property<float>("edhrecSaltiness")
                        .HasColumnType("real");

                    b.Property<string>("faceName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("frameVersion")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("hasAlternativeDeckLimit")
                        .HasColumnType("bit");

                    b.Property<bool?>("hasFoil")
                        .HasColumnType("bit");

                    b.Property<bool?>("hasNonFoil")
                        .HasColumnType("bit");

                    b.Property<bool?>("isAlternative")
                        .HasColumnType("bit");

                    b.Property<bool>("isOnlineOnly")
                        .HasColumnType("bit");

                    b.Property<string>("mcmId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("number")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("originalText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("originalType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("power")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("scryfallId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("setCode")
                        .HasColumnType("varchar(8)")
                        .HasAnnotation("Relational:JsonPropertyName", "setCode");

                    b.Property<string>("side")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MTGCardsID");

                    b.HasIndex("MTGSetsID");

                    b.HasIndex("name", "setCode", "number")
                        .IsUnique()
                        .HasFilter("[setCode] IS NOT NULL AND [number] IS NOT NULL");

                    b.ToTable("MTGCard");
                });

            modelBuilder.Entity("Leng.Domain.Models.MTGDeck", b =>
                {
                    b.Property<int>("MTGDeckID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MTGDeckID"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Format")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FormatID")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MTGDeckID");

                    b.ToTable("MTGDeck");
                });

            modelBuilder.Entity("Leng.Domain.Models.MTGSets", b =>
                {
                    b.Property<int>("MTGSetsID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MTGSetsID"));

                    b.Property<int>("baseSetSize")
                        .HasColumnType("int");

                    b.Property<string>("block")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool?>("isFoilOnly")
                        .HasColumnType("bit");

                    b.Property<bool?>("isForeignOnly")
                        .HasColumnType("bit");

                    b.Property<bool?>("isNonFoilOnly")
                        .HasColumnType("bit");

                    b.Property<bool>("isOnlineOnly")
                        .HasColumnType("bit");

                    b.Property<bool>("isPartialPreview")
                        .HasColumnType("bit");

                    b.Property<int?>("mcmId")
                        .HasColumnType("int");

                    b.Property<int?>("mcmIdExtras")
                        .HasColumnType("int");

                    b.Property<string>("mcmName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("mtgoCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("releaseDate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("setCode")
                        .IsRequired()
                        .HasColumnType("varchar(8)")
                        .HasAnnotation("Relational:JsonPropertyName", "code");

                    b.Property<string>("tokenSetCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("totalSetSize")
                        .HasColumnType("int");

                    b.Property<int?>("translationsMTGTranslationsID")
                        .HasColumnType("int");

                    b.Property<string>("type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MTGSetsID");

                    b.HasIndex("setCode")
                        .IsUnique();

                    b.HasIndex("translationsMTGTranslationsID");

                    b.ToTable("MTGSets");
                });

            modelBuilder.Entity("Leng.Domain.Models.MTGTranslations", b =>
                {
                    b.Property<int>("MTGTranslationsID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MTGTranslationsID"));

                    b.Property<string>("French")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("German")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Italian")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Spanish")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("MTGTranslationsID");

                    b.ToTable("MTGTranslations");
                });

            modelBuilder.Entity("Leng.Domain.Models.LengUserDeck", b =>
                {
                    b.HasOne("Leng.Domain.Models.LengUser", "LengUser")
                        .WithMany("LengUserDecks")
                        .HasForeignKey("LengUserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Leng.Domain.Models.MTGDeck", "MTGDeck")
                        .WithMany()
                        .HasForeignKey("MTGDeckID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LengUser");

                    b.Navigation("MTGDeck");
                });

            modelBuilder.Entity("Leng.Domain.Models.LengUserMTGCards", b =>
                {
                    b.HasOne("Leng.Domain.Models.LengUser", "LengUser")
                        .WithMany("LengUserMTGCards")
                        .HasForeignKey("LengUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Leng.Domain.Models.MTGCards", "MTGCards")
                        .WithMany("LengUserMTGCards")
                        .HasForeignKey("MTGCardsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LengUser");

                    b.Navigation("MTGCards");
                });

            modelBuilder.Entity("Leng.Domain.Models.MTGCards", b =>
                {
                    b.HasOne("Leng.Domain.Models.MTGSets", "MTGSets")
                        .WithMany("Cards")
                        .HasForeignKey("MTGSetsID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MTGSets");
                });

            modelBuilder.Entity("Leng.Domain.Models.MTGSets", b =>
                {
                    b.HasOne("Leng.Domain.Models.MTGTranslations", "translations")
                        .WithMany()
                        .HasForeignKey("translationsMTGTranslationsID");

                    b.Navigation("translations");
                });

            modelBuilder.Entity("Leng.Domain.Models.LengUser", b =>
                {
                    b.Navigation("LengUserDecks");

                    b.Navigation("LengUserMTGCards");
                });

            modelBuilder.Entity("Leng.Domain.Models.MTGCards", b =>
                {
                    b.Navigation("LengUserMTGCards");
                });

            modelBuilder.Entity("Leng.Domain.Models.MTGSets", b =>
                {
                    b.Navigation("Cards");
                });
#pragma warning restore 612, 618
        }
    }
}
