using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Model.helper;

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