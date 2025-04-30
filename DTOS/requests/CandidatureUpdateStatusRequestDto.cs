using System.ComponentModel.DataAnnotations;

namespace CastFlow.Api.Dtos.Request
{
    public class CandidatureUpdateStatusRequestDto
    {
        [Required]
        [StringLength(50)]
        public string NouveauStatut { get; set; } = string.Empty; // Ex: "ASSIGNE", "REFUSE", etc.
    }
}