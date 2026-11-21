using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace FineArtsMuseum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IConfiguration _configuration; 

        public AccountController(IConfiguration conf)
        {
            _configuration = conf;


        }

        [HttpGet]
        [Route("GetAccounts")]
        public JsonResult GetAccounts()
        {
            JsonResult result;


			try
            {
                string query = "SELECT * from dbo.account";


                DataTable table = Database.DatabaseManager.runSql(_configuration, query);

				result =  new JsonResult(table);
            }
            catch (Exception ex)
            {
				result =new JsonResult(ex); 


			}

            return result;


		}

        [HttpPost]
        [Route("Login")]
        public JsonResult Login([FromForm] string username, [FromForm] string password)
        {
            string query = "SELECT * from dbo.account where lower(username) = lower(@username) AND password = @password";
            DataTable table = new DataTable();
            string returnMsg = username + " is not found";


            string sqlDatasource = _configuration.GetConnectionString("MuseumSystem");
            SqlDataReader reader;

            using (SqlConnection myConn = new SqlConnection(sqlDatasource))
            {
                myConn.Open();
                using (SqlCommand cmd = new SqlCommand(query, myConn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    reader = cmd.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    myConn.Close();

                }

            }
            
            return new JsonResult(table);

        }

		[HttpPost]
		[Route("AddStaff")]
		public JsonResult AddStaff(int accountID, [FromForm] string role, [FromForm] string department, [FromForm] string monthlySalary )
		{
			string query = "INSERT INTO Staff (Role, Department ,MonthlySalary, AccountID) VALUES (@role ,@department ,@monthlySalary ,@accountID)";

			DataTable table = new DataTable();

			string sqlDatasource = _configuration.GetConnectionString("MuseumSystem");
			SqlDataReader reader;

			using (SqlConnection myConn = new SqlConnection(sqlDatasource))
			{
				myConn.Open();
				using (SqlCommand cmd = new SqlCommand(query, myConn))
				{
					cmd.Parameters.AddWithValue("@role", role);
					cmd.Parameters.AddWithValue("@department", department);
					cmd.Parameters.AddWithValue("@monthlySalary", monthlySalary);
					cmd.Parameters.AddWithValue("@accountID", accountID);
					reader = cmd.ExecuteReader();
					table.Load(reader);
					reader.Close();
					myConn.Close();
					myConn.Close();

				}

			}
			return new JsonResult("Created New Staff.");

		}


		[HttpPost]
        [Route("AddAccount")]
        public JsonResult AddAccount([FromForm] string fullname, [FromForm] string password, [FromForm] string username, [FromForm] string emailAddress, [FromForm] string homeAddress, [FromForm] string accountType, [FromForm] string phoneNumber)
        {
            string query = "INSERT INTO Account (Name, EmailAddress ,HomeAddress ,PhoneNumber ,Password ,Username , AccountType) VALUES (@fullname ,@emailAddress ,@homeAddress ,@phoneNumber ,@password ,@username , @accountType)";

            DataTable table = new DataTable();

            string sqlDatasource = _configuration.GetConnectionString("MuseumSystem");
            SqlDataReader reader;

            using (SqlConnection myConn = new SqlConnection(sqlDatasource))
            {
                myConn.Open();
                using (SqlCommand cmd = new SqlCommand(query, myConn))
                {
                    cmd.Parameters.AddWithValue("@FullName", fullname);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@UserName", username);
					cmd.Parameters.AddWithValue("@emailAddress", emailAddress);
					cmd.Parameters.AddWithValue("@homeAddress", homeAddress);
					cmd.Parameters.AddWithValue("@accountType", accountType);
					cmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);
					reader = cmd.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    myConn.Close();

                }

            }
            return this.Login(username, password);

        }

        [HttpPost]
        [Route("AddMember")]
        public JsonResult AddMember(int accountID, [FromForm] string mailSubscription, [FromForm] DateTime endDate, [FromForm] int cardNum)
        {
            string query = "INSERT INTO Membership (DateIssued, HasMailSubscription ,AccountID, EndDate, CardNumber) VALUES (GETDATE() ,@mailSubscription ,@accountID, @endDate, @cardNum)";

            DataTable table = new DataTable();

            string sqlDatasource = _configuration.GetConnectionString("MuseumSystem");
            SqlDataReader reader;

            using (SqlConnection myConn = new SqlConnection(sqlDatasource))
            {
                myConn.Open();
                using (SqlCommand cmd = new SqlCommand(query, myConn))
                {
                    cmd.Parameters.AddWithValue("@mailSubscription", mailSubscription);
                    cmd.Parameters.AddWithValue("@accountID", accountID);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@cardNum", cardNum);
                    reader = cmd.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    myConn.Close();
                    myConn.Close();

                }

            }
            return this.FetchMemberData(accountID);

		}

		[HttpPost]
		[Route("FetchMemberData")]
		public JsonResult FetchMemberData(int accountID)
		{
			string query = "SELECT * from dbo.Membership where AccountID = @accountID";
			DataTable table = new DataTable();


			string sqlDatasource = _configuration.GetConnectionString("MuseumSystem");
			SqlDataReader reader;

			using (SqlConnection myConn = new SqlConnection(sqlDatasource))
			{
				myConn.Open();
				using (SqlCommand cmd = new SqlCommand(query, myConn))
				{
					cmd.Parameters.AddWithValue("@accountID", accountID);
					reader = cmd.ExecuteReader();
					table.Load(reader);
					reader.Close();
					myConn.Close();

				}

			}

			return new JsonResult(table);

		}

		[HttpDelete]
		[Route("DeleteAccount")]
		public JsonResult DeleteAccount(int accountID)
		{
			string query = "delete from dbo.account where accountID = @AccountID";

			DataTable table = new DataTable();

			string sqlDatasource = _configuration.GetConnectionString("MuseumSystem");
			SqlDataReader reader;

			using (SqlConnection myConn = new SqlConnection(sqlDatasource))
			{
				myConn.Open();
				using (SqlCommand cmd = new SqlCommand(query, myConn))
				{
					cmd.Parameters.AddWithValue("@AccountID", accountID);
					reader = cmd.ExecuteReader();
					table.Load(reader);
					reader.Close();
					myConn.Close();

				}

			}
			return new JsonResult("Deleted sucessfully.");

		}

		[HttpGet]
		[Route("GetMemberInfo")]
		public JsonResult GetMemberInfo(int accountID)
		{
			JsonResult result;
			try
			{
				string query = "SELECT * from dbo.Membership WHERE AccountID = " + accountID.ToString();


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
        [Route("GetStaffInfo")]
        public JsonResult GetStaffInfo(int accountID)
        {
            JsonResult result;

            try
            {
                string query = "SELECT * from dbo.Staff WHERE AccountID = " + accountID.ToString();


                DataTable table = Database.DatabaseManager.runSql(_configuration, query);

                result = new JsonResult(table);
            }
            catch (Exception ex)
            {
                result = new JsonResult(ex);
            }

            return result;


        }


        [HttpPost]
        [Route("AddDonation")]
        public JsonResult AddDonation(int AccountID, [FromForm] int AmountDonated, [FromForm] string Name)
        {
            string query = "INSERT INTO Donation (DonorName, AmountDonated ,Date, AccountID) VALUES (@Name ,@AmountDonated ,GetDate() ,@AccountID)";

            DataTable table = new DataTable();

            string sqlDatasource = _configuration.GetConnectionString("MuseumSystem");
            SqlDataReader reader;

            using (SqlConnection myConn = new SqlConnection(sqlDatasource))
            {
                myConn.Open();
                using (SqlCommand cmd = new SqlCommand(query, myConn))
                {
                    cmd.Parameters.AddWithValue("@Name", Name);
                    cmd.Parameters.AddWithValue("@AmountDonated", AmountDonated);
                    cmd.Parameters.AddWithValue("@AccountID", AccountID);
                    reader = cmd.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    myConn.Close();
                    myConn.Close();

                }

            }
            return new JsonResult("Created New Donation.");

        }

        [HttpGet("GetDonationsBySearch")]
        public IActionResult GetDonationsBySearch(string donorName, DateTime startDate, DateTime endDate)
        {
            string orderBy = " ORDER BY d.DonationID";
            string query = "SELECT d.DonationID, a.Name, d.AmountDonated, d.Date DateDonated "+
                "FROM Donation d, Account a"+
                "WHERE a.AccountID = d.AccountID";

            if (!String.IsNullOrEmpty(donorName))
            {
                query += " AND d.DonorName = @DonorName";
            }

            query += " AND d.Date BETWEEN @StartDate AND @EndDate";
            query += orderBy;

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("MuseumSystem");
            SqlDataReader reader;

            using (SqlConnection connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DonorName", donorName);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);

                    reader = command.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                }
                connection.Close();
            }

            return new JsonResult(table);
        }

    }
}
