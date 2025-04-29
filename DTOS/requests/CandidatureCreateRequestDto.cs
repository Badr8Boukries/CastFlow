namespace CastFlow.Api.Dtos.Request
{
    public class CandidatureCreateRequestDto
    {
        // RoleId sera dans l'URL
        // TalentId sera pris du contexte d'authentification

        public string? CommentaireTalent { get; set; } // Le seul champ envoyé dans le corps
    }
}