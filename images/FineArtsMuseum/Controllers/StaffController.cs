+++++++++++++++using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace FineArtsMuseum.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StaffController : Controller
	{
		private IConfiguration _configuration;

		public StaffController(IConfiguration conf)
		{
			_configuration = conf;

		}


		[HttpGet]
		[Route("GetStaffList")]
		public JsonResult GetStaffList(string department)
		{
			JsonResult result;


			try
			{
				string query = "SELECT DISTINCT Account.AccountID, StaffID, Name, EmailAddress, PhoneNumber, DateCreated, Role, MonthlySalary FROM dbo.Staff, dbo.Account WHERE Staff.Department = '" + department.ToUpper() + "' AND dbo.Account.AccountID = dbo.Staff.AccountID";


				DataTable table = Database.DatabaseManager.runSql(_configuration, query);

				result = new JsonResult(table);
			}
			catch (Exception ex)
			{
				result = new JsonResult(ex);


			}

			return result;

		}

        [HttpGet]
        [Route("GetNotifList")]
        public JsonResult GetNotifList(string staffID)
        {
            JsonResult result;


            try
            {
                string query = "SELECT * FROM Manager_notifs WHERE Manager_notifs.ManagerID = " + staffID.ToUpper();


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
