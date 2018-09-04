using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEAM
{
    internal class MetadataValidation
    {

        /// <summary>
        ///    This class ensures that a source object exists in the physical model
        /// </summary>
        internal static string ValidateObjectExistence (string Object, string connectionString)
        {
            var conn = new SqlConnection { ConnectionString = connectionString };
            conn.Open();

            // Execute the check
            var cmd = new SqlCommand("SELECT CASE WHEN EXISTS ((SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + Object + "')) THEN 1 ELSE 0 END", conn);
            var exists = (int) cmd.ExecuteScalar() == 1;

            conn.Close();

            // return the result of the test;
            return exists.ToString();
        }
    }
}
