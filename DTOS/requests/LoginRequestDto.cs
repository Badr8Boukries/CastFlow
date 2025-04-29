using System.ComponentModel.DataAnnotations;

namespace CastFlow.Api.Dtos.Request
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string MotDePasse { get; set; } = string.Empty;
    }
}