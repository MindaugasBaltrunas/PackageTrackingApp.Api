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
            _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
        }

        [HttpPost]
        public async Task<IActionResult> AddPackage([FromBody] PackageRequest package)
        {
            var result = await _packageService.AddPackageAsync(package);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPackages()
        {
            var result = await _packageService.GetAllPackagesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPackage(string id)
        {
            var result = await _packageService.GetPackageByIdAsync(id);
            return Ok(result);
        }

        [HttpPut("{packageId}/status/{status}")]
        public async Task<IActionResult> UpdatePackageStatus(string packageId, int status)
        {
            var result = await _packageService.ExchangeStatusAsync(packageId, status);
            return Ok(result);
        }

        [HttpGet("history/{id}")]
        public async Task<IActionResult> GetPackageHistory(string id)
        {
            var resul = await _packageService.GetStatusHistory(id);
            return Ok(resul);
        }

        [HttpGet("search/{trackingId}/status/{status}")]
        public async Task<IActionResult> SearchPackage(string trackingId, int status)
        {
            var result = await _packageService.FilterAllPackagesAsync(trackingId, status);
            return Ok(result);
        } 
    }
}