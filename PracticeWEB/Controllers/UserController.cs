using Microsoft.AspNetCore.Mvc;
using PracticeWEB.Api;
using System.Xml.Linq;
using Refit;
using System.Threading.Tasks;
using PracticeWEB.Api;
using PracticeWEB.Models;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PracticeWEB.Controllers
{
    public class UserController : Controller
    {
        private readonly IApiService _api;

        private readonly ILogger<UserController> _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(ILogger<UserController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _api = RestService.For<IApiService>(ApiConfig.BaseUrl);
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public async Task<IActionResult> RegisterApi(string login, string password, string name)
        {
            Console.WriteLine(login);
            if (string.IsNullOrEmpty(login) && string.IsNullOrEmpty(password) && string.IsNullOrEmpty(name))
            {
                TempData["SuccessMessage"] = "Заполните поля!";
            }

            User usr = new User
            {
                LoginUser = login,
                PasswordUser = password,
                NameUser = name
            };

            try
            {
                var registeredUser = await _api.RegisterUser(usr);
                TempData["SuccessMessage"] = "Успешная регистрация!";
                return RedirectToAction("Login", "User");
            }
            catch (ApiException ex)
            {
                Console.WriteLine(ex.Message);
                TempData["DangerMessage"] = "Данный логин уже имеется";
                return RedirectToAction("Register", "User");
            }
        }

        public async Task<IActionResult> LoginApi(string login, string password)
        {
            if (string.IsNullOrEmpty(login) && string.IsNullOrEmpty(password))
            {
                TempData["DangerMessage"] = "Поля не должны быть пустыми";
            }

            User usr = new User
            {
                LoginUser = login,
                PasswordUser = password,
            };

            try
            {
                var token = _api.LoginUser(usr);
                Console.WriteLine(token.Result.ToString());

                Response.Cookies.Append("JwtToken", token.Result.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict
                    // Другие параметры по необходимости
                });

                // Расшифровка JWT-токена и создание идентификационных данных пользователя
                var tokenHandler = new JwtSecurityTokenHandler();
                var decodedToken = tokenHandler.ReadJwtToken(token.Result.ToString());
                var claims = decodedToken.Claims.Select(c => new Claim(c.Type, c.Value));

                var identity = new ClaimsIdentity(claims, "jwt");

                // Создание принципала на основе идентификационных данных пользователя
                var principal = new ClaimsPrincipal(identity);

                // Выполнение входа пользователя
                await HttpContext.SignInAsync(principal);




                return RedirectToAction("Index", "Home");
            }
            catch
            {
                TempData["DangerMessage"] = "Введён неверный лонин или пароль";
            }

            return RedirectToAction("Login", "User");
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Удаление куки с токеном
            Response.Cookies.Delete("token");

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> UserList()
        {
            var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["JwtToken"];
            var response = await _api.GetListUser("Bearer " + jwtToken);

            return View(response);
        }

        public async Task<IActionResult> EditUser(int IdUser)
        {
            var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["JwtToken"];
            var response = await _api.GetInfoUser("Bearer " + jwtToken, IdUser);

            return View(response);
        }

        public async Task<IActionResult> UpdateUser(User user)
        {
            try
            {
                var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["JwtToken"];
                var response = _api.UpdateUser("Bearer " + jwtToken, user);
            }
            catch
            {
                TempData["DangerMessage"] = "Ошибка";
                return RedirectToAction("EditUser", "User");
            }
            return RedirectToAction("Index","Home");
        }


    }
}
