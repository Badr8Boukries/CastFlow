// Models/UserTalent.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CastFlow.Api.Models
{
    [Table("UserTalents")]
    public class UserTalent
    {
        [Key]
        public long TalentId { get; set; }

        [StringLength(50)] 
        public string? Prenom { get; set; } 

        [StringLength(50)] 
        public string? Nom { get; set; } 

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public string? MotDePasseHash { get; set; } 

        
        public DateTime DateNaissance { get; set; } 

        [StringLength(20)]
        public string? Sex { get; set; } 

        [Phone]
        [StringLength(20)]
        public string? Telephone { get; set; } 

        [Url]
        [StringLength(2048)]
        public string? UrlPhoto { get; set; } 

        [Url]
        [StringLength(2048)]
        public string? UrlCv { get; set; } 

        [Required]
        public bool IsEmailVerified { get; set; } = false;

        [Required]
        public bool IsDeleted { get; set; } = false; 

        public DateTime CreeLe { get; set; } = DateTime.UtcNow;
        public DateTime ModifieLe { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Candidature> Candidatures { get; set; } = new List<Candidature>();
    }
}