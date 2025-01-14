using Microsoft.AspNetCore.Mvc;
using api.Cache;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICacheService _cacheService;
        private const string CacheKey = "products";

        public ProductController(ApplicationDbContext dbContext, ICacheService cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = _cacheService.GetData<List<Product>>(CacheKey);
            if (products == null)
            {
                products = await _dbContext.Products.Include(p => p.Category).ToListAsync();
                _cacheService.SetData(CacheKey, products, DateTimeOffset.Now.AddMinutes(10));
            }

            return Ok(products);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var products = await _dbContext.Products.Include(p => p.Category).ToListAsync();
            _cacheService.SetData(CacheKey, products, DateTimeOffset.Now.AddMinutes(10));

            return Ok(product);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product updatedProduct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Quantity = updatedProduct.Quantity;
            product.Category = updatedProduct.Category;

            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();

            var products = await _dbContext.Products.Include(p => p.Category).ToListAsync();
            _cacheService.SetData(CacheKey, products, DateTimeOffset.Now.AddMinutes(10));

            return Ok(product);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            var products = await _dbContext.Products.Include(p => p.Category).ToListAsync();
            _cacheService.SetData(CacheKey, products, DateTimeOffset.Now.AddMinutes(10));

            return Ok(new { success = true });
        }

        [HttpDelete("remove-cache")]
        public IActionResult ClearCache()
        {
            var result = _cacheService.RemoveData(CacheKey);
            return Ok(new { success = result });
        }
    }
}
