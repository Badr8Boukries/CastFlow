using System;
using System.ComponentModel.DataAnnotations;

namespace CastFlow.Api.Dtos.Request
{

    public class CandidatureCreateRequestDto
    {
        [Required]
        public long RoleId { get; set; } 

        [DataType(DataType.Date)]
        public DateTime? DispoDebut { get; set; } 

        [DataType(DataType.Date)]
        public DateTime? DispoFin { get; set; }   

        [StringLength(1000, ErrorMessage = "Le commentaire ne peut pas dépasser 1000 caractères.")]
        public string? CommentaireTalent { get; set; }
        public IFormFile? VideoAuditionFile { get; set; } // Le fichier vidéo uploadé

    }
}