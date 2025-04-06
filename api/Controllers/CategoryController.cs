using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using api.Data;
using api.Models;

namespace api.Controllers
{
    [Route("/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private const string CacheKey = "categories";

        public CategoryController(ApplicationDbContext dbContext, IDistributedCache cache)
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
                var cachedCategories = JsonSerializer.Deserialize<List<Category>>(cachedData);
                return Ok(cachedCategories);
            }

            var categories = await _dbContext.Categories.ToListAsync();
            await RefreshCache(categories);

            return Ok(categories);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] string cateName)
        {
            var newCategory = new Category { Name = cateName };
            await _dbContext.Categories.AddAsync(newCategory);
            await _dbContext.SaveChangesAsync();

            var categories = await _dbContext.Categories.ToListAsync();
            await RefreshCache(categories);

            return Ok(categories);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] string cateName)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            category.Name = cateName;
            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync();

            var categories = await _dbContext.Categories.ToListAsync();
            await RefreshCache(categories);

            return Ok(categories);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();

            var categories = await _dbContext.Categories.ToListAsync();
            await RefreshCache(categories);

            return Ok(new { success = true, data = categories });
        }

        [HttpPost("refresh-cache")]
        public async Task<IActionResult> RefreshCacheManually()
        {
            var categories = await _dbContext.Categories.ToListAsync();
            await RefreshCache(categories);

            return Ok(new { success = true, data = categories });
        }

        private async Task RefreshCache(List<Category> categories)
        {
            var serializedData = JsonSerializer.Serialize(categories);
            await _cache.SetStringAsync(CacheKey, serializedData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
        }
    }
}
