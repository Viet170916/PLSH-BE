using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Model.Entity
{
    public class Resource
    {
        public int? Id { get; set; }
        public ResourceType Type { get; set; } // Loại tài nguyên (image, pdf, audio)
        public string? Name { get; set; }
        public long? SizeByte { get; set; } // Kích thước file (byte)
        public string? FileType { get; set; } // Loại file (ví dụ: image/jpeg)
        public string? LocalUrl { get; set; } // URL local
        public byte[] File { get; set; } // File dữ liệu (dạng byte array)

    }
}
