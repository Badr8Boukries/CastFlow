using System.ComponentModel.DataAnnotations;

namespace CastFlow.Api.Dtos.Request
{
    public class InviteAdminRequestDto
    {
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Nom { get; set; } = string.Empty;
    }
}