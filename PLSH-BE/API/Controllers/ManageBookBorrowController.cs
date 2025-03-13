using Data.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageBookBorrowController : ControllerBase
    {
        private readonly AppDbContext _context;
       

        public ManageBookBorrowController(AppDbContext context)
        {
            _context = context;
          
        }
    }
}
