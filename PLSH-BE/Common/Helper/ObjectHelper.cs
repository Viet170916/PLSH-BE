using System.Dynamic;

namespace Common.Helper;

using System.Reflection;

public static class ObjectHelper
{
  public static object? RemoveNullProperties<T>(this T? obj) where T : class, new()
  {
    if (obj == null) return null;
    IDictionary<string, object> expandoObject = new ExpandoObject()!;
    var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
    foreach (var property in properties)
    {
      if (!property.CanRead) continue;
      var value = property.GetValue(obj);
      if (value != null)
      {
        if (IsComplexType(property.PropertyType)) { expandoObject[property.Name] = RemoveNullProperties(value)!; }
        else { expandoObject[property.Name] = value; }
      }
    }

    return expandoObject;
  }

  private static bool IsComplexType(Type type) { return type.IsClass && type != typeof(string); }
}