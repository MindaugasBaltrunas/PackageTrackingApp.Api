using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Service.Interfaces;

namespace PackageTrackingApp.Api.Controllers
{
    public class SenderController : BaseController<Sender>
    {
        public SenderController(IBaseService<Sender> baseService) : base(baseService)
        {

        }
    }
}
