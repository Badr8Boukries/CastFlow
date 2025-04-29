using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// PAS DE using CastFlow.Api.Enums;

namespace CastFlow.Api.Models
{
    [Table("UserTalents")]
    public class UserTalent
    {
        [Key]
        public long TalentId { get; set; }

        [Required]
        [StringLength(50)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        // ATTENTION: Unicité à vérifier manuellement vs UserAdmin
        public string Email { get; set; } = string.Empty;

        [Required]
        public string MotDePasseHash { get; set; } = string.Empty;

        [Required]
        public DateTime DateNaissance { get; set; }

        [Required]
        [StringLength(20)] // Ajuster taille si besoin
        public string Sex { get; set; } = string.Empty; // Ex: "Homme", "Femme"

        [Phone]
        [StringLength(20)]
        public string? Telephone { get; set; }

        [Url]
        [StringLength(2048)]
        public string? UrlPhoto { get; set; }

        [Url]
        [StringLength(2048)]
        public string? UrlCv { get; set; }

        public DateTime CreeLe { get; set; } = DateTime.UtcNow;
        public DateTime ModifieLe { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Candidature> Candidatures { get; set; } = new List<Candidature>();
    }
}