using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book.Dto;
using Model.Entity.LibraryRoom;

namespace API.Controllers.LibRoomControllers;

public partial class LibraryRoomController
{
  [HttpPost("shelf/{shelfId}/row/add")] [HttpPost("add")]
  public async Task<IActionResult> AddRowShelf(long shelfId)
  {
    var shelf = await context.Shelves.FindAsync(shelfId);
    if (shelf is null) return NotFound(new { Message = "Shelf not found." });
    var position = await context.RowShelves
                                .Where(r => r.ShelfId == shelfId)
                                .OrderByDescending(r => r.Position)
                                .Select(r => r.Position ?? -1)
                                .FirstOrDefaultAsync();
    var row = new RowShelf() { ShelfId = shelfId, MaxCol = 10, Position = position, };
    context.RowShelves.Add(row);
    await context.SaveChangesAsync();
    var rowMap = mapper.Map<LibraryRoomDto.RowShelfDto>(row);
    return Ok(rowMap);
  }

  [HttpDelete("shelf/row/delete/{id}")] public async Task<IActionResult> DeleteRowShelf(long id)
  {
    var rowShelf = await context.RowShelves.FindAsync(id);
    if (rowShelf is null) return NotFound(new { Message = "RowShelf not found." });
    await context.BookInstances
                 .Where(b => b.RowShelfId == id)
                 .ExecuteUpdateAsync(setter => setter.SetProperty(b => b.RowShelfId, (int?)null));
    context.RowShelves.Remove(rowShelf);
    await context.SaveChangesAsync();
    return Ok(new { Message = $"RowShelf {id} and related BookShelves deleted.", Success = true, });
  }

  public class RowShelfUpdate
  {
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int MaxCol { get; set; }
  }

  [HttpPut("shelf/row/update/{id}")]
  public async Task<IActionResult> UpdateRowShelf(long id, [FromBody] RowShelfUpdate updatedRowShelf)
  {
    var rowShelf = await context.RowShelves.FindAsync(id);
    if (rowShelf is null) return NotFound(new { Message = "RowShelf not found." });
    rowShelf.Name = updatedRowShelf.Name;
    rowShelf.Description = updatedRowShelf.Description;
    rowShelf.MaxCol = updatedRowShelf.MaxCol;
    await context.SaveChangesAsync();
    var rowMap = mapper.Map<LibraryRoomDto.RowShelfDto>(rowShelf);
    return Ok(rowMap);
  }

  [HttpGet("shelf/row/has-books/{rowShelfId}")]
  public async Task<IActionResult> HasBooksInRowShelf(long rowShelfId)
  {
    var hasBooks = await context.BookInstances.AnyAsync(b => b.RowShelfId == rowShelfId);
    return Ok(new { rowShelfId, hasBooks });
  }
}