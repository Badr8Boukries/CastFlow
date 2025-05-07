using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CastFlow.Api.Services.Interfaces
{
    public interface IRoleService
    {
      
        Task<RoleDetailResponseDto?> CreateRoleAsync(long projetId, RoleCreateRequestDto createDto);

        Task<RoleDetailResponseDto?> GetRoleByIdAsync(long roleId);

        Task<IEnumerable<RoleSummaryResponseDto>> GetRolesForProjetAsync(long projetId);

        Task<RoleDetailResponseDto?> UpdateRoleAsync(long roleId, RoleUpdateRequestDto updateDto);


        Task<bool> DeleteRoleAsync(long roleId);


    }
}