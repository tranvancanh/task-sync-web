using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using task_sync_web.Commons;
using task_sync_web.Models;

namespace task_sync_web.Controllers
{

    public class BaseController : Controller
    {
        public LoginUserModel LoginUser;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var isValid = false;
            try
            {
                var userClaims = context.HttpContext.User.Claims.ToList();
                var userModel = new LoginUserModel()
                {
                    CompanyName = userClaims.Where(x => x.Type == CustomClaimTypes.ClaimType_CampanyName).First().Value,
                    CompanyDatabaseName = userClaims.Where(x => x.Type == CustomClaimTypes.ClaimType_CompanyDatabaseName).First().Value,
                    AdministratorId = Convert.ToInt32(userClaims.Where(x => x.Type == CustomClaimTypes.ClaimType_AdministratorId).First().Value),
                    AdministratorLoginId = userClaims.Where(x => x.Type == CustomClaimTypes.ClaimType_AdministratorLoginId).First().Value,
                    AdministratorName = userClaims.Where(x => x.Type == CustomClaimTypes.ClaimType_AdministratorName).First().Value,
                    LoginDateTime = Convert.ToDateTime(userClaims.Where(x => x.Type == CustomClaimTypes.ClaimType_TimeStamp).First().Value)
                };

                LoginUser = userModel;
                isValid = true;
            }
            catch (Exception)
            {
                isValid = false;
            }

            if (isValid)
                await next();
            else
            {
                var viewResult = RedirectToAction("Logout", "Login", new { param = 0 });
                context.Result = viewResult;
                //context.Result = new BadRequestObjectResult("Invalid!");
            }

        }
    }
}
