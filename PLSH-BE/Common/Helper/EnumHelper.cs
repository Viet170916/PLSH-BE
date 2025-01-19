using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using Common.Enums;

namespace Common.Helper;

public static class EnumHelper
{
  public static string GetDescription<T>(this T value) where T : Enum
  {
    var type = value.GetType();
    var memberInfo = type.GetMember(value.ToString());
    if (memberInfo.Length > 0)
    {
      var attribute = memberInfo[0].GetCustomAttribute<EnumMemberAttribute>();
      if (attribute != null) { return attribute.Value ?? ""; }
    }

    return value.ToString();
  }
}