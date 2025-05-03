#nullable enable
namespace BU.Models.DTO;

public class BaseResponse<TDataType>
{
  public string Message { get; set; }
  public TDataType Data { get; set; }
  public string? Status { get; set; }
  public int? Count { get; set; }
  public int? Page { get; set; }
  public int? Limit { get; set; }
  public int? CurrentPage { get; set; }
  public int? PageCount { get; set; }

}
