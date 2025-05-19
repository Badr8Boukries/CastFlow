using AutoMapper;
using CastFlow.Api.Models;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using System;

namespace CastFlow.Api.Mappers
{
    public class CandidatureMappingProfile : Profile
    {
        public CandidatureMappingProfile()
        {
            CreateMap<CandidatureCreateRequestDto, Candidature>()
                .ForMember(dest => dest.CandidatureId, opt => opt.Ignore())
                .ForMember(dest => dest.TalentId, opt => opt.Ignore())
                .ForMember(dest => dest.DateCandidature, opt => opt.Ignore())
                .ForMember(dest => dest.Statut, opt => opt.Ignore())
                .ForMember(dest => dest.DateAssignation, opt => opt.Ignore())
                .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
                .ForMember(dest => dest.Talent, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.UrlVideoAudition, opt => opt.Ignore());

            CreateMap<Candidature, MyCandidatureResponseDto>()
                .ForMember(dest => dest.RoleNom, opt => opt.MapFrom(src => src.Role != null ? src.Role.Nom : "N/A"))
                .ForMember(dest => dest.ProjetId, opt => opt.MapFrom(src => src.Role != null ? src.Role.ProjetId : 0))
                .ForMember(dest => dest.ProjetTitre, opt => opt.MapFrom(src => src.Role != null && src.Role.Projet != null ? src.Role.Projet.Titre : "N/A"))
                .ForMember(dest => dest.RoleEstSupprime, opt => opt.MapFrom(src => src.Role != null ? src.Role.IsDeleted : true));

            CreateMap<Candidature, CandidatureSummaryResponseDto>()
                .ForMember(dest => dest.TalentNomComplet, opt => opt.MapFrom(src => (src.Talent != null) ? $"{src.Talent.Prenom ?? ""} {src.Talent.Nom ?? ""}".Trim() : "Talent Inconnu"))
                .ForMember(dest => dest.TalentUrlPhoto, opt => opt.MapFrom(src => (src.Talent != null) ? src.Talent.UrlPhoto : null))
                .ForMember(dest => dest.TalentAge, opt => opt.MapFrom(src => (src.Talent != null) ? MappingProfileUtils.CalculateAge(src.Talent.DateNaissance) : 0));

            CreateMap<Candidature, CandidatureDetailResponseDto>()
                .ForMember(dest => dest.Talent, opt => opt.MapFrom(src => src.Talent))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));
        }
    }
}