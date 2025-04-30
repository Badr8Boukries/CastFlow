using System.ComponentModel.DataAnnotations;

namespace CastFlow.Api.Dtos.Request
{
    public class VerificationRequestDto 
    {
        [Required] 
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)] 
        public string Code { get; set; } = string.Empty;
    }
}