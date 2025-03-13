using System.Runtime.Serialization;

namespace Common.Enums;

public enum HttpStatus
{
  [EnumMember(Value = "OK")] OK = 200,
  [EnumMember(Value = "NOT_FOUND")] NOT_FOUND = 404,
  [EnumMember(Value = "INTERNAL_ERROR")] INTERNAL_ERROR = 500,
  [EnumMember(Value = "UNAUTHORIZED")] UNAUTHORIZED = 401,
  [EnumMember(Value = "BAD_REQUEST")] BAD_REQUEST = 400,
  [EnumMember(Value = "FORBIDDEN")] FORBIDDEN = 403,
  [EnumMember(Value = "CONFLICT")] CONFLICT = 409,
}
