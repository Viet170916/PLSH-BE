using System.Collections.Generic;
using BU.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book.Dto;
using Model.Entity.LibraryRoom;

namespace API.Controllers.LibRoomControllers;
// [Authorize("LibrarianPolicy")]

public partial class LibraryRoomController
{
  [Authorize("LibrarianPolicy")] [HttpGet("shelf")]
  public async Task<ActionResult<List<LibraryRoomDto.ShelfDto>>> GetShelves([FromQuery] bool? minimum)
  {
    List<Shelf> shelves;
    if (minimum is null or false)
    {
      shelves = await context.Shelves
                             .Include(s => s.RowShelves)
                             .ThenInclude(rs => rs.BookInstances)
                             .Where(r => r.Type == "SHELF_LARGE" || r.Type == "SHELF_SMALL")
                             .OrderBy(r => r.Name)
                             .ToListAsync();
      var result = mapper.Map<List<LibraryRoomDto.ShelfDto>>(shelves);
      foreach (var rowShelfDtose in result.Select(r => r.RowShelves))
      {
        foreach (var rowShelfDto in rowShelfDtose) { rowShelfDto.BookInstances = []; }
      }

      return Ok(result);
    }

    shelves = await context.Shelves
                           .Where(r => r.Type == "SHELF_LARGE" || r.Type == "SHELF_SMALL")
                           .ToListAsync();
    return Ok(shelves);
  }

  [Authorize("LibrarianPolicy")] [HttpGet("shelves/selection")]
  public async Task<ActionResult<List<LibraryRoomDto.ShelfDto>>> GetShelvesSelection([FromQuery] bool? minimum)
  {
    var shelves = await context.Shelves
                               .Where(r => r.Type == "SHELF_LARGE" || r.Type == "SHELF_SMALL")
                               .Select(sh => new LibraryRoomDto.ShelfDto { Id = sh.Id,
                                 Name = sh.Name??$"Kệ[{sh.X},{sh.Y}]", })
                               .ToListAsync();
    return Ok(new BaseResponse<List<LibraryRoomDto.ShelfDto>> { Data = shelves, });
  }

  [Authorize("LibrarianPolicy")] [HttpGet("items")]
  public async Task<ActionResult<List<LibraryRoomDto.ShelfDto>>> GetItems([FromQuery] bool? minimum)
  {
    List<Shelf> shelves;
    if (minimum is null or false)
    {
      shelves = await context.Shelves
                             .Include(s => s.RowShelves)
                             .ThenInclude(rs => rs.BookInstances)
                             // .Where(r => r.Type == "SHELF_LARGE" || r.Type == "SHELF_SMALL")
                             .ToListAsync();
      var result = mapper.Map<List<LibraryRoomDto.ShelfDto>>(shelves);
      foreach (var rowShelfDtose in result.Select(r => r.RowShelves))
      {
        foreach (var rowShelfDto in rowShelfDtose) { rowShelfDto.BookInstances = []; }
      }

      return Ok(result);
    }

    shelves = await context.Shelves
                           // .Where(r => r.Type == "SHELF_LARGE" || r.Type == "SHELF_SMALL")
                           .ToListAsync();
    return Ok(shelves);
  }

  [Authorize("LibrarianPolicy")] [HttpGet("shelf/check")]
  public async Task<IActionResult> CheckShelfExists([FromQuery] int? id, [FromQuery] string? name)
  {
    if (id == null && string.IsNullOrEmpty(name))
      return BadRequest(new { Message = "Please provide an 'id' or 'name' to check." });
    var exists = await context.Shelves.AnyAsync(s =>
      (id != null && s.Id == id) ||
      (!string.IsNullOrEmpty(name) && s.Name == name));
    return Ok(new { exists, });
  }

  [Authorize("LibrarianPolicy")] [HttpGet("shelf/{id:long}")]
  public async Task<IActionResult> GetShelfById(long id)
  {
    var shelf = await context.Shelves
                             .Include(sh => sh.RowShelves)
                             .ThenInclude(rs => rs.BookInstances)!
                             .ThenInclude(bit => bit.Book)
                             .ThenInclude(book => book.Category)
                             .Include(sh => sh.RowShelves)
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

  [Authorize("LibrarianPolicy")] [HttpPost("item/upsert")]
  public async Task<IActionResult> UpsertItem([FromBody] LibraryRoomDto.ShelfDto itemDto)
  {
    var libRoomId = (await context.LibraryRooms.FirstOrDefaultAsync())?.Id;
    var existingItem = await context.Shelves.FindAsync(itemDto.Id);
    if (existingItem != null)
    {
      mapper.Map(itemDto, existingItem);
      context.Shelves.Update(existingItem);
    }
    else
    {
      var newItem = mapper.Map<Shelf>(itemDto);
      newItem.RoomId = libRoomId ?? 1;
      context.Shelves.Add(newItem);
    }

    await context.SaveChangesAsync();
    return Ok(new BaseResponse<LibraryRoomDto.ShelfDto> { Data = itemDto, });
  }

  [Authorize("LibrarianPolicy")] [HttpDelete("item/{id}")]
  public async Task<IActionResult> DeleteItem(long id, [FromQuery] bool? force = false)
  {
    var item = await context.Shelves.FindAsync(id);
    if (item == null)
    {
      return BadRequest(new BaseResponse<object?> { Message = "Item không tồn tại", Data = null, Status = "error" });
    }

    if (item.Type is "SHELF_SMALL" or "SHELF_LARGE")
    {
      var shelfRows = await context.RowShelves
                                   .Where(r => r.ShelfId == id)
                                   .ToListAsync();
      if (shelfRows.Count == 0)
      {
        context.Shelves.Remove(item);
        await context.SaveChangesAsync();
        return Ok(new BaseResponse<object?> { Message = "Xoá thành công", Data = null, Status = "success" });
      }

      var rowShelfIds = shelfRows.Select(r => r.Id).ToList();
      var hasBook = await context.BookInstances
                                 .AnyAsync(b => rowShelfIds.Contains(b.RowShelfId ?? -1));
      if (hasBook)
      {
        if (force is null or false)
        {
          return Ok(new BaseResponse<object?>
          {
            Message = "Trên shelf hiện đang chứa sách", Data = null, Status = "pending"
          });
        }

        var bookInstances = await context.BookInstances
                                         .Where(b => rowShelfIds.Contains(b.RowShelfId ?? -1))
                                         .ToListAsync();
        foreach (var bookInstance in bookInstances) { bookInstance.RowShelfId = null; }

        context.BookInstances.UpdateRange(bookInstances);
      }

      context.RowShelves.RemoveRange(shelfRows);
      context.Shelves.Remove(item);
      await context.SaveChangesAsync();
      return Ok(new BaseResponse<object?> { Message = "Xoá thành công", Data = null, Status = "success" });
    }

    context.Shelves.Remove(item);
    await context.SaveChangesAsync();
    return Ok(new BaseResponse<object?> { Message = "Xoá thành công", Data = null, Status = "success" });
  }
}
