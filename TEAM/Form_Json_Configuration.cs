using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TEAM
{
    public partial class FormJsonConfiguration : FormBase
    {
        public FormJsonConfiguration()
        {
            InitializeComponent();

            // Make sure the configuration information is available in this form.
            try
            {
                var jsonExportConfigurationFileName = globalParameters.ConfigurationPath + globalParameters.JsonExportConfigurationFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.FileExtension;

                // If the JSON export configuration file does not exist yet, create it.
                if (!File.Exists(jsonExportConfigurationFileName))
                {
                    JsonExportSetting.CreateDummyJsonConfigurationFile(jsonExportConfigurationFileName, TeamEventLog);
                }

                // Load the validation settings file using the paths retrieved from the application root contents (configuration path).
                JsonExportSetting.LoadJsonConfigurationFile(jsonExportConfigurationFileName);

                // ReSharper disable once LocalizableElement
                richTextBoxJsonExportInformation.Text += $"The JSON extract configuration file {jsonExportConfigurationFileName} has been loaded.\r\n";

                // Apply the values to the form.
                LocalInitialiseJsonExtractSettings();
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        /// <summary>
        /// Reusable / convenience information update for when a JSON export setting was not detected.
        /// </summary>
        /// <param name="settingName"></param>
        /// <returns></returns>
        private string JsonConfigurationWarning(string settingName)
        {
            return $"The value for '{settingName}' was not found. This needs to be either True (enabled) or False (disabled). The setting has been automatically disabled as a result. Please save your JSON export configuration file to persist this.\r\n";
        }

        /// <summary>
        /// This method will update the JSON export configuration values on the form.
        /// </summary>
        private void LocalInitialiseJsonExtractSettings()
        {
            int issueCounter = 0;

            #region Data Objects

            // AddTypeAsClassificationToDataObject
            EvaluateJsonExportCheckbox(checkBoxAddType, JsonExportSetting.AddTypeAsClassificationToDataObject, ref issueCounter);

            // AddDataItemsToDataObject
            EvaluateJsonExportCheckbox(checkBoxDataObjectDataItems, JsonExportSetting.AddDataItemsToDataObject, ref issueCounter);

            #endregion

            #region Connections

            // AddDatabaseAsExtensionToConnection
            EvaluateJsonExportCheckbox(checkBoxDatabaseExtension, JsonExportSetting.AddDatabaseAsExtensionToConnection, ref issueCounter);

            // AddSchemaAsExtensionToConnection
            EvaluateJsonExportCheckbox(checkBoxSchemaExtension, JsonExportSetting.AddSchemaAsExtensionToConnection, ref issueCounter);

            #endregion

            #region Data Items

            // AddDataTypeToDataItem
            EvaluateJsonExportCheckbox(checkBoxDataItemDataType, JsonExportSetting.AddDataTypeToDataItem, ref issueCounter);

            // AddParentDataObjectToDataItem
            EvaluateJsonExportCheckbox(checkBoxDataItemAddParentDataObject, JsonExportSetting.AddParentDataObjectToDataItem, ref issueCounter);

            #endregion

            #region Related Data Objects

            // AddMetadataAsRelatedDataObject
            EvaluateJsonExportCheckbox(checkBoxAddMetadataConnection, JsonExportSetting.AddMetadataAsRelatedDataObject, ref issueCounter);

            // AddNextUpDataObjectsAsRelatedDataObject
            EvaluateJsonExportCheckbox(checkBoxNextUpDataObjects, JsonExportSetting.AddNextUpDataObjectsAsRelatedDataObject, ref issueCounter);

            // AddParentDataObjectAsRelatedDataObject
            EvaluateJsonExportCheckbox(checkBoxAddParentDataObject, JsonExportSetting.AddParentDataObjectAsRelatedDataObject, ref issueCounter);

            #endregion

            // Report back the the user
            if (issueCounter > 0)
            {
                MessageBox.Show(@"Non-critical issues were detected loading the JSON export configuration file. Please check the information on the screen, review the settings and save again to resolve this. Otherwise, some settings may be ignored during the activation process.", @"An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        /// <summary>
        /// Reusable / convenience method to handle enabling of form checkboxes based on values retrieved from the configuration file.
        /// </summary>
        /// <param name="checkBox"></param>
        /// <param name="exportSettingValue"></param>
        /// <param name="issueCounter"></param>
        private void EvaluateJsonExportCheckbox(CheckBox checkBox, string exportSettingValue, ref int issueCounter)
        {
            switch (exportSettingValue)
            {
                case "True":
                    checkBox.Checked = true;
                    break;
                case "False":
                    checkBox.Checked = false;
                    break;
                default:
                    richTextBoxJsonExportInformation.Text += JsonConfigurationWarning(checkBox.Text);
                    checkBox.Checked = false;
                    issueCounter++;
                    break;
            }
        }

        private void openConfigurationFileToolStripMenuItem_Click(object sender, EventArgs args)
        {
            var jsonExportConfigurationFileName = globalParameters.ConfigurationPath + globalParameters.JsonExportConfigurationFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.FileExtension;

            try
            {
                Process.Start(jsonExportConfigurationFileName);

            }
            catch (Exception exception)
            {
                richTextBoxJsonExportInformation.Text += $@"An error has occurred while attempting to open the JSON export configuration file '{jsonExportConfigurationFileName}'. The error message is: '{exception.Message}'.";
            }
        }

        /// <summary>
        /// Save settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ToolStripMenuItemSaveSettings_Click(object sender, EventArgs args)
        {
            try
            {
                // AddDataItemsToDataObject
                var stringDataObjectDataItems = checkBoxDataObjectDataItems.Checked ? "True" : "False";
                JsonExportSetting.AddDataItemsToDataObject = stringDataObjectDataItems;

                // AddTypeAsClassificationToDataObject
                var stringTypeClassification = checkBoxAddType.Checked ? "True" : "False";
                JsonExportSetting.AddTypeAsClassificationToDataObject = stringTypeClassification;

                // AddDatabaseAsExtensionToConnection
                var stringDatabaseExtension = checkBoxDatabaseExtension.Checked ? "True" : "False";
                JsonExportSetting.AddDatabaseAsExtensionToConnection = stringDatabaseExtension;

                // AddSchemaAsExtensionToConnection
                var stringSchemaExtension = checkBoxSchemaExtension.Checked ? "True" : "False";
                JsonExportSetting.AddSchemaAsExtensionToConnection = stringSchemaExtension;
                
                // AddDataTypeToDataItem
                var stringSourceDataTypes = checkBoxDataItemDataType.Checked ? "True" : "False";
                JsonExportSetting.AddDataTypeToDataItem = stringSourceDataTypes;

                // AddParentDataObjectToDataItem
                var stringAddParentDataObject = checkBoxDataItemAddParentDataObject.Checked ? "True" : "False";
                JsonExportSetting.AddParentDataObjectToDataItem = stringAddParentDataObject;

                // AddMetadataAsRelatedDataObject
                var stringAddMetadataConnection = checkBoxAddMetadataConnection.Checked ? "True" : "False";
                JsonExportSetting.AddMetadataAsRelatedDataObject = stringAddMetadataConnection;

                // AddNextUpDataObjectsAsRelatedDataObject
                var stringAddNextUpObjects = checkBoxNextUpDataObjects.Checked ? "True" : "False";
                JsonExportSetting.AddNextUpDataObjectsAsRelatedDataObject = stringAddNextUpObjects;

                // AddParentDataObjectAsRelatedDataObject
                var stringParentDataObject = checkBoxAddParentDataObject.Checked ? "True" : "False";
                JsonExportSetting.AddParentDataObjectAsRelatedDataObject = stringParentDataObject;

                // AddDrivingKeyAsBusinessKeyExtension
                var stringDrivingKeyAsExtension = checkBoxAddDrivingKeyAsExtension.Checked ? "True" : "False";
                JsonExportSetting.AddDrivingKeyAsBusinessKeyExtension = stringDrivingKeyAsExtension;

                // Write to disk
                JsonExportSetting.SaveJsonConfigurationFile(globalParameters);

                richTextBoxJsonExportInformation.Text += "The values have been saved successfully.\r\n";
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Error: Could not write values to memory and disk. Original error: " + ex.Message, @"An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenConfigurationDirectoryToolStripMenuItem_Click(object sender, EventArgs args)
        {
            try
            {
                Process.Start(globalParameters.ConfigurationPath);
            }
            catch (Exception ex)
            {
                richTextBoxJsonExportInformation.Text = $@"An error has occurred while attempting to open the configuration directory. The error message is: '{ex.Message}'.";
            }
        }
        private void ExitToolStripMenuItem_Click(object sender, EventArgs args)
        {
            Close();
        }
    }
}
