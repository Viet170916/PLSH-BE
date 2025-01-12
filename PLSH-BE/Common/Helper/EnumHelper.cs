using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Common.Helper
{
    [ExcludeFromCodeCoverage]
    public static class EnumHelper
    {
        public static T GetValueFromDescription<T>(this string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                    {
                        return (T)field.GetValue(null);
                    }
                }
                else
                {
                    if (field.Name == description)
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }
            return default(T);
        }

        public static IEnumerable<ValueName> GetItems<TEnum>()
        where TEnum : struct, IConvertible, IComparable, IFormattable
        {
         
            var res = from e in Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
                      select new ValueName() { Id = Convert.ToInt32(e), Description = e.GetDescription() };
            return res;
        }

        public static string GetDescription<T>(this T enumValue)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                return null;
            }

            var description = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    description = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return description;
        }
        public static T ToEnum<T>(this string @this)
        {
            Type enumType = typeof(T);
            return (T)Enum.Parse(enumType, @this);
        }
        public static List<T> EnumToList<T>()
        {
            Type enumType = typeof(T);

            if (enumType.BaseType != typeof(Enum))
            {
                return null;
            }

            Array enumValArray = Enum.GetValues(enumType);

            List<T> enumValList = new List<T>(enumValArray.Length);

            foreach (int val in enumValArray)
            {
                enumValList.Add((T)Enum.Parse(enumType, val.ToString()));
            }

            return enumValList;
        }
    }
    [ExcludeFromCodeCoverage]
    public struct ValueName : IEquatable<ValueName>
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool Equals(ValueName other)
        {
            return false;
            // throw new NotImplementedException();
        }
    }
}
