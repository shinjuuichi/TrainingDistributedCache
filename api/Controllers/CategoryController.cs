using Microsoft.AspNetCore.Mvc;
using api.Cache;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICacheService _cacheService;
        private const string CacheKey = "categories";

        public CategoryController(ApplicationDbContext dbContext, ICacheService cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = _cacheService.GetData<List<Category>>(CacheKey);
            if (categories == null)
            {
                categories = await _dbContext.Categories.ToListAsync();
                _cacheService.SetData(CacheKey, categories, DateTimeOffset.Now.AddMinutes(10));
            }

            return Ok(categories);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] Category category)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            var categories = await _dbContext.Categories.ToListAsync();
            _cacheService.SetData(CacheKey, categories, DateTimeOffset.Now.AddMinutes(10));

            return Ok(new { data = categories });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category updatedCategory)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            category.Name = updatedCategory.Name;

            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync();

            var categories = await _dbContext.Categories.ToListAsync();
            _cacheService.SetData(CacheKey, categories, DateTimeOffset.Now.AddMinutes(10));

            return Ok(new {data = categories});
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
            _cacheService.SetData(CacheKey, categories, DateTimeOffset.Now.AddMinutes(10));

            return Ok(new { success = true , data = categories});
        }

        [HttpDelete("remove-cache")]
        public IActionResult ClearCache()
        {
            var result = _cacheService.RemoveData(CacheKey);
            return Ok(new { success = result });
        }
    }
}
