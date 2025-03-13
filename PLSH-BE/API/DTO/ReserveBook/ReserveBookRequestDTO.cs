namespace API.DTO.ReserveBook
{
    public class ReserveBookRequestDto
    {
        public int Id { get; set; }
        public int BookDetailId { get; set; } // Thay BookId thành BookDetailId
        public int AccountId { get; set; }
        public DateTime ReservationDate { get; set; }
        public int Status { get; set; } // 1: Đang đặt | 0: Đã hủy
    }
}
