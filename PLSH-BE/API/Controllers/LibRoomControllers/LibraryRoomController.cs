using AutoMapper;
using Data.DatabaseContext;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Entity.LibraryRoom;

namespace API.Controllers.LibRoomControllers;
// [Authorize("LibrarianPolicy")]

[ApiController]
[Route("api/v1/library-room")]
public partial class LibraryRoomController(AppDbContext context, ILogger<LibraryRoomController> logger, IMapper mapper)
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
    return
      Ok(libraryRoom.FirstOrDefault());
  }

  [HttpPost("upsert")] public async Task<IActionResult> UpsertLibraryRoomState([FromBody] LibraryRoom request)
  {
    if (request is null) return BadRequest(new { Message = "Invalid data." });
    var existingRoom = await context.LibraryRooms
                                    .Include(r => r.Shelves)
                                    .FirstOrDefaultAsync();
    if (existingRoom is null)
    {
      var room = await context.LibraryRooms.AddAsync(request);
      await context.SaveChangesAsync();
      request.Shelves.ForEach(s => s.RoomId = room.Entity.Id);
      await context.BulkInsertAsync(request.Shelves);
    }
    else
    {
      existingRoom.ColumnSize = request.ColumnSize;
      existingRoom.RowSize = request.RowSize;
      var existingShelves = existingRoom.Shelves.ToDictionary(s => s.Id);
      foreach (var shelf in request.Shelves)
      {
        if (existingShelves.TryGetValue(shelf.Id, out var existingShelf))
        {
          existingShelf.X = shelf.X;
          existingShelf.Y = shelf.Y;
          existingShelf.Angle = shelf.Angle;
        }
        else
        {
          shelf.RoomId = existingRoom.Id;
          await context.Shelves.AddAsync(shelf);
        }
      }

      await context.SaveChangesAsync();
    }

    var newShelves = await context.Shelves
                                  .Where(s => s.RoomId == existingRoom.Id &&
                                              !context.RowShelves.Any(rs => rs.ShelfId == s.Id))
                                  .ToListAsync();
    foreach (var sh in newShelves.Where(sh => sh.Type is "SHELF_SMALL" or "SHELF_LARGE"))
    {
      context.RowShelves.Add(new RowShelf { MaxCol = 10, Position = 0, ShelfId = sh.Id, });
    }

    await context.SaveChangesAsync();
    return Ok(request);
  }
}
