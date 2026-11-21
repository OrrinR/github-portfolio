using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace FineArtsMuseum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : Controller
    {
        private IConfiguration _configuration;

        public MemberController(IConfiguration conf)
        {
            _configuration = conf;

        }


        [HttpGet]
        [Route("GetMemberList")]
        public JsonResult GetMemberList()
        {
            JsonResult result;


            try
            {
                string query = "SELECT m.MembershipID, a.Name, a.Username, a.EmailAddress, a.HomeAddress, a.PhoneNumber, m.EndDate "+
                    "FROM Membership m, Account a "+
                    "WHERE m.AccountID = a.AccountID";


                DataTable table = Database.DatabaseManager.runSql(_configuration, query);

                result = new JsonResult(table);
            }
            catch (Exception ex)
            {
                result = new JsonResult(ex);
            }

            return result;

        }

       
    }
}
