using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastFlow.Api.Services.Interfaces
{
    public interface IRoleService
    {
        /// <summary>Crée un nouveau rôle pour un projet donné.</summary>
        /// <param name="projetId">ID du projet parent.</param>
        /// <param name="createDto">Données du rôle à créer.</param>
        /// <returns>DTO du rôle créé ou null si le projet n'existe pas.</returns>
        Task<RoleDetailResponseDto?> CreateRoleAsync(long projetId, RoleCreateRequestDto createDto);

        /// <summary>Récupère les détails d'un rôle actif par son ID.</summary>
        Task<RoleDetailResponseDto?> GetRoleByIdAsync(long roleId);

        /// <summary>Récupère tous les rôles actifs pour un projet donné.</summary>
        Task<IEnumerable<RoleSummaryResponseDto>> GetRolesForProjetAsync(long projetId);

        /// <summary>Met à jour un rôle existant.</summary>
        Task<RoleDetailResponseDto?> UpdateRoleAsync(long roleId, RoleUpdateRequestDto updateDto);

        /// <summary>Marque un rôle comme supprimé (Soft Delete).</summary>
        Task<bool> DeleteRoleAsync(long roleId);
    }
}