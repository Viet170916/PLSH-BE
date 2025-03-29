using System.Collections.Generic;
using System.Security.Claims;
using API.Common;
using API.DTO;
using API.DTO.Loan;
using AutoMapper;
using Data.DatabaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model.Entity;
using Model.Entity.Borrow;

namespace API.Controllers.BorrowControllers;

[Authorize(Policy = "LibrarianPolicy")]
[Route("api/v1/loan")]
[ApiController]
public class LoanController(AppDbContext context, IMapper mapper) : ControllerBase
{
  [HttpGet] public async Task<IActionResult> GetLoans()
  {
    var loans = await context.Loans
                             .Include(l => l.BookBorrowings)
                             .ToListAsync();
    return Ok(new BaseResponse<List<LoanDto>> { data = mapper.Map<List<LoanDto>>(loans), });
  }

  [HttpGet("{id}")] public async Task<ActionResult<LoanDto>> GetLoan(int id)
  {
    var loan = await context.Loans
                            .Include(l => l.BookBorrowings)
                            .FirstOrDefaultAsync(l => l.Id == id);
    if (loan == null) return NotFound(new BaseResponse<string> { message = "Loan not found", });
    return Ok(new BaseResponse<LoanDto> { data = mapper.Map<LoanDto>(loan), });
  }

  [HttpPost("create")] public async Task<IActionResult> CreateLoan([FromBody] LoanDto loanDto)
  {
    var librarianId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    var loan = mapper.Map<Loan>(loanDto);
    loanDto.BookBorrowings.ForEach(bb =>
    {
      var addedBorrowedBook = loan.BookBorrowings.FirstOrDefault(br => br.BookInstanceId == bb.BookInstanceId);
      if (addedBorrowedBook is not null)
      {
        addedBorrowedBook.BookImagesBeforeBorrow = bb
                                                   .BookImagesBeforeBorrow
                                                   .Select(image => new Resource
                                                   {
                                                     Type = image.Type,
                                                     FileType = image.FileType,
                                                     Name = image.Name,
                                                     SizeByte = image.SizeByte,
                                                     LocalUrl =
                                                       $"{StaticFolder.DIRPath_BORROWING_BEFORE}/{DateTimeOffset.Now.ToUnixTimeSeconds()}-{image.Name}",
                                                   })
                                                   .ToList();
      }
    });
    loan.LibrarianId = librarianId;
    context.Loans.Add(loan);
    await context.SaveChangesAsync();
    var beforeResponse = mapper.Map<LoanDto>(loan);
    return Ok(new BaseResponse<LoanDto> { data = beforeResponse });
  }

  [HttpPut("update/{id}")] public async Task<IActionResult> UpdateLoan(int id, [FromBody] LoanDto loanDto)
  {
    if (id != loanDto.Id) return BadRequest();
    var existingLoan = await context.Loans.Include(l => l.BookBorrowings).FirstOrDefaultAsync(l => l.Id == id);
    if (existingLoan == null) return NotFound();
    mapper.Map(loanDto, existingLoan);
    await context.SaveChangesAsync();
    return NoContent();
  }

  [HttpDelete("delete/{id}")] public async Task<IActionResult> DeleteLoan(int id)
  {
    var loan = await context.Loans.FindAsync(id);
    if (loan == null) return NotFound();
    context.Loans.Remove(loan);
    await context.SaveChangesAsync();
    return NoContent();
  }
}