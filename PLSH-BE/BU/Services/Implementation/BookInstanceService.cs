using BU.Services.Interface;
using Data.DatabaseContext;
using Model.Entity.book;

namespace BU.Services.Implementation;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class BookInstanceService(AppDbContext context) : IBookInstanceService
{
  public async Task AddBookInstancesIfNeeded(int? bookId, int requiredQuantity)
  {
    if (bookId is not null)
    {
      var currentCount = await context.BookInstances.CountAsync(bi => bi.BookId == bookId);
      var additionalCount = requiredQuantity - currentCount;
      if (additionalCount > 0)
      {
        var newInstances = Enumerable.Range(0, additionalCount)
                                     .Select(_ => new BookInstance { BookId = (int)bookId, Code = GenerateCode(), })
                                     .ToList();
        context.BookInstances.AddRange(newInstances);
        await context.SaveChangesAsync();
      }
    }
  }

  private static string GenerateCode() { return new Random().Next(1000000000, int.MaxValue).ToString(); }
}