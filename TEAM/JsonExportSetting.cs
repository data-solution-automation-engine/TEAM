using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TEAM_Library;

namespace TEAM
{
    /// <summary>
    /// Configuration settings related to the export of Json files.
    /// This class manages the settings (true/false) that dictate whether certain objects are generated or not.
    /// </summary>
    public class JsonExportSetting
    {
        // Data Item
        public string GenerateSourceDataItemTypes { get; set; }
        public string GenerateTargetDataItemTypes { get; set; }

        // Data Object Connection
        public string GenerateSourceDataObjectConnection { get; set; }
        public string GenerateTargetDataObjectConnection { get; set; }
        public string GenerateDatabaseAsExtension { get; set; }
        public string GenerateSchemaAsExtension { get; set; }

        // Related Data Objects
        public string AddMetadataAsRelatedDataObject { get; set; }
        public string AddUpstreamDataObjectsAsRelatedDataObject { get; set; }

        /// <summary>
        /// Retrieve the validation information from disk and save this to memory.
        /// </summary>
        /// <param name="filename"></param>
        internal void LoadJsonConfigurationFile(string filename)
        {
            try
            {
                var configList = new Dictionary<string, string>();
                var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var sr = new StreamReader(fs);

                string textline;
                while ((textline = sr.ReadLine()) != null)
                {
                    if (textline.IndexOf(@"/*", StringComparison.Ordinal) == -1 && textline.Trim() != "")
                    {
                        var line = textline.Split('|');
                        configList.Add(line[0], line[1]);
                    }
                }

                sr.Close();
                fs.Close();

                GenerateSourceDataItemTypes = configList["GenerateSourceDataItemTypes"];
                GenerateTargetDataItemTypes = configList["GenerateTargetDataItemTypes"];

                GenerateSourceDataObjectConnection = configList["GenerateSourceDataObjectConnection"];
                GenerateTargetDataObjectConnection = configList["GenerateTargetDataObjectConnection"];
                GenerateDatabaseAsExtension = configList["GenerateDatabaseAsExtension"];
                GenerateSchemaAsExtension = configList["GenerateSchemaAsExtension"];

                AddMetadataAsRelatedDataObject = configList["AddMetadataAsRelatedDataObject"];
                AddUpstreamDataObjectsAsRelatedDataObject = configList["AddUpstreamDataObjectsAsRelatedDataObject"];
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        internal void SaveJsonConfigurationFile()
        {
            try
            {
                // Creating the file
                var validationFile = new StringBuilder();
                validationFile.AppendLine("/* TEAM Json Export Configuration Settings */");
                validationFile.AppendLine("/* Saved at " + DateTime.Now + " */");
                validationFile.AppendLine("GenerateSourceDataItemTypes|" + GenerateSourceDataItemTypes + "");
                validationFile.AppendLine("GenerateTargetDataItemTypes|" + GenerateTargetDataItemTypes + "");

                validationFile.AppendLine("GenerateSourceDataObjectConnection|" + GenerateSourceDataObjectConnection + "");
                validationFile.AppendLine("GenerateTargetDataObjectConnection|" + GenerateTargetDataObjectConnection + "");
                validationFile.AppendLine("GenerateDatabaseAsExtension|" + GenerateDatabaseAsExtension + "");
                validationFile.AppendLine("GenerateSchemaAsExtension|" + GenerateSchemaAsExtension + "");

                validationFile.AppendLine("AddMetadataAsRelatedDataObject|" +AddMetadataAsRelatedDataObject + "");
                validationFile.AppendLine("AddUpstreamDataObjectsAsRelatedDataObject|" + AddUpstreamDataObjectsAsRelatedDataObject + "");

                // Closing off
                validationFile.AppendLine("/* End of file */");

                using (var outfile =
                    new StreamWriter(FormBase.GlobalParameters.ConfigurationPath +
                                     FormBase.GlobalParameters.JsonExportConfigurationFileName + '_' +
                                     FormBase.GlobalParameters.WorkingEnvironment +
                                     FormBase.GlobalParameters.FileExtension))
                {
                    outfile.Write(validationFile.ToString());
                    outfile.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred saving the Json configuration file. The error message is " + ex, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Method to create a new validation file with default values at the default location
        /// Checks if the file already exists. If it does, nothing will happen.
        /// </summary>
        internal static void CreateDummyJsonConfigurationFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                var validationFile = new StringBuilder();

                validationFile.AppendLine("/* TEAM Json Export File Settings */");

                validationFile.AppendLine("GenerateSourceDataItemTypes|True");
                validationFile.AppendLine("GenerateTargetDataItemTypes|True");
                validationFile.AppendLine("GenerateSourceDataObjectConnection|True");
                validationFile.AppendLine("GenerateTargetDataObjectConnection|True");
                validationFile.AppendLine("GenerateDatabaseAsExtension|True");
                validationFile.AppendLine("GenerateSchemaAsExtension|True");
                validationFile.AppendLine("AddMetadataAsRelatedDataObject|True");
                validationFile.AppendLine("AddUpstreamDataObjectsAsRelatedDataObject|True");

                validationFile.AppendLine("/* End of file */");

                using (var outfile = new StreamWriter(fileName))
                {
                    outfile.Write(validationFile.ToString());
                    outfile.Close();
                }

                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"A new Json extract configuration file was created for {fileName}."));

            }
            else
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The existing Json extract configuration file {fileName} was detected."));
            }
        }
    }
}
