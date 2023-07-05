﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EFinfrastructure.Migrations
{
    [DbContext(typeof(ItemDbContext))]
    [Migration("20230705082950_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("WolverineEnabled", "true");

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EF.Item", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("Wolverine.EntityFrameworkCore.Internals.IncomingMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<int>("Attempts")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0)
                        .HasColumnName("attempts");

                    b.Property<byte[]>("Body")
                        .IsRequired()
                        .HasColumnType("varbinary(max)")
                        .HasColumnName("body");

                    b.Property<DateTimeOffset?>("ExecutionTime")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("execution_time");

                    b.Property<string>("MessageType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("message_type");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int")
                        .HasColumnName("owner_id");

                    b.Property<string>("ReceivedAt")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("received_at");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.ToTable("wolverine_incoming_envelopes", null, t =>
                        {
                            t.ExcludeFromMigrations();
                        });
                });

            modelBuilder.Entity("Wolverine.EntityFrameworkCore.Internals.OutgoingMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<int>("Attempts")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0)
                        .HasColumnName("attempts");

                    b.Property<byte[]>("Body")
                        .IsRequired()
                        .HasColumnType("varbinary(max)")
                        .HasColumnName("body");

                    b.Property<DateTimeOffset?>("DeliverBy")
                        .HasColumnType("datetimeoffset")
                        .HasColumnName("deliver_by");

                    b.Property<string>("Destination")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("destination");

                    b.Property<string>("MessageType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("message_type");

                    b.Property<int>("OwnerId")
                        .HasColumnType("int")
                        .HasColumnName("owner_id");

                    b.HasKey("Id");

                    b.ToTable("wolverine_outgoing_envelopes", null, t =>
                        {
                            t.ExcludeFromMigrations();
                        });
                });
#pragma warning restore 612, 618
        }
    }
}