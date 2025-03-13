using System.Collections.Generic;

namespace API.DTO.Borrower
{
    public class BorrowerDto
    {
        public int Id { get; set; }
        // public string FullName { get; set; }
        public DateOnly BirthDate { get; set; }
        public string RoleInSchool { get; set; }//(student or teacher)
        public string? ClassRoom { get; set; }//if student
        public int AccountId { get; set; }

        public int FavoriteId { get; set; }
        public int LoanId { get; set; }

    }
}
