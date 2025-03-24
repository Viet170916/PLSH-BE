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
  public async Task<IActionResult> UpdateBookInstances([FromBody] List<LibraryRoomDto.BookInstanceDto> bookInstancesDto)
  {
    if (bookInstancesDto == null || bookInstancesDto.Count == 0)
    {
      return BadRequest("List of book instances cannot be empty.");
    }

    var updatedInstances = mapper.Map<List<BookInstance>>(bookInstancesDto);
    await context.BulkUpdateAsync(updatedInstances);
    return Ok(new { Message = "Book instances updated successfully.", Success = true, });
  }
}