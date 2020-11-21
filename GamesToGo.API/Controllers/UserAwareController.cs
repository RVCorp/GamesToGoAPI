using System.Security.Claims;
using GamesToGo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace GamesToGo.API.Controllers
{
    public class UserAwareController : ControllerBase
    {
        protected readonly GamesToGoContext Context;

        public UserAwareController(GamesToGoContext context)
        {
            Context = context;
        }

        protected UserPasswordless LoggedUser => LoginController.GetOnlineUserForClaims(((ClaimsIdentity) HttpContext.User.Identity).Claims, Context);
    }
}