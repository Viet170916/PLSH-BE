using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FuzzySharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers.AuthorControllers;

public partial class AuthorController
{
  [HttpGet("check-duplicate")] public async Task<IActionResult> CheckDuplicateAuthor(string inputName)
  {
    if (string.IsNullOrWhiteSpace(inputName))
    {
      return BadRequest(new { message = "Tên tác giả không được để trống." });
    }

    inputName = inputName.Trim();
    var abbreviationMap = await GenerateAbbreviationMap();
    var inputVariations = GenerateNameVariations(inputName, abbreviationMap);
    var existingAuthors = await context.Authors.ToListAsync();
    var matchedAuthors = existingAuthors
                         .Select(a => new
                         {
                           Author = a, Variations = GenerateNameVariations(a.FullName, abbreviationMap)
                         })
                         .Where(a => a.Variations.Any(v1 =>
                           inputVariations.Any(v2 =>
                             Fuzz.PartialRatio(v1, v2) >= 85))) // Dùng PartialRatio để tăng độ chính xác
                         .Select(a => a.Author)
                         .ToList();
    if (matchedAuthors.Any())
    {
      return Ok(new { message = "Có thể tác giả này đã tồn tại.", existingAuthors = matchedAuthors });
    }

    return Ok(new { message = "Không tìm thấy tác giả trùng lặp." });
  }

  private List<string> GenerateNameVariations(string name, Dictionary<string, List<string>> abbreviationMap)
  {
    if (string.IsNullOrWhiteSpace(name)) return new List<string>();
    name = NormalizeName(name);
    var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var variations = new List<string>();

    // Tạo danh sách biến thể của tên
    GenerateCombinations(words, 0, new List<string>(), variations, abbreviationMap);

    // Tạo danh sách tạm để tránh sửa danh sách khi đang duyệt
    var tempVariations = new List<string>(variations);

    // Thêm phiên bản không dấu của tất cả các biến thể
    tempVariations.AddRange(variations.Select(RemoveDiacritics));

    // Thêm biến thể đảo thứ tự tên
    tempVariations.AddRange(variations.Select(v => string.Join(" ", v.Split().Reverse())));
    return tempVariations.Distinct().ToList();
  }

  private void GenerateCombinations(
    string[] words,
    int index,
    List<string> current,
    List<string> variations,
    Dictionary<string, List<string>> abbreviationMap
  )
  {
    if (index == words.Length)
    {
      variations.Add(string.Join(" ", current));
      return;
    }

    string word = words[index];

    // Thêm từ đầy đủ
    current.Add(word);
    GenerateCombinations(words, index + 1, current, variations, abbreviationMap);
    current.RemoveAt(current.Count - 1);

    // Thêm từ viết tắt nếu có
    if (abbreviationMap.TryGetValue(word, out var abbreviations))
    {
      foreach (var abbr in abbreviations)
      {
        current.Add(abbr);
        GenerateCombinations(words, index + 1, current, variations, abbreviationMap);
        current.RemoveAt(current.Count - 1);
      }
    }
  }

  private async Task<Dictionary<string, List<string>>> GenerateAbbreviationMap()
  {
    var authors = await context.Authors.Select(a => a.FullName).ToListAsync();
    var abbreviationMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
    foreach (var fullName in authors)
    {
      var words = NormalizeName(fullName).Split(' ', StringSplitOptions.RemoveEmptyEntries);
      foreach (var word in words)
      {
        string key = word.Substring(0, 1).ToLower();
        if (!abbreviationMap.ContainsKey(key)) { abbreviationMap[key] = new List<string>(); }

        if (!abbreviationMap[key].Contains(word)) { abbreviationMap[key].Add(word); }
      }
    }

    return abbreviationMap;
  }

  private string NormalizeName(string name)
  {
    if (string.IsNullOrWhiteSpace(name)) return string.Empty;
    name = name.Trim().ToLower();
    name = Regex.Replace(name, @"\s+", " ");
    return name;
  }

  // private string ExpandAbbreviation(string name, Dictionary<string, string> abbreviationMap)
  // {
  //   if (abbreviationMap.TryGetValue(name, out var fullName))
  //   {
  //     return fullName; // Nếu tìm thấy trong bản đồ thì trả về tên đầy đủ
  //   }
  //
  //   return name; // Nếu không có viết tắt thì giữ nguyên
  // }

  private string RemoveDiacritics(string text)
  {
    if (string.IsNullOrWhiteSpace(text)) return "";
    var normalizedString = text.Normalize(NormalizationForm.FormD);
    var stringBuilder = new StringBuilder();
    foreach (var c in normalizedString)
    {
      var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
      if (unicodeCategory != UnicodeCategory.NonSpacingMark) { stringBuilder.Append(c); }
    }

    return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
  }
}