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
                                    .Include(r => r.Shelves)
                                    .FirstOrDefaultAsync();
    if (existingRoom is null)
    {
      // Thêm mới nếu chưa có phòng
      var room = await context.LibraryRooms.AddAsync(request);
      await context.SaveChangesAsync();
      request.Shelves.ForEach(s => s.RoomId = room.Entity.Id);
      await context.BulkInsertAsync(request.Shelves);
    }
    else
    {
      // Cập nhật kích thước phòng
      existingRoom.ColumnSize = request.ColumnSize;
      existingRoom.RowSize = request.RowSize;

      // Lấy danh sách các shelf hiện tại theo RoomId
      var existingShelves = existingRoom.Shelves.ToDictionary(s => s.Id);

      // Duyệt qua danh sách mới, nếu shelf đã tồn tại thì chỉ cập nhật vị trí x, y
      foreach (var shelf in request.Shelves)
      {
        if (existingShelves.TryGetValue(shelf.Id, out var existingShelf))
        {
          existingShelf.X = shelf.X;
          existingShelf.Y = shelf.Y;
        }
        else
        {
          shelf.RoomId = existingRoom.Id;
          await context.Shelves.AddAsync(shelf);
        }
      }

      await context.SaveChangesAsync();
    }

    // Kiểm tra các shelves chưa có RowShelves và thêm mới
    var newShelves = await context.Shelves
                                  .Where(s => s.RoomId == existingRoom.Id &&
                                              !context.RowShelves.Any(rs => rs.ShelfId == s.Id))
                                  .ToListAsync();
    foreach (var sh in newShelves)
    {
      context.RowShelves.Add(new RowShelf { MaxCol = 10, Position = 0, ShelfId = sh.Id });
    }

    await context.SaveChangesAsync();
    return Ok(request);
  }
}
