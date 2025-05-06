using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CastFlow.Api.Models
{
    [Table("Candidatures")]
    public class Candidature
    {
        [Key]
        public long CandidatureId { get; set; }

        [Required]
        public long TalentId { get; set; } 
        [ForeignKey("TalentId")]
        public virtual UserTalent Talent { get; set; } = null!;

        [Required]
        public long RoleId { get; set; } 
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [Required]
        public DateTime DateCandidature { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "text")]
        public string? CommentaireTalent { get; set; } 

        [Required]
        [StringLength(50)]
        public string Statut { get; set; } = "RECUE"; 

        public DateTime? DateAssignation { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DispoDebut { get; set; } 

        [DataType(DataType.Date)]
        public DateTime? DispoFin { get; set; }   

        [Required]
        public DateTime CreeLe { get; set; } = DateTime.UtcNow;
    }
}