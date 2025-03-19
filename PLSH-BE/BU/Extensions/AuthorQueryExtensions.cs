using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ExceptionTypes;
using Microsoft.EntityFrameworkCore;
using Model.Entity.book;

namespace BU.Extensions;

public static class AuthorQueryExtensions
{
  public static async Task<List<Author>> FindSimilarAuthors(this DbSet<Author> authors, string fullName)
  {
    var possibleMatches = await authors.FromSqlRaw(@"
            SELECT * FROM Authors 
            WHERE MATCH(FullName) AGAINST ({0} IN NATURAL LANGUAGE MODE)",
                                         fullName)
                                       .ToListAsync();
    return possibleMatches
           .Where(a => StringExtensions.LevenshteinDistance(a.FullName, fullName) <= 3)
           .ToList();
  }
}