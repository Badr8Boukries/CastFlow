using System.Threading.Tasks;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;

namespace CastFlow.Api.Services.Interfaces
{
    public interface IAdminManagementService
    {
        Task<bool> InviteAdminAsync(InviteAdminRequestDto inviteDto, long invitingAdminId);
        Task<AuthResponseDto> SetupAdminAccountAsync(SetupAdminAccountRequestDto setupDto);
    }
}