using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TEAM
{
    public partial class FormManageConfiguration : FormBase
    {
        public FormManageConfiguration()
        {
            InitializeComponent();
        }

        public FormManageConfiguration(FormMain parent) : base(parent)
        {
            InitializeComponent();

            //Make sure the root directories exist, based on hard-coded (tool) parameters
            //Also create the initial file with the configuration if it doesn't exist already
            EnvironmentConfiguration.InitialisePath();

            //Set the root path (in the base form), to make sure the application is able to locate the path file and load it
            InitialiseRootPath();

            var configurationSettings = new ConfigurationSettings();

            // Load the configuration file using the paths retrieved from the application root contents (configuration path)
            try
            {
                LocalInitialiseConnections(configurationSettings.ConfigurationPath + GlobalParameters.ConfigfileName);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText("Errors occured trying to load the configuration file, the message is " + ex + ". No default values were loaded. \r\n\r\n");
            }
        }


        /// <summary>
        /// This method will load an existing configuration file and display the values on the form, or create a new dummy one if not available
        /// </summary>
        /// <param name="chosenFile"></param>
        private void LocalInitialiseConnections(string chosenFile)
        {
            // If the config file does not exist yet, create it by calling the EnvironmentConfiguration Class
            if (!File.Exists(chosenFile))
            {
                var newEnvironmentConfiguration = new EnvironmentConfiguration();
                newEnvironmentConfiguration.CreateNewEnvironmentConfiguration(chosenFile);
            }


            // Open the configuration file
            var configList = new Dictionary<string, string>();
            var fs = new FileStream(chosenFile, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs);

            try
            {
                string textline;
                while ((textline = sr.ReadLine()) != null)
                {
                    if (textline.IndexOf(@"/*", StringComparison.Ordinal) == -1)
                    {
                        var line = textline.Split('|');
                        configList.Add(line[0], line[1]);
                    }
                }

                sr.Close();
                fs.Close();


                //Replace values for formatting and connection string layout
                var connectionStringOmd = configList["connectionStringMetadata"];
                connectionStringOmd = connectionStringOmd.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringSource = configList["connectionStringSource"];
                connectionStringSource = connectionStringSource.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringStg = configList["connectionStringStaging"];
                connectionStringStg = connectionStringStg.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringHstg = configList["connectionStringPersistentStaging"];
                connectionStringHstg = connectionStringHstg.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringInt = configList["connectionStringIntegration"];
                connectionStringInt = connectionStringInt.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringPres = configList["connectionStringPresentation"];
                connectionStringPres = connectionStringPres.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");


                //Paths
                textBoxOutputPath.Text = configList["OutputPath"];
                textBoxConfigurationPath.Text = configList["ConfigurationPath"];

                //Connections
                textBoxIntegrationConnection.Text = connectionStringInt;
                textBoxPSAConnection.Text = connectionStringHstg;
                textBoxSourceConnection.Text = connectionStringSource;
                textBoxStagingConnection.Text = connectionStringStg;
                textBoxMetadataConnection.Text = connectionStringOmd;
                textBoxPresentationConnection.Text = connectionStringPres;

                //DWH settings
                textBoxHubTablePrefix.Text = configList["HubTablePrefix"];
                textBoxSatPrefix.Text = configList["SatTablePrefix"];
                textBoxLinkTablePrefix.Text = configList["LinkTablePrefix"];
                textBoxLinkSatPrefix.Text = configList["LinkSatTablePrefix"];
                textBoxDWHKeyIdentifier.Text = configList["KeyIdentifier"];
                textBoxSchemaName.Text = configList["SchemaName"];
                textBoxEventDateTime.Text = configList["EventDateTimeStamp"];
                textBoxLDST.Text = configList["LoadDateTimeStamp"];
                textBoxExpiryDateTimeName.Text = configList["ExpiryDateTimeStamp"];
                textBoxChangeDataCaptureIndicator.Text = configList["ChangeDataIndicator"];
                textBoxRecordSource.Text = configList["RecordSourceAttribute"];
                textBoxETLProcessID.Text = configList["ETLProcessID"];
                textBoxETLUpdateProcessID.Text = configList["ETLUpdateProcessID"];
                textBoxSourcePrefix.Text = configList["SourceSystemPrefix"];
                textBoxStagingAreaPrefix.Text = configList["StagingAreaPrefix"];
                textBoxPSAPrefix.Text = configList["PersistentStagingAreaPrefix"];
                textBoxSourceRowId.Text = configList["RowID"];
                textBoxSourceDatabase.Text = configList["SourceDatabase"];
                textBoxStagingDatabase.Text = configList["StagingDatabase"];
                textBoxPSADatabase.Text = configList["PersistentStagingDatabase"];
                textBoxIntegrationDatabase.Text = configList["IntegrationDatabase"];
                textBoxPresentationDatabase.Text = configList["PresentationDatabase"];
                textBoxRecordChecksum.Text = configList["RecordChecksum"];
                textBoxCurrentRecordAttributeName.Text = configList["CurrentRecordAttribute"];
                textBoxAlternativeRecordSource.Text = configList["AlternativeRecordSource"];
                textBoxHubAlternativeLDTSAttribute.Text = configList["AlternativeHubLDTS"];
                textBoxSatelliteAlternativeLDTSAttribute.Text = configList["AlternativeSatelliteLDTS"];
                textBoxLogicalDeleteAttributeName.Text = configList["LogicalDeleteAttribute"];
                textBoxLinkedServer.Text = configList["LinkedServerName"];

                //Checkbox setting based on loaded configuration
                CheckBox myConfigurationCheckBox;

                if (configList["AlternativeRecordSourceFunction"] == "False")
                {
                    myConfigurationCheckBox = checkBoxAlternativeRecordSource;
                    myConfigurationCheckBox.Checked = false;
                    textBoxAlternativeRecordSource.Enabled = false;
                }
                else
                {
                    myConfigurationCheckBox = checkBoxAlternativeRecordSource;
                    myConfigurationCheckBox.Checked = true; 
                }

                if (configList["AlternativeHubLDTSFunction"] == "False")
                {
                    myConfigurationCheckBox = checkBoxAlternativeHubLDTS;
                    myConfigurationCheckBox.Checked = false;
                    textBoxHubAlternativeLDTSAttribute.Enabled = false;
                }
                else
                {
                    myConfigurationCheckBox = checkBoxAlternativeHubLDTS;
                    myConfigurationCheckBox.Checked = true;
                }

                if (configList["AlternativeSatelliteLDTSFunction"] == "False")
                {
                    myConfigurationCheckBox = checkBoxAlternativeSatLDTS;
                    myConfigurationCheckBox.Checked = false;
                    textBoxSatelliteAlternativeLDTSAttribute.Enabled = false;
                }
                else
                {
                    myConfigurationCheckBox = checkBoxAlternativeSatLDTS;
                    myConfigurationCheckBox.Checked = true;
                }
                

                //Radiobutton setting for prefix / suffix 
                RadioButton myTableRadioButton;

                if (configList["TableNamingLocation"] == "Prefix")
                {
                    myTableRadioButton = tablePrefixRadiobutton;
                    myTableRadioButton.Checked = true;
                }
                else
                {
                    myTableRadioButton = tableSuffixRadiobutton;
                    myTableRadioButton.Checked = true;
                }

                //Radiobutton settings for on key location
                RadioButton myKeyRadioButton;

                if (configList["KeyNamingLocation"] == "Prefix")
                {
                    myKeyRadioButton = keyPrefixRadiobutton;
                    myKeyRadioButton.Checked = true;
                }
                else
                {
                    myKeyRadioButton = keySuffixRadiobutton;
                    myKeyRadioButton.Checked = true;
                }

                //Radiobutton settings for PSA Natural Key determination
                RadioButton myPsaBusinessKeyLocation;

                if (configList["PSAKeyLocation"] == "PrimaryKey")
                {
                    myPsaBusinessKeyLocation = radioButtonPSABusinessKeyPK;
                    myPsaBusinessKeyLocation.Checked = true;
                }
                else
                {
                    myPsaBusinessKeyLocation = radioButtonPSABusinessKeyIndex;
                    myPsaBusinessKeyLocation.Checked = true;
                }

                //Radiobutton settings for repository type
                RadioButton myMetadatarepositoryType;

                if (configList["metadataRepositoryType"] == "JSON")
                {
                    myMetadatarepositoryType = radioButtonJSON;
                    myMetadatarepositoryType.Checked = true;
                }
                else
                {
                    myMetadatarepositoryType = radioButtonSQLServer;
                    myMetadatarepositoryType.Checked = true;
                }


                // Also commit the values to memory
                UpdateConfigurationInMemory();

                richTextBoxInformation.AppendText(@"The file " + chosenFile + " was uploaded successfully. \r\n\r\n");
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText("\r\n\r\nAn error occured while loading the configuration file. The original error is: '" + ex.Message + "'");
            }
        }


        /// <summary>
        ///    Open the Windows Explorer (directory) using the value available as Output Directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openOutputDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(textBoxOutputPath.Text);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occured while attempting to open the output directory. The error message is: "+ex;
            }
        }


        /// <summary>
        ///    Select a configuration file from disk, apply this to memory and display the values on the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var configurationSettings = new ConfigurationSettings();
            var configurationPath = configurationSettings.ConfigurationPath; 
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Configuration File",
                Filter = @"Text files|*.txt",
                InitialDirectory = @""+configurationPath+""
            };

            if (theDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                var myStream = theDialog.OpenFile();

                using (myStream)
                {
                    richTextBoxInformation.Clear();
                    var chosenFile = theDialog.FileName;
                    LocalInitialiseConnections(chosenFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message, "An issues has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        ///    Close the Configuration Settings Form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        /// <summary>
        ///    Commit the changes to memory, save the configuration settings to disk and create a backup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Update the root path file, part of the core solution to be able to store the config and output path
            var rootPathConfigurationFile = new StringBuilder();
            rootPathConfigurationFile.AppendLine("/* TEAM File Path Settings */");
            rootPathConfigurationFile.AppendLine("/* Saved at " + DateTime.Now + " */");
            rootPathConfigurationFile.AppendLine("ConfigurationPath|" + textBoxConfigurationPath.Text + "");
            rootPathConfigurationFile.AppendLine("OutputPath|" + textBoxOutputPath.Text + "");
            rootPathConfigurationFile.AppendLine("/* End of file */");

            using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + GlobalParameters.PathfileName))
            {
                outfile.Write(rootPathConfigurationFile.ToString());
                outfile.Close();
            }


            // Create a file backup for the configuration file
            try
            {
                EnvironmentConfiguration.CreateEnvironmentConfigurationBackupFile();
                richTextBoxInformation.Text = "A backup of the current configuration was made in " + textBoxConfigurationPath.Text + ".";
            }
            catch (Exception)
            {
                richTextBoxInformation.Text = "TEAM was unable to create a backup of the configuration file.";
            }

            
            // Update the in-memory variables for use throughout the application, to commit the saved changes for runtime use. 
            // This is needed before saving to disk, as the EnvironmentConfiguration Class retrieves the values from memory.
            UpdateConfigurationInMemory();


            // Save the information 
            EnvironmentConfiguration.SaveEnvironmentConfiguration();
        }


        /// <summary>
        ///    Retrieve the information from the Configuration Settings from and commit these to memory
        /// </summary>
        private void UpdateConfigurationInMemory()
        {
            var configurationSettings = new ConfigurationSettings();

            configurationSettings.SourceDatabaseName = textBoxSourceDatabase.Text;
            configurationSettings.StagingDatabaseName = textBoxStagingDatabase.Text;
            configurationSettings.PsaDatabaseName = textBoxPSADatabase.Text;
            configurationSettings.IntegrationDatabaseName = textBoxIntegrationDatabase.Text;
            configurationSettings.PresentationDatabaseName = textBoxPresentationDatabase.Text;

            configurationSettings.ConnectionStringSource = textBoxSourceConnection.Text;
            configurationSettings.ConnectionStringStg = textBoxStagingConnection.Text;
            configurationSettings.ConnectionStringHstg = textBoxPSAConnection.Text;
            configurationSettings.ConnectionStringInt = textBoxIntegrationConnection.Text;
            configurationSettings.ConnectionStringOmd = textBoxMetadataConnection.Text;
            configurationSettings.ConnectionStringPres = textBoxPresentationConnection.Text;

            if (radioButtonJSON.Checked)
            {
                configurationSettings.metadataRepositoryType = "JSON";
            }
            else if (radioButtonSQLServer.Checked)
            {
                configurationSettings.metadataRepositoryType = "SQLServer";
            }
            else
            {
                richTextBoxInformation.AppendText("Issues storing the metadata repository type. Is one of the radio buttons checked?");
            }

            configurationSettings.OutputPath = textBoxOutputPath.Text;
            configurationSettings.ConfigurationPath = textBoxConfigurationPath.Text;

            configurationSettings.SourceSystemPrefix = textBoxSourcePrefix.Text;
            configurationSettings.StgTablePrefixValue = textBoxStagingAreaPrefix.Text;
            configurationSettings.PsaTablePrefixValue = textBoxPSAPrefix.Text;
            configurationSettings.HubTablePrefixValue = textBoxHubTablePrefix.Text;
            configurationSettings.SatTablePrefixValue = textBoxSatPrefix.Text;
            configurationSettings.LinkTablePrefixValue = textBoxLinkTablePrefix.Text;
            configurationSettings.LsatPrefixValue = textBoxLinkSatPrefix.Text;

            if (keyPrefixRadiobutton.Checked)
            {
                configurationSettings.KeyNamingLocation = "Prefix";
            }
            else if (keySuffixRadiobutton.Checked)
            {
                configurationSettings.KeyNamingLocation = "Suffix";
            }
            else
            {
                richTextBoxInformation.AppendText("Issues storing the key location (prefix/suffix). Is one of the radio buttons checked?");
            }

            configurationSettings.DwhKeyIdentifier = textBoxDWHKeyIdentifier.Text;

            configurationSettings.SchemaName = textBoxSchemaName.Text;
            configurationSettings.RowIdAttribute = textBoxSourceRowId.Text;
            configurationSettings.EventDateTimeAttribute = textBoxEventDateTime.Text;
            configurationSettings.LoadDateTimeAttribute = textBoxLDST.Text;
            configurationSettings.ExpiryDateTimeAttribute = textBoxExpiryDateTimeName.Text;
            configurationSettings.ChangeDataCaptureAttribute = textBoxChangeDataCaptureIndicator.Text;
            configurationSettings.RecordSourceAttribute = textBoxRecordSource.Text;
            configurationSettings.EtlProcessAttribute = textBoxETLProcessID.Text;
            configurationSettings.EtlProcessUpdateAttribute = textBoxETLUpdateProcessID.Text;
            configurationSettings.LogicalDeleteAttribute = textBoxLogicalDeleteAttributeName.Text;
            configurationSettings.LinkedServer = textBoxLinkedServer.Text;

            if (tablePrefixRadiobutton.Checked)
            {
                configurationSettings.TableNamingLocation = "Prefix";
            }
            else if (tableSuffixRadiobutton.Checked)
            {
                configurationSettings.TableNamingLocation = "Suffix";
            }
            else
            {
                richTextBoxInformation.AppendText("Issues storing the table prefix location (prefix/suffix). Is one of the radio buttons checked?");
            }

            if (radioButtonPSABusinessKeyIndex.Checked)
            {
                configurationSettings.PsaKeyLocation = "UniqueIndex";
            }
            else if (radioButtonPSABusinessKeyPK.Checked)
            {
                configurationSettings.PsaKeyLocation = "PrimaryKey";
            }
            else
            {
                richTextBoxInformation.AppendText("Issues storing the table prefix location (prefix/suffix). Is one of the radio buttons checked?");
            }

        }


        /// <summary>
        ///    Open the Windows Explorer (directory) using the value available as Configuration Directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openConfigurationDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(textBoxConfigurationPath.Text);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occured while attempting to open the configuration directory. The error message is: " + ex;
            }
        }
    }
}
