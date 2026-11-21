using Microsoft.AspNetCore.Mvc;
using MuseumOfFineArts.Shared.Models;
using System;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace MuseumOfFineArts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly string _connectionString;

        public TicketController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MuseumSystem");
        }

        // POST: api/Ticket
        // Endpoint to purchase a new ticket
        [HttpPost]
        public IActionResult PurchaseTicket([FromBody] Ticket ticketRequest)
        {
            const decimal ticketPrice = 24.0M;  // Ticket price fixed at $24
            string sql = "INSERT INTO Ticket (DateIssued, Price) VALUES (@DateIssued, @Price); SELECT CAST(SCOPE_IDENTITY() as int);";

            using (var connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@DateIssued", ticketRequest.DateIssued);
                command.Parameters.AddWithValue("@Price", ticketPrice);  // Use the fixed price

                connection.Open();
                var result = command.ExecuteScalar();
                if (result != null)
                {
                    return Ok(new { TicketID = (int)result, DateIssued = ticketRequest.DateIssued, Price = ticketPrice });
                }
                return BadRequest("Unable to purchase ticket.");
            }
        }
    }
}
