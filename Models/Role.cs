using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CastFlow.Api.Models;
namespace CastFlow.Api.Models
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        public long RoleId { get; set; }

        [Required]
        public long ProjetId { get; set; }
        [ForeignKey("ProjetId")]
        public virtual Projet Projet { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "text")]
        public string Description { get; set; } = string.Empty;

        public int? AgeMin { get; set; }
        public int? AgeMax { get; set; }

        [Required]
        [StringLength(20)] 
        public string ExigenceSex { get; set; } = string.Empty; // Ex: "Homme", "Femme", "Indifferent"

        [Required]
        public DateTime DateLimiteCandidature { get; set; }

        [Required]
        public bool EstPublie { get; set; }

        public DateTime CreeLe { get; set; } = DateTime.UtcNow;
        [Required]
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<Candidature> Candidatures { get; set; } = new List<Candidature>();
    }
}