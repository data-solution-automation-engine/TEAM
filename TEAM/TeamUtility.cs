using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TEAM
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class ExtensionMethod
    {
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
            TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        private static string CleanifyBrowserPath(string p)
        {
            string[] url = p.Split('"');
            string clean = url[1];
            return clean;
        }

        public static string GetDefaultBrowserPath()
        {
            string urlAssociation = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http";
            string browserPathKey = @"$BROWSER$\shell\open\command";

            Microsoft.Win32.RegistryKey userChoiceKey = null;
            string browserPath = "";

            try
            {
                //Read default browser path from userChoiceLKey
                userChoiceKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(urlAssociation + @"\UserChoice", false);

                //If user choice was not found, try machine default
                if (userChoiceKey == null)
                {
                    //Read default browser path from Win XP registry key
                    var browserKey = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

                    //If browser path wasn’t found, try Win Vista (and newer) registry key
                    if (browserKey == null)
                    {
                        browserKey =
                        Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                        urlAssociation, false);
                    }
                    var path = CleanifyBrowserPath(browserKey.GetValue(null) as string);
                    browserKey.Close();
                    return path;
                }
                else
                {
                    // user defined browser choice was found
                    string progId = (userChoiceKey.GetValue("ProgId").ToString());
                    userChoiceKey.Close();

                    // now look up the path of the executable
                    string concreteBrowserKey = browserPathKey.Replace("$BROWSER$", progId);
                    var kp = Registry.ClassesRoot.OpenSubKey(concreteBrowserKey, false);
                    browserPath = CleanifyBrowserPath(kp.GetValue(null) as string);
                    kp.Close();
                    return browserPath;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }

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
                MessageBox.Show("An error has occurred while creating a file backup. The error message is " + ex,
                    "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}