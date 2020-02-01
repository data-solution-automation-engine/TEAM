using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEAM
{
    public class EventLog : List<Event> { }

    enum EventTypes
    {
        Information = 0,
        Error = 1,
        Warning = 2
    }

    public class Event
    {
        public int eventCode { get; set; }
        public string eventDescription { get; set; }
    }

    public class Utility
    {
        public static EventLog SaveOutputToDisk(string targetFile, string textContent)
        {
            EventLog eventLog = new EventLog();

            try

            {
                //Output to file
                using (var outfile = new StreamWriter(targetFile))
                {
                    outfile.Write(textContent);
                    outfile.Close();
                }
            }
            catch (Exception ex)
            {
                var localEvent = new Event
                {
                    eventCode = 1,
                    eventDescription =
                        @"There was an issue saving the output to disk. The message is: " + ex
                };

                eventLog.Add(localEvent);
            }

            return eventLog;
        }
    }

    internal static class DatabaseHandling
    {
        public static List<string> GetItemList(string inputQuery, SqlConnection conn)
        {
            List<string> returnList = new List<string>();

            try
            {
                var tables = GetDataTable(ref conn, inputQuery);

                foreach (DataRow row in tables.Rows)
                {
                    returnList.Add(row["TARGET_NAME"].ToString());
                }
            }
            catch (Exception)
            {
                // IGNORE FOR NOW
            }

            return returnList;
        }

        /// <summary>
        /// Load a data set into an in-memory datatable
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(ref SqlConnection sqlConnection, string sql)
        {
            // Pass the connection to a command object
            var sqlCommand = new SqlCommand(sql, sqlConnection);
            var sqlDataAdapter = new SqlDataAdapter { SelectCommand = sqlCommand };

            var dataTable = new DataTable();

            // Adds or refreshes rows in the DataSet to match those in the data source
            try
            {
                sqlDataAdapter.Fill(dataTable);
            }

            catch (Exception)
            {
                return null;
            }
            return dataTable;
        }
    }
}