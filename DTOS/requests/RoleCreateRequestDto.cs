using System;
using System.ComponentModel.DataAnnotations;

namespace CastFlow.Api.Dtos.Request
{
    public class RoleCreateRequestDto
    {
        // ProjetId sera dans l'URL

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public int? AgeMin { get; set; }
        public int? AgeMax { get; set; }

        [Required]
        [StringLength(20)]
        public string ExigenceSex { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateLimiteCandidature { get; set; }

        [Required]
        public bool EstPublie { get; set; }
    }
}