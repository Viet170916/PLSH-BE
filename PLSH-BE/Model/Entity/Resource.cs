using System.ComponentModel.DataAnnotations;

namespace Model.Entity
{
  public class Resource
  {
    public int? Id { get; set; }

    [MaxLength(10)]
    public required string Type { get; set; } // Loại tài nguyên (image, pdf, audio)

    [MaxLength(255)]
    public string? Name { get; set; }

    public long? SizeByte { get; set; } // Kích thước file (byte)

    [MaxLength(20)]
    public string? FileType { get; set; } // Loại file (ví dụ: image/jpeg)

    [MaxLength(255)]
    public string? LocalUrl { get; set; }
  }
}