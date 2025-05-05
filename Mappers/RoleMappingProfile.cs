using AutoMapper;
using CastFlow.Api.Models;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;

namespace CastFlow.Api.Mappers
{
    public class RoleMappingProfile : Profile
    {
        public RoleMappingProfile()
        {
            CreateMap<RoleCreateRequestDto, Role>()
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForMember(dest => dest.ProjetId, opt => opt.Ignore()) // Défini dans le service
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
                 .ForMember(dest => dest.ExigenceSex, opt => opt.Condition(src => src.ExigenceSex != null))
                 .ForMember(dest => dest.DateLimiteCandidature, opt => opt.Condition(src => src.DateLimiteCandidature.HasValue))
                 .ForMember(dest => dest.EstPublie, opt => opt.Condition(src => src.EstPublie.HasValue));


            CreateMap<Role, RoleSummaryResponseDto>()
                .ForMember(dest => dest.ProjetTitre, opt => opt.MapFrom(src => src.Projet != null ? src.Projet.Titre : "N/A"));

            CreateMap<Role, RoleDetailResponseDto>()
                 // Ajout du mapping pour ProjetTitre et ProjetLogline (nécessite que Projet soit chargé)
                 .ForMember(dest => dest.ProjetTitre, opt => opt.MapFrom(src => src.Projet != null ? src.Projet.Titre : "N/A"))
                 .ForMember(dest => dest.ProjetLogline, opt => opt.MapFrom(src => src.Projet != null ? src.Projet.Logline ?? string.Empty : string.Empty))
                 .ForMember(dest => dest.Candidatures, opt => opt.Ignore()); // Gérer séparément si besoin d'afficher les candidatures ici
        }
    }
}