using System.Threading.Tasks;
using BU.Models.DTO.Book;
using Model.Entity.book;

namespace BU.Services.Interface;

public interface IGeminiService
{
  Task<BookNewDto> GetBookFromGeminiPromptAsync(BookNewDto input);
  Task<string> GetFromGeminiPromptAsync(string prompt);
}
