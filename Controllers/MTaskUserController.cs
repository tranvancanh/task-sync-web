using Microsoft.AspNetCore.Mvc;
using task_sync_web.Models;

namespace task_sync_web.Controllers
{
    public class MTaskUserController : BaseController
    {
        [HttpGet]
        public IActionResult Index(MTaskUserViewModel viewModel, string command = null)
        {

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Index(MTaskUserViewModel viewModel)
        {

            return View(viewModel);
        }
    }
}
