using ECommerce.Shared;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace ECommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IDatabase _redisDb;
        private const string CacheKey = "catalog:products";

        public ProductsController(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var cachedData = await _redisDb.StringGetAsync(CacheKey);

            if (!cachedData.IsNullOrEmpty)
            {
                var cachedProducts = JsonSerializer.Deserialize<List<Product>>(cachedData);
                return Ok(cachedProducts);
            }

            var products = new List<Product>
            {
                new Product { Name = "Coffee Mug", Price = 15.99m, StockQuantity = 100},
                new Product { Name = "Mechanical Keyboard", Price = 120.00m, StockQuantity = 50},
                new Product { Name = "Noise-Cancelling Headphones", Price = 250.00m, StockQuantity = 30}
            };

            var serializedProducts = JsonSerializer.Serialize(products);
            await _redisDb.StringSetAsync(CacheKey, serializedProducts, TimeSpan.FromMinutes(5));

            return Ok(products); 

        }
    }
}
