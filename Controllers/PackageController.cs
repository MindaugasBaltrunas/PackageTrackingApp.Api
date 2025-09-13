using Microsoft.AspNetCore.Mvc;
using PackageTrackingApp.Service.Dtos;
using PackageTrackingApp.Service.Interfaces;

namespace PackageTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService)
        {
            _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService)); ;
        }

        [HttpPost]
        public async Task<IActionResult> AddPackage([FromBody] PackageRequest package)
        {
            var result = await _packageService.AddPackageAsync(package);
            return Ok(result);
        }
    }
}
