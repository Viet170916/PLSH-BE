using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book.Dto;

namespace API.Controllers.LibRoomControllers;

public partial class LibraryRoomController
{
  [HttpGet("shelf")] public async Task<ActionResult<List<LibraryRoomDto.ShelfDto>>> GetShelves()
  {
    var shelves = await context.Shelves
                               .Include(s => s.RowShelves)
                               .ThenInclude(rs => rs.BookInstances)
                               .ToListAsync();
    return Ok(mapper.Map<List<LibraryRoomDto.ShelfDto>>(shelves));
  }

  [HttpGet("shelf/check")]
  public async Task<IActionResult> CheckShelfExists([FromQuery] int? id, [FromQuery] string? name)
  {
    if (id == null && string.IsNullOrEmpty(name))
      return BadRequest(new { Message = "Please provide an 'id' or 'name' to check." });
    var exists = await context.Shelves.AnyAsync(s =>
      (id != null && s.Id == id) ||
      (!string.IsNullOrEmpty(name) && s.Name == name));
    return Ok(new { exists, });
  }

  [HttpGet("shelf/{id:long}")] public async Task<IActionResult> GetShelfById(long id)
  {
    var shelf = await context.Shelves
                             .Include(sh => sh.RowShelves)
                             .ThenInclude(rs => rs.BookInstances)!
                             .ThenInclude(bit => bit.Book)
                             .ThenInclude(book => book.Category).Include(sh => sh.RowShelves)
                             .ThenInclude(rs => rs.BookInstances)!
                             .ThenInclude(bit => bit.Book)
                             .ThenInclude(book => book.CoverImageResource)
                             .FirstOrDefaultAsync(s => s.Id == id);
    if (shelf is null) return NotFound(new { Message = "Shelf not found." });
    var rowShelves = await context.RowShelves.Where(r => r.ShelfId == shelf.Id).ToListAsync();
    shelf.RowShelves = rowShelves;
    var shelfMapped = mapper.Map<LibraryRoomDto.ShelfDto>(shelf);
    return Ok(new { ShelfBaseInfo = shelfMapped, rowShelves = shelfMapped.RowShelves, });
  }
}