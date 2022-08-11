using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TEAM_Library;

namespace TEAM
{
    /// <summary>
    /// Configuration settings related to the export of JSON files. This class manages the settings (true/false) that dictate whether certain objects are generated or not.
    /// </summary>
    public class JsonExportSetting
    {
        // Data Objects
        public string GenerateDataObjectConnection { get; set; }
        public string GenerateDatabaseAsExtension { get; set; }
        public string GenerateSchemaAsExtension { get; set; }
        public string GenerateTypeAsClassification { get; set; }
        public string GenerateDataObjectDataItems { get; set; }

        // Data Items
        public string GenerateDataItemDataTypes { get; set; }
        public string GenerateParentDataObject { get; set; }

        // Related Data Objects
        public string AddMetadataAsRelatedDataObject { get; set; }
        public string AddUpstreamDataObjectsAsRelatedDataObject { get; set; }

        /// <summary>
        /// Retrieve the JSON export settings from disk and store them in the application (memory).
        /// </summary>
        /// <param name="fileName"></param>
        internal void LoadJsonConfigurationFile(string fileName, bool applyChecks = false)
        {
            try
            {
                var configList = new Dictionary<string, string>();
                var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                var streamReader = new StreamReader(fileStream);

                string textLine;
                while ((textLine = streamReader.ReadLine()) != null)
                {
                    if (textLine.IndexOf(@"/*", StringComparison.Ordinal) == -1 && textLine.Trim() != "")
                    {
                        var line = textLine.Split('|');
                        configList.Add(line[0], line[1]);
                    }
                }

                streamReader.Close();
                fileStream.Close();

                GenerateDataObjectConnection = configList["GenerateDataObjectConnection"];
                GenerateDataObjectDataItems = configList["GenerateDataObjectDataItems"];
                GenerateDatabaseAsExtension = configList["GenerateDatabaseAsExtension"];
                GenerateSchemaAsExtension = configList["GenerateSchemaAsExtension"];
                GenerateTypeAsClassification = configList["GenerateTypeAsClassification"];

                GenerateDataItemDataTypes = configList["GenerateDataItemDataTypes"];
                GenerateParentDataObject = configList["GenerateParentDataObject"];

                AddMetadataAsRelatedDataObject = configList["AddMetadataAsRelatedDataObject"];
                AddUpstreamDataObjectsAsRelatedDataObject = configList["AddUpstreamDataObjectsAsRelatedDataObject"];
            }
            catch (Exception exception)
            {
                if (applyChecks)
                {
                    MessageBox.Show($@"A non-fatal error occurred loading the JSON configuration file. This is likely related to a specific setting not found in the file. Please check the JSON export file settings and save the configuration again so that all values can be reset and/or updated. The full technical error message is '{exception.Message}'.", @"An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Save the JSON export settings to disk.
        /// </summary>
        internal void SaveJsonConfigurationFile()
        {
            try
            {
                // Creating the file
                var validationFile = new StringBuilder();
                validationFile.AppendLine("/* TEAM JSON Export Configuration Settings */");
                validationFile.AppendLine("/* Saved at " + DateTime.Now + " */");

                validationFile.AppendLine("GenerateDataObjectConnection|" + GenerateDataObjectConnection + "");
                validationFile.AppendLine("GenerateDataObjectDataItems|" + GenerateDataObjectDataItems + "");
                validationFile.AppendLine("GenerateDatabaseAsExtension|" + GenerateDatabaseAsExtension + "");
                validationFile.AppendLine("GenerateSchemaAsExtension|" + GenerateSchemaAsExtension + "");
                validationFile.AppendLine("GenerateTypeAsClassification|" + GenerateTypeAsClassification + "");

                validationFile.AppendLine("GenerateDataItemDataTypes|" + GenerateDataItemDataTypes + "");
                validationFile.AppendLine("GenerateParentDataObject|" + GenerateParentDataObject + "");

                validationFile.AppendLine("AddMetadataAsRelatedDataObject|" +AddMetadataAsRelatedDataObject + "");
                validationFile.AppendLine("AddUpstreamDataObjectsAsRelatedDataObject|" + AddUpstreamDataObjectsAsRelatedDataObject + "");

                // Closing off
                validationFile.AppendLine("/* End of file */");

                using (var outfile =
                    new StreamWriter(FormBase.GlobalParameters.ConfigurationPath + FormBase.GlobalParameters.JsonExportConfigurationFileName + '_' + FormBase.GlobalParameters.WorkingEnvironment +
                                     FormBase.GlobalParameters.FileExtension))
                {
                    outfile.Write(validationFile.ToString());
                    outfile.Close();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($@"An error occurred saving the JSON configuration file. The error message is {exception.Message}", @"An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Method to create a new validation file with default values, at the default location. Checks if the file already exists. If it does, nothing will happen.
        /// </summary>
        internal static void CreateDummyJsonConfigurationFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                var validationFile = new StringBuilder();

                validationFile.AppendLine("/* TEAM JSON Export File Settings */");

                // Data Object group.
                validationFile.AppendLine("GenerateDataObjectConnection|True");
                validationFile.AppendLine("GenerateDataObjectDataItems|True");
                validationFile.AppendLine("GenerateDatabaseAsExtension|True");
                validationFile.AppendLine("GenerateSchemaAsExtension|True");
                validationFile.AppendLine("GenerateTypeAsClassification|True");

                // Data Item group.
                validationFile.AppendLine("GenerateDataItemDataTypes|True");
                validationFile.AppendLine("GenerateTargetDataItemTypes|True");
                validationFile.AppendLine("GenerateParentDataObject|True");

                // Related Data Objects.
                validationFile.AppendLine("AddMetadataAsRelatedDataObject|True");
                validationFile.AppendLine("AddUpstreamDataObjectsAsRelatedDataObject|True");

                validationFile.AppendLine("/* End of file */");

                using (var outfile = new StreamWriter(fileName))
                {
                    outfile.Write(validationFile.ToString());
                    outfile.Close();
                }

                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"A new JSON extract configuration file was created ({fileName})."));
            }
            else
            {
                FormBase.GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"An existing JSON extract configuration file ({fileName}) was detected and used."));
            }
        }
    }
}
