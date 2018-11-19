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
        private bool _formLoading = true;

        public FormManageConfiguration()
        {
            InitializeComponent();
        }

        public FormManageConfiguration(FormMain parent) : base(parent)
        {
            InitializeComponent();

            //Make sure the root directories exist, based on hard-coded (tool) parameters
            //Also create the initial file with the configuration if it doesn't exist already
            EnvironmentConfiguration.InitialiseRootPath();

            // Set the core TEAM (path) file using the information retrieved from memory. These values were loaded into memory from the path file in the main form.
            //Dev or prod environment (working environment)
            RadioButton radioButtonWorkingEnvironment;
            if (GlobalParameters.WorkingEnvironment == "Development")
            {
                radioButtonWorkingEnvironment = radioButtonDevelopment;
                radioButtonWorkingEnvironment.Checked = true;
            }
            else if (GlobalParameters.WorkingEnvironment == "Production")
            {
                radioButtonWorkingEnvironment = radioButtonProduction;
                radioButtonWorkingEnvironment.Checked = true;
            }

            //Paths
            textBoxOutputPath.Text = GlobalParameters.OutputPath;
            textBoxConfigurationPath.Text = GlobalParameters.ConfigurationPath;

            // Load the configuration file using the paths retrieved from the application root contents (configuration path)
            try
            {
                LocalInitialiseConnections(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigfileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText("Errors occured trying to load the configuration file, the message is " + ex + ". No default values were loaded. \r\n\r\n");
            }

            _formLoading = false;
        }


        /// <summary>
        ///    This method will load an existing configuration file and display the values on the form, or create a new dummy one if not available
        /// </summary>
        /// <param name="chosenFile"></param>
        private void LocalInitialiseConnections(string chosenFile)
        {
            // If the config file does not exist yet, create it by calling the EnvironmentConfiguration Class
            if (!File.Exists(chosenFile))
            {
                var newEnvironmentConfiguration = new EnvironmentConfiguration();
                newEnvironmentConfiguration.CreateDummyEnvironmentConfiguration(chosenFile);
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
                if (textBoxOutputPath.Text != "")
                {
                    Process.Start(textBoxOutputPath.Text);
                }
                else
                {
                    richTextBoxInformation.Text =
                        "There is no value given for the Output Path. Please enter a valid path name.";
                }
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
            var theDialog = new OpenFileDialog
            {
                Title = @"Open Configuration File",
                Filter = @"Text files|*.txt",
                InitialDirectory = @""+ GlobalParameters.ConfigurationPath+""
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
            string workingEnvironment = "";

            if (radioButtonDevelopment.Checked)
            {
                workingEnvironment = "Development";
            }
            else if (radioButtonProduction.Checked)
            {
                workingEnvironment = "Production";
            }
            else
            {
                MessageBox.Show("An error occurred: neither the Development or Production radiobutton was selected.", "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Update the root path file, part of the core solution to be able to store the config and output path
            var rootPathConfigurationFile = new StringBuilder();
            rootPathConfigurationFile.AppendLine("/* TEAM File Path Settings */");
            rootPathConfigurationFile.AppendLine("/* Saved at " + DateTime.Now + " */");
            rootPathConfigurationFile.AppendLine("ConfigurationPath|" + textBoxConfigurationPath.Text + "");
            rootPathConfigurationFile.AppendLine("OutputPath|" + textBoxOutputPath.Text + "");
            rootPathConfigurationFile.AppendLine("WorkingEnvironment|" + workingEnvironment + "");
            rootPathConfigurationFile.AppendLine("/* End of file */");

            using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + GlobalParameters.PathfileName + GlobalParameters.FileExtension))
            {
                outfile.Write(rootPathConfigurationFile.ToString());
                outfile.Close();
            }

            // Update the paths in memory
            GlobalParameters.OutputPath = textBoxOutputPath.Text;
            GlobalParameters.ConfigurationPath = textBoxConfigurationPath.Text;

            GlobalParameters.WorkingEnvironment = workingEnvironment;

            // Make sure the new paths as updated are available upon save for backup etc.
            EnvironmentConfiguration.InitialiseConfigurationPath();


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
            EnvironmentConfiguration.SaveConfigurationFile();
        }


        /// <summary>
        ///    Retrieve the information from the Configuration Settings from and commit these to memory
        /// </summary>
        private void UpdateConfigurationInMemory()
        {
            ConfigurationSettings.SourceDatabaseName = textBoxSourceDatabase.Text;
            ConfigurationSettings.StagingDatabaseName = textBoxStagingDatabase.Text;
            ConfigurationSettings.PsaDatabaseName = textBoxPSADatabase.Text;
            ConfigurationSettings.IntegrationDatabaseName = textBoxIntegrationDatabase.Text;
            ConfigurationSettings.PresentationDatabaseName = textBoxPresentationDatabase.Text;
            
            ConfigurationSettings.ConnectionStringSource = textBoxSourceConnection.Text;
            ConfigurationSettings.ConnectionStringStg = textBoxStagingConnection.Text;
            ConfigurationSettings.ConnectionStringHstg = textBoxPSAConnection.Text;
            ConfigurationSettings.ConnectionStringInt = textBoxIntegrationConnection.Text;
            ConfigurationSettings.ConnectionStringOmd = textBoxMetadataConnection.Text;
            ConfigurationSettings.ConnectionStringPres = textBoxPresentationConnection.Text;

            if (radioButtonJSON.Checked)
            {
                ConfigurationSettings.MetadataRepositoryType = "JSON";
            }
            else if (radioButtonSQLServer.Checked)
            {
                ConfigurationSettings.MetadataRepositoryType = "SQLServer";
            }
            else
            {
                richTextBoxInformation.AppendText("Issues storing the metadata repository type. Is one of the radio buttons checked?");
            }

            GlobalParameters.OutputPath = textBoxOutputPath.Text;
            GlobalParameters.ConfigurationPath = textBoxConfigurationPath.Text;

            ConfigurationSettings.SourceSystemPrefix = textBoxSourcePrefix.Text;
            ConfigurationSettings.StgTablePrefixValue = textBoxStagingAreaPrefix.Text;
            ConfigurationSettings.PsaTablePrefixValue = textBoxPSAPrefix.Text;
            ConfigurationSettings.HubTablePrefixValue = textBoxHubTablePrefix.Text;
            ConfigurationSettings.SatTablePrefixValue = textBoxSatPrefix.Text;
            ConfigurationSettings.LinkTablePrefixValue = textBoxLinkTablePrefix.Text;
            ConfigurationSettings.LsatPrefixValue = textBoxLinkSatPrefix.Text;

            if (keyPrefixRadiobutton.Checked)
            {
                ConfigurationSettings.KeyNamingLocation = "Prefix";
            }
            else if (keySuffixRadiobutton.Checked)
            {
                ConfigurationSettings.KeyNamingLocation = "Suffix";
            }
            else
            {
                richTextBoxInformation.AppendText("Issues storing the key location (prefix/suffix). Is one of the radio buttons checked?");
            }

            ConfigurationSettings.DwhKeyIdentifier = textBoxDWHKeyIdentifier.Text;
            ConfigurationSettings.SchemaName = textBoxSchemaName.Text;
            ConfigurationSettings.RowIdAttribute = textBoxSourceRowId.Text;
            ConfigurationSettings.EventDateTimeAttribute = textBoxEventDateTime.Text;
            ConfigurationSettings.LoadDateTimeAttribute = textBoxLDST.Text;
            ConfigurationSettings.ExpiryDateTimeAttribute = textBoxExpiryDateTimeName.Text;
            ConfigurationSettings.ChangeDataCaptureAttribute = textBoxChangeDataCaptureIndicator.Text;
            ConfigurationSettings.RecordSourceAttribute = textBoxRecordSource.Text;
            ConfigurationSettings.EtlProcessAttribute = textBoxETLProcessID.Text;
            ConfigurationSettings.EtlProcessUpdateAttribute = textBoxETLUpdateProcessID.Text;
            ConfigurationSettings.LogicalDeleteAttribute = textBoxLogicalDeleteAttributeName.Text;
            ConfigurationSettings.LinkedServer = textBoxLinkedServer.Text;

            if (tablePrefixRadiobutton.Checked)
            {
                ConfigurationSettings.TableNamingLocation = "Prefix";
            }
            else if (tableSuffixRadiobutton.Checked)
            {
                ConfigurationSettings.TableNamingLocation = "Suffix";
            }
            else
            {
                richTextBoxInformation.AppendText("Issues storing the table prefix location (prefix/suffix). Is one of the radio buttons checked?");
            }

            if (radioButtonPSABusinessKeyIndex.Checked)
            {
                ConfigurationSettings.PsaKeyLocation = "UniqueIndex";
            }
            else if (radioButtonPSABusinessKeyPK.Checked)
            {
                ConfigurationSettings.PsaKeyLocation = "PrimaryKey";
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
                if (textBoxConfigurationPath.Text != "")
                {
                    Process.Start(textBoxConfigurationPath.Text);
                }
                else
                {
                    richTextBoxInformation.Text =
                        "There is no value given for the Configuration Path. Please enter a valid path name.";
                }
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = "An error has occured while attempting to open the configuration directory. The error message is: " + ex;
            }
        }

        private void radioButtonDevelopment_CheckedChanged(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (rb.Checked && _formLoading == false)
                {
                    GlobalParameters.WorkingEnvironment = "Development";
                    //MessageBox.Show("Dev");
                    try
                    {
                        LocalInitialiseConnections(GlobalParameters.ConfigurationPath +
                                                   GlobalParameters.ConfigfileName + '_' +
                                                   GlobalParameters.WorkingEnvironment +
                                                   GlobalParameters.FileExtension);
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.AppendText(
                            "Errors occured trying to load the configuration file, the message is " + ex +
                            ". No default values were loaded. \r\n\r\n");
                    }

                }
            }
        }

        private void radioButtonProduction_CheckedChanged(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                if (rb.Checked && _formLoading == false)
                {
                    GlobalParameters.WorkingEnvironment = "Production";
                    //MessageBox.Show("Prod");
                    try
                    {
                        LocalInitialiseConnections(GlobalParameters.ConfigurationPath +
                                                   GlobalParameters.ConfigfileName + '_' +
                                                   GlobalParameters.WorkingEnvironment +
                                                   GlobalParameters.FileExtension);
                    }
                    catch (Exception ex)
                    {
                        richTextBoxInformation.AppendText(
                            "Errors occured trying to load the configuration file, the message is " + ex +
                            ". No default values were loaded. \r\n\r\n");
                    }
                }
            }
        }

        private void checkBoxAlternativeRecordSource_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAlternativeRecordSource.Checked)
            {
                textBoxAlternativeRecordSource.Enabled = true;
            }
            if (!checkBoxAlternativeRecordSource.Checked)
            {
                textBoxAlternativeRecordSource.Enabled = false;
            }
        }

        private void checkBoxAlternativeHubLDTS_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAlternativeHubLDTS.Checked)
            {
                textBoxHubAlternativeLDTSAttribute.Enabled = true;
            }
            if (!checkBoxAlternativeHubLDTS.Checked)
            {
                textBoxHubAlternativeLDTSAttribute.Enabled = false;
            }
        }

        private void checkBoxAlternativeSatLDTS_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAlternativeSatLDTS.Checked)
            {
                textBoxSatelliteAlternativeLDTSAttribute.Enabled = true;
            }

            if (!checkBoxAlternativeSatLDTS.Checked)
            {
                textBoxSatelliteAlternativeLDTSAttribute.Enabled = false;
            }
        }
    }
}
