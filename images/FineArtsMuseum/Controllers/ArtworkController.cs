using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MuseumOfFineArts.Shared.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MuseumOfFineArts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtworkController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<ArtworkController> _logger;  // Ensure to log the internal states and errors

        public ArtworkController(IConfiguration configuration, ILogger<ArtworkController> logger)
        {
            _connectionString = configuration.GetConnectionString("MuseumSystem");
            _logger = logger;
        }

        // GET: api/Artwork
        [HttpGet]
        public IActionResult GetArtworks()
        {
            try
            {
                List<Artwork> artworks = new List<Artwork>();
                string sql = "SELECT ArtworkID, ArtistID, Title, Year, Description, Medium, LinkURL FROM Artwork";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Artwork artwork = new Artwork
                        {
                            ArtworkID = reader.GetInt32(reader.GetOrdinal("ArtworkID")),
                            ArtistID = reader.GetInt32(reader.GetOrdinal("ArtistID")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Year = reader.GetDateTime(reader.GetOrdinal("Year")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            Medium = reader.GetString(reader.GetOrdinal("Medium")),
                            LinkURL = reader.IsDBNull(reader.GetOrdinal("LinkURL")) ? null : reader.GetString(reader.GetOrdinal("LinkURL"))
                        };
                        artworks.Add(artwork);
                    }
                    reader.Close();
                }
                return Ok(artworks);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to retrieve artworks: {Error}", ex.Message);
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // GET: api/Artwork/5
        [HttpGet("{id}")]
        public IActionResult GetArtwork(int id)
        {
            try
            {
                Artwork artwork = null;
                string sql = "SELECT ArtworkID, ArtistID, Title, Year, Description, Medium, LinkURL FROM Artwork WHERE ArtworkID = @id";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        artwork = new Artwork
                        {
                            ArtworkID = reader.GetInt32(reader.GetOrdinal("ArtworkID")),
                            ArtistID = reader.GetInt32(reader.GetOrdinal("ArtistID")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Year = reader.GetDateTime(reader.GetOrdinal("Year")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            Medium = reader.GetString(reader.GetOrdinal("Medium")),
                            LinkURL = reader.IsDBNull(reader.GetOrdinal("LinkURL")) ? null : reader.GetString(reader.GetOrdinal("LinkURL"))
                        };
                    }
                    reader.Close();
                }
                return artwork != null ? Ok(artwork) : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to retrieve artwork with ID {ID}: {Error}", id, ex.Message);
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // POST: api/Artwork
        [HttpPost]
        public IActionResult PostArtwork([FromBody] Artwork artwork)
        {
            if (artwork == null)
            {
                return BadRequest("Artwork cannot be null");
            }

            _logger.LogDebug("Posting new artwork: {Artwork}", JsonConvert.SerializeObject(artwork));

            try
            {
                string sql = "INSERT INTO Artwork (ArtistID, Title, Year, Description, Medium, LinkURL) VALUES (@ArtistID, @Title, @Year, @Description, @Medium, @LinkURL); SELECT CAST(SCOPE_IDENTITY() as int);";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ArtistID", artwork.ArtistID);
                    cmd.Parameters.AddWithValue("@Title", artwork.Title);
                    cmd.Parameters.AddWithValue("@Year", artwork.Year);
                    cmd.Parameters.AddWithValue("@Description", (object)artwork.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Medium", artwork.Medium);
                    cmd.Parameters.AddWithValue("@LinkURL", (object)artwork.LinkURL ?? DBNull.Value);
                    conn.Open();
                    int newId = (int)cmd.ExecuteScalar();
                    artwork.ArtworkID = newId;
                    return CreatedAtAction(nameof(GetArtwork), new { id = artwork.ArtworkID }, artwork);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to post new artwork: {Error}", ex.Message);
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}", exception = ex.ToString() });
            }
        }

        // PUT: api/Artwork/5
        [HttpPut("{id}")]
        public IActionResult PutArtwork(int id, [FromBody] Artwork artwork)
        {
            if (artwork == null)
            {
                return BadRequest("Artwork cannot be null");
            }

            _logger.LogDebug("Updating artwork ID {ID}: {Artwork}", id, JsonConvert.SerializeObject(artwork));

            try
            {
                string sql = "UPDATE Artwork SET ArtistID = @ArtistID, Title = @Title, Year = @Year, Description = @Description, Medium = @Medium, LinkURL = @LinkURL WHERE ArtworkID = @ArtworkID;";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ArtworkID", id);
                    cmd.Parameters.AddWithValue("@ArtistID", artwork.ArtistID);
                    cmd.Parameters.AddWithValue("@Title", artwork.Title);
                    cmd.Parameters.AddWithValue("@Year", artwork.Year);
                    cmd.Parameters.AddWithValue("@Description", (object)artwork.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Medium", artwork.Medium);
                    cmd.Parameters.AddWithValue("@LinkURL", (object)artwork.LinkURL ?? DBNull.Value);
                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    return result > 0 ? NoContent() : NotFound("Artwork not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update artwork ID {ID}: {Error}", id, ex.Message);
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // DELETE: api/Artwork/5
        [HttpDelete("{id}")]
        public IActionResult DeleteArtwork(int id)
        {
            try
            {
                string sql = "DELETE FROM Artwork WHERE ArtworkID = @ArtworkID";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ArtworkID", id);
                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    return result > 0 ? NoContent() : NotFound("Artwork not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete artwork ID {ID}: {Error}", id, ex.Message);
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
