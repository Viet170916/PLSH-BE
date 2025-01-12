using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Model.Entity;

namespace Model.Entity.BaseEntity
{
    [ExcludeFromCodeCoverage]
    public abstract class Entity : IEntity
    {
        [Column(TypeName = "Datetime2")]
        public DateTime? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        [Column(TypeName = "Datetime2")]
        public DateTime? LastUpdatedDate { get; set; }

        public string LastUpdatedBy { get; set; }

        public void AuditCreated(string employeeId)
        {
            CreatedDate = DateTime.Now;
            CreatedBy = employeeId;
            LastUpdatedDate = DateTime.Now;
            LastUpdatedBy = employeeId;
        }
        public void AuditUpdated(string employeeId)
        {
            LastUpdatedDate = DateTime.Now;
            LastUpdatedBy = employeeId;
        }
    }
}
