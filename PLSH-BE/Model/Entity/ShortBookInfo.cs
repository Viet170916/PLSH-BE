using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
  public class ShortBookInfo
  {
    public int? Id { get; set; }

    [MaxLength(255)]
    public string? Title { get; set; }

    public int? PageCount { get; set; }

    [MaxLength(255)]
    public string? CoverUrl { get; set; }

    public DateTime? PublishDate { get; set; }
  }
}