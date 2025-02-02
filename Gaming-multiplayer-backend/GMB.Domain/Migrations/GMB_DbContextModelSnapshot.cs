﻿// <auto-generated />
using System;
using GMB.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GMB.Domain.Migrations
{
    [DbContext(typeof(GMB_DbContext))]
    partial class GMB_DbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GMB.Domain.Entities.Client", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ConnectionId")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("PublicIp")
                        .HasMaxLength(45)
                        .HasColumnType("character varying(45)");

                    b.Property<int?>("PublicPort")
                        .HasColumnType("integer");

                    b.Property<Guid?>("RoomId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ConnectionId")
                        .IsUnique()
                        .HasDatabaseName("IX_Client_ConnectionId");

                    b.HasIndex("RoomId");

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("GMB.Domain.Entities.Room", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<int>("MaxPlayers")
                        .HasColumnType("integer");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("bytea");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique()
                        .HasDatabaseName("IX_Room_Code");

                    b.ToTable("Rooms");
                });

            modelBuilder.Entity("GMB.Domain.Entities.Client", b =>
                {
                    b.HasOne("GMB.Domain.Entities.Room", "Room")
                        .WithMany("Clients")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Room");
                });

            modelBuilder.Entity("GMB.Domain.Entities.Room", b =>
                {
                    b.Navigation("Clients");
                });
#pragma warning restore 612, 618
        }
    }
}
