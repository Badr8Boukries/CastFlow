using System;
using System.ComponentModel.DataAnnotations;

namespace CastFlow.Api.Dtos.Request
{
  
    public class RoleUpdateRequestDto
    {
        [StringLength(100, MinimumLength = 1)]
        public string? Nom { get; set; }

        public string? Description { get; set; } 

        [Range(0, 120)] 
        public int? AgeMin { get; set; }

        [Range(0, 120)] 
        public int? AgeMax { get; set; } 

        [StringLength(20)]
        public string? ExigenceSex { get; set; } 

        [DataType(DataType.Date)]
        public DateTime? DateLimiteCandidature { get; set; } 

        public bool? EstPublie { get; set; } 
    }
}