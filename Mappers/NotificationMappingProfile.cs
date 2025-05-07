using AutoMapper;
using CastFlow.Api.Models;
using CastFlow.Api.Dtos.Response;

namespace CastFlow.Api.Mappers
{
    public class NotificationMappingProfile : Profile
    {
        public NotificationMappingProfile()
        {
            CreateMap<Notification, NotificationResponseDto>();
        }
    }
}