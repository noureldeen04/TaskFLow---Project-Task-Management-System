using Microsoft.AspNetCore.Mvc;

namespace TaskFlow_Project_Managment_System.Controllers
{
    public class ActivityLogsController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Activity Logs";
            return View();
        }
    }
}