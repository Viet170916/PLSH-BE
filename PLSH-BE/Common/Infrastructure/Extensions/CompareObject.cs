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

            //Compare objects  
            IEnumerable<Difference> differences;
            var isEqual = comparer.Compare(oldObject, newObject, out differences);

            //Print results  
            return isEqual ? string.Empty : string.Concat($"Action: {action}\n",string.Concat("Update Fields: ", 
                string.Join(Environment.NewLine, differences.Select(x => x.MemberPath.Split(".")[0]))));
        }
        public static T CloneObject(T objSource)
        {
            if (objSource != null)
            {
                //step : 1 Get the type of source object and create a new instance of that type
                Type typeSource = objSource.GetType();
                T objTarget = (T)Activator.CreateInstance(typeSource);
                //Step2 : Get all the properties of source object type
                PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                //Step : 3 Assign all source property to taget object 's properties
                foreach (PropertyInfo property in propertyInfo)
                {
                    //Check whether property can be written to
                    if (property.CanWrite)
                    {
                        //Step : 4 check whether property type is value type, enum or string type
                        if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                        {
                            property.SetValue(objTarget, property.GetValue(objSource, null), null);
                        }
                        //else property type is object/complex types, so need to recursively call this method until the end of the tree is reached
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
