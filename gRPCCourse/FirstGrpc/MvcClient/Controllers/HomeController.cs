using Basics;
using Microsoft.AspNetCore.Mvc;
using MvcClient.Models;
using System.Diagnostics;

namespace MvcClient.Controllers
{
    public class HomeController (FirstServiceDefinition.FirstServiceDefinitionClient client) : Controller
    {
        public IActionResult Index()
        {
            var firstCall = client.Unary(new Request { Content = "Hello from MVC Client!" });
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
