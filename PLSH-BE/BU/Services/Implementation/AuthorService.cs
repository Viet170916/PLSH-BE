using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BU.Services.Interface;
using Data.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;

namespace BU.Services.Implementation;

public class AuthorService(AppDbContext context) : IAuthorService
{
  public async Task<IList<Author>> AddIfAuthorsNameDoesntExistAsync(IList<string> authorNames)
  {
    var existingAuthorNames = await context.Authors
                                           .Where(a => authorNames.Contains(a.FullName))
                                           .Select(a => a.FullName)
                                           .ToListAsync();

    var newAuthors = authorNames
                     .Where(name => !existingAuthorNames.Contains(name))
                     .Select(name => new Author { FullName = name })
                     .ToList();
    if (newAuthors.Count <= 0) return newAuthors;
    context.Authors.AddRange(newAuthors);
    await context.SaveChangesAsync();

    return newAuthors;
  }
}