using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using api.Data;
using api.Models;
using api.NewFolder;

namespace api.Controllers
{
    [Route("/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private const string CacheKey = "products";

        public ProductController(ApplicationDbContext dbContext, IDistributedCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cachedData = await _cache.GetStringAsync(CacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                var cachedProducts = JsonSerializer.Deserialize<List<Product>>(cachedData);
                return Ok(cachedProducts);
            }

            var products = await _dbContext.Products.Include(p => p.Category).ToListAsync();
            await RefreshCache(products);

            return Ok(products);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] ProductDTO product)
        {
            var category = await _dbContext.Categories.FindAsync(product.CategoryId);
            if (category == null)
                return NotFound(new { message = "Category not found" });

            var newProduct = new Product
            {
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
                Category = category
            };

            await _dbContext.Products.AddAsync(newProduct);
            await _dbContext.SaveChangesAsync();

            var products = await _dbContext.Products.Include(p => p.Category).ToListAsync();
            await RefreshCache(products);

            return Ok(newProduct);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDTO updatedProduct)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            var category = await _dbContext.Categories.FindAsync(updatedProduct.CategoryId);
            if (category == null)
                return NotFound(new { message = "Category not found" });

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Quantity = updatedProduct.Quantity;
            product.Category = category;

            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();

            var products = await _dbContext.Products.Include(p => p.Category).ToListAsync();
            await RefreshCache(products);

            return Ok(product);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            var products = await _dbContext.Products.Include(p => p.Category).ToListAsync();
            await RefreshCache(products);

            return Ok(new { success = true });
        }

        [HttpPost("refresh-cache")]
        public async Task<IActionResult> RefreshCacheManually()
        {
            var products = await _dbContext.Products.Include(p => p.Category).ToListAsync();
            await RefreshCache(products);

            return Ok(new { success = true, data = products });
        }

        private async Task RefreshCache(List<Product> products)
        {
            var serializedData = JsonSerializer.Serialize(products);
            await _cache.SetStringAsync(CacheKey, serializedData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
        }
    }
}
