using Microsoft.AspNetCore.Mvc;
using MuseumOfFineArts.Shared.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Newtonsoft.Json;

namespace MuseumOfFineArts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistController : ControllerBase
    {
        private readonly string _connectionString;

        public ArtistController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MuseumSystem");
        }

        // GET: api/Artist
        [HttpGet]
        public IActionResult GetArtists()
        {
            try
            {
                List<Artist> artists = new List<Artist>();
                string sql = "SELECT ArtistID, Name, Nationality, Description, BirthYear, DeathYear, ImageURL FROM Artist";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        artists.Add(new Artist
                        {
                            ArtistID = reader.GetInt32(reader.GetOrdinal("ArtistID")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Nationality = reader.GetString(reader.GetOrdinal("Nationality")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            BirthYear = reader.GetDateTime(reader.GetOrdinal("BirthYear")),
                            DeathYear = reader.IsDBNull(reader.GetOrdinal("DeathYear")) ? default(DateTime?) : reader.GetDateTime(reader.GetOrdinal("DeathYear")),
                            ImageURL = reader.IsDBNull(reader.GetOrdinal("ImageURL")) ? null : reader.GetString(reader.GetOrdinal("ImageURL"))
                        });
                    }
                    reader.Close();
                }
                return Ok(artists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetArtist(int id)
        {
            try
            {
                Artist artist = null;
                string sql = "SELECT ArtistID, Name, Nationality, Description, BirthYear, DeathYear, ImageURL FROM Artist WHERE ArtistID = @id";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        artist = new Artist
                        {
                            ArtistID = reader.GetInt32("ArtistID"),
                            Name = reader.GetString("Name"),
                            Nationality = reader.GetString("Nationality"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                            BirthYear = reader.GetDateTime("BirthYear"),
                            DeathYear = reader.IsDBNull(reader.GetOrdinal("DeathYear")) ? default(DateTime?) : reader.GetDateTime("DeathYear"),
                            ImageURL = reader.IsDBNull(reader.GetOrdinal("ImageURL")) ? null : reader.GetString("ImageURL")
                        };
                    }
                    reader.Close();
                }
                return artist != null ? Ok(artist) : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }


        // POST: api/Artist
        [HttpPost]
        public IActionResult PostArtist([FromBody] Artist artist)
        {
            try
            {
                string sql = @"
            INSERT INTO Artist (Name, Nationality, Description, BirthYear, DeathYear, ImageURL) 
            VALUES (@Name, @Nationality, @Description, @BirthYear, @DeathYear, @ImageURL);
            SELECT CAST(SCOPE_IDENTITY() as int);
        ";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Name", artist.Name);
                    cmd.Parameters.AddWithValue("@Nationality", artist.Nationality);
                    cmd.Parameters.AddWithValue("@Description", artist.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BirthYear", artist.BirthYear);
                    cmd.Parameters.AddWithValue("@DeathYear", artist.DeathYear ?? (object)DBNull.Value);  // Handle nullable DateTime
                    cmd.Parameters.AddWithValue("@ImageURL", artist.ImageURL ?? (object)DBNull.Value);

                    conn.Open();
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    artist.ArtistID = newId;
                    return CreatedAtAction(nameof(GetArtist), new { id = artist.ArtistID }, artist);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }


        // DELETE: api/Artist/5
        [HttpDelete("{id}")]
        public IActionResult DeleteArtist(int id)
        {
            try
            {
                string sql = "DELETE FROM Artist WHERE ArtistID = @ArtistID";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ArtistID", id);

                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                        return NoContent(); // Successfully deleted
                    else
                        return NotFound("Artist not found.");
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, "Internal server error: Cannot delete artist because it is referenced by an art collection.");
            }
        }


        // PUT: api/Artist/5
        [HttpPut("{id}")]
        public IActionResult PutArtist(int id, [FromBody] Artist artist)
        {
            try
            {
                string sql = @"
            UPDATE Artist 
            SET Name = @Name, 
                Nationality = @Nationality, 
                Description = @Description, 
                BirthYear = @BirthYear, 
                DeathYear = @DeathYear, 
                ImageURL = @ImageURL 
            WHERE ArtistID = @ArtistID";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ArtistID", id);
                    cmd.Parameters.AddWithValue("@Name", artist.Name);
                    cmd.Parameters.AddWithValue("@Nationality", artist.Nationality);
                    cmd.Parameters.AddWithValue("@Description", artist.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BirthYear", artist.BirthYear);
                    cmd.Parameters.AddWithValue("@DeathYear", artist.DeathYear ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ImageURL", artist.ImageURL ?? (object)DBNull.Value);

                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        return NoContent();
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }


    }
}
