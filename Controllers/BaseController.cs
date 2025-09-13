using Microsoft.AspNetCore.Mvc;
using PackageTrackingApp.Service.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class BaseController<T> : ControllerBase where T : class
{
    protected readonly IBaseService<T> _baseService;

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
