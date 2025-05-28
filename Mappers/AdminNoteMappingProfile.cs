using AutoMapper;
using CastFlow.Api.Models;
using CastFlow.Api.Dtos.Response;
namespace CastFlow.Api.Mappers
{
    public class AdminNoteMappingProfile : Profile
    {
        public AdminNoteMappingProfile()
        {
            CreateMap<AdminCandidatureNote, AdminNoteResponseDto>()
                .ForMember(dest => dest.AdminNomComplet, opt => opt.MapFrom(src => src.Admin != null ? $"{src.Admin.Prenom} {src.Admin.Nom}" : "Admin Inconnu"));
        }
    }
}