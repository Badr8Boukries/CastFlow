using AutoMapper;
using CastFlow.Api.Models;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using System.Linq; 

namespace CastFlow.Api.Mappers
{
    public class RoleMappingProfile : Profile
    {
        public RoleMappingProfile()
        {
            CreateMap<RoleCreateRequestDto, Role>()
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForMember(dest => dest.ProjetId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
                .ForMember(dest => dest.ModifieLe, opt => opt.Ignore())
                .ForMember(dest => dest.Projet, opt => opt.Ignore())
                .ForMember(dest => dest.Candidatures, opt => opt.Ignore());

            CreateMap<RoleUpdateRequestDto, Role>()
                 .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                 .ForMember(dest => dest.ProjetId, opt => opt.Ignore()) 
                 .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                 .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
                 .ForMember(dest => dest.ModifieLe, opt => opt.Ignore()) 
                 .ForMember(dest => dest.Projet, opt => opt.Ignore())
                 .ForMember(dest => dest.Candidatures, opt => opt.Ignore())
                 .ForMember(dest => dest.Nom, opt => opt.Condition(src => src.Nom != null))
                 .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null))
                 .ForMember(dest => dest.AgeMin, opt => opt.Condition(src => src.AgeMin.HasValue))
                 .ForMember(dest => dest.AgeMax, opt => opt.Condition(src => src.AgeMax.HasValue))
                 .ForMember(dest => dest.ExigenceSex, opt => opt.Condition(src => src.ExigenceSex != null))
                 .ForMember(dest => dest.DateLimiteCandidature, opt => opt.Condition(src => src.DateLimiteCandidature.HasValue))
                 .ForMember(dest => dest.EstPublie, opt => opt.Condition(src => src.EstPublie.HasValue));


            CreateMap<Role, RoleSummaryResponseDto>()
                 .ForMember(dest => dest.ProjetTitre, opt => opt.MapFrom(src => src.Projet != null ? src.Projet.Titre : "N/A"));


            CreateMap<Role, RoleDetailResponseDto>()
        .ForMember(dest => dest.ProjetTitre, opt => opt.MapFrom(src => src.Projet != null ? src.Projet.Titre : "N/A"))
        .ForMember(dest => dest.ProjetLogline, opt => opt.MapFrom(src => src.Projet != null ? src.Projet.Logline ?? string.Empty : string.Empty))
        .ForMember(dest => dest.Candidatures, opt => opt.Ignore()) 
        .ForMember(dest => dest.NombreTotalCandidatures, opt => opt.Ignore());
        }
    }
}