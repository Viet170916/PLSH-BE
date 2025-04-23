using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;

namespace API.Controllers.PolicyController
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibraryPolicyController : ControllerBase
    {
        private readonly string _connectionString = "Data Source=library.db;Version=3;";
        private readonly string _policyFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "initial_policy.json");
        private readonly ILogger<LibraryPolicyController> _logger;

        public LibraryPolicyController(ILogger<LibraryPolicyController> logger)
        {
            _logger = logger;

            Directory.CreateDirectory(Path.GetDirectoryName(_policyFilePath)!);

            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Policy (
                    Id INTEGER PRIMARY KEY,
                    Title TEXT NOT NULL,
                    Sections TEXT NOT NULL
                )";
            command.ExecuteNonQuery();

            command.CommandText = "SELECT COUNT(*) FROM Policy WHERE Id = 1";
            if ((long)command.ExecuteScalar() == 0)
            {
                var defaultPolicy = new PolicyDto
                {
                    Title = "CHÍNH SÁCH THƯ VIỆN TRƯỜNG TRUNG HỌC CƠ SỞ",
                    Sections = "Chính sách thư viện đã được cập nhật"
                };

                command.CommandText = "INSERT INTO Policy (Id, Title, Sections) VALUES (1, @title, @sections)";
                command.Parameters.AddWithValue("@title", defaultPolicy.Title);
                command.Parameters.AddWithValue("@sections", defaultPolicy.Sections);
                command.ExecuteNonQuery();

                System.IO.File.WriteAllText(_policyFilePath, JsonSerializer.Serialize(defaultPolicy));
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePolicy([FromBody] PolicyDto inputData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(inputData.Title) || string.IsNullOrWhiteSpace(inputData.Sections))
                {
                    return BadRequest(new { error = "Title and Sections are required" });
                }

                var policy = new PolicyDto
                {
                    Title = inputData.Title.Trim(),
                    Sections = inputData.Sections.Replace("\r\n", "\n").Replace("\r", "\n").Trim()
                };

                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "UPDATE Policy SET Title = @title, Sections = @sections WHERE Id = 1";
                command.Parameters.AddWithValue("@title", policy.Title);
                command.Parameters.AddWithValue("@sections", policy.Sections);
                var rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    _logger.LogWarning("Policy not found with Id = 1");
                    return NotFound(new { error = "Policy not found" });
                }

                await System.IO.File.WriteAllTextAsync(_policyFilePath, JsonSerializer.Serialize(policy, new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                }), Encoding.UTF8);

                return Ok(new { message = "Policy updated successfully", policy });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update policy");
                return StatusCode(500, new { error = "Failed to update policy: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetPolicy()
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT Title, Sections FROM Policy WHERE Id = 1";
                using var reader = command.ExecuteReader();

                if (!reader.Read())
                    return NotFound(new { error = "Policy not found" });

                return Ok(new PolicyDto
                {
                    Title = reader.GetString(0),
                    Sections = reader.GetString(1)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve policy: " + ex.Message });
            }
        }
    }

    public class PolicyDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(500, ErrorMessage = "Title cannot exceed 500 characters")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Sections is required")]
        public string Sections { get; set; } = "";
    }
}