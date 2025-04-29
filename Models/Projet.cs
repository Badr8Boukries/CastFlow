using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CastFlow.Api.Models
{
    [Table("Projets")]
    public class Projet
    {
        [Key]
        public long ProjetId { get; set; }



        [Required]
        [StringLength(150)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TypeProjet { get; set; } = string.Empty; // Ex: "FILM", "SERIE"

        [Required]
        [StringLength(50)] 
        public string Statut { get; set; } = string.Empty; // Ex: "CASTING_OUVERT", "FERME"

        [Required]
        [StringLength(50)]
        public string realisateur { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Logline { get; set; }

        [Column(TypeName = "text")]
        public string? Synopsis { get; set; }

        public DateTime CreeLe { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}