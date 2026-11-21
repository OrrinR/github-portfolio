using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace FineArtsMuseum.Database
{
    public class DatabaseManager
    {
        public static DataTable runSql(IConfiguration conf, string sql)
        {
            DataTable table = new DataTable();
            string sqlDatasource = conf.GetConnectionString("MuseumSystem");
            SqlDataReader reader;

            using (SqlConnection myConn = new SqlConnection(sqlDatasource))
            {
                myConn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, myConn))
                {                    
                    reader = cmd.ExecuteReader();
                    table.Load(reader);
                    reader.Close();
                    myConn.Close();

                }

            }

            return table;
        }
    }
}
