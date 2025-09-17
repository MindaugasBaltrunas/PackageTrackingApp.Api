using Microsoft.AspNetCore.Mvc;
using PackageTrackingApp.Service.Interfaces;

namespace PackageTrackingApp.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BaseController<T> : ControllerBase where T : class
    {
        private readonly IBaseService<T> _baseService;

        public BaseController(IBaseService<T> baseService)
        {
            _baseService = baseService ?? throw new ArgumentNullException(nameof(baseService));
        }

        [HttpPost]
        public virtual async Task<IActionResult> Add([FromBody] T entity)
        {
            var added = await _baseService.AddEntityAsync(entity);
            return Ok(added);
        }
    }
}