namespace Model.Entity.book
{
  public class PhysicalBook
  {
    public int Id { get; set; }
    public string? QrCode { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public double? Price { get; set; }
    public double? Fine { get; set; }
  }
}