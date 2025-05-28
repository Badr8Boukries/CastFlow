using Microsoft.EntityFrameworkCore;
using CastFlow.Api.Models; 
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
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<AdminCandidatureNote> AdminCandidatureNotes { get; set; } = null!; // <-- NOUVEAU DBSET

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuration pour UserAdmin ---
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

            // --- Configuration pour UserTalent ---
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

            // --- Configuration pour Projet ---
            modelBuilder.Entity<Projet>(entity =>
            {
                entity.ToTable("Projets");
                entity.HasKey(p => p.ProjetId);
                entity.Property(p => p.Titre).IsRequired().HasMaxLength(150);
                entity.Property(p => p.TypeProjet).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Statut).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Realisateur).IsRequired().HasMaxLength(100); // Vérifie le nom de la propriété dans ton modèle Projet.cs
                entity.Property(p => p.Logline).HasMaxLength(500);
                entity.Property(p => p.Synopsis).HasColumnType("text");
                entity.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);
                entity.Property(p => p.CreeLe).IsRequired();
                entity.Property(p => p.ModifieLe).IsRequired();

                entity.HasQueryFilter(p => !p.IsDeleted); 

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
                entity.Property(r => r.InstructionsVideo).HasMaxLength(1000); 
                entity.Property(r => r.IsDeleted).IsRequired().HasDefaultValue(false);
                entity.Property(r => r.CreeLe).IsRequired();
                entity.Property(r => r.ModifieLe).IsRequired();
                entity.Property(r => r.Statut).IsRequired().HasMaxLength(50).HasDefaultValue("OUVERT_AU_CASTING");

                entity.Property(r => r.TalentAssigneId).IsRequired(false);

                entity.HasQueryFilter(r => !r.IsDeleted); 

                entity.HasOne(r => r.Projet) 
                      .WithMany(p => p.Roles)
                      .HasForeignKey(r => r.ProjetId)
                      .OnDelete(DeleteBehavior.NoAction); 

                entity.HasOne(r => r.TalentAssigne) 
                      .WithMany() 
                      .HasForeignKey(r => r.TalentAssigneId)
                      .OnDelete(DeleteBehavior.SetNull); 

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
                entity.Property(c => c.DispoDebut); 
                entity.Property(c => c.DispoFin);   
                entity.Property(c => c.UrlVideoAudition).HasMaxLength(2048);
                entity.Property(c => c.CreeLe).IsRequired();

                entity.Property(c => c.NoteMoyenne).HasColumnType("decimal(2, 1)").IsRequired(false); 
                entity.HasOne(c => c.Talent)
                      .WithMany(ut => ut.Candidatures)
                      .HasForeignKey(c => c.TalentId);

                entity.HasOne(c => c.Role)
                      .WithMany(r => r.Candidatures)
                      .HasForeignKey(c => c.RoleId)
                      .OnDelete(DeleteBehavior.Cascade); 
                                                       

                entity.HasMany(c => c.AdminNotes)
                      .WithOne(an => an.Candidature)
                      .HasForeignKey(an => an.CandidatureId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasOne(n => n.DestinataireTalent)
                      .WithMany() 
                      .HasForeignKey(n => n.DestinataireTalentId)
                      .OnDelete(DeleteBehavior.Cascade);
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
                // ... (tes configurations)
                entity.HasOne(t => t.InvitedByAdmin)
                     .WithMany()
                     .HasForeignKey(t => t.InvitedByAdminId)
                     .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.CreatedAdmin)
                      .WithMany()
                      .HasForeignKey(t => t.CreatedAdminId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<AdminCandidatureNote>(entity =>
            {
                entity.ToTable("AdminCandidatureNotes");
                entity.HasKey(an => an.NoteId);
                entity.Property(an => an.CandidatureId).IsRequired();
                entity.Property(an => an.AdminId).IsRequired();
                entity.Property(an => an.NoteValue).IsRequired().HasColumnType("decimal(2, 1)");
                entity.Property(an => an.DateNote).IsRequired();

                entity.HasIndex(an => new { an.CandidatureId, an.AdminId }).IsUnique();


                entity.HasOne(an => an.Admin)
                      .WithMany() 
                      .HasForeignKey(an => an.AdminId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });
        }
    }
}