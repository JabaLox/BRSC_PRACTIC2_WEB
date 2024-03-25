using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Refit;
using PracticeWEB.Models;
using System.Runtime.CompilerServices;

namespace PracticeWEB.Api
{
    public interface IApiService
    {
        [Get("/product/list")]
        Task<List<Product>> GetListProduct();

        [Get("/product/category")]
        Task<List<Category>> GetListCategory();

        [Get("/product/manufacture")]
        Task<List<Manufacture>> GetListManufacture();

        [Post("/product/create")]
        Task<bool> ProductCreate([Header("Authorization")] string authorizationHeader, Product product);

        [Get("/product/{article}")]
        Task<Product> GetInfoProduct([Header("Authorization")]  string authorizationHeader, string article);

        [Put("/product/edit/{article}")]
        Task<bool> UpdateProduct([Header("Authorization")] string authorizationHeader, string article, Product product);

        [Delete("/product/delete/{article}")]
        Task<bool> DeleteProduct([Header("Authorization")] string Authorization, string article);

        [Post("/user/register")]
        Task<bool> RegisterUser(User user);

        [Post("/user/login")]
        Task<string> LoginUser(User user);

        [Get("/user/list")]
        Task<List<User>> GetListUser([Header("Authorization")] string Authorization);

        [Get("/user/info/{Id}")]
        Task<User> GetInfoUser([Header("Authorization")] string Authorization, int Id);


        [Put("/user/edit")]
        Task<bool> UpdateUser([Header("Authorization")] string authorizationHeader, User user);

        [Get("/order/list")]
        Task<List<OrderProduct>> GetListOrderProduct([Header("Authorization")] string Authorization);

        [Post("/order/create")]
        Task<bool> PlaceOrder([Header("Authorization")] string authorizationHeader, int idUser, int count, string article);



    }
}
