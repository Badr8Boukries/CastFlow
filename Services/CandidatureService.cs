using AutoMapper;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using CastFlow.Api.Models;
using CastFlow.Api.Repository;
using CastFlow.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; 
using System.Net.Mail;
using System.Threading.Tasks;

namespace CastFlow.Api.Services
{
    public class CandidatureService : ICandidatureService
    {
        private readonly ICandidatureRepository _candidatureRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUserTalentRepository _talentRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<CandidatureService> _logger;
        private readonly INotificationService _notificationService; 
        private readonly IConfiguration _configuration; 

        public CandidatureService(
            ICandidatureRepository candidatureRepo,
            IRoleRepository roleRepo,
            IUserTalentRepository talentRepo,
            IMapper mapper,
            ILogger<CandidatureService> logger,
            INotificationService notificationService, 
            IConfiguration configuration) 
        {
            _candidatureRepo = candidatureRepo ?? throw new ArgumentNullException(nameof(candidatureRepo));
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _talentRepo = talentRepo ?? throw new ArgumentNullException(nameof(talentRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService)); 
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration)); 
        }

        public async Task<MyCandidatureResponseDto?> ApplyToRoleAsync(long talentId, CandidatureCreateRequestDto createDto)
        {
            _logger.LogInformation("Talent ID {TalentId} postule pour Role ID {RoleId}", talentId, createDto.RoleId);
            var talent = await _talentRepo.GetActiveByIdAsync(talentId);
            if (talent == null) { _logger.LogWarning("Talent ID {TalentId} non trouvé ou inactif.", talentId); return null; }

            var role = await _roleRepo.GetActiveByIdWithProjectAsync(createDto.RoleId); 
            if (role == null || !role.EstPublie || role.DateLimiteCandidature < DateTime.UtcNow)
            { _logger.LogWarning("Role ID {RoleId} invalide pour candidature Talent ID {TalentId}.", createDto.RoleId, talentId); return null; }

            if (await _candidatureRepo.HasActiveApplicationAsync(talentId, createDto.RoleId))
            { _logger.LogWarning("Talent ID {TalentId} a déjà postulé pour Role ID {RoleId}.", talentId, createDto.RoleId); return null; }

            var candidature = _mapper.Map<Candidature>(createDto);
            candidature.TalentId = talentId; candidature.DateCandidature = DateTime.UtcNow;
            candidature.Statut = "RECUE"; candidature.CreeLe = DateTime.UtcNow;

            try
            {
                await _candidatureRepo.AddAsync(candidature); await _candidatureRepo.SaveChangesAsync();
                _logger.LogInformation("Candidature ID {CandidatureId} créée pour Talent ID {TalentId} à Role ID {RoleId}", candidature.CandidatureId, talentId, createDto.RoleId);

                var responseDto = _mapper.Map<MyCandidatureResponseDto>(candidature);
                responseDto.RoleNom = role.Nom;
                if (role.Projet != null) { responseDto.ProjetTitre = role.Projet.Titre; responseDto.ProjetId = role.Projet.ProjetId; }
                return responseDto;
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur création candidature Talent ID {TalentId} pour Role ID {RoleId}", talentId, createDto.RoleId); return null; }
        }

        public async Task<IEnumerable<MyCandidatureResponseDto>> GetMyApplicationsAsync(long talentId)
        {
            _logger.LogInformation("Récupération candidatures Talent ID {TalentId}", talentId);
            var applications = await _candidatureRepo.GetActiveApplicationsForTalentAsync(talentId);
            return _mapper.Map<List<MyCandidatureResponseDto>>(applications);
        }

        public async Task<bool> WithdrawApplicationAsync(long candidatureId, long talentId)
        {
            _logger.LogWarning("Retrait candidature ID {CandidatureId} par Talent ID {TalentId}", candidatureId, talentId);
            var candidature = await _candidatureRepo.GetByIdWithDetailsAsync(candidatureId);
            if (candidature == null || candidature.TalentId != talentId || candidature.Talent == null || candidature.Talent.IsDeleted)
            { _logger.LogWarning("Retrait échoué: Candidature {CandidatureId} invalide.", candidatureId); return false; }
            try
            {
                _candidatureRepo.Delete(candidature); await _candidatureRepo.SaveChangesAsync();
                _logger.LogInformation("Candidature ID {CandidatureId} retirée par Talent ID {TalentId}", candidatureId, talentId); return true;
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur retrait candidature ID {CandidatureId}", candidatureId); return false; }
        }

        public async Task<CandidatureDetailResponseDto?> GetApplicationDetailsForAdminAsync(long candidatureId)
        {
            _logger.LogInformation("Récupération détails candidature ID {CandidatureId} par Admin", candidatureId);
            var candidature = await _candidatureRepo.GetByIdForAdminDetailsAsync(candidatureId); // Cette méthode doit inclure Talent, Role, et Projet du Role
            if (candidature == null) return null;
            return _mapper.Map<CandidatureDetailResponseDto>(candidature);
        }

        public async Task<IEnumerable<CandidatureSummaryResponseDto>> GetApplicationsForRoleAsync(long roleId)
        {
            _logger.LogInformation("Récupération candidatures pour Role ID {RoleId} par Admin", roleId);
            var applications = await _candidatureRepo.GetActiveApplicationsForRoleAsync(roleId);
            return _mapper.Map<List<CandidatureSummaryResponseDto>>(applications);
        }

        public async Task<CandidatureSummaryResponseDto?> UpdateApplicationStatusAsync(long candidatureId, CandidatureUpdateStatusRequestDto statusDto)
        {
            _logger.LogInformation("MàJ statut Candidature ID {CandidatureId} vers {NouveauStatut}", candidatureId, statusDto.NouveauStatut);
            var candidature = await _candidatureRepo.GetByIdWithTalentAsync(candidatureId); 

            if (candidature == null || candidature.Talent == null || candidature.Talent.IsDeleted)
            { _logger.LogWarning("Candidature ID {CandidatureId} invalide pour MàJ statut.", candidatureId); return null; }

            string nouveauStatutUpper = statusDto.NouveauStatut.ToUpperInvariant();
            string messageNotificationPourTalent = statusDto.MessagePourTalent ?? string.Empty;
            string lienFront = $"/mes-candidatures/{candidature.CandidatureId}"; // Exemple

            if (nouveauStatutUpper == "PRESELECTIONNE")
            {
                int countPreselectionnes = await _candidatureRepo.CountActiveByStatusForRoleAsync(candidature.RoleId, "PRESELECTIONNE");
                if (countPreselectionnes >= 5 && candidature.Statut != "PRESELECTIONNE")
                { _logger.LogWarning("Limite présélection atteinte Role ID {RoleId}", candidature.RoleId); return _mapper.Map<CandidatureSummaryResponseDto>(candidature); }
                messageNotificationPourTalent = string.IsNullOrWhiteSpace(statusDto.MessagePourTalent) ? $"Félicitations ! Vous avez été présélectionné(e) pour le rôle '{candidature.Role?.Nom ?? "N/A"}'. Plus d'infos à venir." : statusDto.MessagePourTalent;
            }
            else if (nouveauStatutUpper == "ASSIGNE")
            {
                bool dejaAssigne = await _candidatureRepo.IsRoleAlreadyAssignedToOtherAsync(candidature.RoleId, candidature.CandidatureId);
                if (dejaAssigne) { _logger.LogWarning("Rôle ID {RoleId} déjà assigné.", candidature.RoleId); return _mapper.Map<CandidatureSummaryResponseDto>(candidature); }
                candidature.DateAssignation = DateTime.UtcNow;
                messageNotificationPourTalent = string.IsNullOrWhiteSpace(statusDto.MessagePourTalent) ? $"Excellente nouvelle ! Vous avez été retenu(e) pour le rôle '{candidature.Role?.Nom ?? "N/A"}' dans le projet '{candidature.Role?.Projet?.Titre ?? "N/A"}' !" : statusDto.MessagePourTalent;

                if (candidature.Talent != null && !string.IsNullOrEmpty(candidature.Talent.Email) && candidature.Role != null && candidature.Role.Projet != null)
                {
                    await SendRoleAssignedEmailAsync(candidature.Talent.Email, candidature.Talent.Prenom ?? "Talent", candidature.Role.Nom, candidature.Role.Projet.Titre);
                }
            }

            candidature.Statut = nouveauStatutUpper;
            _candidatureRepo.Update(candidature);

            if (!string.IsNullOrWhiteSpace(messageNotificationPourTalent) && (nouveauStatutUpper == "PRESELECTIONNE" || nouveauStatutUpper == "ASSIGNE" || nouveauStatutUpper == "NON_RETENU"))
            {
                await _notificationService.CreateNotificationForTalentAsync(candidature.TalentId, messageNotificationPourTalent, "CANDIDATURE", candidature.CandidatureId, lienFront);
            }
            await _candidatureRepo.SaveChangesAsync();
            _logger.LogInformation("Statut Candidature ID {CandidatureId} mis à jour vers {NouveauStatut}.", candidatureId, nouveauStatutUpper);
            return _mapper.Map<CandidatureSummaryResponseDto>(candidature);
        }

        private async Task SendRoleAssignedEmailAsync(string talentEmail, string talentPrenom, string roleNom, string projetTitre)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            string? server = smtpSettings["Server"]; string? portStr = smtpSettings["Port"]; string? senderName = smtpSettings["SenderName"];
            string? senderEmail = smtpSettings["SenderEmail"]; string? senderPassword = smtpSettings["SenderPassword"]; string? enableSslStr = smtpSettings["EnableSsl"];

            if (string.IsNullOrWhiteSpace(server) || !int.TryParse(portStr, out int port) || string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword) || !bool.TryParse(enableSslStr, out bool enableSsl)) { _logger.LogCritical("Cfg SMTP incomplète pour email d'assignation."); return; }
            senderName ??= "CastFlow";
            string subject = $"Félicitations ! Vous avez été retenu(e) pour le rôle {roleNom} !";
            string body = $"Bonjour {talentPrenom},\n\nNous avons le plaisir de vous informer que votre candidature pour le rôle '{roleNom}' dans le projet '{projetTitre}' a été retenue !\n\nL'équipe de production vous contactera prochainement.\n\nCordialement,\nL'équipe CastFlow";

            try
            {
                using var client = new SmtpClient(server) { Port = port, Credentials = new NetworkCredential(senderEmail, senderPassword), EnableSsl = enableSsl };
                var mailMessage = new MailMessage { From = new MailAddress(senderEmail, senderName), Subject = subject, Body = body, IsBodyHtml = false, };
                mailMessage.To.Add(talentEmail);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email d'assignation envoyé à {TalentEmail} pour rôle {RoleNom}", talentEmail, roleNom);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur envoi email d'assignation à {TalentEmail}.", talentEmail); }
        }
    }
}