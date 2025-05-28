using System;

namespace CastFlow.Api.Dtos.Response
{

    public class CandidatureDetailResponseDto
    {
        public long CandidatureId { get; set; }
        public DateTime DateCandidature { get; set; }
        public string Statut { get; set; } = string.Empty;
        public string? CommentaireTalent { get; set; }
        public DateTime? DateAssignation { get; set; }
        public DateTime? DispoDebut { get; set; } 
        public DateTime? DispoFin { get; set; }   

        public TalentProfileResponseDto Talent { get; set; } = null!;
        public RoleSummaryResponseDto Role { get; set; } = null!;
        public string? UrlVideoAudition { get; set; }
        public decimal? NoteMoyenne { get; set; }
        public List<AdminNoteResponseDto> NotesIndividuelles { get; set; } = new List<AdminNoteResponseDto>();
    }
}