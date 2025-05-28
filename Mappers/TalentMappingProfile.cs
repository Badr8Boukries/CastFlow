using AutoMapper;
using CastFlow.Api.Models;
using CastFlow.Api.Dtos.Response;
using CastFlow.Api.Dtos.Request;
using System;

namespace CastFlow.Api.Mappers
{
    public static class MappingProfileUtils 
    {
        public static int CalculateAge(DateTime? birthDate)
        {
            if (!birthDate.HasValue) return 0;
            var today = DateTime.Today;
            var age = today.Year - birthDate.Value.Year;
            if (birthDate.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }

    public class TalentMappingProfile : Profile
    {
        public TalentMappingProfile()
        {
            CreateMap<UserTalent, TalentProfileResponseDto>()
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => MappingProfileUtils.CalculateAge(src.DateNaissance)))
                
                 .ForMember(dest => dest.DateNaissance, opt => opt.MapFrom(src => src.DateNaissance ?? default(DateTime)))
                 .ForMember(dest => dest.Prenom, opt => opt.MapFrom(src => src.Prenom ?? string.Empty))
                 .ForMember(dest => dest.Nom, opt => opt.MapFrom(src => src.Nom ?? string.Empty))
                 .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty))
                  .ForMember(dest => dest.NomComplet, opt => opt.MapFrom(src => $"{src.Prenom} {src.Nom}"))
                 .ForMember(dest => dest.Sex, opt => opt.MapFrom(src => src.Sex ?? string.Empty));


            CreateMap<RegisterTalentRequestDto, UserTalent>()
                .ForMember(dest => dest.TalentId, opt => opt.Ignore())
                .ForMember(dest => dest.MotDePasseHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
                .ForMember(dest => dest.ModifieLe, opt => opt.Ignore())
                .ForMember(dest => dest.UrlPhoto, opt => opt.Ignore())
                .ForMember(dest => dest.UrlCv, opt => opt.Ignore())
                .ForMember(dest => dest.Candidatures, opt => opt.Ignore());

            CreateMap<UserTalent, TalentSummaryForRoleDto>()
            .ForMember(dest => dest.TalentId, opt => opt.MapFrom(src => src.TalentId))
            .ForMember(dest => dest.Prenom, opt => opt.MapFrom(src => src.Prenom ?? string.Empty)) 
            .ForMember(dest => dest.Nom, opt => opt.MapFrom(src => src.Nom ?? string.Empty));

            CreateMap<TalentProfileUpdateRequestDto, UserTalent>()
               .ForMember(dest => dest.TalentId, opt => opt.Ignore())
               .ForMember(dest => dest.Email, opt => opt.Ignore())
               .ForMember(dest => dest.MotDePasseHash, opt => opt.Ignore())
               .ForMember(dest => dest.IsEmailVerified, opt => opt.Ignore())
               .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
               .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
               .ForMember(dest => dest.ModifieLe, opt => opt.Ignore())
               .ForMember(dest => dest.UrlPhoto, opt => opt.Ignore())
               .ForMember(dest => dest.UrlCv, opt => opt.Ignore())
               .ForMember(dest => dest.Candidatures, opt => opt.Ignore());
        }
    }
}