using System.ComponentModel.DataAnnotations;

namespace Model.Entity.System;

public class ApiKey
{
  [Key]
  public int Id { get; set; }
  public string Key { get; set; }
}
