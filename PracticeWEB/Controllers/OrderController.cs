using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PracticeWEB.Api;
using PracticeWEB.Models;
using Refit;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;

namespace PracticeWEB.Controllers
{
    public class OrderController : Controller
    {

        private readonly IApiService _api;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ILogger<OrderController> _logger;

        public OrderController(ILogger<OrderController> logger, IHttpContextAccessor httpContextAccessor) 
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _api = RestService.For<IApiService>(ApiConfig.BaseUrl);
        }

        public async Task<IActionResult> Order()
        {
            var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["JwtToken"];
            var listOrderProduct = await _api.GetListOrderProduct("Bearer " + jwtToken);
            return View(listOrderProduct);
        }

        public async Task<IActionResult> AddToCart(string article, int quantity)
        {
            var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["JwtToken"];

            var product = await _api.GetInfoProduct("Bearer " + jwtToken, article);
            string cartJsonFromSession = HttpContext.Session.GetString("Cart"); // Получаем JSON-строку из сессии
            List<CartItem> cartFromSession = cartJsonFromSession != null ? JsonSerializer.Deserialize<List<CartItem>>(cartJsonFromSession) : new List<CartItem>(); // Десериализуем JSON-строку обратно в объект

            var cartItem = cartFromSession.FirstOrDefault(item => item.Product.ArticleProduct == article);

            if (cartItem != null)
            {
                // Если товар уже есть в корзине, добавляем к количеству в корзине запрошенное количество,
                // но перед этим проверяем, не превышает ли суммарное количество товара (уже в корзине и добавляемое) количество товара на складе
                if (cartItem.Count + quantity > product.CountInStockProduct)
                {
                    // Если суммарное количество превышает количество на складе, выводим сообщение об ошибке и не добавляем товар в корзину
                    TempData["Error"] = $"Превышено кол-во товара чем на складе!";
                    return RedirectToAction("Index", "Home"); // Замените "Index" на ваше действие, отображающее каталог товаров
                }
                cartItem.Count += quantity;
            }
            else
            {
                // Если товара еще нет в корзине, проверяем, не превышает ли запрошенное количество количество товара на складе
                if (quantity > product.CountInStockProduct)
                {
                    // Если запрошенное количество превышает количество на складе, выводим сообщение об ошибке и не добавляем товар в корзину
                    TempData["Error"] = $"Превышено кол-во товара чем на складе!";
                    return RedirectToAction("Index", "Home"); // Замените "Index" на ваше действие, отображающее каталог товаров
                }
                cartFromSession.Add(new CartItem { Product = product, Count = quantity });
            }

            string updatedCartJson = JsonSerializer.Serialize(cartFromSession); // Сериализуем обновленный список обратно в JSON-строку
            HttpContext.Session.SetString("Cart", updatedCartJson); // Сохраняем корзину в сеансе

            return RedirectToAction("Cart");
        }

        // Увеличение количества товара в корзине
        public IActionResult IncreaseCartItem(string article)
        {
            string cartJsonFromSession = HttpContext.Session.GetString("Cart"); // Получаем JSON-строку из сессии
            List<CartItem> cartFromSession = cartJsonFromSession != null ? JsonSerializer.Deserialize<List<CartItem>>(cartJsonFromSession) : new List<CartItem>(); // Десериализуем JSON-строку обратно в объект

            var cartItem = cartFromSession.FirstOrDefault(item => item.Product.ArticleProduct == article);
            if (cartItem != null)
            {
                if (cartItem.Product.CountInStockProduct > cartItem.Count)
                {
                    cartItem.Count++;
                    string updatedCartJson = JsonSerializer.Serialize(cartFromSession); // Сериализуем обновленный список обратно в JSON-строку
                    HttpContext.Session.SetString("Cart", updatedCartJson);
                }
            }

            return RedirectToAction("Cart");
        }

        // Уменьшение количества товара в корзине
        public IActionResult DecreaseCartItem(string article)
        {
            string cartJsonFromSession = HttpContext.Session.GetString("Cart"); // Получаем JSON-строку из сессии
            List<CartItem> cartFromSession = cartJsonFromSession != null ? JsonSerializer.Deserialize<List<CartItem>>(cartJsonFromSession) : new List<CartItem>(); // Десериализуем JSON-строку обратно в объект

            var cartItem = cartFromSession.FirstOrDefault(item => item.Product.ArticleProduct == article);
            if (cartItem != null && cartItem.Count > 1)
            {
                cartItem.Count--;
                string updatedCartJson = JsonSerializer.Serialize(cartFromSession); // Сериализуем обновленный список обратно в JSON-строку
                HttpContext.Session.SetString("Cart", updatedCartJson);
            }

            return RedirectToAction("Cart");
        }

        // Удаление товара из корзины
        public IActionResult RemoveCartItem(string article)
        {
            string cartJsonFromSession = HttpContext.Session.GetString("Cart"); // Получаем JSON-строку из сессии
            List<CartItem> cartFromSession = cartJsonFromSession != null ? JsonSerializer.Deserialize<List<CartItem>>(cartJsonFromSession) : new List<CartItem>(); // Десериализуем JSON-строку обратно в объект

            var cartItem = cartFromSession.FirstOrDefault(item => item.Product.ArticleProduct == article);
            if (cartItem != null)
            {
                cartFromSession.Remove(cartItem);
                string updatedCartJson = JsonSerializer.Serialize(cartFromSession); // Сериализуем обновленный список обратно в JSON-строку
                HttpContext.Session.SetString("Cart", updatedCartJson);
            }

            return RedirectToAction("Cart");
        }


        public async Task<IActionResult> Checkout()
        {
            string jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["JwtToken"];
            string cartJsonFromSession = HttpContext.Session.GetString("Cart");
            List<CartItem> cart = cartJsonFromSession != null ? JsonSerializer.Deserialize<List<CartItem>>(cartJsonFromSession) : new List<CartItem>();

            List<OrderProduct> orderProducts = new List<OrderProduct>();

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var userClaims = identity.Claims;

            int idUser = Convert.ToInt32(userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value);

            bool orderPlaced = false;
            foreach (var item in cart)
            {
                orderPlaced = await _api.PlaceOrder("Bearer " + jwtToken, idUser, item.Count, item.Product.ArticleProduct);
            }

            

            if (orderPlaced)
            {
                // Успешно оформлен заказ, можно очистить корзину
                HttpContext.Session.Remove("Cart");
                TempData["Success"] = "Заказ успешно оформлен!";
            }
            else
            {
                TempData["Error"] = "Не удалось оформить заказ. Попробуйте еще раз.";
            }

            return RedirectToAction("Cart");
        }


        [HttpGet]
        public IActionResult Cart()
        {
            string cartJsonFromSession = HttpContext.Session.GetString("Cart"); // Получаем JSON-строку из сессии
            List<CartItem> cart = cartJsonFromSession != null ? JsonSerializer.Deserialize<List<CartItem>>(cartJsonFromSession) : new List<CartItem>(); // Десериализуем JSON-строку обратно в объект
            return View(cart);
        }



    }
}
