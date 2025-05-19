using System;
using System.ComponentModel.DataAnnotations;

namespace CastFlow.Api.Dtos.Request
{
    public class RoleCreateRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(0, 120)] 
        public int? AgeMin { get; set; }

        [Range(0, 120)] 
        public int? AgeMax { get; set; }

        [Required]
        [StringLength(20)]
        public string ExigenceSex { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateLimiteCandidature { get; set; }

        [Required]
        public bool EstPublie { get; set; } = false;
      
        [StringLength(1000)]
        public string? InstructionsVideo { get; set; }

    }
}