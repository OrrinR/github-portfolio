using Microsoft.AspNetCore.Mvc;
using MuseumOfFineArts.Shared.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace MuseumOfFineArts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtCollectionController : ControllerBase
    {
        private readonly string _connectionString;

        public ArtCollectionController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MuseumSystem");
        }

        // GET: api/ArtCollection
        [HttpGet]
        public IActionResult GetArtCollections()
        {
            try
            {
                List<ArtCollection> collections = new List<ArtCollection>();
                string sql = "SELECT CollectionID, Title, DateCurated, Description, CountryOfOrigin, ArtMovement FROM ArtCollection";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        collections.Add(new ArtCollection
                        {
                            CollectionID = reader.GetInt32(reader.GetOrdinal("CollectionID")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            DateCurated = reader.GetDateTime(reader.GetOrdinal("DateCurated")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            CountryOfOrigin = reader.GetString(reader.GetOrdinal("CountryOfOrigin")),
                            ArtMovement = reader.GetString(reader.GetOrdinal("ArtMovement"))
                        });
                    }
                    reader.Close();
                }
                return Ok(collections);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // GET: api/ArtCollection/5
        [HttpGet("{id}")]
        public IActionResult GetArtCollection(int id)
        {
            try
            {
                ArtCollection collection = null;
                string sql = "SELECT CollectionID, Title, DateCurated, Description, CountryOfOrigin, ArtMovement FROM ArtCollection WHERE CollectionID = @id";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        collection = new ArtCollection
                        {
                            CollectionID = reader.GetInt32("CollectionID"),
                            Title = reader.GetString("Title"),
                            DateCurated = reader.GetDateTime("DateCurated"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                            CountryOfOrigin = reader.GetString("CountryOfOrigin"),
                            ArtMovement = reader.GetString("ArtMovement")
                        };
                    }
                    reader.Close();
                }
                return collection != null ? Ok(collection) : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // POST: api/ArtCollection
        [HttpPost]
        public IActionResult PostArtCollection([FromBody] ArtCollection collection)
        {
            try
            {
                string sql = @"
                INSERT INTO ArtCollection (Title, DateCurated, Description, CountryOfOrigin, ArtMovement) 
                VALUES (@Title, @DateCurated, @Description, @CountryOfOrigin, @ArtMovement);
                SELECT CAST(SCOPE_IDENTITY() as int);
            ";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Title", collection.Title);
                    cmd.Parameters.AddWithValue("@DateCurated", collection.DateCurated);
                    cmd.Parameters.AddWithValue("@Description", collection.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CountryOfOrigin", collection.CountryOfOrigin);
                    cmd.Parameters.AddWithValue("@ArtMovement", collection.ArtMovement);

                    conn.Open();
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    collection.CollectionID = newId;
                    return CreatedAtAction(nameof(GetArtCollection), new { id = collection.CollectionID }, collection);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // DELETE: api/ArtCollection/5
        [HttpDelete("{id}")]
        public IActionResult DeleteArtCollection(int id)
        {
            try
            {
                string sql = "DELETE FROM ArtCollection WHERE CollectionID = @CollectionID";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@CollectionID", id);

                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                        return NoContent(); // Successfully deleted
                    else
                        return NotFound("Art Collection not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // PUT: api/ArtCollection/5
        [HttpPut("{id}")]
        public IActionResult PutArtCollection(int id, [FromBody] ArtCollection collection)
        {
            try
            {
                string sql = @"
                UPDATE ArtCollection 
                SET Title = @Title, 
                    DateCurated = @DateCurated, 
                    Description = @Description, 
                    CountryOfOrigin = @CountryOfOrigin, 
                    ArtMovement = @ArtMovement
                WHERE CollectionID = @CollectionID";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@CollectionID", id);
                    cmd.Parameters.AddWithValue("@Title", collection.Title);
                    cmd.Parameters.AddWithValue("@DateCurated", collection.DateCurated);
                    cmd.Parameters.AddWithValue("@Description", collection.Description ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CountryOfOrigin", collection.CountryOfOrigin);
                    cmd.Parameters.AddWithValue("@ArtMovement", collection.ArtMovement);

                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        return NoContent();
                    }
                    else
                    {
                        return NotFound("Art Collection not found.");
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
