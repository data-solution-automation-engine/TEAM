using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

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

        /// <summary>
        ///    Create a file backup for the configuration file at the provided location
        /// </summary>
        internal static void CreateFileBackup(string fileName, string filePath = "")
        {
            var localFileName = Path.GetFileName(fileName);

            // Manage that the backup path can be defaulted or derived.
            if (filePath == "")
            {
                filePath = FormBase.GlobalParameters.BackupPath;
            }
            else
            {
                filePath = Path.GetDirectoryName(fileName);
            }

            try
            {
                if (File.Exists(fileName))
                {
                    var targetFilePathName = filePath + string.Concat("Backup_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_", localFileName);

                    if (fileName != null)
                    {
                        File.Copy(fileName, targetFilePathName);
                    }
                    else
                    {
                        FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The file cannot be backed up because it cannot be identified."));
                    }
                }
                else
                {
                    MessageBox.Show(
                        "TEAM couldn't locate a configuration file! Can you check the paths and existence of directories?",
                        "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured while creating a file backup. The error message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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