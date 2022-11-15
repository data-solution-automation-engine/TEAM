using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TEAM_Library
{
    /// <summary>
    /// Configuration settings related to the export of JSON files. This class manages the settings (true/false) that dictate whether certain objects are generated or not.
    /// </summary>
    public class JsonExportSetting
    {
        // Data Objects
        public string AddDatabaseAsExtension { get; set; }
        public string AddSchemaAsExtension { get; set; }
        public string AddTypeAsClassification { get; set; }
        public string AddDataObjectDataItems { get; set; }

        // Data Items
        public string AddDataItemDataTypes { get; set; }
        public string AddParentDataObject { get; set; }

        // Related Data Objects
        public string AddMetadataAsRelatedDataObject { get; set; }
        public string AddRelatedDataObjectsAsRelatedDataObject { get; set; }

        public bool IsAddDatabaseAsExtension()
        {
            bool returnValue = AddDatabaseAsExtension == "True";

            return returnValue;
        }

        public bool IsAddSchemaAsExtension()
        {
            bool returnValue = AddSchemaAsExtension == "True";

            return returnValue;
        }

        public bool IsAddTypeAsClassification()
        {
            bool returnValue = AddTypeAsClassification == "True";

            return returnValue;
        }

        public bool IsAddDataObjectDataItems()
        {
            bool returnValue = AddDataObjectDataItems == "True";

            return returnValue;
        }

        public bool IsAddDataItemDataTypes()
        {
            bool returnValue = AddDataItemDataTypes == "True";

            return returnValue;
        }

        public bool IsAddParentDataObject()
        {
            bool returnValue = AddParentDataObject == "True";

            return returnValue;
        }

        public bool IsAddMetadataAsRelatedDataObject()
        {
            bool returnValue = AddMetadataAsRelatedDataObject == "True";

            return returnValue;
        }

        public bool IsAddRelatedDataObjectsAsRelatedDataObject()
        {
            bool returnValue = AddRelatedDataObjectsAsRelatedDataObject == "True";

            return returnValue;
        }

        /// <summary>
        /// Retrieve the JSON export settings from disk and store them in the application (memory).
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="applyChecks"></param>
        public void LoadJsonConfigurationFile(string fileName, bool applyChecks = false)
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

                AddDataObjectDataItems = configList["GenerateDataObjectDataItems"];
                AddDatabaseAsExtension = configList["GenerateDatabaseAsExtension"];
                AddSchemaAsExtension = configList["GenerateSchemaAsExtension"];
                AddTypeAsClassification = configList["GenerateTypeAsClassification"];

                AddDataItemDataTypes = configList["GenerateDataItemDataTypes"];
                AddParentDataObject = configList["GenerateParentDataObject"];

                AddMetadataAsRelatedDataObject = configList["AddMetadataAsRelatedDataObject"];
                AddRelatedDataObjectsAsRelatedDataObject = configList["AddUpstreamDataObjectsAsRelatedDataObject"];
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
        public void SaveJsonConfigurationFile(GlobalParameters globalParameters)
        {
            try
            {
                // Creating the file
                var validationFile = new StringBuilder();
                validationFile.AppendLine("/* TEAM JSON Export Configuration Settings */");

                validationFile.AppendLine("GenerateDataObjectDataItems|" + AddDataObjectDataItems + "");
                validationFile.AppendLine("GenerateDatabaseAsExtension|" + AddDatabaseAsExtension + "");
                validationFile.AppendLine("GenerateSchemaAsExtension|" + AddSchemaAsExtension + "");
                validationFile.AppendLine("GenerateTypeAsClassification|" + AddTypeAsClassification + "");

                validationFile.AppendLine("GenerateDataItemDataTypes|" + AddDataItemDataTypes + "");
                validationFile.AppendLine("GenerateParentDataObject|" + AddParentDataObject + "");

                validationFile.AppendLine("AddMetadataAsRelatedDataObject|" +AddMetadataAsRelatedDataObject + "");
                validationFile.AppendLine("AddUpstreamDataObjectsAsRelatedDataObject|" + AddRelatedDataObjectsAsRelatedDataObject + "");

                // Closing off
                validationFile.AppendLine("/* End of file */");

                using (var outfile = new StreamWriter(globalParameters.ConfigurationPath + globalParameters.JsonExportConfigurationFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.FileExtension))
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
        public void CreateDummyJsonConfigurationFile(string fileName, EventLog eventLog)
        {
            if (!File.Exists(fileName))
            {
                var validationFile = new StringBuilder();

                validationFile.AppendLine("/* TEAM JSON Export File Settings */");

                // Data Object group.
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

                eventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"A new JSON extract configuration file was created ({fileName})."));
            }
            else
            {
                eventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"An existing JSON extract configuration file ({fileName}) was detected and used."));
            }
        }
    }
}
