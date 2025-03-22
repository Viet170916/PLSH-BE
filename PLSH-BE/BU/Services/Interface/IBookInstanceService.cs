using System.Threading.Tasks;

namespace BU.Services.Interface;

public interface IBookInstanceService
{

  public Task AddBookInstancesIfNeeded(int? bookId, int requiredQuantity);
}