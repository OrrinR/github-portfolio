using Microsoft.AspNetCore.Mvc;
using MuseumOfFineArts.Shared.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace MuseumOfFineArts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExhibitionController : ControllerBase
    {
        private readonly string _connectionString;

        public ExhibitionController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MuseumSystem");
        }

        [HttpGet]
        public IActionResult GetExhibitions()
        {
            List<Exhibition> exhibitions = new List<Exhibition>();
            string query = "SELECT ExhibitionID, Title, StartDate, EndDate FROM Exhibition";  // Removed Description from the query
            using (var connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exhibitions.Add(new Exhibition
                        {
                            ExhibitionID = reader.GetInt32(reader.GetOrdinal("ExhibitionID")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                        });
                    }
                }
            }
            return Ok(exhibitions);
        }

        [HttpGet("{id}")]
        public IActionResult GetExhibition(int id)
        {
            string query = "SELECT ExhibitionID, Title, StartDate, EndDate FROM Exhibition WHERE ExhibitionID = @ExhibitionID";  // Removed Description from the query
            using (var connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ExhibitionID", id);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Exhibition exhibition = new Exhibition
                        {
                            ExhibitionID = reader.GetInt32("ExhibitionID"),
                            Title = reader.GetString("Title"),
                            StartDate = reader.GetDateTime("StartDate"),
                            EndDate = reader.GetDateTime("EndDate"),
                        };
                        return Ok(exhibition);
                    }
                }
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult PostExhibition([FromBody] Exhibition exhibition)
        {
            string commandText = "INSERT INTO Exhibition (Title, StartDate, EndDate) VALUES (@Title, @StartDate, @EndDate); SELECT CAST(SCOPE_IDENTITY() as int);";
            using (var connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@Title", exhibition.Title);
                command.Parameters.AddWithValue("@StartDate", exhibition.StartDate);
                command.Parameters.AddWithValue("@EndDate", exhibition.EndDate);

                connection.Open();
                var result = command.ExecuteScalar();
                return result != null ? CreatedAtAction("GetExhibition", new { id = result }, exhibition) : BadRequest();
            }
        }

        [HttpPut("{id}")]
        public IActionResult PutExhibition(int id, [FromBody] Exhibition exhibition)
        {
            string commandText = "UPDATE Exhibition SET Title = @Title, StartDate = @StartDate, EndDate = @EndDate WHERE ExhibitionID = @ExhibitionID";
            using (var connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@ExhibitionID", id);
                command.Parameters.AddWithValue("@Title", exhibition.Title);
                command.Parameters.AddWithValue("@StartDate", exhibition.StartDate);
                command.Parameters.AddWithValue("@EndDate", exhibition.EndDate);

                connection.Open();
                int result = command.ExecuteNonQuery();
                return result > 0 ? NoContent() : NotFound();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteExhibition(int id)
        {
            string commandText = "DELETE FROM Exhibition WHERE ExhibitionID = @ExhibitionID";
            using (var connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.AddWithValue("@ExhibitionID", id);

                connection.Open();
                try
                {
                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        return NoContent();
                    }
                    else
                    {
                        return NotFound($"Exhibition with ID {id} not found.");
                    }
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, $"Server error: {ex.Message}");
                }
            }
        }


    }
}
