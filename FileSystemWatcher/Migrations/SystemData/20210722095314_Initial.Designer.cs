﻿// <auto-generated />
using System;
using FileSystemWatcher.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FileSystemWatcher.Migrations.SystemData
{
    [DbContext(typeof(SystemDataContext))]
    [Migration("20210722095314_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.8");

            modelBuilder.Entity("FileSystemWatcher.Model.SystemData", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<long>("ProcessedFiles")
                        .HasColumnType("bigint");

                    b.Property<long>("SendPhotos")
                        .HasColumnType("bigint");

                    b.Property<long>("SendVideos")
                        .HasColumnType("bigint");

                    b.HasKey("id");

                    b.ToTable("SystemDatas");
                });
#pragma warning restore 612, 618
        }
    }
}
