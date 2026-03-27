using AutoMapper;
using Domain.Models;
using Services.Abstractions.DTOs.Notification;

namespace Services.Mapping;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<Notification, NotificationDto>();
    }
}
