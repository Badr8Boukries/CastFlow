using System;
using System.Collections.Generic;

namespace CastFlow.Api.Dtos.Response
{
    public class RoleDetailResponseDto
    {
        public long RoleId { get; set; }
        public long ProjetId { get; set; }
        public string ProjetTitre { get; set; } = string.Empty;
        public string ProjetLogline { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? AgeMin { get; set; }
        public int? AgeMax { get; set; }
        public string ExigenceSex { get; set; } = string.Empty;
        public DateTime DateLimiteCandidature { get; set; }

        public List<CandidatureSummaryResponseDto>? Candidatures { get; set; }
        public int? NombreTotalCandidatures { get; set; }
        public string? InstructionsVideo { get; set; }
    }
}