using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Configuration;

namespace WpfApp1
{
    public static class DbAdapter
    {
        public static string sqlConnStr = ConfigurationManager.AppSettings["sqlConnectionString"];

        public static List<Object> GetAllValues(string connStr)
        {
            var list = new List<Object>();

            using (var sqlConnection = new SqlConnection(connStr))
            {
                sqlConnection.Open();

                using (var cmd = new SqlCommand(connStr))
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "SELECT [id],[datastamp],[MD],[GRBX] FROM [TestDataBase].[dbo].[GammaRay]";
                    cmd.Connection = sqlConnection;
                    cmd.Prepare();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // id
                            var id = int.Parse(reader["id"].ToString());
                            // datastamp
                            var datastamp = new DateTimeOffset(DateTime.Parse(reader["datastamp"].ToString())).ToUnixTimeSeconds();
                            // MD
                            var MD = float.Parse(reader["MD"].ToString());
                            // GRBX
                            var GRBX = float.Parse(reader["GRBX"].ToString());

                            list.Add(new Object[] { id, datastamp, MD, GRBX });
                        }
                    }
                }
            }

            return list;
        }

       
    }
}
