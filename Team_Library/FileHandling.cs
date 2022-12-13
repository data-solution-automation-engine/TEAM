using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TEAM_Library
{
    public enum TeamPathTypes
    {
        [Display(Name = "Input Path")]
        InputPath,
        [Display(Name = "Output Path")]
        OutputPath,
        [Display(Name = "Metadata Path")]
        MetadataPath,
        [Display(Name = "Configuration Path")]
        ConfigurationPath,
        [Display(Name = "Core Path")]
        CorePath,
        [Display(Name = "Backup Path")]
        BackupPath,
        [Display(Name = "Files Path")]
        FilesPath,
        [Display(Name = "Script Path")]
        ScriptPath,
        [Display(Name = "Schema Path")]
        SchemaPath
    }
    /// <summary>
    /// The FileHandling class concerns the basic IO operations required to create directories and configuration files (without any specific content).
    /// </summary>
    public static class FileHandling
    {
        /// <summary>
        ///    Create a file backup for the configuration file at the provided location
        /// </summary>
        public static Event CreateFileBackup(string fileName, string filePath)
        {
            Event localEvent = new Event();

            var localFileName = Path.GetFileName(fileName);

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
                        localEvent = Event.CreateNewEvent(EventTypes.Error, $"The file cannot be backed up because it cannot be identified.");
                    }
                }
                else
                {
                    MessageBox.Show("TEAM couldn't locate a configuration file! Can you check the paths and existence of directories?", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occurred while creating a file backup. The error message is " + ex, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return localEvent;
        }

        public static Event SaveTextToFile(string targetFile, string textContent)
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
        /// Check if the path exists and create it if necessary.
        /// </summary>
        /// <param name="inputPath"></param>
        public static void InitialisePath(string inputPath, TeamPathTypes pathType, EventLog eventLog)
        {
            if (!string.IsNullOrEmpty(inputPath))
            {
                bool isError = false;

                try
                {
                    if (!Directory.Exists(inputPath))
                    {
                        Directory.CreateDirectory(inputPath);
                        eventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"Created a new directory '{inputPath}'."));
                    }
                }
                catch (Exception ex)
                {
                    eventLog.Add(Event.CreateNewEvent(EventTypes.Error,
                        "The directories required to operate TEAM are not available and can not be created. Do you have administrative privileges in the installation directory to create these additional directories?"));
                    eventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"Error creation directory '{inputPath}' the message is {ex.Message}.\r\n"));
                    isError = true;
                }

                if (isError == false)
                {
                    eventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"'{pathType}' set as '{inputPath}'."));
                }
            }
        }

        /// <summary>
        /// Create a new file with input content, if it does not exist yet. Returns an Event.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        public static Event CreateConfigurationFile(string fileName, StringBuilder fileContent)
        {
            Event localEvent = new Event();

            try
            {
                if (!File.Exists(fileName))
                {
                    using (var outfile = new StreamWriter(fileName))
                    {
                        outfile.Write(fileContent.ToString());
                        outfile.Close();
                    }

                    localEvent = Event.CreateNewEvent(EventTypes.Information, $"The new file {fileName} has been created.\r\n");
                }
            }
            catch (Exception ex)
            {
                localEvent = Event.CreateNewEvent(EventTypes.Error, $"Error creating a new configuration file at {fileName}. The message is {ex}.\r\n");
            }

            return localEvent;
        }

        /// <summary>
        /// Retrieve the values of a settings file and return this as a dictionary [string,string] object containing the configuration settings. Returns an Event.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Dictionary<string, string> LoadConfigurationFile(string filename)
        {
            // This is the hardcoded base path that always needs to be accessible, it has the main file which can locate the rest of the configuration
            var configList = new Dictionary<string, string>();

            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs);

            string textLine;
            while ((textLine = sr.ReadLine()) != null)
            {
                if (textLine.IndexOf(@"/*", StringComparison.Ordinal) == -1 && textLine.Trim() != "")
                {
                    var line = textLine.Split('|');
                    configList.Add(line[0], line[1]);
                }
            }

            sr.Close();
            fs.Close();

            return configList;
        }
    }
}
