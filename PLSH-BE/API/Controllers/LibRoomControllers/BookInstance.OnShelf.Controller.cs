using System.Collections.Generic;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;
using LibraryRoomDto = Model.Entity.book.Dto.LibraryRoomDto;

namespace API.Controllers.LibRoomControllers;

public partial class LibraryRoomController
{
  [HttpPut("put-books-on-shelf")]
  public async Task<IActionResult> PutBooksOnShelf([FromBody] List<LibraryRoomDto.BookInstanceDto> bookInstancesDto)
  {
    if (bookInstancesDto == null || bookInstancesDto.Count == 0)
    {
      return BadRequest("List of book instances cannot be empty.");
    }

    var updatedInstances = mapper.Map<List<BookInstance>>(bookInstancesDto);
        
    foreach (var instance in updatedInstances)
    {
      if (!instance.RowShelfId.HasValue) continue;
      var maxCol = await context.RowShelves
                                .Where(shelf => shelf.Id == instance.RowShelfId)
                                .Select(shelf => shelf.MaxCol)
                                .FirstOrDefaultAsync();
                
      var occupiedPositions = await context.BookInstances
                                           .Where(b => b.RowShelfId == instance.RowShelfId && b.Position.HasValue)
                                           .Select(b => b.Position.Value)
                                           .ToListAsync();
                
      var newPosition = Enumerable.Range(1, maxCol)
                                  .Except(occupiedPositions)
                                  .FirstOrDefault();
                
      instance.Position = newPosition > 0 ? newPosition : maxCol;
    }
        
    await context.BulkUpdateAsync(updatedInstances);
        
    var updatedDtos = mapper.Map<List<LibraryRoomDto.BookInstanceDto>>(updatedInstances);
    return Ok(new { Message = "Books placed on shelf successfully.", Success = true, Data = updatedDtos });
  }
  [HttpPut("delete-out-of-row-shelf")]
  public async Task<IActionResult> RemoveFromRowShelf([FromBody] List<int> bookInstanceIds)
  {
    if (bookInstanceIds == null || bookInstanceIds.Count == 0)
    {
      return BadRequest("List of book instance IDs cannot be empty.");
    }

    var bookInstances = await context.BookInstances
                                     .Where(b => bookInstanceIds.Contains(b.Id))
                                     .ToListAsync();
    if (!bookInstances.Any()) { return NotFound("No matching book instances found."); }

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
}