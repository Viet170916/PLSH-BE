using System;
using System.Diagnostics.CodeAnalysis;

namespace Model.Entity.BaseEntity
{
  [ExcludeFromCodeCoverage]
  public abstract class AuditableEntity<T> : IAuditableEntity
  {
    public T Id { get; set; }
    public DateTime? CreateDate { get; set; }
    public string CreateBy { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string UpdateBy { get; set; }
    public DateTime? DeleteDate { get; set; }
    public string DeleteBy { get; set; }
    public bool? DeleteFlag { get; set; }
  }
}