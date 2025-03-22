using Data.DatabaseContext;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Entity.LibraryRoom;

namespace API.Controllers;

[ApiController]
[Route("api/v1/library-room")]
public class LibraryRoomController(AppDbContext context, ILogger<LibraryRoomController> logger)
  : Controller
{
  [HttpGet] public IActionResult Index()
  {
    var libraryRoom =
      from lib in context.LibraryRooms
      join sh in context.Shelves on lib.Id equals sh.RoomId
      group sh by new { lib.Id, ColumnSize = lib.ColumnSize, lib.RowSize, lib.Name } into groupedLib
      select new LibraryRoom
      {
        Id = groupedLib.Key.Id,
        ColumnSize = groupedLib.Key.ColumnSize,
        RowSize = groupedLib.Key.RowSize,
        Name = groupedLib.Key.Name,
        Shelves = groupedLib.ToList(),
      };

    // var shelves = context.Shelves.Where(s => );
    return
      Ok(libraryRoom.FirstOrDefault());
  }

  [HttpPost("upsert")] public async Task<IActionResult> UpsertLibraryRoomState([FromBody] LibraryRoom request)
  {
    if (request is null) return BadRequest("Invalid data.");
    var existingRoom = await context.LibraryRooms
                                    .FirstOrDefaultAsync();
    if (existingRoom is null)
    {
      var room = await context.LibraryRooms.AddAsync(request);
      await context.SaveChangesAsync(); // Đảm bảo ID được cập nhật
      request.Shelves.ForEach(s => { s.RoomId = room.Entity.Id; });
      await context.BulkInsertAsync(request.Shelves);
    }
    else
    {
      // Cập nhật thông tin phòng
      existingRoom.ColumnSize = request.ColumnSize;
      existingRoom.RowSize = request.RowSize;

      // Xóa Shelves cũ trước khi thêm mới
      await context.Shelves
                   .Where(s => s.RoomId == existingRoom.Id)
                   .ExecuteDeleteAsync();
      request.Shelves.ForEach(s => s.RoomId = existingRoom.Id);
      await context.BulkInsertAsync(request.Shelves);
    }

    var newShelves =
      from shelves in context.Shelves
      join row in context.RowShelves on shelves.Id equals row.ShelfId into grouped
      where !grouped.Any()
      select shelves;
    newShelves.ToList()
              .ForEach(sh =>
              {
                context.RowShelves.Add(new RowShelf() { MaxCol = 10, Position = 0, ShelfId = sh.Id, });
              });
    await context.SaveChangesAsync();
    return Ok(request);
  }

  [HttpGet("shelf/check")]
  public async Task<IActionResult> CheckShelfExists([FromQuery] int? id, [FromQuery] string? name)
  {
    if (id == null && string.IsNullOrEmpty(name)) return BadRequest("Please provide an 'id' or 'name' to check.");
    bool exists = await context.Shelves.AnyAsync(s =>
      (id != null && s.Id == id) ||
      (!string.IsNullOrEmpty(name) && s.Name == name));
    return Ok(new { exists, });
  }

  [HttpGet("shelf/{id:long}")] public async Task<IActionResult> GetShelfById(long id)
  {
    var shelf = await context.Shelves.FirstOrDefaultAsync(s => s.Id == id);
    if (shelf is null) return NotFound("Shelf not found.");
    var rowShelves = await context.RowShelves.Where(r => r.ShelfId == shelf.Id).ToListAsync();
    shelf.RowShelves = rowShelves;
    return Ok(shelf);
  }

  [HttpPost("shelf/{shelfId}/row/add")] [HttpPost("add")]
  public async Task<IActionResult> AddRowShelf(long shelfId, [FromBody] RowShelf request)
  {
    if (request is null) return BadRequest("Invalid data.");
    var shelf = await context.Shelves.FindAsync(shelfId);
    if (shelf is null) return NotFound("Shelf not found.");
    int maxPosition = await context.RowShelves
                                   .Where(r => r.ShelfId == shelfId)
                                   .OrderByDescending(r => r.Position)
                                   .Select(r => r.Position ?? -1)
                                   .FirstOrDefaultAsync();
    request.ShelfId = shelfId;
    request.MaxCol = 10;
    request.Position = maxPosition + 1;
    context.RowShelves.Add(request);
    await context.SaveChangesAsync();
    return Ok(request);
  }

  [HttpDelete("shelf/row/delete/{id}")] public async Task<IActionResult> DeleteRowShelf(long id)
  {
    var rowShelf = await context.RowShelves.FindAsync(id);
    if (rowShelf is null) return NotFound("RowShelf not found.");
    var books = context.BookInstances.Where(b => b.RowShelfId == id)
                       .ExecuteUpdateAsync(setter => setter.SetProperty(b => b.RowShelfId, (int?)null));
    context.RowShelves.Remove(rowShelf);
    await context.SaveChangesAsync();
    return Ok($"RowShelf {id} and related BookShelves deleted.");
  }

  [HttpPut("shelf/row/update/{id}")]
  public async Task<IActionResult> UpdateRowShelf(long id, [FromBody] RowShelf updatedRowShelf)
  {
    var rowShelf = await context.RowShelves.FindAsync(id);
    if (rowShelf is null) return NotFound("RowShelf not found.");
    rowShelf.Name = updatedRowShelf.Name;
    rowShelf.Description = updatedRowShelf.Description;
    rowShelf.Position = updatedRowShelf.Position;
    rowShelf.MaxCol = updatedRowShelf.MaxCol;
    await context.SaveChangesAsync();
    return Ok(rowShelf);
  }

  [HttpGet("shelf/row/has-books/{rowShelfId}")]
  public async Task<IActionResult> HasBooksInRowShelf(long rowShelfId)
  {
    var hasBooks = await context.BookInstances.AnyAsync(b => b.RowShelfId == rowShelfId);
    return Ok(new { rowShelfId, hasBooks });
  }
}