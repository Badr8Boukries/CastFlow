using AutoMapper;
using CastFlow.Api.Models;
using CastFlow.Api.Dtos.Response;
using CastFlow.Api.Dtos.Request;
using System; 

namespace CastFlow.Api.Mappers
{
    public class TalentMappingProfile : Profile
    {
        public TalentMappingProfile()
        {
           
            CreateMap<UserTalent, TalentProfileResponseDto>()
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.DateNaissance)));
            
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

      
        private int CalculateAge(DateTime? birthDate)
        {
            if (!birthDate.HasValue) return 0;
            var today = DateTime.Today;
            var age = today.Year - birthDate.Value.Year;
            if (birthDate.Value.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}