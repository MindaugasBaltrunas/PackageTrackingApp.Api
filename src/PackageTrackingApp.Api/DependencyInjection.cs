using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PackageTrackingApp.Data.Context;
using PackageTrackingApp.Data.Repositories;
using PackageTrackingApp.Domain.Interfaces;
using PackageTrackingApp.Service.Dtos;
using PackageTrackingApp.Service.Interfaces;
using PackageTrackingApp.Service.MapperProfile;
using PackageTrackingApp.Service.Services;
using PackageTrackingApp.Service.Validators;

namespace PackageTrackingApp.Api
{
    public static class DependencyInjection
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // AutoMapper Configuration (scan assembly for profiles)
            services.AddAutoMapper(cfg => { }, typeof(MapperProfile).Assembly);

            // Database Configuration (in-memory for dev/testing)
            services.AddDbContext<AppDbContext>(options =>
               options.UseInMemoryDatabase("InMemoryDb"), ServiceLifetime.Scoped);

            // Repository Registration
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

            // Service Registration
            services.AddScoped<IPackageService, PackageService>();
            services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));

            // Factory Registration
            services.AddScoped<IResultFactory, ResultFactory>();

            // FluentValidation - register all validators in assembly
            services.AddValidatorsFromAssemblyContaining<PackageValidator>();
            services.AddScoped<IValidStatusTransition, IsValidStatusTransition>();

            // CORS Configuration
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .WithOrigins("http://localhost:3000", "http://localhost:5013")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }
    }
}
