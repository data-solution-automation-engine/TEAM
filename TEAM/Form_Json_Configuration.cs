using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TEAM
{
    public partial class FormJsonConfiguration : FormBase
    {
        private readonly FormManageMetadata _myParent;

        public FormJsonConfiguration(FormManageMetadata parent)
        {
            _myParent = parent;
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
        /// This method will update the validation values on the form.
        /// </summary>
        private void LocalInitialiseJsonExtractSettings()
        {
            int issueCounter = 0;

            #region Data Objects
            // GenerateTypeAsClassification
            EvaluateJsonExportCheckbox(checkBoxAddType, JsonExportSetting.AddTypeAsClassification, ref issueCounter);

            // GenerateDataObjectDataItems
            EvaluateJsonExportCheckbox(checkBoxDataObjectDataItems, JsonExportSetting.AddDataObjectDataItems, ref issueCounter);

            // GenerateDataObjectConnection
            switch (JsonExportSetting.AddDataObjectConnection)
            {
                case "True":
                    checkBoxSourceConnectionKey.Checked = true;

                    // GenerateDatabaseAsExtension
                    EvaluateJsonExportCheckbox(checkBoxDatabaseExtension, JsonExportSetting.AddDatabaseAsExtension, ref issueCounter);

                    // GenerateSchemaAsExtension
                    EvaluateJsonExportCheckbox(checkBoxSchemaExtension, JsonExportSetting.AddSchemaAsExtension, ref issueCounter);

                    break;
                case "False":
                    checkBoxSourceConnectionKey.Checked = false;
                    checkBoxDatabaseExtension.Visible = false; // Hide these checkboxes because they are related to the connection.
                    checkBoxSchemaExtension.Visible = false;

                    break;

                default:
                    richTextBoxJsonExportInformation.Text += JsonConfigurationWarning(checkBoxSourceConnectionKey.Text);
                    checkBoxSourceConnectionKey.Checked = false;
                    issueCounter++;
                    break;
            }
            #endregion

            #region Data Items
            // GenerateDataItemTypes
            EvaluateJsonExportCheckbox(checkBoxDataItemDataType, JsonExportSetting.AddDataItemDataTypes, ref issueCounter);

            // GenerateParentDataObject
            EvaluateJsonExportCheckbox(checkBoxDataItemAddParentDataObject, JsonExportSetting.AddParentDataObject, ref issueCounter);
            #endregion

            #region Related Data Objects
            // Add Metadata as object
            EvaluateJsonExportCheckbox(checkBoxAddMetadataConnection, JsonExportSetting.AddMetadataAsRelatedDataObject, ref issueCounter);

            // Add upstream connections as objects
            EvaluateJsonExportCheckbox(checkBoxNextUpDataObjects, JsonExportSetting.AddRelatedDataObjectsAsRelatedDataObject, ref issueCounter);
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
                // GenerateDataObjectConnection
                var stringSourceConnection = checkBoxSourceConnectionKey.Checked ? "True" : "False";
                JsonExportSetting.AddDataObjectConnection = stringSourceConnection;

                // GenerateDataObjectDataItems
                var stringDataObjectDataItems = checkBoxDataObjectDataItems.Checked ? "True" : "False";
                JsonExportSetting.AddDataObjectDataItems = stringDataObjectDataItems;

                // GenerateDatabaseAsExtension
                var stringDatabaseExtension = checkBoxDatabaseExtension.Checked ? "True" : "False";
                JsonExportSetting.AddDatabaseAsExtension = stringDatabaseExtension;

                // GenerateSchemaAsExtension
                var stringSchemaExtension = checkBoxSchemaExtension.Checked ? "True" : "False";
                JsonExportSetting.AddSchemaAsExtension = stringSchemaExtension;

                // GenerateTypeAsClassification
                var stringTypeClassification = checkBoxAddType.Checked ? "True" : "False";
                JsonExportSetting.AddTypeAsClassification = stringTypeClassification;

                // GenerateDataItemDataTypes
                var stringSourceDataTypes = checkBoxDataItemDataType.Checked ? "True" : "False";
                JsonExportSetting.AddDataItemDataTypes = stringSourceDataTypes;

                // GenerateParentDataObject
                var stringAddParentDataObject = checkBoxDataItemAddParentDataObject.Checked ? "True" : "False";
                JsonExportSetting.AddParentDataObject = stringAddParentDataObject;

                // AddMetadataAsRelatedDataObject
                var stringAddMetadataConnection = checkBoxAddMetadataConnection.Checked ? "True" : "False";
                JsonExportSetting.AddMetadataAsRelatedDataObject = stringAddMetadataConnection;

                // AddUpstreamDataObjectsAsRelatedDataObject
                var stringAddNextUpObjects = checkBoxNextUpDataObjects.Checked ? "True" : "False";
                JsonExportSetting.AddRelatedDataObjectsAsRelatedDataObject = stringAddNextUpObjects;

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

        private void checkBoxSourceConnectionKey_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBoxSourceConnectionKey.Checked)
            {
                checkBoxDatabaseExtension.Visible = false;
                checkBoxSchemaExtension.Visible = false;
            }

            if (checkBoxSourceConnectionKey.Checked)
            {
                checkBoxDatabaseExtension.Visible = true;
                checkBoxSchemaExtension.Visible = true;
            }
        }
    }
}
