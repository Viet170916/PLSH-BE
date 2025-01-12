using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Common.Infrastructure.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class EnumExtensions
    {
        public static string GetDisplayName<T>(this T enumValue)
        {
            return enumValue.GetType()
              .GetMember(enumValue.ToString())
              .First()
              .GetCustomAttribute<DisplayAttribute>()
              ?.GetName();
        }
    }
}
