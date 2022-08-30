﻿// <auto-generated />
using System;
using Aurora.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Aurora.Infrastructure.Migrations
{
    [DbContext(typeof(SearchContext))]
    [Migration("20220830155152_RemoveQueueItems")]
    partial class RemoveQueueItems
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Aurora.Application.Entities.SearchOptionSnapshot", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("IsProcessed")
                        .HasColumnType("boolean");

                    b.Property<Guid>("SearchOptionId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Time")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("SearchOptionId");

                    b.ToTable("Snapshots");
                });

            modelBuilder.Entity("Aurora.Application.Entities.SearchRequestOption", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("ContentType")
                        .HasColumnType("integer");

                    b.Property<int>("OccurredCount")
                        .HasColumnType("integer");

                    b.Property<string>("SearchTerm")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Website")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SearchTerm", "Website", "ContentType")
                        .IsUnique();

                    b.ToTable("Options");
                });

            modelBuilder.Entity("Aurora.Application.Entities.SearchResult", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AdditionalData")
                        .HasColumnType("text");

                    b.Property<DateTime>("FoundTimeUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ImagePreviewUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SearchItemUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("SearchOptionSnapshotId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SearchOptionSnapshotId");

                    b.ToTable("Result");
                });

            modelBuilder.Entity("Aurora.Application.Entities.SearchOptionSnapshot", b =>
                {
                    b.HasOne("Aurora.Application.Entities.SearchRequestOption", "SearchOption")
                        .WithMany("Snapshots")
                        .HasForeignKey("SearchOptionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SearchOption");
                });

            modelBuilder.Entity("Aurora.Application.Entities.SearchResult", b =>
                {
                    b.HasOne("Aurora.Application.Entities.SearchOptionSnapshot", "SearchOptionSnapshot")
                        .WithMany()
                        .HasForeignKey("SearchOptionSnapshotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SearchOptionSnapshot");
                });

            modelBuilder.Entity("Aurora.Application.Entities.SearchRequestOption", b =>
                {
                    b.Navigation("Snapshots");
                });
#pragma warning restore 612, 618
        }
    }
}
