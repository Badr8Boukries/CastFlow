using System;

namespace CastFlow.Api.Dtos.Response
{

    public class MyCandidatureResponseDto
    {
        public long CandidatureId { get; set; }
        public long RoleId { get; set; }
        public string RoleNom { get; set; } = string.Empty;
        public long ProjetId { get; set; }
        public string ProjetTitre { get; set; } = string.Empty;
        public DateTime DateCandidature { get; set; }
        public string Statut { get; set; } = string.Empty; 
        public string? CommentaireTalent { get; set; } 
        public DateTime? DispoDebut { get; set; } 
        public DateTime? DispoFin { get; set; }
    }
}