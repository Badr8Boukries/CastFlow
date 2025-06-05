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
        private readonly IFileStorageService _fileStorageService;

        public CandidatureService(
            ICandidatureRepository candidatureRepo,
            IRoleRepository roleRepo,
            IUserTalentRepository talentRepo,
            IMapper mapper,
            ILogger<CandidatureService> logger,
            INotificationService notificationService, 
            IConfiguration configuration, IFileStorageService fileStorageService)
            
        {
            _candidatureRepo = candidatureRepo ?? throw new ArgumentNullException(nameof(candidatureRepo));
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _talentRepo = talentRepo ?? throw new ArgumentNullException(nameof(talentRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService)); 
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
           _fileStorageService = fileStorageService;

        }

        public async Task<MyCandidatureResponseDto?> ApplyToRoleAsync(long talentId, CandidatureCreateRequestDto createDto)
        {

            var candidature = _mapper.Map<Candidature>(createDto);
            candidature.TalentId = talentId;
            candidature.DateCandidature = DateTime.UtcNow;
            candidature.Statut = "RECUE";
            candidature.CreeLe = DateTime.UtcNow;

            string? videoUrl = null;
            if (createDto.VideoAuditionFile != null && createDto.VideoAuditionFile.Length > 0)
            {
                if (createDto.VideoAuditionFile.Length > 60 * 1024 * 1024) // Limite de 60MB (exemple)
                {
                    _logger.LogWarning("Fichier vidéo trop volumineux pour candidature Talent ID {TalentId} à Role ID {RoleId}", talentId, createDto.RoleId);
                }
                else
                {
                    try
                    {
                        string videoFileNamePrefix = $"talent_{talentId}_role_{createDto.RoleId}_audition_{Guid.NewGuid()}";
                        videoUrl = await _fileStorageService.SaveFileAsync(createDto.VideoAuditionFile, "audition-videos", videoFileNamePrefix);
                        if (!string.IsNullOrWhiteSpace(videoUrl))
                        {
                            candidature.UrlVideoAudition = videoUrl;
                            _logger.LogInformation("Vidéo d'audition sauvegardée pour candidature Talent ID {TalentId} à Role ID {RoleId}: {VideoUrl}", talentId, createDto.RoleId, videoUrl);
                        }
                        else
                        {
                            _logger.LogWarning("Échec de la sauvegarde de la vidéo d'audition pour Talent ID {TalentId}", talentId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors de la sauvegarde de la vidéo d'audition pour Talent ID {TalentId}", talentId);
                    }
                }
            }


            try
            {
                await _candidatureRepo.AddAsync(candidature);
                await _candidatureRepo.SaveChangesAsync();
                _logger.LogInformation("Candidature ID {CandidatureId} créée.", candidature.CandidatureId);

                var role = await _roleRepo.GetActiveByIdWithProjectAsync(createDto.RoleId); // Recharger pour infos projet
                var responseDto = _mapper.Map<MyCandidatureResponseDto>(candidature);
                if (role?.Projet != null) { responseDto.ProjetTitre = role.Projet.Titre; responseDto.ProjetId = role.Projet.ProjetId; }
                if (role != null) { responseDto.RoleNom = role.Nom; }

                return responseDto;
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur création candidature Talent ID {TalentId} pour Role ID {RoleId}", talentId, createDto.RoleId); return null; }

            // ... (catch existant) ...
        }
        public async Task<IEnumerable<MyCandidatureResponseDto>> GetMyApplicationsAsync(long talentId)
        {
            _logger.LogInformation("Récupération des candidatures pour Talent ID {TalentId}", talentId);
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
            _logger.LogInformation("MàJ statut Candidature ID {CandidatureId} vers {NouveauStatut} par Admin.", candidatureId, statusDto.NouveauStatut);

            // ✅ CORRECTION: Utilise une méthode qui charge TOUTES les données nécessaires
            var candidature = await _candidatureRepo.GetByIdForAdminDetailsAsync(candidatureId); // Cette méthode charge Talent, Role, et Projet

            if (candidature == null)
            {
                _logger.LogWarning("Candidature ID {CandidatureId} introuvable.", candidatureId);
                return null;
            }

            if (candidature.Talent == null || candidature.Talent.IsDeleted)
            {
                _logger.LogWarning("Talent manquant ou supprimé pour candidature ID {CandidatureId}.", candidatureId);
                return null;
            }

            if (candidature.Role == null)
            {
                _logger.LogWarning("Rôle manquant pour candidature ID {CandidatureId}.", candidatureId);
                return null;
            }

            string nouveauStatutUpper = statusDto.NouveauStatut.ToUpperInvariant();

            if (nouveauStatutUpper == "PRESELECTIONNE")
            {
                int countPreselectionnes = await _candidatureRepo.CountActiveByStatusForRoleAsync(candidature.RoleId, "PRESELECTIONNE");
                if (countPreselectionnes >= 5 && candidature.Statut != "PRESELECTIONNE")
                {
                    _logger.LogWarning("Limite de 5 présélectionnés atteinte pour Role ID {RoleId}.", candidature.RoleId);
                    throw new InvalidOperationException("La limite de 5 candidats présélectionnés est atteinte.");
                }
                if (countPreselectionnes == 4 && candidature.Statut != "PRESELECTIONNE")
                {
                    var rolePourFermeture = await _roleRepo.GetActiveByIdAsync(candidature.RoleId);
                    if (rolePourFermeture != null && rolePourFermeture.EstPublie)
                    {
                        rolePourFermeture.EstPublie = false;
                        rolePourFermeture.ModifieLe = DateTime.UtcNow;
                        _roleRepo.Update(rolePourFermeture);
                        _logger.LogInformation("Casting pour Rôle ID {RoleId} fermé automatiquement (5 présélections atteintes).", candidature.RoleId);
                    }
                }
            }
            else if (nouveauStatutUpper == "ASSIGNE")
            {
                bool dejaAssigne = await _candidatureRepo.IsRoleAlreadyAssignedToOtherAsync(candidature.RoleId, candidature.CandidatureId);
                if (dejaAssigne)
                {
                    throw new InvalidOperationException("Un talent est déjà assigné à ce rôle.");
                }

                candidature.DateAssignation = DateTime.UtcNow;

                // ✅ Mise à jour du rôle avec assignation
                var roleAAssigner = candidature.Role; // On utilise le rôle déjà chargé
                roleAAssigner.TalentAssigneId = candidature.TalentId;
                roleAAssigner.EstPublie = false;
                roleAAssigner.Statut = "POURVU";
                roleAAssigner.ModifieLe = DateTime.UtcNow;
                _roleRepo.Update(roleAAssigner);

                // ✅ ENVOI DE L'EMAIL D'ASSIGNATION
                try
                {
                    string projetTitre = candidature.Role?.Projet?.Titre ?? "Non spécifié";
                    string roleNom = candidature.Role?.Nom ?? "Non spécifié";

                    _logger.LogInformation("Préparation envoi email assignation - Talent: {TalentEmail}, Prenom: {TalentPrenom}, Role: {RoleNom}, Projet: {ProjetTitre}",
                        candidature.Talent.Email, candidature.Talent.Prenom, roleNom, projetTitre);

                    await SendRoleAssignedEmailAsync(
                        candidature.Talent.Email,
                        candidature.Talent.Prenom,
                        roleNom,
                        projetTitre
                    );

                    _logger.LogInformation("Email d'assignation envoyé avec succès pour candidature ID {CandidatureId} - Talent: {TalentEmail}, Rôle: {RoleNom}",
                        candidature.CandidatureId, candidature.Talent.Email, roleNom);
                }
                catch (Exception emailEx)
                {
                    // On log l'erreur mais on ne fait pas échouer toute l'opération d'assignation
                    _logger.LogError(emailEx, "Erreur lors de l'envoi de l'email d'assignation pour candidature ID {CandidatureId} - Talent: {TalentEmail}",
                        candidature.CandidatureId, candidature.Talent.Email);
                }
            }

            // Mise à jour du statut de la candidature
            candidature.Statut = nouveauStatutUpper;
            _candidatureRepo.Update(candidature);

            try
            {
                await _candidatureRepo.SaveChangesAsync();
                _logger.LogInformation("Statut candidature ID {CandidatureId} mis à jour vers {NouveauStatut}", candidatureId, nouveauStatutUpper);
                return _mapper.Map<CandidatureSummaryResponseDto>(candidature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde de la mise à jour de statut pour candidature ID {CandidatureId}", candidatureId);
                throw;
            }
        }

        // ✅ Méthode d'envoi d'email améliorée avec plus de logs
        private async Task SendRoleAssignedEmailAsync(string talentEmail, string talentPrenom, string roleNom, string projetTitre)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                string? server = smtpSettings["Server"];
                string? portStr = smtpSettings["Port"];
                string? senderName = smtpSettings["SenderName"];
                string? senderEmail = smtpSettings["SenderEmail"];
                string? senderPassword = smtpSettings["SenderPassword"];
                string? enableSslStr = smtpSettings["EnableSsl"];

                _logger.LogInformation("Configuration SMTP - Server: {Server}, Port: {Port}, SenderEmail: {SenderEmail}, EnableSsl: {EnableSsl}",
                    server, portStr, senderEmail, enableSslStr);

                if (string.IsNullOrWhiteSpace(server) ||
                    !int.TryParse(portStr, out int port) ||
                    string.IsNullOrWhiteSpace(senderEmail) ||
                    string.IsNullOrWhiteSpace(senderPassword) ||
                    !bool.TryParse(enableSslStr, out bool enableSsl))
                {
                    _logger.LogCritical("Configuration SMTP incomplète pour email d'assignation. Server: {Server}, Port: {Port}, SenderEmail: {SenderEmail}",
                        server, portStr, senderEmail);
                    return;
                }

                senderName ??= "CastFlow";
                string subject = $"Félicitations ! Vous avez été retenu(e) pour le rôle {roleNom} !";
                string body = $@"Bonjour {talentPrenom},

Nous avons le plaisir de vous informer que votre candidature pour le rôle '{roleNom}' dans le projet '{projetTitre}' a été retenue !

🎉 Félicitations pour votre sélection !

L'équipe de production vous contactera prochainement avec tous les détails concernant :
- Les dates de tournage
- Le lieu de rendez-vous
- Les informations pratiques

Nous sommes impatients de travailler avec vous sur ce projet.

Cordialement,
L'équipe CastFlow";

                using var client = new SmtpClient(server)
                {
                    Port = port,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(talentEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email d'assignation envoyé avec succès à {TalentEmail} pour rôle {RoleNom} dans projet {ProjetTitre}",
                    talentEmail, roleNom, projetTitre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur critique lors de l'envoi de l'email d'assignation à {TalentEmail} pour rôle {RoleNom}",
                    talentEmail, roleNom);
                throw; // On peut choisir de throw ou pas selon le comportement souhaité
            }
        }

        public async Task<CandidatureDetailResponseDto?> AddOrUpdateAdminNoteAsync(long candidatureId, long adminId, decimal noteValue)
{
    var candidature = await _candidatureRepo.GetByIdWithNotesAndTalentAsync(candidatureId); // Doit inclure AdminNotes
    if (candidature == null) { _logger.LogWarning("Candidature non trouvée ID {CandidatureId} pour noter.", candidatureId); return null; }

    var note = new AdminCandidatureNote
    {
        CandidatureId = candidatureId,
        AdminId = adminId,
        NoteValue = noteValue,
        DateNote = DateTime.UtcNow
    };
    await _candidatureRepo.AddOrUpdateAdminNoteAsync(note);
    await _candidatureRepo.SaveChangesAsync();

    var allNotesForCandidature = await _candidatureRepo.GetNotesForCandidatureAsync(candidatureId);
    if (allNotesForCandidature.Any())
    {
        candidature.NoteMoyenne = allNotesForCandidature.Average(n => n.NoteValue);
        candidature.NoteMoyenne = Math.Round(candidature.NoteMoyenne.Value, 1); 
    }
    else { candidature.NoteMoyenne = null; }

    _candidatureRepo.Update(candidature); 
    await _candidatureRepo.SaveChangesAsync();

    _logger.LogInformation("Note admin MàJ pour Candidature ID {CandidatureId}. Nouvelle moyenne: {Moyenne}", candidatureId, candidature.NoteMoyenne);
    return _mapper.Map<CandidatureDetailResponseDto>(candidature);
}

    }
}