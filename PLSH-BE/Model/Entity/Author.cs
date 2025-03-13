using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
    public class Author
    {
        public int? Id { get; set; } // ID tác giả (nếu có)
        public string FullName { get; set; } // Tên tác giả (nên là bắt buộc)
        public string AvatarUrl { get; set; } // URL ảnh đại diện
        public int ResourceAuthorId { get; set; } // Tài nguyên liên quan
        public string Description { get; set; } // Mô tả tác giả
        public string SummaryDescription { get; set; } // Mô tả ngắn gọn
        public string? BirthYear { get; set; } // Năm sinh
        public string? DeathYear { get; set; } // Năm mất
    }
}
