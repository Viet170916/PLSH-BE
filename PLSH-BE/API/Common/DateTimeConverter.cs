using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Common;

[ExcludeFromCodeCoverage]
public class DateTimeConverter : JsonConverter<DateTime>
{
  public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
  {
    return DateTime.Parse(reader.GetString());
  }

  public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
  {
    string jsonDateTimeFormat = DateTime.SpecifyKind(value, DateTimeKind.Utc)
                                        .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                                          System.Globalization.CultureInfo.InvariantCulture);
    writer.WriteStringValue(jsonDateTimeFormat);
  }

  public static int CalculateOverdueDays(
    DateTime borrowDate,
    List<DateTime> returnDates,
    List<DateTime> extendDates,
    DateTime actualReturnDate
  )
  {
    if (returnDates == null || returnDates.Count == 0) { return (actualReturnDate - borrowDate).Days; }

    returnDates.Sort();
    extendDates.Sort();
    int overdueDays = 0;
    DateTime lastReturnDate = borrowDate;
    foreach (var returnDate in returnDates)
    {
      DateTime? nextExtendDate = extendDates.FirstOrDefault(e => e > lastReturnDate && e <= returnDate);
      if (nextExtendDate != null) { overdueDays += Math.Max(0, (nextExtendDate.Value - lastReturnDate).Days); }
      else { overdueDays += Math.Max(0, (returnDate - lastReturnDate).Days); }

      lastReturnDate = returnDate;
    }

    overdueDays += Math.Max(0, (actualReturnDate - lastReturnDate).Days);
    return overdueDays;
  }
}
