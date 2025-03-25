namespace API.DTO;

public class BaseResponse<TDataType>
{
  public string message { get; set; }
  public TDataType data { get; set; }
  public string? status { get; set; }
  public int? count { get; set; }
  public int? page { get; set; }
  public int? limit { get; set; }
  public int? currenPage { get; set; }
}