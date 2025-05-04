using System.Collections.Generic;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;
using LibraryRoomDto = Model.Entity.book.Dto.LibraryRoomDto;

namespace API.Controllers.LibRoomControllers;
// [Authorize("LibrarianPolicy")]

public partial class LibraryRoomController
{
  public async Task<IActionResult> PutBooksOnShelf([FromBody] List<LibraryRoomDto.BookInstanceDto> bookInstancesDto)
  {
    if (bookInstancesDto == null || bookInstancesDto.Count == 0)
    {
      return BadRequest(new { Message = "List of Book instances cannot be empty." });
    }

    var updatedInstances = mapper.Map<List<BookInstance>>(bookInstancesDto);
    var groupedInstances = updatedInstances.GroupBy(i => i.RowShelfId).ToList();
    var failedInstances = new List<LibraryRoomDto.BookInstanceDto>();
    foreach (var group in groupedInstances)
    {
      if (!group.Key.HasValue) continue;
      var rowShelfId = group.Key.Value;
      var maxCol = await context.RowShelves
                                .Where(shelf => shelf.Id == rowShelfId)
                                .Select(shelf => shelf.MaxCol)
                                .FirstOrDefaultAsync();
      var occupiedPositions = await context.BookInstances
                                           .Where(b => b.RowShelfId == rowShelfId && b.Position.HasValue)
                                           .Select(b => b.Position.Value)
                                           .ToListAsync();
      var availablePositions = Enumerable.Range(0, maxCol-1)
                                         .Except(occupiedPositions)
                                         .ToList();
      foreach (var instance in group)
      {
        if (instance.Position.HasValue)
        {
          if (instance.Position < 0 || instance.Position >= maxCol)
          {
            failedInstances.Add(mapper.Map<LibraryRoomDto.BookInstanceDto>(instance));
            continue;
          }

          if (occupiedPositions.Contains(instance.Position.Value))
          {
            failedInstances.Add(mapper.Map<LibraryRoomDto.BookInstanceDto>(instance));
            continue;
          }

          occupiedPositions.Add(instance.Position.Value);
        }
        else
        {
          if (availablePositions.Count == 0)
          {
            failedInstances.Add(mapper.Map<LibraryRoomDto.BookInstanceDto>(instance));
            continue;
          }

          instance.Position = availablePositions.First();
          availablePositions.RemoveAt(0);
        }
      }
    }

    var successfulInstances = updatedInstances
                              .Where(inst => !failedInstances.Any(f => f.Id == inst.Id))
                              .ToList();
    await context.BulkUpdateAsync(successfulInstances);
    var successfulDtos = mapper.Map<List<LibraryRoomDto.BookInstanceDto>>(successfulInstances);
    return Ok(new
    {
      Message = "Sách thêm vào kệ thành công",
      Success = true,
      Data = successfulDtos,
      Failed = failedInstances,
      FailedCount = failedInstances.Count
    });
  }

  [HttpPut("delete-out-of-row-shelf")]
  public async Task<IActionResult> RemoveFromRowShelf([FromBody] List<int> bookInstanceIds)
  {
    if (bookInstanceIds == null || bookInstanceIds.Count == 0)
    {
      return BadRequest(new { Message = "List of Book instance IDs cannot be empty." });
    }

    var bookInstances = await context.BookInstances
                                     .Where(b => bookInstanceIds.Contains(b.Id))
                                     .ToListAsync();
    if (!bookInstances.Any()) { return NotFound(new { Message = "No matching Book instances found." }); }

    foreach (var instance in bookInstances)
    {
      instance.RowShelfId = null;
      instance.Position = null;
    }

    await context.BulkUpdateAsync(bookInstances);
    var updatedDtos = mapper.Map<List<LibraryRoomDto.BookInstanceDto>>(bookInstances);
    return Ok(
      new { Message = "Book instances removed from RowShelf successfully.", Success = true, Data = updatedDtos });
  }

  [HttpGet("Book-instances/on-shelf")]
  public async Task<IActionResult> GetBookInstancesByBookAndShelf([FromQuery] int bookId, [FromQuery] int rowShelfId)
  {
    var bookInstances = await context.BookInstances
                                     .Include(b => b.RowShelf)
                                     .ThenInclude(r => r.Shelf)
                                     .Where(b => b.BookId == bookId && b.RowShelfId == rowShelfId)
                                     .ToListAsync();
    if (!bookInstances.Any()) { return NotFound(new { Message = "No matching Book instances found." }); }

    var bookInstanceDtos = mapper.Map<List<LibraryRoomDto.BookInstanceDto>>(bookInstances);
    return Ok(new { Message = "Book instances retrieved successfully.", Success = true, Data = bookInstanceDtos });
  }
}
