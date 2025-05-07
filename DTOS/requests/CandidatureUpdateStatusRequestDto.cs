using System.ComponentModel.DataAnnotations;
namespace CastFlow.Api.Dtos.Request
{
    public class CandidatureUpdateStatusRequestDto
    {
        [Required]
        [StringLength(50)]
        public string NouveauStatut { get; set; } = string.Empty; 

        [StringLength(1000, ErrorMessage = "Le message ne peut pas dépasser 1000 caractères.")]
        public string? MessagePourTalent { get; set; } 
    }
}