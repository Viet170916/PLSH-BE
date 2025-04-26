using System.Threading.Tasks;

namespace BU.Services.Interface;

public interface IBookInstanceService
{
  public Task AddBookInstances(int? bookId, int requiredQuantity);
  public Task AddBookInstancesIfNeeded(int? bookId, int requiredQuantity);
}
