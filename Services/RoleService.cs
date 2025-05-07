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
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepo;
        private readonly IProjetRepository _projetRepo; 
        private readonly IMapper _mapper;
        private readonly ILogger<RoleService> _logger;

        public RoleService(IRoleRepository roleRepo, IProjetRepository projetRepo, IMapper mapper, ILogger<RoleService> logger)
        {
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _projetRepo = projetRepo ?? throw new ArgumentNullException(nameof(projetRepo));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RoleDetailResponseDto?> CreateRoleAsync(long projetId, RoleCreateRequestDto createDto)
        {
            // Vérifier si le projet parent existe et est actif
            if (!await _projetRepo.ActiveExistsAsync(projetId))
            {
                _logger.LogWarning("Tentative de création de rôle pour projet inexistant ou inactif ID {ProjetId}", projetId);
                return null;
            }

            var role = _mapper.Map<Role>(createDto);
            role.ProjetId = projetId; // Lier au projet

            await _roleRepo.AddAsync(role);
            await _roleRepo.SaveChangesAsync();

            _logger.LogInformation("Rôle '{RoleNom}' créé pour Projet ID {ProjetId} avec ID {RoleId}", role.Nom, projetId, role.RoleId);
            var roleDetail = await GetRoleByIdAsync(role.RoleId);
            return roleDetail; // Retourne le DTO détaillé
        }

        public async Task<RoleDetailResponseDto?> GetRoleByIdAsync(long roleId)
        {
            _logger.LogInformation("Récupération Rôle ID {RoleId}", roleId);
            var role = await _roleRepo.GetActiveByIdWithProjectAsync(roleId); // Charge le projet lié
            if (role == null) return null;
            return _mapper.Map<RoleDetailResponseDto>(role);
        }

        public async Task<IEnumerable<RoleSummaryResponseDto>> GetRolesForProjetAsync(long projetId)
        {
            _logger.LogInformation("Récupération des rôles pour Projet ID {ProjetId}", projetId);
            if (!await _projetRepo.ActiveExistsAsync(projetId)) 
            {
                _logger.LogWarning("Projet ID {ProjetId} non trouvé ou inactif lors de la récupération des rôles.", projetId);
                return new List<RoleSummaryResponseDto>(); 
            }
            // SI LE PROJET EXISTE, ON ARRIVE ICI
            var roles = await _roleRepo.GetActiveRolesForProjetAsync(projetId);
            return _mapper.Map<List<RoleSummaryResponseDto>>(roles); 

            
        }
        public async Task<RoleDetailResponseDto?> UpdateRoleAsync(long roleId, RoleUpdateRequestDto updateDto)
        {
            _logger.LogInformation("Mise à jour Rôle ID {RoleId}", roleId);
            var role = await _roleRepo.GetActiveByIdAsync(roleId); 

            
            _mapper.Map(updateDto, role); 

            role.ModifieLe = DateTime.UtcNow; 

            

            await _roleRepo.SaveChangesAsync(); 
            _logger.LogInformation("Rôle ID {RoleId} mis à jour en BDD.", roleId);

          
            return _mapper.Map<RoleDetailResponseDto>(role); 
                                                             
        }



        public async Task<bool> DeleteRoleAsync(long roleId) 
        {
            _logger.LogWarning("Tentative de désactivation Rôle ID {RoleId}", roleId);
            var role = await _roleRepo.GetByIdAsync(roleId); // Récupère même si supprimé
            if (role == null) return false;
            if (role.IsDeleted) return true; 

            _roleRepo.MarkAsDeleted(role); 
            await _roleRepo.SaveChangesAsync();
            _logger.LogInformation("Rôle ID {RoleId} marqué comme supprimé.", roleId);
            return true;
        }
    }
}