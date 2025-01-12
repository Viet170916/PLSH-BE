using System;

namespace Model.Entity.BaseEntity
{
    public interface IAuditableEntity
    {
        DateTime? CreateDate { get; set; }

        string CreateBy { get; set; }

        DateTime? UpdateDate { get; set; }

        string UpdateBy { get; set; }

        public DateTime? DeleteDate { get; set; }

        public string DeleteBy { get; set; }

        public bool? DeleteFlag { get; set; }
    }
}
