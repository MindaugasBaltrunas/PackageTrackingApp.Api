using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Service.Interfaces;

namespace PackageTrackingApp.Api.Controllers
{
    public class RecipientController : BaseController<Recipient>
    {
        public RecipientController(IBaseService<Recipient> baseService) : base(baseService)
        {

        }
    }
}
