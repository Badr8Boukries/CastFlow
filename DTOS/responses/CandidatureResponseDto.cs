using System;

namespace CastFlow.Api.Dtos.Response
{
    public class CandidatureResponseDto 
    {
        public long CandidatureId { get; set; }
        public long RoleId { get; set; }
        public string RoleNom { get; set; } = string.Empty; 
        public long TalentId { get; set; }
        public string TalentNomComplet { get; set; } = string.Empty; 
        public string? TalentUrlPhoto { get; set; } 
        public DateTime DateCandidature { get; set; }
        public string Statut { get; set; } = string.Empty; 
        public string? CommentaireTalent { get; set; } 
        public DateTime? DateAssignation { get; set; } 
    }
}