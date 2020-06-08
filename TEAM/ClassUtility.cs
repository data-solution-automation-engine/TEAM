using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace TEAM
{
  public class TeamUtility
    {
        internal static Event SaveTextToFile(string targetFile, string textContent)
        {
            Event localEvent = new Event();
            try
            {
                //Output to file
                using (var outfile = new StreamWriter(targetFile))
                {
                    outfile.Write(textContent);
                    outfile.Close();
                }

                localEvent = Event.CreateNewEvent(EventTypes.Information, "The file was successfully saved to disk.\r\n");
            }
            catch (Exception ex)
            {
                localEvent = Event.CreateNewEvent(EventTypes.Error, "There was an issue saving the output to disk. The message is: " + ex + ".\r\n");
            }

            return localEvent;
        }
    }

    internal static class DatabaseHandling
    {
        public static List<string> GetItemList(string inputQuery, SqlConnection conn)
        {
            List<string> returnList = new List<string>();

            try
            {
                var tables = Utility.GetDataTable(ref conn, inputQuery);

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
    }
}