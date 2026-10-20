using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebFramework.Filters;

namespace SanjeshP.Payeh.WebFramework.Api
{
    [ApiController]
    [ApiResultFilter]
    [Route("api/v{version:apiVersion}/[controller]")]  //api/v1/[controller]
    [Authorize]
    public class BaseController : ControllerBase
    {
        //public UserRepository UserRepository { get; set; } => property injection
       
        public bool UserIsAutheticated => HttpContext.User.Identity.IsAuthenticated;

    }
}
