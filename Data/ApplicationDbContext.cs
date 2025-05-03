using Microsoft.EntityFrameworkCore;
using CastFlow.Api.Models; // Assure-toi que ce namespace pointe vers tes modèles

namespace CastFlow.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet pour chaque entité
        public DbSet<UserAdmin> UserAdmins { get; set; } = null!;
        public DbSet<UserTalent> UserTalents { get; set; } = null!;
        public DbSet<Projet> Projets { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Candidature> Candidatures { get; set; } = null!;
        public DbSet<EmailVerifier> EmailVerifiers { get; set; } = null!; 
        public DbSet<AdminInvitationToken> AdminInvitationTokens { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


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

            modelBuilder.Entity<UserTalent>(entity =>
            {
                entity.ToTable("UserTalents");
                entity.HasKey(ut => ut.TalentId);
                entity.Property(ut => ut.Prenom).HasMaxLength(50); 
                entity.Property(ut => ut.Nom).HasMaxLength(50);    
                entity.Property(ut => ut.Email).HasMaxLength(100); 
                entity.Property(ut => ut.MotDePasseHash);
                entity.Property(ut => ut.DateNaissance);
                entity.Property(ut => ut.Sex).HasMaxLength(20);    
                entity.Property(ut => ut.Telephone).HasMaxLength(20);
                entity.Property(ut => ut.UrlPhoto).HasMaxLength(2048);
                entity.Property(ut => ut.UrlCv).HasMaxLength(2048);
                entity.Property(ut => ut.IsEmailVerified).IsRequired();
                entity.Property(ut => ut.IsDeleted).IsRequired().HasDefaultValue(false); 
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

            modelBuilder.Entity<EmailVerifier>(entity =>
            {
                entity.ToTable("EmailVerifiers");
                entity.HasKey(ev => ev.Id);
                entity.Property(ev => ev.Email).IsRequired().HasMaxLength(100);
                entity.Property(ev => ev.VerificationCode).IsRequired().HasMaxLength(100);
                entity.Property(ev => ev.ExpiresAt).IsRequired();
                entity.Property(ev => ev.UserId).IsRequired(); 
                entity.Property(ev => ev.IsVerified).IsRequired();
                entity.Property(ev => ev.CreatedAt).IsRequired();

                entity.HasIndex(ev => new { ev.Email, ev.VerificationCode, ev.IsVerified, ev.ExpiresAt });

                entity.HasOne(ev => ev.UserTalent)
                      .WithMany() 
                      .HasForeignKey(ev => ev.UserId)
                      .OnDelete(DeleteBehavior.Cascade); 

            });

            modelBuilder.Entity<AdminInvitationToken>(entity =>
            {
                entity.ToTable("AdminInvitationTokens");
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Email).IsRequired().HasMaxLength(100);
                entity.Property(t => t.ActivationToken).IsRequired().HasMaxLength(100);
                entity.Property(t => t.ExpiresAt).IsRequired();
                entity.Property(t => t.IsUsed).IsRequired();
                entity.Property(t => t.InvitedByAdminId).IsRequired();
                entity.Property(t => t.CreatedAt).IsRequired();
                entity.Property(t => t.CreatedAdminId); 

                entity.HasIndex(t => t.ActivationToken).IsUnique();
                entity.HasIndex(t => t.Email);

                // Relation vers l'admin qui invite (UserAdmin -> AdminInvitationToken : 1-N)
                entity.HasOne(t => t.InvitedByAdmin)
                      .WithMany() // UserAdmin n'a pas de collection d'invitations envoyées
                      .HasForeignKey(t => t.InvitedByAdminId)
                      .OnDelete(DeleteBehavior.Restrict); 

                // Relation vers l'admin créé (UserAdmin -> AdminInvitationToken : 0..1-N)
                entity.HasOne(t => t.CreatedAdmin)
                      .WithMany() // UserAdmin n'a pas de collection liée à son token d'activation
                      .HasForeignKey(t => t.CreatedAdminId)
                      .OnDelete(DeleteBehavior.SetNull); 

            });
        }
    }
}