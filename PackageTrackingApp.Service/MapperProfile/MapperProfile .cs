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

            CreateMap<Package, PackageResponse>();
        }

    }
}
