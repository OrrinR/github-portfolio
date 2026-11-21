using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace FineArtsMuseum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiftShopController : ControllerBase
    {
        private IConfiguration _configuration;

        public GiftShopController(IConfiguration conf)
        {
            _configuration = conf;


        }


        [HttpPost]
        [Route("RetreiveReport")]
        public JsonResult RetreiveReport([FromForm] string? ItemName, [FromForm] string? CustomerName, [FromForm] DateTime startDate, [FromForm] DateTime endDate)
        {
            string orderBy = " ORDER BY t.DateOfTransaction";
            string query = "SELECT t.transactionID, gi.Name ItemName,  m.MembershipID, a.Name CustomerName, t.PaymentMethod, ti.ItemQuantity * gi.Price total, t.DateOfTransaction " +
                "FROM TRANSACTIONS t,TRANSACTION_ITEM ti, GiftShopItem gi, Membership m, Account a " +
                "WHERE  ti.TransactionID = t.TransactionID AND gi.UPC = ti.ItemID AND t.MembershipID = m.MembershipID AND m.AccountID = a.AccountID";
            if(!String.IsNullOrEmpty(ItemName))
            {
                query = query + " AND gi.Name = '" + ItemName + "'";
            }
            if (!String.IsNullOrEmpty(CustomerName))
            {
                query = query + " AND a.Name = '" + CustomerName + "'";
            }
            
            query += " AND t.DateOfTransaction BETWEEN '" + startDate.ToString("yyyy-MM-dd") + "' AND '" + endDate.ToString("yyyy-MM-dd") + "'";
            query += orderBy;

            DataTable table = new DataTable();

            string sqlDatasource = _configuration.GetConnectionString("MuseumSystem");
            SqlDataReader reader;

            using (SqlConnection myConn = new SqlConnection(sqlDatasource))
            {
                myConn.Open();
                using (SqlCommand cmd = new SqlCommand(query, myConn))
                {
                    reader = cmd.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    myConn.Close();
                    myConn.Close();

                }

            }
            return new JsonResult(table);

        }



    }
}
