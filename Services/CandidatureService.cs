using AutoMapper;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using CastFlow.Api.Models;
using CastFlow.Api.Repository;
using CastFlow.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System; 

namespace CastFlow.Api.Services
{
    public class CandidatureService : ICandidatureService
    {
        private readonly ICandidatureRepository _candidatureRepo;
        private readonly IRoleRepository _roleRepo; 
        private readonly IUserTalentRepository _talentRepo; 
        private readonly IMapper _mapper;
        private readonly ILogger<CandidatureService> _logger;

        public CandidatureService(
            ICandidatureRepository candidatureRepo,
            IRoleRepository roleRepo,
            IUserTalentRepository talentRepo,
            IMapper mapper,
            ILogger<CandidatureService> logger)
        {
            _candidatureRepo = candidatureRepo ?? throw new ArgumentNullException(nameof(candidatureRepo));
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _talentRepo = talentRepo ?? throw new ArgumentNullException(nameof(talentRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MyCandidatureResponseDto?> ApplyToRoleAsync(long talentId, CandidatureCreateRequestDto createDto)
        {
            _logger.LogInformation("Tentative de candidature par Talent ID {TalentId} pour Role ID {RoleId}", talentId, createDto.RoleId);

            var talent = await _talentRepo.GetActiveByIdAsync(talentId);
            if (talent == null) { _logger.LogWarning("Talent ID {TalentId} non trouvé ou inactif.", talentId); return null; }

            var role = await _roleRepo.GetActiveByIdAsync(createDto.RoleId); 
            if (role == null || !role.EstPublie || role.DateLimiteCandidature < DateTime.UtcNow)
            {
                _logger.LogWarning("Role ID {RoleId} non trouvé, non publié ou expiré pour candidature Talent ID {TalentId}.", createDto.RoleId, talentId);
                return null; 
            }

            if (await _candidatureRepo.HasActiveApplicationAsync(talentId, createDto.RoleId))
            {
                _logger.LogWarning("Talent ID {TalentId} a déjà postulé pour Role ID {RoleId}.", talentId, createDto.RoleId);
                return null;
            }

            var candidature = _mapper.Map<Candidature>(createDto);
            candidature.TalentId = talentId;
            candidature.DateCandidature = DateTime.UtcNow;
            candidature.Statut = "RECUE"; 
            candidature.CreeLe = DateTime.UtcNow;

            try
            {
                await _candidatureRepo.AddAsync(candidature);
                await _candidatureRepo.SaveChangesAsync();
                _logger.LogInformation("Candidature ID {CandidatureId} créée pour Talent ID {TalentId} à Role ID {RoleId}", candidature.CandidatureId, talentId, createDto.RoleId);

                var responseDto = _mapper.Map<MyCandidatureResponseDto>(candidature);
                if (role.Projet != null)
                {
                    responseDto.ProjetTitre = role.Projet.Titre;
                    responseDto.ProjetId = role.Projet.ProjetId;
                }
                responseDto.RoleNom = role.Nom;

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la candidature pour Talent ID {TalentId} à Role ID {RoleId}", talentId, createDto.RoleId);
                return null;
            }
        }

        public async Task<IEnumerable<MyCandidatureResponseDto>> GetMyApplicationsAsync(long talentId)
        {
            _logger.LogInformation("Récupération des candidatures pour Talent ID {TalentId}", talentId);
            var applications = await _candidatureRepo.GetActiveApplicationsForTalentAsync(talentId);
            return _mapper.Map<List<MyCandidatureResponseDto>>(applications); 
        }

        public async Task<bool> WithdrawApplicationAsync(long candidatureId, long talentId)
        {
            _logger.LogWarning("Tentative de retrait de candidature ID {CandidatureId} par Talent ID {TalentId}", candidatureId, talentId);
            var candidature = await _candidatureRepo.GetByIdWithDetailsAsync(candidatureId); 

            if (candidature == null || candidature.TalentId != talentId || candidature.Talent == null || candidature.Talent.IsDeleted)
            {
                _logger.LogWarning("Retrait échoué: Candidature {CandidatureId} non trouvée, n'appartient pas à Talent ID {TalentId} ou talent inactif.", candidatureId, talentId);
                return false; 
            }

            

            try
            {
                _candidatureRepo.Delete(candidature); 
                await _candidatureRepo.SaveChangesAsync();
                _logger.LogInformation("Candidature ID {CandidatureId} retirée avec succès par Talent ID {TalentId}", candidatureId, talentId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du retrait de la candidature ID {CandidatureId} par Talent ID {TalentId}", candidatureId, talentId);
                return false;
            }
        }



        public async Task<CandidatureDetailResponseDto?> GetApplicationDetailsForAdminAsync(long candidatureId)
        {
            _logger.LogInformation("Récupération détails candidature ID {CandidatureId} par Admin", candidatureId);
            var candidature = await _candidatureRepo.GetByIdForAdminDetailsAsync(candidatureId);

            if (candidature == null) return null;
            return _mapper.Map<CandidatureDetailResponseDto>(candidature);
        }

        public async Task<IEnumerable<CandidatureSummaryResponseDto>> GetApplicationsForRoleAsync(long roleId)
        {
            _logger.LogInformation("Récupération candidatures pour Role ID {RoleId} par Admin", roleId);
            var applications = await _candidatureRepo.GetActiveApplicationsForRoleAsync(roleId);
            return _mapper.Map<List<CandidatureSummaryResponseDto>>(applications);
        }

        public async Task<CandidatureSummaryResponseDto?> UpdateApplicationStatusAsync(long candidatureId, CandidatureUpdateStatusRequestDto statusDto/*, long adminId*/)
        {
            _logger.LogInformation("Mise à jour statut pour Candidature ID {CandidatureId} vers {NouveauStatut}", candidatureId, statusDto.NouveauStatut);

            var candidature = await _candidatureRepo.GetByIdWithTalentAsync(candidatureId);

            if (candidature == null || candidature.Talent == null || candidature.Talent.IsDeleted)
            {
                _logger.LogWarning("Candidature ID {CandidatureId} non trouvée, ou talent associé inactif/supprimé, pour MàJ statut.", candidatureId);
                return null;
            }

            string nouveauStatutUpper = statusDto.NouveauStatut.ToUpperInvariant();

            candidature.Statut = nouveauStatutUpper;
            if (nouveauStatutUpper == "ASSIGNE") 
            {
                candidature.DateAssignation = DateTime.UtcNow;
            
            }
           


           

            _candidatureRepo.Update(candidature); 
            await _candidatureRepo.SaveChangesAsync(); 

            _logger.LogInformation("Statut Candidature ID {CandidatureId} mis à jour vers {NouveauStatut}", candidatureId, nouveauStatutUpper);

          
            return _mapper.Map<CandidatureSummaryResponseDto>(candidature);
        }

    }
}