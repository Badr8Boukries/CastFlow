using System.ComponentModel.DataAnnotations;

namespace CastFlow.Api.Dtos.Request
{
    public class SetupAdminAccountRequestDto
    {
        [Required]
        public string ActivationToken { get; set; } = string.Empty; // Le token reste requis

        [Required]
        [StringLength(50)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string MotDePasse { get; set; } = string.Empty;

        [Required]
        [Compare("MotDePasse")]
        public string ConfirmMotDePasse { get; set; } = string.Empty;
    }
}