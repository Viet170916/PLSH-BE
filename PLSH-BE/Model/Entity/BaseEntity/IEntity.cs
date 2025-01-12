namespace Model.Entity.BaseEntity
{
    public interface IEntity
    {
        DateTime? CreatedDate { get; set; }

        string CreatedBy { get; set; }

        DateTime? LastUpdatedDate { get; set; }

        string LastUpdatedBy { get; set; }
    }
}
