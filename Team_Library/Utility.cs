using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Data;

namespace TEAM
{
    public class Utility
    {
        /// <summary>
        /// Receives a comma-separated string and turns it into an array without spaces.
        /// </summary>
        /// <param name="labels"></param>
        /// <returns></returns>
        public static string[] SplitLabelIntoArray(string labels)
        {
            string[] SplitLabelArray = { "default" };
            if (labels != null)
            {
                SplitLabelArray = labels.Replace(" ", "").Split(',');
            }

            return SplitLabelArray;
        }

        public static string SandingElement { get; set; } = "^@#%7!";

        /// <summary>
        /// Generate a MD5 hash based on the string input and sanding element.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="sandingElement"></param>
        /// <returns></returns>
        public static string CreateMd5(string[] input, string sandingElement)
        {
            string hashingString = "";
            // Make sure the values in the string array are appropriately sanded
            foreach (string inputElement in input)
            {
                hashingString = hashingString + inputElement+ sandingElement;
            }

            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(hashingString);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Generate a random number value.
        /// </summary>
        /// <param name="maxNumber"></param>
        /// <returns></returns>
        public static int GetRandomNumber(int maxNumber)
        {
            if (maxNumber < 1)
                throw new Exception("The maxNumber value should be greater than 1");
            var b = new byte[4];
            new RNGCryptoServiceProvider().GetBytes(b);
            var seed = (b[0] & 0x7f) << 24 | b[1] << 16 | b[2] << 8 | b[3];
            var r = new Random(seed);
            return r.Next(1, maxNumber);
        }

        /// <summary>
        /// Generate random date value.
        /// </summary>
        /// <param name="startYear"></param>
        /// <returns></returns>
        public static DateTime GetRandomDate(int startYear = 1995)
        {
            var start = new DateTime(startYear, 1, 1);
            var range = (DateTime.Today - start).Days;
            var b = new byte[4];
            new RNGCryptoServiceProvider().GetBytes(b);
            var seed = (b[0] & 0x7f) << 24 | b[1] << 16 | b[2] << 8 | b[3];
            return start.AddDays(new Random(seed).Next(1, range)).AddSeconds(new Random(seed).Next(1, 86400));
        }

        /// <summary>
        /// Generate a random string value.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomString(int length)
        {
            var array = new[]
            {
                "0","2","3","4","5","6","8","9",
                "a","b","c","d","e","f","g","h","j","k","m","n","p","q","r","s","t","u","v","w","x","y","z",
                "A","B","C","D","E","F","G","H","J","K","L","M","N","P","R","S","T","U","V","W","X","Y","Z"
            };
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++) sb.Append(array[GetRandomNumber(53)]);
            return sb.ToString();
        }

        /// <summary>
        /// Populate a datatable by loading from a Sql Server database table.
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="sqlQuery"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(ref SqlConnection sqlConnection, string sqlQuery)
        {
            // Pass the connection to a command object
            var sqlCommand = new SqlCommand(sqlQuery, sqlConnection);
            var sqlDataAdapter = new SqlDataAdapter { SelectCommand = sqlCommand };

            var dataTable = new DataTable();

            // Adds or refreshes rows in the DataSet to match those in the data source
            try
            {
                sqlDataAdapter.Fill(dataTable);
            }

            catch (Exception)
            {
                //  MessageBox.Show(@"SQL error: " + exception.Message + "\r\n\r\n The executed query was: " + sql + "\r\n\r\n The connection used was " + sqlConnection.ConnectionString, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return dataTable;

        }

        /// <summary>
        /// Convert a list of objects into a datatable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            DataTable table = new DataTable();

            try
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
                foreach (PropertyDescriptor prop in properties)
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                foreach (T item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    table.Rows.Add(row);
                }
            }
            catch (Exception)
            {
                // IGNORE
            }
            return table;
        }

        /// <summary>
        /// Returns the default type (e.g. null value, empty or default) for a given input object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ConvertFromDBVal<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default(T); // returns the default value for the type
            }
            else
            {
                return (T)obj;
            }
        }
    }
}
