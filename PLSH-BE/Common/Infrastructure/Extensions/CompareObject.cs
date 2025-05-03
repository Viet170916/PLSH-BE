using ObjectsComparer;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Common.Infrastructure.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class CompareObject<T>
    {

        public static string ConcatDefferentValue(T oldObject, T newObject, string action)
        {
            var comparer = new ObjectsComparer.Comparer<T>();

            IEnumerable<Difference> differences;
            var isEqual = comparer.Compare(oldObject, newObject, out differences);

            return isEqual ? string.Empty : string.Concat($"Action: {action}\n",string.Concat("Update Fields: ",
                string.Join(Environment.NewLine, differences.Select(x => x.MemberPath.Split(".")[0]))));
        }
        public static T CloneObject(T objSource)
        {
            if (objSource != null)
            {
                Type typeSource = objSource.GetType();
                T objTarget = (T)Activator.CreateInstance(typeSource);
                PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (PropertyInfo property in propertyInfo)
                {
                    if (property.CanWrite)
                    {
                        if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                        {
                            property.SetValue(objTarget, property.GetValue(objSource, null), null);
                        }
                        else
                        {
                            T objPropertyValue = (T)property.GetValue(objSource, null);
                            if (objPropertyValue == null)
                            {
                                property.SetValue(objTarget, null, null);
                            }
                            else
                            {
                                property.SetValue(objTarget, CloneObject(objPropertyValue), null);
                            }
                        }
                    }
                }
                return objTarget;
            }
            return objSource;
        }
    }
}
