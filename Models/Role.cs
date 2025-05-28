using System;
using System.Collections.Generic; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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


        [Column(TypeName = "nvarchar(MAX)")] 
        public string? InstructionsVideo { get; set; }

        public int? AgeMin { get; set; }
        public int? AgeMax { get; set; }

        [Required]
        [StringLength(20)]
        public string ExigenceSex { get; set; } = string.Empty;

        [Required]
        public DateTime DateLimiteCandidature { get; set; }

        [Required]
        public bool EstPublie { get; set; } = false;
        [Required]
        [StringLength(50)]
        public string Statut { get; set; } = "OUVERT_AU_CASTING";

        [Required]
        public bool IsDeleted { get; set; } = false; 

        [Required]
        public DateTime CreeLe { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime ModifieLe { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Candidature> Candidatures { get; set; } = new List<Candidature>();
        public long? TalentAssigneId { get; set; } 
        [ForeignKey("TalentAssigneId")]
        public virtual UserTalent? TalentAssigne { get; set; }
    }
}