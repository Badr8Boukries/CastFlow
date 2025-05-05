using AutoMapper;
using CastFlow.Api.Models;
using CastFlow.Api.Dtos.Response;

namespace CastFlow.Api.Mappers
{
    public class RoleMappingProfile : Profile
    {
        public RoleMappingProfile()
        {
            CreateMap<Role, RoleSummaryResponseDto>(); 
        }
    }
}