using System;
using System.Collections.Generic;
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
    public string Type { get; set; } // Loại tài nguyên (image, pdf, audio)
    public string? Name { get; set; }
    public long? SizeByte { get; set; } // Kích thước file (byte)
    public string? FileType { get; set; } // Loại file (ví dụ: image/jpeg)
    public string? LocalUrl { get; set; }
  }
}