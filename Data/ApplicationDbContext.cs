﻿using Microsoft.EntityFrameworkCore;
using CastFlow.Api.Models; // Assure-toi que le namespace pointe vers tes modèles

namespace CastFlow.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet pour chaque entité
        public DbSet<UserAdmin> UserAdmins { get; set; } = null!; // Null-forgiving operator pour C# moderne
        public DbSet<UserTalent> UserTalents { get; set; } = null!;
        public DbSet<Projet> Projets { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Candidature> Candidatures { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuration Spécifique (Fluent API) ---

            // Configuration pour UserAdmin
            modelBuilder.Entity<UserAdmin>(entity =>
            {
                entity.ToTable("UserAdmins"); 
                entity.HasKey(ua => ua.AdminId); 
                entity.HasIndex(ua => ua.Email).IsUnique(); 
                entity.Property(ua => ua.Prenom).IsRequired().HasMaxLength(50);
                entity.Property(ua => ua.Nom).IsRequired().HasMaxLength(50);
                entity.Property(ua => ua.Email).IsRequired().HasMaxLength(100);
                entity.Property(ua => ua.MotDePasseHash).IsRequired();
                entity.Property(ua => ua.CreeLe).IsRequired();
            });

            // Configuration pour UserTalent
            modelBuilder.Entity<UserTalent>(entity =>
            {
                entity.ToTable("UserTalents");
                entity.HasKey(ut => ut.TalentId);
                entity.HasIndex(ut => ut.Email).IsUnique(); 
                entity.Property(ut => ut.Prenom).IsRequired().HasMaxLength(50);
                entity.Property(ut => ut.Nom).IsRequired().HasMaxLength(50);
                entity.Property(ut => ut.Email).IsRequired().HasMaxLength(100);
                entity.Property(ut => ut.MotDePasseHash).IsRequired();
                entity.Property(ut => ut.DateNaissance).IsRequired();
                entity.Property(ut => ut.Sex).IsRequired().HasMaxLength(20); 
                entity.Property(ut => ut.Telephone).HasMaxLength(20);
                entity.Property(ut => ut.UrlPhoto).HasMaxLength(2048);
                entity.Property(ut => ut.UrlCv).HasMaxLength(2048);
                entity.Property(ut => ut.CreeLe).IsRequired();
                entity.Property(ut => ut.ModifieLe).IsRequired();

                entity.HasMany(ut => ut.Candidatures)
                      .WithOne(c => c.Talent) 
                      .HasForeignKey(c => c.TalentId) 
                      .OnDelete(DeleteBehavior.Restrict); 
            });

            modelBuilder.Entity<Projet>(entity =>
            {
                entity.ToTable("Projets");
                entity.HasKey(p => p.ProjetId);
                entity.Property(p => p.Titre).IsRequired().HasMaxLength(150);
                entity.Property(p => p.TypeProjet).IsRequired().HasMaxLength(50); 
                entity.Property(p => p.Statut).IsRequired().HasMaxLength(50);     
                entity.Property(p => p.realisateur).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Logline).HasMaxLength(500);
                entity.Property(p => p.Synopsis).HasColumnType("text");
                entity.Property(p => p.CreeLe).IsRequired();

                
                entity.HasMany(p => p.Roles)
                      .WithOne(r => r.Projet) 
                      .HasForeignKey(r => r.ProjetId) 
                      .OnDelete(DeleteBehavior.Cascade); 
            });

            // Configuration pour Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(r => r.RoleId);
                entity.Property(r => r.ProjetId).IsRequired();
                entity.Property(r => r.Nom).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Description).IsRequired().HasColumnType("text");
                entity.Property(r => r.ExigenceSex).IsRequired().HasMaxLength(20); 
                entity.Property(r => r.DateLimiteCandidature).IsRequired();
                entity.Property(r => r.EstPublie).IsRequired();
                entity.Property(r => r.CreeLe).IsRequired();

                entity.HasMany(r => r.Candidatures)
                      .WithOne(c => c.Role) 
                      .HasForeignKey(c => c.RoleId) 
                      .OnDelete(DeleteBehavior.Cascade); 
            });

            modelBuilder.Entity<Candidature>(entity =>
            {
                entity.ToTable("Candidatures");
                entity.HasKey(c => c.CandidatureId);
                entity.Property(c => c.TalentId).IsRequired();
                entity.Property(c => c.RoleId).IsRequired();
                entity.Property(c => c.DateCandidature).IsRequired();
                entity.Property(c => c.CommentaireTalent).HasColumnType("text");
                entity.Property(c => c.Statut).IsRequired().HasMaxLength(50); 
                entity.Property(c => c.CreeLe).IsRequired();

            });
        }
    }
}