using System;
using System.ComponentModel.DataAnnotations;

namespace CastFlow.Api.Dtos.Request
{
    public class RegisterTalentRequestDto
    {
        [Required]
        [StringLength(50)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)] 
        public string MotDePasse { get; set; } = string.Empty;

        [Required]
        [Compare("MotDePasse")] 
        public string ConfirmMotDePasse { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateNaissance { get; set; }

        [Required]
        [StringLength(20)]
        public string Sex { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Telephone { get; set; }
        public IFormFile? PhotoFile { get; set; }
        public IFormFile? CvFile { get; set; }
    }
}