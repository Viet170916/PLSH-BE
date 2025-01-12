using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.SqlServer.Management.SqlParser.Metadata;

namespace BU.Utils
{
  [ExcludeFromCodeCoverage]
  public static class Helpers
  {
    public static DataTable ToDataTable<T>(this IEnumerable<T> values)
    {
      var table = new DataTable { Locale = CultureInfo.InvariantCulture };
      table.Columns.Add("Item", typeof(T));
      if (values != null)
      {
        foreach (var value in values) { table.Rows.Add(value); }
      }

      return table;
    }

    public static Tuple<DataTable, List<int>> ToDataTableExport<T>(this IEnumerable<T> data, string sheetName)
    {
      PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
      DataTable table = new DataTable();
      table.Locale = CultureInfo.InvariantCulture;
      table.TableName = sheetName;
      var list = new List<int>();
      for (int i = 0; i < props.Count; i++)
      {
        PropertyDescriptor prop = props[i];
        if (prop.PropertyType == typeof(decimal)) { list.Add(i + 1); }

        table.Columns.Add(prop.Description, prop.PropertyType);
      }

      object[] values = new object[props.Count];
      if (data != null)
      {
        foreach (T item in data)
        {
          for (int i = 0; i < values.Length; i++) { values[i] = props[i].GetValue(item); }

          table.Rows.Add(values);
        }
      }

      return new Tuple<DataTable, List<int>>(table, list);
    }

    public static Tuple<DataTable, List<int>> ToDataTableEntityExport<T>(this IEnumerable<T> data, string sheetName)
    {
      PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
      DataTable table = new DataTable();
      table.Locale = CultureInfo.InvariantCulture;
      table.TableName = sheetName;
      var list = new List<int>();
      for (int i = 0; i < props.Count; i++)
      {
        PropertyDescriptor prop = props[i];
        if (prop.PropertyType == typeof(decimal)) { list.Add(i + 1); }

        table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
      }

      object[] values = new object[props.Count];
      if (data != null)
      {
        foreach (T item in data)
        {
          for (int i = 0; i < values.Length; i++) { values[i] = props[i].GetValue(item); }

          table.Rows.Add(values);
        }
      }

      return new Tuple<DataTable, List<int>>(table, list);
    }

    public static Tuple<DataTable, List<int>> ToDataTableEntityExportBudgetOverView<T>(
      this IEnumerable<T> data,
      string sheetName,
      bool isCheckPage
    )
    {
      var status = new string[] { "Status" };
      var toRemoveColum = new string[]
      {
        "Planned Revenue @ Realisation Jan", "Planned Revenue @ Realisation Feb", "Planned Revenue @ Realisation Mar",
        "Planned Revenue @ Realisation Apr", "Planned Revenue @ Realisation May", "Planned Revenue @ Realisation Jun",
        "Planned Revenue @ Realisation Jul", "Planned Revenue @ Realisation Aug", "Planned Revenue @ Realisation Sep",
        "Planned Revenue @ Realisation Oct", "Planned Revenue @ Realisation Nov", "Planned Revenue @ Realisation Dec",
        "Planned Billings Jan (SGD)", "Planned Billings Feb (SGD)", "Planned Billings Mar (SGD)",
        "Planned Billings Apr (SGD)", "Planned Billings May (SGD)", "Planned Billings Jun (SGD)",
        "Planned Billings Jul (SGD)", "Planned Billings Aug (SGD)", "Planned Billings Sep (SGD)",
        "Planned Billings Oct (SGD)", "Planned Billings Nov (SGD)", "Planned Billings Dec (SGD)",
        "Last year of Audit (to exclude from APIC budget)"
      };
      PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
      DataTable table = new DataTable();
      table.Locale = CultureInfo.InvariantCulture;
      table.TableName = sheetName;
      var list = new List<int>();
      for (int i = 0; i < props.Count; i++)
      {
        PropertyDescriptor prop = props[i];
        if (prop.PropertyType == typeof(decimal)) { list.Add(i + 1); }

        table.Columns.Add(prop.Description, prop.PropertyType);
      }

      object[] values = new object[props.Count];
      if (data != null)
      {
        foreach (T item in data)
        {
          for (int i = 0; i < values.Length; i++) { values[i] = props[i].GetValue(item); }

          table.Rows.Add(values);
        }
      }

      if (!isCheckPage)
      {
        foreach (var col in toRemoveColum) { table.Columns.Remove(col); }
      }
      else
      {
        foreach (var col in status) { table.Columns.Remove(col); }
      }

      return new Tuple<DataTable, List<int>>(table, list);
    }

    public static DataTable ToUserDefinedDataTable<T>(this IEnumerable<T> values) where T : IUserDefinedType
    {
      var table = new DataTable { Locale = CultureInfo.InvariantCulture };
      var properties = typeof(T).GetProperties();
      foreach (var prop in properties)
      {
        table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
      }

      if (values == null) { return table; }

      foreach (var value in values)
      {
        var newRow = table.NewRow();
        foreach (var prop in properties)
        {
          if (table.Columns.Contains(prop.Name)) { newRow[prop.Name] = prop.GetValue(value, null) ?? DBNull.Value; }
        }

        table.Rows.Add(newRow);
      }

      return table;
    }

    public static bool HasColumn(this IDataRecord dr, string columnName)
    {
      for (var i = 0; i < dr?.FieldCount; i++)
      {
        if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase)) { return true; }
      }

      return false;
    }

    /// <summary>
    /// Check data is not null and has value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns>return true if data is sastified conditions else return false</returns>
    public static bool IsAny<T>(this IEnumerable<T> data) { return data != null && data.Any(); }
  }
}