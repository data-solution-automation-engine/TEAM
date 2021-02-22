using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TEAM_Library
{
    /// <summary>
    /// The FileHandling class concerns the basic IO operations required to create directories and configuration files (without any specific content).
    /// </summary>
    public static class FileHandling
    {
        /// <summary>
        /// Check if the path exists and create it if necessary. Returns an Event.
        /// </summary>
        /// <param name="inputPath"></param>
        public static Event InitialisePath(string inputPath)
        {
            Event localEvent = new Event();

            // Create the configuration directory if it does not exist yet
            try
            {
                if (!Directory.Exists(inputPath))
                {
                    Directory.CreateDirectory(inputPath);
                    localEvent = Event.CreateNewEvent(EventTypes.Information,
                        $"Created a new directory for {inputPath}.");

                }
            }
            catch (Exception ex)
            {
                localEvent = Event.CreateNewEvent(EventTypes.Error,
                    $"Error creation directory at {inputPath} the message is {ex}.\r\n");
            }

            return localEvent;
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

                    localEvent = Event.CreateNewEvent(EventTypes.Information,
                        $"The new file {fileName} has been created.\r\n");
                }
            }
            catch (Exception ex)
            {
                localEvent = Event.CreateNewEvent(EventTypes.Error,
                    $"Error creating a new configuration file at {fileName}. The message is {ex}.\r\n");
            }

            return localEvent;
        }

        /// <summary>
        /// Retrieve the values of a settings file and return this as a dictionary<string,string> object containing the configuration settings. Returns an Event.
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
