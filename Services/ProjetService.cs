using AutoMapper;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using CastFlow.Api.Models;
using CastFlow.Api.Repository;
using CastFlow.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; 

namespace CastFlow.Api.Services
{
    public class ProjetService : IProjetService
    {
        private readonly IProjetRepository _projetRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<ProjetService> _logger;
        private readonly IRoleRepository _roleRepo;
        public ProjetService(IProjetRepository projetRepo, IMapper mapper, ILogger<ProjetService> logger, IRoleRepository roleRepo)
        {
            _projetRepo = projetRepo ?? throw new ArgumentNullException(nameof(projetRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _roleRepo = roleRepo;
          _roleRepo = roleRepo;
        }

        public async Task<ProjetDetailResponseDto?> CreateProjetAsync(ProjetCreateRequestDto createDto)
        {
            var projet = _mapper.Map<Projet>(createDto);

            await _projetRepo.AddAsync(projet);
            await _projetRepo.SaveChangesAsync();
            _logger.LogInformation("Projet créé ID {ProjetId}", projet.ProjetId);
            return _mapper.Map<ProjetDetailResponseDto>(projet);
        }

        public async Task<ProjetDetailResponseDto?> GetProjetByIdAsync(long projetId)
        {
            _logger.LogInformation("Récupération Projet ID {ProjetId}", projetId);
            var projet = await _projetRepo.GetActiveByIdWithRolesAsync(projetId);
            if (projet == null) return null;
            return _mapper.Map<ProjetDetailResponseDto>(projet);
        }

        public async Task<IEnumerable<ProjetSummaryResponseDto>> GetAllProjetsAsync()
        {
            _logger.LogInformation("Récupération de tous les projets actifs");
            var projets = await _projetRepo.GetAllActiveAsync();
            var projetDtos = _mapper.Map<List<ProjetSummaryResponseDto>>(projets);

            foreach (var dto in projetDtos)
            {
                dto.NombreRoles = await _roleRepo.CountActiveRolesForProjectAsync(dto.ProjetId);
            }
            return projetDtos;
        }



        public async Task<ProjetDetailResponseDto?> UpdateProjetAsync(long projetId, ProjetUpdateRequestDto updateDto)
        {
            _logger.LogInformation("Mise à jour Projet ID {ProjetId}", projetId);
            var projet = await _projetRepo.GetActiveByIdWithRolesAsync(projetId); 
            if (projet == null) return null;

            _mapper.Map(updateDto, projet); // Applique les mises à jour
                                            
            await _projetRepo.SaveChangesAsync();
            _logger.LogInformation("Projet ID {ProjetId} mis à jour", projetId);
            return await GetProjetByIdAsync(projetId); 
        }

        public async Task<bool> DeleteProjetAsync(long projetId) // Soft delete
        {
            _logger.LogWarning("Tentative de désactivation Projet ID {ProjetId}", projetId);
            var projet = await _projetRepo.GetByIdAsync(projetId); // Récupère même si supprimé
            if (projet == null) return false;
            if (projet.IsDeleted) return true; 

            _projetRepo.MarkAsDeleted(projet); 

            await _projetRepo.SaveChangesAsync();
            _logger.LogInformation("Projet ID {ProjetId} marqué comme supprimé/archivé.", projetId);
            return true;
        }

        public async Task<IEnumerable<ProjetSummaryResponseDto>> GetAllArchivedProjetsAsync()
        {
            _logger.LogInformation("Récupération de tous les projets archivés");
            var projets = await _projetRepo.GetAllArchivedAsync();
            var projetDtos = _mapper.Map<List<ProjetSummaryResponseDto>>(projets);

           
            foreach (var dto in projetDtos)
            {
                dto.NombreRoles = await _roleRepo.CountActiveRolesForProjectAsync(dto.ProjetId);
               
            }
            return projetDtos;
        }
    }
}