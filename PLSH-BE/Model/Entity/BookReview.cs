using static Common.Library.Constants;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Model.Entity
{
  public class BookReview
  {
    public int Id { get; set; }
    public int BookId { get; set; }
    public int AccountId { get; set; }
    public int Rating { get; set; }

    [MaxLength(255)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}