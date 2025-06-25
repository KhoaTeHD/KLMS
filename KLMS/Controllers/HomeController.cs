using KLMS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace KLMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // add action cho Error 404
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error404()
        {
            Response.StatusCode = 404;
            return View();
        }

        // add action cho Error 403
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error403()
        {
            Response.StatusCode = 403;
            return View();
        }

        // Action tong quat cho cac status code khac
        [Route("Home/ErrorStatus/{statusCode:int}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ErrorStatus(int statusCode)
        {
            Response.StatusCode = statusCode;
            Console.WriteLine(statusCode);

            return statusCode switch
            {
                404 => View("Error404"),
                403 => View("Error403"),
                _ => View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier })
            };
        }
    }
}
