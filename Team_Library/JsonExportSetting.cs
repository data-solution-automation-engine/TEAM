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
        #region Properties

        // Data Objects
        public string AddTypeAsClassificationToDataObject { get; set; }
        public string AddDataItemsToDataObject { get; set; }

        // Connections
        public string AddDatabaseAsExtensionToConnection { get; set; }
        public string AddSchemaAsExtensionToConnection { get; set; }

        // Data Items
        public string AddDataTypeToDataItem { get; set; }
        public string AddParentDataObjectToDataItem { get; set; }


        // Related Data Objects
        public string AddMetadataAsRelatedDataObject { get; set; }
        public string AddNextUpDataObjectsAsRelatedDataObject { get; set; }
        public string AddParentDataObjectAsRelatedDataObject { get; set; }

        // Data Vault
        public string AddDrivingKeyAsBusinessKeyExtension { get; set; }

        #endregion

        // Data Objects
        public bool IsAddTypeAsClassificationToDataObject()
        {
            bool returnValue = AddTypeAsClassificationToDataObject == "True";

            return returnValue;
        }

        public bool IsAddDataItemsToDataObject()
        {
            bool returnValue = AddDataItemsToDataObject == "True";

            return returnValue;
        }

        // Connections
        public bool IsAddDatabaseAsExtensionToConnection()
        {
            bool returnValue = AddDatabaseAsExtensionToConnection == "True";

            return returnValue;
        }

        public bool IsAddSchemaAsExtensionToConnection()
        {
            bool returnValue = AddSchemaAsExtensionToConnection == "True";

            return returnValue;
        }

        // Data Items
        public bool IsAddDataTypeToDataItem()
        {
            bool returnValue = AddDataTypeToDataItem == "True";

            return returnValue;
        }

        public bool IsAddParentDataObjectToDataItem()
        {
            bool returnValue = AddParentDataObjectToDataItem == "True";

            return returnValue;
        }

        // Related Data Objects
        public bool IsAddMetadataAsRelatedDataObject()
        {
            bool returnValue = AddMetadataAsRelatedDataObject == "True";

            return returnValue;
        }

        public bool IsAddNextUpDataObjectsAsRelatedDataObject()
        {
            bool returnValue = AddNextUpDataObjectsAsRelatedDataObject == "True";

            return returnValue;
        }

        public bool IsAddParentDataObjectAsRelatedDataObject()
        {
            bool returnValue = AddParentDataObjectAsRelatedDataObject == "True";

            return returnValue;
        }

        // Data Vault.
        public bool IsAddDrivingKeyAsBusinessKeyExtension()
        {
            bool returnValue = AddDrivingKeyAsBusinessKeyExtension == "True";

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

                AddDataItemsToDataObject = configList["AddDataItemsToDataObject"];
                AddDatabaseAsExtensionToConnection = configList["AddDatabaseAsExtensionToConnection"];
                AddSchemaAsExtensionToConnection = configList["AddSchemaAsExtensionToConnection"];
                AddTypeAsClassificationToDataObject = configList["AddTypeAsClassificationToDataObject"];

                AddDataTypeToDataItem = configList["AddDataTypeToDataItem"];
                AddParentDataObjectToDataItem = configList["AddParentDataObjectToDataItem"];

                AddMetadataAsRelatedDataObject = configList["AddMetadataAsRelatedDataObject"];
                AddNextUpDataObjectsAsRelatedDataObject = configList["AddNextUpDataObjectsAsRelatedDataObject"];
                AddParentDataObjectAsRelatedDataObject = configList["AddParentDataObjectAsRelatedDataObject"];

                AddDrivingKeyAsBusinessKeyExtension = configList["AddDrivingKeyAsBusinessKeyExtension"];
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

                validationFile.AppendLine("AddTypeAsClassificationToDataObject|" + AddTypeAsClassificationToDataObject + "");
                validationFile.AppendLine("AddDataItemsToDataObject|" + AddDataItemsToDataObject + "");

                validationFile.AppendLine("AddDatabaseAsExtensionToConnection|" + AddDatabaseAsExtensionToConnection + "");
                validationFile.AppendLine("AddSchemaAsExtensionToConnection|" + AddSchemaAsExtensionToConnection + "");
                
                validationFile.AppendLine("AddDataTypeToDataItem|" + AddDataTypeToDataItem + "");
                validationFile.AppendLine("AddParentDataObjectToDataItem|" + AddParentDataObjectToDataItem + "");

                validationFile.AppendLine("AddMetadataAsRelatedDataObject|" + AddMetadataAsRelatedDataObject + "");
                validationFile.AppendLine("AddNextUpDataObjectsAsRelatedDataObject|" + AddNextUpDataObjectsAsRelatedDataObject + "");
                validationFile.AppendLine("AddParentDataObjectAsRelatedDataObject|" + AddParentDataObjectAsRelatedDataObject + "");

                validationFile.AppendLine("AddDrivingKeyAsBusinessKeyExtension|" + AddDrivingKeyAsBusinessKeyExtension + "");

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
                validationFile.AppendLine("AddTypeAsClassificationToDataObject|True");
                validationFile.AppendLine("AddDataItemsToDataObject|True");
                // Connections
                validationFile.AppendLine("AddDatabaseAsExtensionToConnection|True");
                validationFile.AppendLine("AddSchemaAsExtensionToConnection|True");
                
                // Data Item group.
                validationFile.AppendLine("AddDataTypeToDataItem|True");
                validationFile.AppendLine("AddParentDataObjectToDataItem|True");

                // Related Data Objects.
                validationFile.AppendLine("AddMetadataAsRelatedDataObject|False");
                validationFile.AppendLine("AddNextUpDataObjectsAsRelatedDataObject|True");
                validationFile.AppendLine("AddParentDataObjectAsRelatedDataObject|False");

                // Data Vault specific.
                validationFile.AppendLine("AddDrivingKeyAsBusinessKeyExtension|False");

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
