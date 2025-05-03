using System.Collections.Generic;
using AutoMapper;
using BU.Models.DTO.Book;
using Data.DatabaseContext;
using FuzzySharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;

namespace API.Controllers.CategoryControllers;

[Route("api/v1/category")]
[ApiController]
public class CategoryController(AppDbContext context, IMapper mapper) : ControllerBase
{
  [Authorize] [HttpGet] public async Task<IActionResult> GetCategories()
  {
    var categories = await context.Categories.ToListAsync();
    return Ok(categories);
  }

  [Authorize("LibrarianPolicy")] [HttpPost]
  public async Task<IActionResult> CreateCategory([FromBody] Category category)
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

  [Authorize("LibrarianPolicy")] [HttpPut("{id}")]
  public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category updatedCategory)
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

  [Authorize("LibrarianPolicy")] [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteCategory(int id)
  {
    var category = await context.Categories.FindAsync(id);
    if (category == null) return NotFound("Category không tồn tại.");
    context.Categories.Remove(category);
    await context.SaveChangesAsync();
    return NoContent();
  }

  [Authorize("LibrarianPolicy")] [HttpGet("check-duplicate")]
  public async Task<IActionResult> CheckDuplicateCategory([FromQuery] string name)
  {
    if (string.IsNullOrWhiteSpace(name)) return BadRequest("Tên Category không hợp lệ.");
    var categories = await context.Categories.ToListAsync();
    var lowPriorityWords = new HashSet<string> { "học", "cơ bản", "nâng cao", "chuyên sâu" };
    string RemoveLowPriorityWords(string input)
    {
      var words = input.Split(' ').Where(w => !lowPriorityWords.Contains(w.ToLower()));
      return string.Join(" ", words);
    }
    var cleanedInput = RemoveLowPriorityWords(name);
    var similarCategories = categories
                            .Select(c => new
                            {
                              Original = c,
                              Score = Fuzz.WeightedRatio(cleanedInput.ToLower(),
                                RemoveLowPriorityWords(c.Name.ToLower()))
                            })
                            .Where(c => c.Score >= 60)
                            .OrderByDescending(c => c.Score)
                            .Select(sm => sm.Original)
                            .ToList();
    if (similarCategories.Count == 0)
      return Ok(new { message = "Không có Category nào trùng lặp.", Status = "no duplicated", });
    var categoryDtos = mapper.Map<List<CategoryDto>>(similarCategories);
    return Ok(new { message = "Có thể trùng với:", suggestions = categoryDtos, Status = "duplicated", });
  }
}
