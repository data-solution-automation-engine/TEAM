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

            // Make sure the configuration information is available in this form
            try
            {
                var configurationFileName = GlobalParameters.ConfigurationPath + GlobalParameters.JsonExportConfigurationFileName + '_' +
                                     GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension;

                // If the config file does not exist yet, create it by calling the EnvironmentConfiguration Class
                if (!File.Exists(configurationFileName))
                {
                    LocalTeamEnvironmentConfiguration.CreateDummyJsonExtractConfigurationFile(configurationFileName);
                }

                // Load the validation settings file using the paths retrieved from the application root contents (configuration path)
                LocalTeamEnvironmentConfiguration.LoadValidationFile(configurationFileName);

                richTextBoxInformation.Text += "The Json extract configuration file " + configurationFileName + " has been loaded.";

                // Apply the values to the form
                LocalInitialiseJsonExtractSettings();
            }
            catch (Exception)
            {
                // Do nothing
            }

        }

        /// <summary>
        /// This method will update the validation values on the form
        /// </summary>
        private void LocalInitialiseJsonExtractSettings()
        {
            // Source data types
            switch (JsonExportSettings.GenerateSourceDataItemTypes)
            {
                case "True":
                    checkBoxSourceDataType.Checked = true;
                    break;
                case "False":
                    checkBoxSourceDataType.Checked = false;
                    break;
                default:
                    MessageBox.Show("There is something wrong with the checkbox values, only true and false are allowed but this was encountered: " + JsonExportSettings.GenerateSourceDataItemTypes + ". Please check the configuration file (TEAM_<environment>_jsonconfiguration.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            // Target data types
            switch (JsonExportSettings.GenerateTargetDataItemTypes)
            {
                case "True":
                    checkBoxTargetDataType.Checked = true;
                    break;
                case "False":
                    checkBoxTargetDataType.Checked = false;
                    break;
                default:
                    MessageBox.Show("There is something wrong with the checkbox values, only true and false are allowed but this was encountered: " + JsonExportSettings.GenerateTargetDataItemTypes + ". Please check the configuration file (TEAM_<environment>_jsonconfiguration.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            // Source connection
            switch (JsonExportSettings.GenerateSourceDataObjectConnection)
            {
                case "True":
                    checkBoxSourceConnectionKey.Checked = true;
                    break;
                case "False":
                    checkBoxSourceConnectionKey.Checked = false;
                    break;
                default:
                    MessageBox.Show("There is something wrong with the checkbox values, only true and false are allowed but this was encountered: " + JsonExportSettings.GenerateSourceDataObjectConnection + ". Please check the configuration file (TEAM_<environment>_jsonconfiguration.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            // Target connection
            switch (JsonExportSettings.GenerateTargetDataObjectConnection)
            {
                case "True":
                    checkBoxTargetConnectionKey.Checked = true;
                    break;
                case "False":
                    checkBoxTargetConnectionKey.Checked = false;
                    break;
                default:
                    MessageBox.Show("There is something wrong with the checkbox values, only true and false are allowed but this was encountered: " + JsonExportSettings.GenerateTargetDataObjectConnection + ". Please check the configuration file (TEAM_<environment>_jsonconfiguration.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            // Database Extension
            switch (JsonExportSettings.GenerateDatabaseAsExtension)
            {
                case "True":
                    checkBoxDatabaseExtension.Checked = true;
                    break;
                case "False":
                    checkBoxDatabaseExtension.Checked = false;
                    break;
                default:
                    MessageBox.Show("There is something wrong with the checkbox values, only true and false are allowed but this was encountered: " + JsonExportSettings.GenerateDatabaseAsExtension + ". Please check the configuration file (TEAM_<environment>_jsonconfiguration.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            // Schema Extension
            switch (JsonExportSettings.GenerateSchemaAsExtension)
            {
                case "True":
                    checkBoxSchemaExtension.Checked = true;
                    break;
                case "False":
                    checkBoxSchemaExtension.Checked = false;
                    break;
                default:
                    MessageBox.Show("There is something wrong with the checkbox values, only true and false are allowed but this was encountered: " + JsonExportSettings.GenerateSchemaAsExtension + ". Please check the configuration file (TEAM_<environment>_jsonconfiguration.txt)", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void openConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Json Configuration File",
                Filter = @"Text files|*.txt",
                InitialDirectory = @"" + GlobalParameters.ConfigurationPath + ""
            };

            if (theDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                var myStream = theDialog.OpenFile();

                using (myStream)
                {
                    richTextBoxInformation.Clear();
                    var chosenFile = theDialog.FileName;
                    
                    // Load from disk into memory
                    LocalTeamEnvironmentConfiguration.LoadJsonConfigurationFile(chosenFile);

                    // Update values on form
                    LocalInitialiseJsonExtractSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message, "An issues has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Save settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripMenuItemSaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                // Source data types
                var stringSourceDataTypes = "";
                stringSourceDataTypes = checkBoxSourceDataType.Checked ? "True" : "False";
                JsonExportSettings.GenerateSourceDataItemTypes = stringSourceDataTypes;


                // Target data types
                var stringTargetDataTypes = "";
                stringTargetDataTypes = checkBoxTargetDataType.Checked ? "True" : "False";
                JsonExportSettings.GenerateTargetDataItemTypes = stringTargetDataTypes;


                // Source connection
                var stringSourceConnection= "";
                stringSourceConnection = checkBoxSourceConnectionKey.Checked ? "True" : "False";
                JsonExportSettings.GenerateSourceDataObjectConnection = stringSourceConnection;


                // Target connection
                var stringTargetConnection = "";
                stringTargetConnection = checkBoxTargetConnectionKey.Checked ? "True" : "False";
                JsonExportSettings.GenerateTargetDataObjectConnection = stringTargetConnection;

                // Target connection
                var stringDatabaseExtension = "";
                stringDatabaseExtension = checkBoxDatabaseExtension.Checked ? "True" : "False";
                JsonExportSettings.GenerateDatabaseAsExtension = stringDatabaseExtension;

                // Target connection
                var stringSchemaExtension = "";
                stringSchemaExtension = checkBoxSchemaExtension.Checked ? "True" : "False";
                JsonExportSettings.GenerateSchemaAsExtension = stringSchemaExtension;

                // Write to disk
                LocalTeamEnvironmentConfiguration.SaveJsonConfigurationFile();

                richTextBoxInformation.Text = "The values have been successfully saved.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not write values to memory and disk. Original error: " + ex.Message, "An issues has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void openConfigurationDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(GlobalParameters.ConfigurationPath);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occurred while attempting to open the configuration directory. The error message is: " + ex;
            }
        }

        private void openOutputDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(GlobalParameters.OutputPath);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occurred while attempting to open the configuration directory. The error message is: " + ex;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
