using Common.Enums;

namespace Model.ResponseModel;

public class ErrorResponse
{
  public string Message { get; set; }
  public string StackTrace { get; set; }
  public HttpStatus StatusCode { get; set; }
  public string Status { get; set; }
  public object? Data { get; set; }
}