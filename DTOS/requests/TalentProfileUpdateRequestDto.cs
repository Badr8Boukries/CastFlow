using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Pour IFormFile si tu gères l'upload ici

namespace CastFlow.Api.Dtos.Request
{
    public class TalentProfileUpdateRequestDto
    {
        [Required] 
        [StringLength(50)]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Nom { get; set; } = string.Empty;

        [Required] // Garder requis
        [DataType(DataType.Date)]
        public DateTime DateNaissance { get; set; }

       
        [StringLength(20)]
        public string Sex { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Telephone { get; set; } 

       
         public IFormFile? PhotoFile { get; set; }
         public IFormFile? CvFile { get; set; }
        
    }
}