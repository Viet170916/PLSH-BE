using System.Collections.Generic;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;
using Model.Entity.LibraryRoom;
using LibraryRoomDto = Model.Entity.book.Dto.LibraryRoomDto;

namespace API.Controllers.LibRoomControllers;

public partial class LibraryRoomController
{
  [HttpPut("put-books-on-shelf")] public async Task<IActionResult> BulkUpdateRowShelves(
    [FromBody] List<LibraryRoomDto.RowShelfDto> rowShelfDtos
  )
  {
    if (rowShelfDtos == null || rowShelfDtos.Count == 0)
      return BadRequest(new { Message = "Danh sách RowShelf không hợp lệ." });
    using var transaction = await context.Database.BeginTransactionAsync();
    try
    {
      // 1️⃣ Lấy danh sách RowShelf hiện có trong DB
      var rowShelfIds = rowShelfDtos.Select(rs => rs.Id).ToList();
      var existingRowShelves = await context.RowShelves
                                            .Where(rs => rowShelfIds.Contains(rs.Id))
                                            .Include(rs => rs.BookInstances)
                                            .ToListAsync();

      // 2️⃣ Chuyển DTO → Entity bằng AutoMapper
      var rowShelfEntities = mapper.Map<List<RowShelf>>(rowShelfDtos);

      // 3️⃣ Bulk Insert or Update RowShelf
      await context.BulkInsertOrUpdateAsync(rowShelfEntities);

      // 4️⃣ Lấy danh sách BookInstance từ DB
      var bookInstanceIds = rowShelfDtos
                            .SelectMany(rs => rs.BookInstances ?? new List<LibraryRoomDto.BookInstanceDto>())
                            .Select(bi => bi.Id)
                            .ToList();
      var existingBookInstances = await context.BookInstances
                                               .Where(bi => bookInstanceIds.Contains(bi.Id))
                                               .ToListAsync();

      // 5️⃣ Chuyển đổi BookInstance DTO → Entity
      var bookInstancesToUpdate = mapper.Map<List<BookInstance>>(rowShelfDtos
                                                                 .SelectMany(rs =>
                                                                   rs.BookInstances ??
                                                                   new List<LibraryRoomDto.BookInstanceDto>())
                                                                 .ToList());

      // Cập nhật RowShelfId cho BookInstance
      foreach (var bookInstance in bookInstancesToUpdate)
      {
        var dto = rowShelfDtos
                  .SelectMany(rs => rs.BookInstances ?? new List<LibraryRoomDto.BookInstanceDto>())
                  .FirstOrDefault(bi => bi.Id == bookInstance.Id);
        if (dto != null) { bookInstance.RowShelfId = dto.RowShelfId; }
      }

      // 6️⃣ Xoá RowShelfId khỏi các BookInstance không còn trong request
      foreach (var existingBookInstance in existingBookInstances)
      {
        if (!bookInstancesToUpdate.Any(bi => bi.Id == existingBookInstance.Id))
        {
          existingBookInstance.RowShelfId = null;
          bookInstancesToUpdate.Add(existingBookInstance);
        }
      }

      // 7️⃣ Bulk Insert or Update BookInstance
      await context.BulkInsertOrUpdateAsync(bookInstancesToUpdate);
      await transaction.CommitAsync();
      return Ok(new
      {
        Message =
          "Cập nhật RowShelves thành công.",
      });
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync();
      return StatusCode(500, new { Message = $"Lỗi server: {ex.Message}", });
    }
  }
}