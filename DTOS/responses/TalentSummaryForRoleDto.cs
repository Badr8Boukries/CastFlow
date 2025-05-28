namespace CastFlow.Api.Dtos.Response
{
    public class TalentSummaryForRoleDto
    {
        public long TalentId { get; set; }
        public string Prenom { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string? UrlPhoto { get; set; }
    }
}