﻿// <auto-generated />
using System;
using CastFlow.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CastFlow.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CastFlow.Api.Models.Candidature", b =>
                {
                    b.Property<long>("CandidatureId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("CandidatureId"));

                    b.Property<string>("CommentaireTalent")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreeLe")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateAssignation")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateCandidature")
                        .HasColumnType("datetime2");

                    b.Property<long>("RoleId")
                        .HasColumnType("bigint");

                    b.Property<string>("Statut")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<long>("TalentId")
                        .HasColumnType("bigint");

                    b.HasKey("CandidatureId");

                    b.HasIndex("RoleId");

                    b.HasIndex("TalentId");

                    b.ToTable("Candidatures", (string)null);
                });

            modelBuilder.Entity("CastFlow.Api.Models.Projet", b =>
                {
                    b.Property<long>("ProjetId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("ProjetId"));

                    b.Property<DateTime>("CreeLe")
                        .HasColumnType("datetime2");

                    b.Property<string>("Logline")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("Statut")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Synopsis")
                        .HasColumnType("text");

                    b.Property<string>("Titre")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<string>("TypeProjet")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("realisateur")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("ProjetId");

                    b.ToTable("Projets", (string)null);
                });

            modelBuilder.Entity("CastFlow.Api.Models.Role", b =>
                {
                    b.Property<long>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("RoleId"));

                    b.Property<int?>("AgeMax")
                        .HasColumnType("int");

                    b.Property<int?>("AgeMin")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreeLe")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateLimiteCandidature")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("EstPublie")
                        .HasColumnType("bit");

                    b.Property<string>("ExigenceSex")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<long>("ProjetId")
                        .HasColumnType("bigint");

                    b.HasKey("RoleId");

                    b.HasIndex("ProjetId");

                    b.ToTable("Roles", (string)null);
                });

            modelBuilder.Entity("CastFlow.Api.Models.UserAdmin", b =>
                {
                    b.Property<long>("AdminId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("AdminId"));

                    b.Property<DateTime>("CreeLe")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("MotDePasseHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Prenom")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("AdminId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("UserAdmins", (string)null);
                });

            modelBuilder.Entity("CastFlow.Api.Models.UserTalent", b =>
                {
                    b.Property<long>("TalentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("TalentId"));

                    b.Property<DateTime>("CreeLe")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateNaissance")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("ModifieLe")
                        .HasColumnType("datetime2");

                    b.Property<string>("MotDePasseHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Prenom")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Sex")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Telephone")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("UrlCv")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("UrlPhoto")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.HasKey("TalentId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("UserTalents", (string)null);
                });

            modelBuilder.Entity("CastFlow.Api.Models.Candidature", b =>
                {
                    b.HasOne("CastFlow.Api.Models.Role", "Role")
                        .WithMany("Candidatures")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CastFlow.Api.Models.UserTalent", "Talent")
                        .WithMany("Candidatures")
                        .HasForeignKey("TalentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("Talent");
                });

            modelBuilder.Entity("CastFlow.Api.Models.Role", b =>
                {
                    b.HasOne("CastFlow.Api.Models.Projet", "Projet")
                        .WithMany("Roles")
                        .HasForeignKey("ProjetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Projet");
                });

            modelBuilder.Entity("CastFlow.Api.Models.Projet", b =>
                {
                    b.Navigation("Roles");
                });

            modelBuilder.Entity("CastFlow.Api.Models.Role", b =>
                {
                    b.Navigation("Candidatures");
                });

            modelBuilder.Entity("CastFlow.Api.Models.UserTalent", b =>
                {
                    b.Navigation("Candidatures");
                });
#pragma warning restore 612, 618
        }
    }
}
