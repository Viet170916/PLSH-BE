using System.Collections.Generic;
using API.Common;
using API.DTO;
using BU.Models.DTO;
using Google.Api.Gax.ResourceNames;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.Entity;

namespace API.Controllers.ResourceControllers;
[Authorize("LibrarianPolicy")]

public partial class ResourceController
{
  public class BeforeBorrowImageRequest
  {
    [FromForm]
    public List<string> pathsToFile { get; set; }

    [FromForm]
    public List<IFormFile> beforeBorrowImages { get; set; }
  }

  [HttpPost("loan/{loanId}/before/upload-images")]
  public async Task<IActionResult> UploadBeforeBorrowImages(
    [FromRoute] int loanId,
    [FromForm] BeforeBorrowImageRequest beforeBorrowImageRequest
  )
  {
    var bookBorrowing = await context.Loans.FindAsync(loanId);
    if (bookBorrowing == null) { return NotFound(new { message = "BookBorrowing not found", }); }

    if (beforeBorrowImageRequest.beforeBorrowImages.Count != beforeBorrowImageRequest.pathsToFile.Count)
    {
      return BadRequest(new BaseResponse<string> { Message = "beforeBorrowImages and pathsToFile are not the same.", });
    }

    try
    {
      for (var fileIndex = 0; fileIndex < beforeBorrowImageRequest.beforeBorrowImages.Count; fileIndex++)
      {
        await googleCloudStorageHelper
          .UploadFileAsync(beforeBorrowImageRequest.beforeBorrowImages[fileIndex].OpenReadStream(),
            beforeBorrowImageRequest.pathsToFile[fileIndex]
            , beforeBorrowImageRequest.beforeBorrowImages[fileIndex].ContentType);
      }
    }
    catch { return BadRequest(new BaseResponse<string> { Message = "Upload before borrow images failed.", }); }

    return Ok(new BaseResponse<List<string>>()
    {
      Message = "Images uploaded successfully",
      Data = beforeBorrowImageRequest.pathsToFile.Select(Converter.ToImageUrl).ToList(),
    });
  }
}
