using Microsoft.AspNetCore.Mvc;
using PracticeWEB.Models;
using System.Diagnostics;
using Refit;
using System.Threading.Tasks;
using PracticeWEB.Api;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PracticeWEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IApiService _api;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _api = RestService.For<IApiService>(ApiConfig.BaseUrl);
        }

        public async Task<IActionResult> Index()
        {
            var listProduct = await _api.GetListProduct();

           

            return View(listProduct);
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