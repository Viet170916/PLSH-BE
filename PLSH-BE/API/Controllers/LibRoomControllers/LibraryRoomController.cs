using AutoMapper;
using Data.DatabaseContext;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Model.Entity.LibraryRoom;

namespace API.Controllers.LibRoomControllers;

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
                                    .FirstOrDefaultAsync();
    if (existingRoom is null)
    {
      var room = await context.LibraryRooms.AddAsync(request);
      await context.SaveChangesAsync();
      request.Shelves.ForEach(s => { s.RoomId = room.Entity.Id; });
    }
    else
    {
      existingRoom.ColumnSize = request.ColumnSize;
      existingRoom.RowSize = request.RowSize;
      await context.Shelves
                   .Where(s => s.RoomId == existingRoom.Id)
                   .ExecuteDeleteAsync();
      request.Shelves.ForEach(s => s.RoomId = existingRoom.Id);
    }

    await context.BulkInsertAsync(request.Shelves);
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
}