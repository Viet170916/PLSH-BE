// using API.DTO.Book;
// using Data.DatabaseContext;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using Model.ResponseModel;
// using Microsoft.EntityFrameworkCore;
// using System.IO;
// using Tesseract;
// using System.Net.Http;
// using Newtonsoft.Json.Linq;
// using QRCoder;
// using System.Drawing;
// using Newtonsoft.Json;
// using System.Text;
// using Common.Enums;
// using Common.Helper;
// using Model.Entity;
// using Model.Entity.book;
//
// namespace API.Controllers
// {
//     public class LibraryController(AppDbContext context, ILogger<ManageAccountController> logger)
//         : ControllerBase
//     {
//         private readonly HttpClient _httpClient = new();
//         //private readonly string _tesseractDataPath = @"./tessdata"; // Đường dẫn thư mục dữ liệu Tesseract
//
//         //Thêm mới Category
//         [HttpPost("addCategory")]
//         public async Task<IActionResult> AddCategory([FromBody] CreateCategoryDTO categoryDto)
//         {
//             if (!ModelState.IsValid)
//             {
//                 return BadRequest(ModelState);
//             }
//
//             // Tạo đối tượng Category từ DTO
//             var category = new Category
//             {
//                 Name = categoryDto.Name,
//                 Description = categoryDto.Description,
//                 CreatedDate = DateTime.UtcNow,
//                 Status = CategoryStatus.Active
//             };
//
//             // Thêm vào DB
//             context.Categories.Add(category);
//             await context.SaveChangesAsync();
//
//             // Trả về kết quả thành công
//             return Ok(new { message = "Category created successfully", categoryId = category.Id });
//         }
//
//
//         //Thêm sach mới
//         [HttpPost("add")]
//         public async Task<IActionResult> AddBook([FromBody] CreateBookDto bookDto)
//         {
//             if (!ModelState.IsValid)
//             {
//                 return BadRequest(ModelState);
//             }
//
//             // Kiểm tra danh mục có tồn tại không
//             var category = await context.Categories.FindAsync(bookDto.CategoryId);
//             if (category == null)
//             {
//                 return NotFound(new { message = "Category not found." });
//             }
//
//             // Tạo đối tượng sách từ BookDTO
//             var book = new Book
//             {
//                 Title = bookDto.Title,
//                 Description = bookDto.Description,
//                 //Author = bookDto.Author,
//                 Publisher = bookDto.Publisher,
//                 PublishDate = bookDto.PublishDate,
//                 Language = bookDto.Language,
//                 //Position = bookDto.Position,
//                 PageCount = bookDto.PageCount,
//                 CategoryId = bookDto.CategoryId,
//                 IsbNumber13 = bookDto.ISBNumber,
//                 TotalCopies = bookDto.TotalCopies,
//                 AvailableCopies = bookDto.AvailableCopies,
//                 Price = bookDto.Price,
//                 Thumbnail = bookDto.Thumbnail,
//                 Fine = bookDto.Fine,
//                 CreateDate = DateTime.UtcNow,
//                 UpdateDate = null,
//                 DeletedAt = null
//             };
//
//             // Sinh QR Code và lưu base64 vào sách
//             var qrContent = $"Title: {book.Title}, " +
//                 //$"Author: {book.Author}, " +
//                 $"ISBNumber: {book.IsbNumber13}";
//             if (!string.IsNullOrEmpty(qrContent))
//             {
//                 var qrGenerator = new QRCodeGenerator();
//                 var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.L);
//                 var qrCode = new QRCode(qrCodeData);
//                 using (var qrCodeImage = qrCode.GetGraphic(10))
//                 using (var memoryStream = new MemoryStream())
//                 {
//                     qrCodeImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
//                    // book.QRCode = Convert.ToBase64String(memoryStream.ToArray());
//                 }
//             }
//
//             // Thêm sách vào database
//             context.Books.Add(book);
//             await context.SaveChangesAsync();
//
//             return Ok(new { message = "Book added successfully."
//                 //, QRCode = book.QRCode 
//             });
//         }
//
//         ////Chuyển hóa Base64 sang QRCode
//         //[HttpPost("generate")]
//         //public IActionResult GenerateQrCode([FromBody] string base64String)
//         //{
//         //    if (string.IsNullOrWhiteSpace(base64String))
//         //    {
//         //        return BadRequest("Base64 string is required.");
//         //    }
//
//         //    try
//         //    {
//         //        QRCodeGenerator qrGenerator = new QRCodeGenerator();
//         //        QRCodeData qrCodeData = qrGenerator.CreateQrCode(base64String, QRCodeGenerator.ECCLevel.L);
//         //        QRCode qrCode = new QRCode(qrCodeData);
//         //        using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
//         //        {
//         //            using (MemoryStream stream = new MemoryStream())
//         //            {
//         //                qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
//         //                var imageBytes = stream.ToArray();
//         //                var imageBase64 = Convert.ToBase64String(imageBytes);
//         //                return Ok(new { QRCode = imageBase64 });
//         //            }
//         //        }
//         //    }
//         //    catch (Exception ex)
//         //    {
//         //        return StatusCode(500, $"Internal server error: {ex.Message}");
//         //    }
//         //}
//         public class GenerateQrRequest
//         {
//             public string Title { get; set; }
//             public string Author { get; set; }
//             public string IsbNumber { get; set; }
//         }
//
//         [HttpPost("generate")]
//         public IActionResult GenerateQrCode([FromBody] GenerateQrRequest request)
//         {
//             if (request == null)
//             {
//                 return BadRequest("Request data is required.");
//             }
//
//             var qrContent = $"Title: {request.Title}, Author: {request.Author}, ISBNumber: {request.IsbNumber}";
//
//             try
//             {
//                 var qrGenerator = new QRCodeGenerator();
//                 var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.L);
//                 var qrCode = new QRCode(qrCodeData);
//                 using (var qrCodeImage = qrCode.GetGraphic(10))
//                 using (var memoryStream = new MemoryStream())
//                 {
//                     qrCodeImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
//                     var imageBase64 = Convert.ToBase64String(memoryStream.ToArray());
//                     return Ok(new { QRCode = imageBase64 });
//                 }
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Internal server error: {ex.Message}");
//             }
//         }
//
//         //Thêm sách mới bằng (Barcode/ÍBN)
//         [HttpPost("add-by-isbn/{isbn}")]
//         public async Task<IActionResult> AddBookByISBN(string isbn)
//         {
//             try
//             {
//                 if (string.IsNullOrEmpty(isbn))
//                 {
//                     return BadRequest(new ErrorResponse
//                     {
//                         Message = "ISBN không hợp lệ.",
//                         StackTrace = null,
//                         StatusCode = HttpStatus.BAD_REQUEST,
//                         Status = HttpStatus.BAD_REQUEST.ToString(),
//                         Data = null
//                     });
//                 }
//
//                 // 🔍 Gọi API Open Library để lấy thông tin sách
//                 string apiUrl = $"https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data";
//                 var response = await _httpClient.GetAsync(apiUrl);
//
//                 if (!response.IsSuccessStatusCode)
//                 {
//                     return NotFound(new ErrorResponse
//                     {
//                         Message = "Không tìm thấy thông tin sách với ISBN này.",
//                         StackTrace = null,
//                         StatusCode = HttpStatus.NOT_FOUND,
//                         Status = HttpStatus.NOT_FOUND.ToString(),
//                         Data = null
//                     });
//                 }
//
//                 var jsonResponse = await response.Content.ReadAsStringAsync();
//                 var bookData = JObject.Parse(jsonResponse)?[$"ISBN:{isbn}"];
//
//                 if (bookData == null)
//                 {
//                     return NotFound(new ErrorResponse
//                     {
//                         Message = "Không tìm thấy dữ liệu sách.",
//                         StackTrace = null,
//                         StatusCode = HttpStatus.NOT_FOUND,
//                         Status = HttpStatus.NOT_FOUND.ToString(),
//                         Data = null
//                     });
//                 }
//
//                 // 📖 Trích xuất thông tin sách từ JSON
//                 var bookTitle = bookData["title"]?.ToString() ?? "Không có tiêu đề";
//                 var bookAuthor = bookData["authors"]?[0]?["name"]?.ToString() ?? "Không rõ tác giả";
//                 var bookPublisher = bookData["publishers"]?[0]?["name"]?.ToString() ?? "Không rõ nhà xuất bản";
//                 var bookPublishDate = bookData["publish_date"]?.ToString();
//                 var bookThumbnail = bookData["cover"]?["medium"]?.ToString() ?? "";
//
//                 // 🛠 Chuyển đổi ngày xuất bản
//                 DateTime? publishDate = null;
//                 if (DateTime.TryParse(bookPublishDate, out DateTime parsedDate))
//                 {
//                     publishDate = parsedDate;
//                 }
//
//                 // 🆕 Tạo sách mới
//                 var newBook = new Book
//                 {
//                     Title = bookTitle,
//                     //Author = bookAuthor,
//                     Publisher = bookPublisher,
//                     PublishDate = publishDate,
//                     IsbNumber13 = isbn,
//                     Thumbnail = bookThumbnail,
//                     CreateDate = DateTime.Now
//                 };
//
//                 // 📌 Thêm sách vào database
//                 context.Books.Add(newBook);
//                 await context.SaveChangesAsync();
//
//                 return Ok(new OkResponse
//                 {
//                     Message = "Thêm sách bằng ISBN thành công.",
//                     StackTrace = null,
//                     StatusCode = HttpStatus.OK,
//                     Status = HttpStatus.OK.ToString(),
//                     Data = newBook
//                 });
//             }
//             catch (Exception ex)
//             {
//                 logger.LogError("Lỗi khi thêm sách từ ISBN: " + ex.Message);
//                 return StatusCode(500, new ErrorResponse
//                 {
//                     Message = "Lỗi khi xử lý yêu cầu ISBN.",
//                     StackTrace = ex.StackTrace,
//                     StatusCode = HttpStatus.INTERNAL_ERROR,
//                     Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                     Data = null
//                 });
//             }
//         }
//
//         //Thêm sách mới bằng (OCR)
//         [HttpPost("add-by-ocr")]
//         //public async Task<IActionResult> AddBookByOCR(IFormFile image)
//         //{
//         //    try
//         //    {
//         //        if (image == null || image.Length == 0)
//         //        {
//         //            return BadRequest(new ErrorResponse
//         //            {
//         //                Message = "Không có hình ảnh được tải lên.",
//         //                StackTrace = null,
//         //                StatusCode = HttpStatus.BAD_REQUEST,
//         //                Status = HttpStatus.BAD_REQUEST.ToString(),
//         //                Data = null
//         //            });
//         //        }
//
//         //        // Lưu file ảnh tạm thời
//         //        var tempFilePath = Path.GetTempFileName();
//         //        using (var stream = new FileStream(tempFilePath, FileMode.Create))
//         //        {
//         //            await image.CopyToAsync(stream);
//         //        }
//
//         //        // Sử dụng Tesseract OCR để nhận diện văn bản từ hình ảnh
//         //        string ocrResult;
//         //        try
//         //        {
//         //            using (var engine = new TesseractEngine(_tesseractDataPath, "eng", EngineMode.Default))
//         //            {
//         //                using (var img = Pix.LoadFromFile(tempFilePath))
//         //                {
//         //                    using (var page = engine.Process(img))
//         //                    {
//         //                        ocrResult = page.GetText();
//         //                    }
//         //                }
//         //            }
//         //        }
//         //        catch (Exception ocrEx)
//         //        {
//         //            _logger.LogError("Lỗi OCR: " + ocrEx.Message);
//         //            return StatusCode(500, new ErrorResponse
//         //            {
//         //                Message = "Lỗi OCR khi xử lý hình ảnh.",
//         //                StackTrace = ocrEx.StackTrace,
//         //                StatusCode = HttpStatus.INTERNAL_ERROR,
//         //                Status = HttpStatus.INTERNAL_ERROR.ToString(),
//         //                Data = null
//         //            });
//         //        }
//         //        finally
//         //        {
//         //            // Xóa file tạm
//         //            if (System.IO.File.Exists(tempFilePath))
//         //            {
//         //                System.IO.File.Delete(tempFilePath);
//         //            }
//         //        }
//
//         //        // Giả lập phân tích văn bản OCR để lấy thông tin sách
//         //        var bookTitle = "Extracted Title from OCR";
//         //        var bookAuthor = "Extracted Author from OCR";
//         //        var bookDescription = ocrResult;
//
//         //        var newBook = new Book
//         //        {
//         //            Title = bookTitle,
//         //            Author = bookAuthor,
//         //            Description = bookDescription,
//         //            CreateDate = DateTime.Now
//         //        };
//
//         //        _context.Books.Add(newBook);
//         //        await _context.SaveChangesAsync();
//
//         //        return Ok(new OkResponse
//         //        {
//         //            Message = "Thêm sách từ OCR thành công.",
//         //            StackTrace = null,
//         //            StatusCode = HttpStatus.OK,
//         //            Status = HttpStatus.OK.ToString(),
//         //            Data = newBook
//         //        });
//         //    }
//         //    catch (Exception ex)
//         //    {
//         //        _logger.LogError("Lỗi khi thêm sách từ OCR: " + ex.Message);
//         //        return StatusCode(500, new ErrorResponse
//         //        {
//         //            Message = "Lỗi trong quá trình xử lý OCR.",
//         //            StackTrace = ex.StackTrace,
//         //            StatusCode = HttpStatus.INTERNAL_ERROR,
//         //            Status = HttpStatus.INTERNAL_ERROR.ToString(),
//         //            Data = null
//         //        });
//         //    }
//         //}
//
//         //Chỉnh sửa thông tin sách(Update)
//         [HttpPut("update/{id}")]
//         public async Task<IActionResult> UpdateBook(int id, [FromBody] BookDto bookDTO)
//         {
//             try
//             {
//                 var existingBook = await context.Books
//                     //.Include(c => c.Category)
//                     .FirstOrDefaultAsync(b => b.Id == id);
//                 if (existingBook == null)
//                 {
//                     return NotFound(new ErrorResponse
//                     {
//                         Message = "Không tìm thấy sách.",
//                         StackTrace = null,
//                         StatusCode = HttpStatus.NOT_FOUND,
//                         Status = HttpStatus.NOT_FOUND.ToString(),
//                         Data = null
//                     });
//                 }
//
//                 // Cập nhật thông tin sách
//                 existingBook.Title = bookDTO.Title ?? existingBook.Title;
//                 existingBook.Description = bookDTO.Description ?? existingBook.Description;
//                 //existingBook.Author = bookDTO.Author ?? existingBook.Author;
//                 existingBook.Publisher = bookDTO.Publisher ?? existingBook.Publisher;
//                 existingBook.PublishDate = bookDTO.PublishDate ?? existingBook.PublishDate;
//                 existingBook.Language = bookDTO.Language ?? existingBook.Language;
//                 //existingBook.Position = bookDTO.Position ?? existingBook.Position;
//                 existingBook.PageCount = bookDTO.PageCount != 0 ? bookDTO.PageCount : existingBook.PageCount;
//                 existingBook.IsbNumber13 = bookDTO.ISBNumber ?? existingBook.IsbNumber13;
//                 existingBook.TotalCopies = bookDTO.TotalCopies != 0 ? bookDTO.TotalCopies : existingBook.TotalCopies;
//                 existingBook.AvailableCopies = bookDTO.AvailableCopies != 0 ? bookDTO.AvailableCopies : existingBook.AvailableCopies;
//                 existingBook.Price = bookDTO.Price ?? existingBook.Price;
//                 existingBook.Thumbnail = bookDTO.Thumbnail ?? existingBook.Thumbnail;
//                 existingBook.Fine = bookDTO.Fine ?? existingBook.Fine;
//                 existingBook.UpdateDate = DateTime.Now;
//
//                 // Cập nhật danh mục sách nếu có thay đổi
//                 if (bookDTO.CategoryId != 0 && bookDTO.CategoryId != existingBook.CategoryId)
//                 {
//                     var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == bookDTO.CategoryId);
//                     if (category == null)
//                     {
//                         return BadRequest(new ErrorResponse
//                         {
//                             Message = "Danh mục không hợp lệ.",
//                             StackTrace = null,
//                             StatusCode = HttpStatus.BAD_REQUEST,
//                             Status = HttpStatus.BAD_REQUEST.ToString(),
//                             Data = null
//                         });
//                     }
//                     existingBook.CategoryId = bookDTO.CategoryId;
//                     //existingBook.Category = category;
//                 }
//
//                 context.Books.Update(existingBook);
//                 await context.SaveChangesAsync();
//
//                 // Trả về thông tin sách sau khi cập nhật
//                 var updatedBookDTO = new BookDto
//                 {
//                     Id = existingBook.Id,
//                     Title = existingBook.Title,
//                     Description = existingBook.Description,
//                   //  Author = existingBook.Author,
//                     Publisher = existingBook.Publisher,
//                     PublishDate = existingBook.PublishDate,
//                     Language = existingBook.Language,
//                    // Position = existingBook.Position,
//                     PageCount = existingBook.PageCount,
//                     CategoryId = existingBook.CategoryId,
//                     //CategoryName = existingBook.Category?.Name, // Lấy tên danh mục
//                     ISBNumber = existingBook.IsbNumber13,
//                     TotalCopies = existingBook.TotalCopies,
//                     AvailableCopies = existingBook.AvailableCopies,
//                     Price = existingBook.Price,
//                     Thumbnail = existingBook.Thumbnail,
//                     Fine = existingBook.Fine,
//                     CreateDate = existingBook.CreateDate,
//                     UpdateDate = existingBook.UpdateDate
//                 };
//
//                 return Ok(new OkResponse
//                 {
//                     Message = "Cập nhật sách thành công.",
//                     StackTrace = null,
//                     StatusCode = HttpStatus.OK,
//                     Status = HttpStatus.OK.ToString(),
//                     Data = updatedBookDTO
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ErrorResponse
//                 {
//                     Message = "Lỗi trong quá trình xử lý yêu cầu.",
//                     StackTrace = ex.StackTrace,
//                     StatusCode = HttpStatus.INTERNAL_ERROR,
//                     Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                     Data = null
//                 });
//             }
//         }
//
//
//
//         //Xóa sách
//
//
//         //InBarcode/QRCode( In mã vạch/QR Code cho sách)
//
//         // ✅ API Tạo QR Code cho sách
//         [HttpGet("books/{id}/qrcode")]
//         public async Task<IActionResult> GetBookQRCode(int id)
//         {
//             try
//             {
//                 // Lấy thông tin sách từ cơ sở dữ liệu
//                 var book = await context.Books
//                    // .Include(b => b.Category)
//                     .Where(b => b.Id == id)
//                     .Select(b => new BookDto
//                     {
//                         Id = b.Id,
//                         Title = b.Title,
//                        // Author = b.Author,
//                         ISBNumber = b.IsbNumber13,
//                         //Category = new CategoryDTOResponse
//                         //{
//                         //    Id = b.Category.Id,
//                         //    Name = b.Category.Name
//                         //}
//                     })
//                     .FirstOrDefaultAsync();
//
//                 if (book == null)
//                 {
//                     return NotFound("Book not found");
//                 }
//
//                 // Tạo chuỗi thông tin sách để mã hóa thành QR code
//                 string bookInfo = $"Title: {book.Title}\nAuthor: {book.Author}\nISBN: {book.ISBNumber}\nCategory: {book.Category.Name}";
//
//                 // Tạo QR code
//                 QRCodeGenerator qrGenerator = new QRCodeGenerator();
//                 QRCodeData qrCodeData = qrGenerator.CreateQrCode(bookInfo, QRCodeGenerator.ECCLevel.Q);
//                 QRCode qrCode = new QRCode(qrCodeData);
//
//                 // Chuyển đổi QR code thành hình ảnh
//                 Bitmap qrCodeImage = qrCode.GetGraphic(20);
//
//                 // Chuyển đổi hình ảnh thành byte array
//                 byte[] qrCodeBytes;
//                 using (MemoryStream ms = new MemoryStream())
//                 {
//                     qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
//                     qrCodeBytes = ms.ToArray();
//                 }
//
//                 // Trả về QR code dưới dạng hình ảnh PNG
//                 return File(qrCodeBytes, "image/png");
//             }
//             catch (Exception ex)
//             {
//                 logger.LogError(ex, "An error occurred while generating the QR code");
//                 return StatusCode(500, "Internal server error");
//             }
//         }
//         //API Tạo QRCode cho sách
//
//         // 1. Tạo QRCode cho sách lưu vào base64
//         [HttpPost("generate-qrcode")]
//         public IActionResult GenerateQrCode([FromBody] BookDto bookDto)
//         {
//             try
//             {
//                 if (string.IsNullOrEmpty(bookDto.QRCode))
//                 {
//                     return Ok(new ErrorResponse
//                     {
//                         Status = HttpStatus.BAD_REQUEST.GetDescription(),
//                         StatusCode = HttpStatus.BAD_REQUEST,
//                         Message = "QR Code content cannot be empty.",
//                     });
//                 }
//
//                 var writer = new ZXing.BarcodeWriterPixelData
//                 {
//                     Format = ZXing.BarcodeFormat.QR_CODE,
//                     Options = new ZXing.Common.EncodingOptions
//                     {
//                         Height = 300,
//                         Width = 300,
//                         Margin = 1
//                     }
//                 };
//
//                 var pixelData = writer.Write(bookDto.QRCode);
//                 using var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
//                 var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
//                 System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, data.Scan0, pixelData.Pixels.Length);
//                 bitmap.UnlockBits(data);
//
//                 using var memoryStream = new MemoryStream();
//                 bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
//                 var base64 = Convert.ToBase64String(memoryStream.ToArray());
//
//                 return Ok(new OkResponse
//                 {
//                     Status = HttpStatus.OK.GetDescription(),
//                     StatusCode = HttpStatus.OK,
//                     Message = "QR Code generated successfully.",
//                     Data = base64
//                 });
//             }
//             catch (Exception ex)
//             {
//                 logger.LogError(ex, "Error occurred while generating QR Code");
//                 return Ok(new ErrorResponse
//                 {
//                     Status = HttpStatus.INTERNAL_ERROR.GetDescription(),
//                     StatusCode = HttpStatus.INTERNAL_ERROR,
//                     Message = "An error occurred while processing your request.",
//                 });
//             }
//         }
//
//
//         // ✅ API Tạo Barcode cho sách
//         //[HttpGet("generate-barcode/{isbn}")]
//         //    public IActionResult GenerateBarcode(string isbn)
//         //    {
//         //        var barcodeWriter = new BarcodeWriter<Bitmap>
//         //        {
//         //            Format = BarcodeFormat.CODE_128,
//         //            Options = new EncodingOptions
//         //            {
//         //                Height = 100,
//         //                Width = 300,
//         //                Margin = 10
//         //            }
//         //        };
//
//         //        using (var bitmap = barcodeWriter.Write(isbn))
//         //        {
//         //            using (var ms = new MemoryStream())
//         //            {
//         //                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
//         //                return File(ms.ToArray(), "image/png");
//         //            }
//         //        }
//         //    }
//
//
//
//
//         //Phân loại sách theo category
//         [HttpGet("books-by-category")]
//         public async Task<IActionResult> GetBooksByCategory(
//             [FromQuery] int? categoryId,
//             [FromQuery] int pageNumber = 1,
//             [FromQuery] int pageSize = 10)
//         {
//             try
//             {
//                 // Kiểm tra số trang và kích thước trang hợp lệ
//                 if (pageNumber < 1 || pageSize < 1)
//                 {
//                     return BadRequest(new ErrorResponse
//                     {
//                         Message = "Số trang và kích thước trang phải lớn hơn 0.",
//                         StatusCode = HttpStatus.BAD_REQUEST,
//                         Status = HttpStatus.BAD_REQUEST.ToString()
//                     });
//                 }
//
//                 // Truy vấn danh sách sách
//                 var query = context.Books
//                     .AsNoTracking()
//                     //.Include(b => b.Category) // Lấy thông tin CategoryName
//                     .AsQueryable();
//
//                 // Nếu có categoryId, lọc theo danh mục
//                 if (categoryId.HasValue)
//                 {
//                     query = query.Where(b => b.CategoryId == categoryId.Value);
//                 }
//
//                 // Tổng số sách sau khi lọc
//                 int totalRecords = await query.CountAsync();
//
//                 // Áp dụng phân trang
//                 var books = await query
//                     .OrderBy(b => b.Title) 
//                     .Skip((pageNumber - 1) * pageSize)
//                     .Take(pageSize)
//                     .Select(b => new
//                     {
//                         b.Id,
//                         b.Title,
//                        // b.Author,
//                         b.Publisher,
//                         b.PublishDate,
//                         b.Language,
//                         b.PageCount,
//                         ISBNumber13 = b.IsbNumber13,
//                         b.TotalCopies,
//                         b.AvailableCopies,
//                         b.Price,
//                         b.Thumbnail,
//                         //Category = b.Category != null ? b.Category.Name : null
//                     })
//                     .ToListAsync();
//
//                 // Trả về kết quả
//                 return Ok(new
//                 {
//                     Message = "Lấy danh sách sách thành công.",
//                     StatusCode = HttpStatus.OK,
//                     Status = HttpStatus.OK.ToString(),
//                     TotalRecords = totalRecords,
//                     PageNumber = pageNumber,
//                     PageSize = pageSize,
//                     Data = books
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ErrorResponse
//                 {
//                     Message = "Lỗi trong quá trình xử lý yêu cầu.",
//                     StackTrace = ex.StackTrace,
//                     StatusCode = HttpStatus.INTERNAL_ERROR,
//                     Status = HttpStatus.INTERNAL_ERROR.ToString()
//                 });
//             }
//         }
//
//
//         //Phân loại sách bằng GENAI
//         [HttpPost("classify-book-genai/{id}")]
//         public async Task<IActionResult> ClassifyBookByGenAI(int id)
//         {
//             try
//             {
//                 // 1. Lấy thông tin sách từ database
//                 var book = await context.Books.FindAsync(id);
//                 if (book == null)
//                 {
//                     return NotFound(new ErrorResponse
//                     {
//                         Message = "Không tìm thấy sách.",
//                         StackTrace = null,
//                         StatusCode = HttpStatus.NOT_FOUND,
//                         Status = HttpStatus.NOT_FOUND.ToString(),
//                         Data = null
//                     });
//                 }
//
//                 // 2. Chuẩn bị nội dung để phân loại (ví dụ: tiêu đề và mô tả)
//                 var textForClassification = $"Title: {book.Title}\nDescription: {book.Description}";
//
//                 // 3. Gọi API GENAI bên ngoài để phân loại sách
//                 //    (Thay đổi URL dưới đây theo endpoint của dịch vụ GENAI mà bạn sử dụng)
//                 string apiUrl = "https://api.genai.example.com/classify";
//                 var requestBody = new { text = textForClassification };
//                 var jsonRequest = JsonConvert.SerializeObject(requestBody);
//                 var requestContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
//
//                 var response = await _httpClient.PostAsync(apiUrl, requestContent);
//                 if (!response.IsSuccessStatusCode)
//                 {
//                     return StatusCode(500, new ErrorResponse
//                     {
//                         Message = "Lỗi từ dịch vụ GENAI.",
//                         StackTrace = null,
//                         StatusCode = HttpStatus.INTERNAL_ERROR,
//                         Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                         Data = null
//                     });
//                 }
//
//                 var responseString = await response.Content.ReadAsStringAsync();
//                 // Giả sử phản hồi từ GENAI có định dạng: { "category": "Fiction" }
//                 dynamic result = JsonConvert.DeserializeObject(responseString);
//                 string predictedCategory = result?.category ?? "Không xác định";
//
//                 // 4. Cập nhật lại category cho sách (tùy chọn)
//                // book.Category = predictedCategory;
//                 book.UpdateDate = DateTime.Now;
//                 context.Books.Update(book);
//                 await context.SaveChangesAsync();
//
//                 // Trả về kết quả
//                 return Ok(new OkResponse
//                 {
//                     Message = "Phân loại sách bằng GENAI thành công.",
//                     StackTrace = null,
//                     StatusCode = HttpStatus.OK,
//                     Status = HttpStatus.OK.ToString(),
//                     Data = new { book.Id, book.Title, Category = predictedCategory }
//                 });
//             }
//             catch (Exception ex)
//             {
//                 logger.LogError("Lỗi trong quá trình phân loại sách bằng GENAI: " + ex.Message);
//                 return StatusCode(500, new ErrorResponse
//                 {
//                     Message = "Lỗi trong quá trình phân loại sách bằng GENAI.",
//                     StackTrace = ex.StackTrace,
//                     StatusCode = HttpStatus.INTERNAL_ERROR,
//                     Status = HttpStatus.INTERNAL_ERROR.ToString(),
//                     Data = null
//                 });
//             }
//         }
//
//         //Quản lý kho sách(Theo dõi số lượng, vị trí đặt sách)
//
//
//         //Kiểm kê sách
//
//
//         //In Barcode/ QR Code
//
//
//         //Quản lý file sách điện tử
//
//
//         //Quản lý file audio
//
//
//         //Dự báo nhu cầu sách
//
//
//         //Quản lý nhập sách
//
//
//         //Quản lý xuất sách
//
//
//     }
// }
