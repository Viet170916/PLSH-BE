using System.ComponentModel.DataAnnotations;

namespace Model.Entity
{
  public class Resource
  {
    public int Id { get; set; }

    [MaxLength(10)]
    public required string Type { get; set; }

    [MaxLength(255)]
    public string? Name { get; set; }

    public long? SizeByte { get; set; }

    [MaxLength(20)]
    public string? FileType { get; set; }

    [MaxLength(255)]
    public string? LocalUrl { get; set; }
  }
}