﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ProgImage.Storage.Domain.Persistence.Database;

namespace ProgImage.Storage.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20201011194012_storage")]
    partial class storage
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ProgImage.Storage.Domain.Models.Image", b =>
                {
                    b.Property<Guid>("ImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("CreatedAt")
                        .HasColumnType("bigint");

                    b.Property<string>("ImageFilePath")
                        .HasColumnType("text");

                    b.HasKey("ImageId");

                    b.ToTable("Images");
                });
#pragma warning restore 612, 618
        }
    }
}
