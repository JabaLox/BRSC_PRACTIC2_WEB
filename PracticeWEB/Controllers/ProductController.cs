using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PracticeWEB.Api;
using PracticeWEB.Models;
using Refit;

namespace PracticeWEB.Controllers
{
    public class ProductController : Controller
    {
        private readonly IApiService _api;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILogger<ProductController> _logger;

        public ProductController(ILogger<ProductController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _api = RestService.For<IApiService>(ApiConfig.BaseUrl);
        }

        public async Task SourceDropMenu()
        {
            var listManu = await _api.GetListManufacture();
            var listCategory = await _api.GetListCategory();

            if (listManu != null && listCategory != null)
            {
                ViewBag.CategoryList = new SelectList(listCategory, "IdCategory", "NameCategory");
                ViewBag.ManufactureList = new SelectList(listManu, "IdManufacture", "NameManufacture");
            }
        }


        public IActionResult AddProduct()
        {
            SourceDropMenu();
            return View();
        }


        public async Task<IActionResult> CreateProduct(Product product, int Category, int Manufacture)
        {
            if (!ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Заполните все поля!";
                return RedirectToAction("AddProduct");
            }

            try
            {
                // Установка значений Category и Manufacture из параметров
                product.Category = Category;
                product.Manufacture = Manufacture;

                var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["JwtToken"];
                var response = await _api.ProductCreate("Bearer " + jwtToken, product);

                Console.WriteLine(response.ToString());

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["SuccessMessage"] = ex.Message;
                return RedirectToAction("AddProduct");
            }
        }

        public async Task<IActionResult> EditProduct(string ArticleProduct)
        {
            var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["JwtToken"];
            var product = await _api.GetInfoProduct("Bearer " + jwtToken, ArticleProduct);

            await SourceDropMenu();

            if (product != null)
            {
                return View(product); // Передаем объект Product в представление
            }
            else
            {
                return NotFound(); 
            }
        }

        public async Task<IActionResult> UpdateProduct(Product product, int Category, int Manufacture)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Заполните все поля!";
                return RedirectToAction("EditProduct");
            }

            try
            {
                product.Category = Category;
                product.Manufacture = Manufacture;

                var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["JwtToken"];
                var pr = await _api.UpdateProduct("Bearer " + jwtToken, product.ArticleProduct, product);

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                TempData["SuccessMessage"] = "Ошибка";
                return RedirectToAction("EditProduct");
            }

        }

        public async Task<IActionResult> DeleteProduct(string ArticleProduct)
        {
            try
            {
                var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["JwtToken"];
                var delete = await _api.DeleteProduct("Bearer " + jwtToken, ArticleProduct);

                return RedirectToAction("Index", "Home");
            }
            catch
            {
                TempData["SuccessMessage"] = "Ошибка";
            }
            return RedirectToAction("EditProduct", "Product");

        }

    }
}
