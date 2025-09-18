using AutoMapper;
using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Service.Dtos;

namespace PackageTrackingApp.Service.MapperProfile
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<PackageRequest, Package>();

            CreateMap<Package, PackageResponse>()
             .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender))
             .ForMember(dest => dest.Recipient, opt => opt.MapFrom(src => src.Recipient));

            CreateMap<Sender, SenderDto>();

            CreateMap<Recipient, RecipientDto>();

            CreateMap<PackageStatusHistory, PackageStatusHistoryResponse>();
        }

    }
}
