using System.Collections.Generic;
using BU.Models.DTO;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;
using Model.Entity.book.Dto;
using Model.Entity.LibraryRoom;

namespace API.Controllers.LibRoomControllers;
// [Authorize("LibrarianPolicy")]

public partial class LibraryRoomController
{
  [HttpPost("shelf/{shelfId}/row/add")]
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
    public List<LibraryRoomDto.BookInstanceDto> BookInstances { get; set; } = [];
  }

  [HttpPut("shelf/row/update/{id}")]
  public async Task<IActionResult> UpdateRowShelf(long id, [FromBody] RowShelfUpdate updatedRowShelf)
  {
    var rowShelf = await context.RowShelves.FindAsync(id);
    if (rowShelf is null) return NotFound(new { Message = "RowShelf not found." });
    rowShelf.Name = updatedRowShelf.Name;
    rowShelf.Description = updatedRowShelf.Description;
    rowShelf.MaxCol = updatedRowShelf.MaxCol;
    var updatedDict = updatedRowShelf.BookInstances
                                     .ToDictionary(b => b.Id, b => b.Position);
    var bookInstancesToUpdate = await context.BookInstances
                                             .Where(b => updatedDict.Keys.Contains(b.Id))
                                             .ToListAsync();
    foreach (var instance in bookInstancesToUpdate)
    {
      instance.Position = updatedDict[instance.Id];
    }

    await context.BulkUpdateAsync(bookInstancesToUpdate);

    // rowShelf.BookInstances = updatedRowShelf.BookInstances.Select(i => new BookInstance { Id = i.Id, Position = i.Position}).ToList();
    await context.SaveChangesAsync();
    var rowMap = mapper.Map<LibraryRoomDto.RowShelfDto>(rowShelf);
    return Ok(new BaseResponse<LibraryRoomDto.RowShelfDto> { Data = rowMap, Message = "Lưu thành công", });
  }

  [HttpGet("shelf/row/has-books/{rowShelfId}")]
  public async Task<IActionResult> HasBooksInRowShelf(long rowShelfId)
  {
    var hasBooks = await context.BookInstances.AnyAsync(b => b.RowShelfId == rowShelfId);
    return Ok(new { rowShelfId, hasBooks });
  }

  [HttpGet("shelf/rows/selection")]
  public async Task<IActionResult> GetRowShelvesWithEmptyPositions(
    [FromQuery] string? name,
    [FromQuery] long? shelfId)
  {
    var query = context.RowShelves.AsQueryable();

    if (!string.IsNullOrEmpty(name))
    {
      query = query.Where(r => r.Name != null && r.Name.Contains(name));
    }

    if (shelfId.HasValue)
    {
      query = query.Where(r => r.ShelfId == shelfId.Value);
    }

    var rowShelves = await query
                           .Select(r => new
                           {
                             r.Id,
                             Name =r.Name??$"Chưa được đặt tên",
                             r.ShelfId,
                             r.MaxCol,
                             EmptyPositions = r.BookInstances != null
                               ? Enumerable.Range(0, r.MaxCol)
                                           .Except(r.BookInstances.Select(bi => bi.Position ?? -1))
                                           .ToList()
                               : Enumerable.Range(0, r.MaxCol).ToList(),
                           })
                           .ToListAsync();

    return Ok(new BaseResponse<object>
    {
      Data = rowShelves,
    });
  }

}
