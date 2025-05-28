using System;
namespace CastFlow.Api.Dtos.Response
{
    public class RoleSummaryResponseDto
    {
        public long RoleId { get; set; }
        public long ProjetId { get; set; } 
        public string ProjetTitre { get; set; } = string.Empty; 
        public string Nom { get; set; } = string.Empty;
        public string ExigenceSex { get; set; } = string.Empty;
        public int? AgeMin { get; set; }
        public int? AgeMax { get; set; }
        public DateTime DateLimiteCandidature { get; set; }
        public bool EstPublie { get; set; }
        public TalentSummaryForRoleDto? TalentAssigneInfo { get; set; }

    }
}