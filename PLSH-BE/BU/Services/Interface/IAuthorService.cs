using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Entity.book;

namespace BU.Services.Interface;

public interface IAuthorService
{
  public Task<IList<Author>> AddIfAuthorsNameDoesntExistAsync(IList<string> authorNames);
}