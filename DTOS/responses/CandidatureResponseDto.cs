using System;

namespace CastFlow.Api.Dtos.Response
{
    public class CandidatureResponseDto // Un seul DTO pour l'instant
    {
        public long CandidatureId { get; set; }
        public long RoleId { get; set; }
        public string RoleNom { get; set; } = string.Empty; // Contexte utile
        public long TalentId { get; set; }
        public string TalentNomComplet { get; set; } = string.Empty; // Contexte utile
        public string? TalentUrlPhoto { get; set; } // Pour la vue admin
        public DateTime DateCandidature { get; set; }
        public string Statut { get; set; } = string.Empty; 
        public string? CommentaireTalent { get; set; } // Visible par l'admin
        public DateTime? DateAssignation { get; set; } // Si statut = ASSIGNE
    }
}