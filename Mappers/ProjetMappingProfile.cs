using AutoMapper;
using CastFlow.Api.Models;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using System.Linq; 

namespace CastFlow.Api.Mappers
{
    public class ProjetMappingProfile : Profile
    {
        public ProjetMappingProfile()
        {

            CreateMap<ProjetCreateRequestDto, Projet>()
                .ForMember(dest => dest.ProjetId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
                .ForMember(dest => dest.ModifieLe, opt => opt.Ignore())
                .ForMember(dest => dest.Roles, opt => opt.Ignore());

            CreateMap<ProjetUpdateRequestDto, Projet>()
                .ForMember(dest => dest.ProjetId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
                .ForMember(dest => dest.ModifieLe, opt => opt.Ignore()) 
                .ForMember(dest => dest.Roles, opt => opt.Ignore())
                .ForMember(dest => dest.Titre, opt => opt.Condition(src => src.Titre != null))
                .ForMember(dest => dest.TypeProjet, opt => opt.Condition(src => src.TypeProjet != null))
                .ForMember(dest => dest.Statut, opt => opt.Condition(src => src.Statut != null))
                .ForMember(dest => dest.Realisateur, opt => opt.Condition(src => src.Realisateur != null));



            CreateMap<Projet, ProjetSummaryResponseDto>()
                .ForMember(dest => dest.NombreRoles, opt => opt.MapFrom(src => src.Roles == null ? 0 : src.Roles.Count(r => !r.IsDeleted))) 
                .ForMember(dest => dest.NombreCandidatures, opt => opt.Ignore());

          
            CreateMap<Projet, ProjetDetailResponseDto>()
                
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Where(r => !r.IsDeleted)))
                   .ForMember(dest => dest.NombreRoles, opt => opt.MapFrom(src => src.Roles == null ? 0 : src.Roles.Count(r => !r.IsDeleted)));

        }
    }
}