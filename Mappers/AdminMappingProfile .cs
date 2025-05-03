using AutoMapper;
using CastFlow.Api.Models;
using CastFlow.Api.Dtos.Response;
using CastFlow.Api.Dtos.Request;

namespace CastFlow.Api.Mappers
{
    public class AdminMappingProfile : Profile
    {
        public AdminMappingProfile()
        {
            CreateMap<UserAdmin, AdminProfileResponseDto>(); 

            
            CreateMap<SetupAdminAccountRequestDto, UserAdmin>()
               .ForMember(dest => dest.AdminId, opt => opt.Ignore())
               .ForMember(dest => dest.Email, opt => opt.Ignore()) 
               .ForMember(dest => dest.MotDePasseHash, opt => opt.Ignore())
               .ForMember(dest => dest.CreeLe, opt => opt.Ignore()); 
        }
    }
}