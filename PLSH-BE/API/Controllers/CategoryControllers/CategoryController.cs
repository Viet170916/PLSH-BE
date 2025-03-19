using System.Collections.Generic;
using Data.DatabaseContext;
using FuzzySharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;

namespace API.Controllers.CategoryControllers;

[Route("api/v1/category")]
[ApiController]
public class CategoryController(AppDbContext context) : ControllerBase
{
  [HttpGet] public async Task<IActionResult> GetCategories()
  {
    var categories = await context.Categories.ToListAsync();
    return Ok(categories);
  }

  // 📌 2️⃣ Thêm mới Category
  [HttpPost] public async Task<IActionResult> CreateCategory([FromBody] Category category)
  {
    if (category == null || string.IsNullOrWhiteSpace(category.Name))
      return BadRequest("Tên Category không được để trống.");

    // Kiểm tra xem tên đã tồn tại chưa
    if (await context.Categories.AnyAsync(c => c.Name == category.Name))
      return Conflict($"Category '{category.Name}' đã tồn tại.");
    context.Categories.Add(category);
    await context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
  }

  // 📌 3️⃣ Sửa Category
  [HttpPut("{id}")] public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category updatedCategory)
  {
    var category = await context.Categories.FindAsync(id);
    if (category == null) return NotFound("Category không tồn tại.");
    category.Name = updatedCategory.Name;
    category.Description = updatedCategory.Description;
    category.Status = updatedCategory.Status;
    category.UpdatedDate = DateTime.UtcNow;
    await context.SaveChangesAsync();
    return Ok(category);
  }

  // 📌 4️⃣ Xóa Category
  [HttpDelete("{id}")] public async Task<IActionResult> DeleteCategory(int id)
  {
    var category = await context.Categories.FindAsync(id);
    if (category == null) return NotFound("Category không tồn tại.");
    context.Categories.Remove(category);
    await context.SaveChangesAsync();
    return NoContent();
  }

  [HttpGet("check-duplicate")] public async Task<IActionResult> CheckDuplicateCategory([FromQuery] string name)
  {
    if (string.IsNullOrWhiteSpace(name)) return BadRequest("Tên Category không hợp lệ.");
    var categories = await context.Categories.Select(c => c.Name).ToListAsync();
    var lowPriorityWords = new HashSet<string> { "học", "cơ bản", "nâng cao", "chuyên sâu" };
    string RemoveLowPriorityWords(string input)
    {
      var words = input.Split(' ').Where(w => !lowPriorityWords.Contains(w.ToLower()));
      return string.Join(" ", words);
    }
    string cleanedInput = RemoveLowPriorityWords(name);
    var similarCategories = categories
                            .Select(c => new
                            {
                              Original = c,
                              Score = Fuzz.WeightedRatio(cleanedInput.ToLower(), RemoveLowPriorityWords(c.ToLower()))
                            })
                            .Where(c => c.Score >= 60) // Chỉ lấy những từ có độ trùng >= 70%
                            .OrderByDescending(c => c.Score) // Sắp xếp theo độ trùng giảm dần
                            .Select(c => c.Original)
                            .ToList();
    if (similarCategories.Count == 0) return Ok(new { message = "Không có Category nào trùng lặp." });
    return Ok(new { message = "Có thể trùng với:", suggestions = similarCategories });
  }
}