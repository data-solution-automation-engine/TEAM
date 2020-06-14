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
        private FormMain parentFormMain;

        public FormManageConfiguration()
        {
            InitializeComponent();
        }
        public FormManageConfiguration(FormMain parent) : base(parent)
        {
            this.parentFormMain = parent;
            InitializeComponent();

            //Paths
            textBoxOutputPath.Text = GlobalParameters.OutputPath;
            textBoxConfigurationPath.Text = GlobalParameters.ConfigurationPath;
            
            // Adding tab pages to the Environment tabs.
            IntPtr localHandle = tabControlEnvironments.Handle;
            foreach (var environment in ConfigurationSettings.environmentDictionary)
            {
                // Adding tabs on the Tab Control
                var lastIndex = tabControlEnvironments.TabCount - 1;
                CustomTabPageEnvironment localCustomTabPage = new CustomTabPageEnvironment(environment.Value);
                localCustomTabPage.OnDeleteEnvironment += DeleteEnvironment;
                localCustomTabPage.OnSaveEnvironment += SaveEnvironment;
                localCustomTabPage.OnChangeMainText += UpdateMainInformationTextBox; 
                tabControlEnvironments.TabPages.Insert(lastIndex, localCustomTabPage);
                tabControlEnvironments.SelectedIndex = 0;

                // Adding items in the drop down list
                comboBoxEnvironments.Items.Add(new KeyValuePair<TeamWorkingEnvironment, string>(environment.Value, environment.Value.environmentKey));
                comboBoxEnvironments.DisplayMember = "Value";
            }

            comboBoxEnvironments.SelectedIndex = comboBoxEnvironments.FindStringExact(GlobalParameters.WorkingEnvironment);
            
            // Connection tabs for the specific environment.
            AddConnectionTabPages();

            comboBoxMetadataConnection.SelectedIndex = comboBoxMetadataConnection.FindStringExact(ConfigurationSettings.MetadataConnection.databaseConnectionKey);

            // Load the configuration file using the paths retrieved from the application root contents (configuration path)
            try
            {
                LocalInitialiseConnections(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigFileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText("Errors occured trying to load the configuration file, the message is " + ex + ". No default values were loaded. \r\n\r\n");
            }

            _formLoading = false;
        }

        /// <summary>
        /// Add Tabs to the Connections Tab Control based on the in-memory values (connection dictionary).
        /// </summary>
        private void AddConnectionTabPages()
        {
            IntPtr localHandle = tabControlConnections.Handle;
            foreach (var connection in ConfigurationSettings.connectionDictionary)
            {
                // Adding tabs on the Tab Control
                var lastIndex = tabControlConnections.TabCount - 1;
                CustomTabPageConnection localCustomTabPage = new CustomTabPageConnection(connection.Value);
                localCustomTabPage.OnDeleteConnection += DeleteConnection;
                localCustomTabPage.OnChangeMainText += UpdateMainInformationTextBox;
                localCustomTabPage.OnSaveConnection += SaveConnection;
                tabControlConnections.TabPages.Insert(lastIndex, localCustomTabPage);
                tabControlConnections.SelectedIndex = 0;

                // Adding items in the drop down list
                comboBoxMetadataConnection.Items.Add(new KeyValuePair<TeamConnectionProfile, string>(connection.Value, connection.Value.databaseConnectionKey));
                comboBoxMetadataConnection.ValueMember = "Key";
                comboBoxMetadataConnection.DisplayMember = "Value";
            }
        }

        /// <summary>
        /// Delegate event handler from the 'main' form to pass back information when the environment is updated.
        /// </summary>
        public event EventHandler<MyWorkingEnvironmentEventArgs> OnUpdateEnvironment = delegate { };

        public void UpdateEnvironment(TeamWorkingEnvironment environment)
        {
            OnUpdateEnvironment(this, new MyWorkingEnvironmentEventArgs(environment));
        }
        
        /// <summary>
        /// This method will load an existing configuration file and display the values on the form, or create a new dummy one if not available.
        /// </summary>
        /// <param name="chosenFile"></param>
        private void LocalInitialiseConnections(string chosenFile)
        {
            // If the config file does not exist yet, create it by calling the EnvironmentConfiguration Class.
            if (!File.Exists(chosenFile))
            {
                EnvironmentConfiguration.CreateDummyEnvironmentConfigurationFile(chosenFile);
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
                    if (textline.IndexOf(@"/*", StringComparison.Ordinal) == -1 && textline.Trim() != "")
                    {
                        var line = textline.Split('|');
                        configList.Add(line[0], line[1]);
                    }
                }

                sr.Close();
                fs.Close();


                // Databases
                if (configList["MetadataConnectionId"] != null)
                {
                    //comboBoxMetadataConnection.SelectedItem = ConfigurationSettings.connectionDictionary[configList["MetadataConnectionId"]];
                   
                    var metadataKey = ConfigurationSettings.connectionDictionary[configList["MetadataConnectionId"]];
                    comboBoxMetadataConnection.SelectedIndex = comboBoxMetadataConnection.FindStringExact(metadataKey.databaseConnectionKey);

                    //comboBoxMetadataConnection.SelectedItem = metadataKey.databaseConnectionKey;
                }


                //DWH settings
                textBoxHubTablePrefix.Text = configList["HubTablePrefix"];
                textBoxSatPrefix.Text = configList["SatTablePrefix"];
                textBoxLinkTablePrefix.Text = configList["LinkTablePrefix"];
                textBoxLinkSatPrefix.Text = configList["LinkSatTablePrefix"];
                textBoxDWHKeyIdentifier.Text = configList["KeyIdentifier"];
                textBoxEventDateTime.Text = configList["EventDateTimeStamp"];
                textBoxLDST.Text = configList["LoadDateTimeStamp"];
                textBoxExpiryDateTimeName.Text = configList["ExpiryDateTimeStamp"];
                textBoxChangeDataCaptureIndicator.Text = configList["ChangeDataIndicator"];
                textBoxRecordSource.Text = configList["RecordSourceAttribute"];
                textBoxETLProcessID.Text = configList["ETLProcessID"];
                textBoxETLUpdateProcessID.Text = configList["ETLUpdateProcessID"];
                textBoxStagingAreaPrefix.Text = configList["StagingAreaPrefix"];
                textBoxPSAPrefix.Text = configList["PersistentStagingAreaPrefix"];
                textBoxSourceRowId.Text = configList["RowID"];
                textBoxRecordChecksum.Text = configList["RecordChecksum"];
                textBoxCurrentRecordAttributeName.Text = configList["CurrentRecordAttribute"];
                textBoxAlternativeRecordSource.Text = configList["AlternativeRecordSource"];
                textBoxHubAlternativeLDTSAttribute.Text = configList["AlternativeHubLDTS"];
                textBoxSatelliteAlternativeLDTSAttribute.Text = configList["AlternativeSatelliteLDTS"];
                textBoxLogicalDeleteAttributeName.Text = configList["LogicalDeleteAttribute"];

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
             
                // Also commit the values to memory
                UpdateConfigurationInMemory();

                richTextBoxInformation.AppendText(@"The file " + chosenFile + " was uploaded successfully.\r\n");
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
        /// Commit the changes to memory, save the configuration settings to disk and create a backup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region root path file
            // Update the paths in memory
            GlobalParameters.OutputPath = textBoxOutputPath.Text;
            GlobalParameters.ConfigurationPath = textBoxConfigurationPath.Text;

            var localEnvironment = (KeyValuePair<TeamWorkingEnvironment, string>) comboBoxEnvironments.SelectedItem;
            GlobalParameters.WorkingEnvironment = localEnvironment.Key.environmentKey;

            // Save the paths from memory to disk.
            UpdateRootPathFile();
            #endregion

            // Make sure the new paths as updated are available upon save for backup etc.
            // Check if the paths and files are available, just to be sure.
            EnvironmentConfiguration.InitialiseRootPath(GlobalParameters.ConfigurationPath);
            EnvironmentConfiguration.InitialiseRootPath(GlobalParameters.OutputPath);
            EnvironmentConfiguration.CreateDummyEnvironmentConfigurationFile(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigFileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension); 
            EnvironmentConfiguration.CreateDummyValidationFile(GlobalParameters.ConfigurationPath + GlobalParameters.ValidationFileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension);
            
            // Create a file backup for the configuration file
            try
            {
                EnvironmentConfiguration.CreateFileBackup(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigFileName + '_' + GlobalParameters.WorkingEnvironment  + GlobalParameters.FileExtension);
                richTextBoxInformation.Text = "A backup of the current configuration was made at " + DateTime.Now + " in " + textBoxConfigurationPath.Text + ".\r\n";
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
            parentFormMain.RevalidateFlag = true;
        }

        // Save the root path file (configuration path, output path and working environment).
        private void UpdateRootPathFile()
        {
            // Update the root path file, part of the core solution to be able to store the config and output path
            var rootPathConfigurationFile = new StringBuilder();
            rootPathConfigurationFile.AppendLine("/* TEAM File Path Settings */");
            rootPathConfigurationFile.AppendLine("/* Saved at " + DateTime.Now + " */");
            rootPathConfigurationFile.AppendLine("ConfigurationPath|" + GlobalParameters.ConfigurationPath + "");
            rootPathConfigurationFile.AppendLine("OutputPath|" + GlobalParameters.OutputPath + "");
            rootPathConfigurationFile.AppendLine("WorkingEnvironment|" + GlobalParameters.WorkingEnvironment + "");
            rootPathConfigurationFile.AppendLine("/* End of file */");

            //using (var outfile = new StreamWriter(GlobalParameters.RootPath + GlobalParameters.PathFileName + GlobalParameters.FileExtension))
            using (var outfile =
                new StreamWriter(GlobalParameters.RootPath + GlobalParameters.PathFileName + GlobalParameters.FileExtension))
            {
                outfile.Write(rootPathConfigurationFile.ToString());
                outfile.Close();
            }
        }


        /// <summary>
        ///    Retrieve the information from the Configuration Settings from and commit these to memory
        /// </summary>
        private void UpdateConfigurationInMemory()
        {

            if (comboBoxMetadataConnection.SelectedItem!=null)
            {
                // Get the object in the Combobox into a Key Value Pair (object / id)
                var localConnectionKeyValuePair = (KeyValuePair<TeamConnectionProfile, string>)(comboBoxMetadataConnection.SelectedItem);

                // Lookup the object in the dictionary using the key (id)
                ConfigurationSettings.MetadataConnection = ConfigurationSettings.connectionDictionary[localConnectionKeyValuePair.Key.connectionInternalId];
            }

            GlobalParameters.OutputPath = textBoxOutputPath.Text;
            GlobalParameters.ConfigurationPath = textBoxConfigurationPath.Text;

            ConfigurationSettings.StgTablePrefixValue = textBoxStagingAreaPrefix.Text;
            ConfigurationSettings.PsaTablePrefixValue = textBoxPSAPrefix.Text;
            ConfigurationSettings.HubTablePrefixValue = textBoxHubTablePrefix.Text;
            ConfigurationSettings.SatTablePrefixValue = textBoxSatPrefix.Text;
            ConfigurationSettings.LinkTablePrefixValue = textBoxLinkTablePrefix.Text;
            ConfigurationSettings.LsatTablePrefixValue = textBoxLinkSatPrefix.Text;

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
            ConfigurationSettings.RowIdAttribute = textBoxSourceRowId.Text;
            ConfigurationSettings.EventDateTimeAttribute = textBoxEventDateTime.Text;
            ConfigurationSettings.LoadDateTimeAttribute = textBoxLDST.Text;
            ConfigurationSettings.ExpiryDateTimeAttribute = textBoxExpiryDateTimeName.Text;
            ConfigurationSettings.ChangeDataCaptureAttribute = textBoxChangeDataCaptureIndicator.Text;
            ConfigurationSettings.RecordSourceAttribute = textBoxRecordSource.Text;
            ConfigurationSettings.EtlProcessAttribute = textBoxETLProcessID.Text;
            ConfigurationSettings.EtlProcessUpdateAttribute = textBoxETLUpdateProcessID.Text;
            ConfigurationSettings.LogicalDeleteAttribute = textBoxLogicalDeleteAttributeName.Text;

            ConfigurationSettings.RecordChecksumAttribute = textBoxRecordChecksum.Text;
            ConfigurationSettings.CurrentRowAttribute = textBoxCurrentRecordAttributeName.Text;

            // Alternative attributes
            if (checkBoxAlternativeHubLDTS.Checked)
            {
                ConfigurationSettings.EnableAlternativeLoadDateTimeAttribute = "True";
                ConfigurationSettings.AlternativeLoadDateTimeAttribute = textBoxHubAlternativeLDTSAttribute.Text;
            }
            else
            {
                ConfigurationSettings.EnableAlternativeLoadDateTimeAttribute = "False";
            }

            if (checkBoxAlternativeRecordSource.Checked)
            {
                ConfigurationSettings.EnableAlternativeRecordSourceAttribute = "True";
                ConfigurationSettings.AlternativeRecordSourceAttribute = textBoxAlternativeRecordSource.Text;
            }
            else
            {
                ConfigurationSettings.EnableAlternativeRecordSourceAttribute = "False";
            }

            if (checkBoxAlternativeSatLDTS.Checked)
            {
                ConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute = "True";
                ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute = textBoxSatelliteAlternativeLDTSAttribute.Text;
            }
            else
            {
                ConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute = "False";
            }

            // Prefix radio buttons
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

        private void FormManageConfiguration_FormClosed(object sender, FormClosedEventArgs e)
        {
            parentFormMain.RevalidateFlag=true;
        }

        /// <summary>
        /// Check if the last tab rectangle contains the mouse clicked point, then insert a tab before the last tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControlConnections_MouseDown(object sender, MouseEventArgs e)
        {
            var lastIndex = tabControlConnections.TabCount - 1;
            if (tabControlConnections.GetTabRect(lastIndex).Contains(e.Location))
            {
                //tabControlConnections.TabPages.Insert(lastIndex, "New Tab");
                TeamConnectionProfile connectionProfile = new TeamConnectionProfile();
                connectionProfile.connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100) }, " % $@");
                connectionProfile.databaseConnectionName = "New connection";
                connectionProfile.databaseConnectionKey = "New";

                TeamDatabaseConnection connectionDatabase = new TeamDatabaseConnection();
                connectionDatabase.schemaName = "<Schema Name>";
                connectionDatabase.serverName = "<Server Name>";
                connectionDatabase.databaseName = "<Database Name>";
                connectionDatabase.namedUserName = "<User Name>";
                connectionDatabase.namedUserPassword = "<Password>";
                connectionDatabase.authenticationType = ServerAuthenticationTypes.NamedUser;

                connectionProfile.databaseServer = connectionDatabase;


                //localCustomTabPage.OnChangeMainText += UpdateMainInformationTextBox;
                //localCustomTabPage.OnClearMainText += (ClearMainInformationTextBox);

                bool newTabExists = false;
                foreach (TabPage customTabPage in tabControlConnections.TabPages)
                {
                    if (customTabPage.Name == "New")
                    {
                        newTabExists = true;
                    }
                    else
                    {
                        // Do nothing
                    }
                }

                if (newTabExists == false)
                {
                    CustomTabPageConnection localCustomTabPage = new CustomTabPageConnection(connectionProfile);
                    localCustomTabPage.OnDeleteConnection += DeleteConnection;
                    localCustomTabPage.OnChangeMainText += UpdateMainInformationTextBox;
                    localCustomTabPage.OnSaveConnection += SaveConnection;
                    tabControlConnections.TabPages.Insert(lastIndex, localCustomTabPage);
                    tabControlConnections.SelectedIndex = lastIndex;

                    GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"A new environment was created."));
                }
                else
                {
                    richTextBoxInformation.AppendText("There is already a 'new connection' tab open. Please close or save this first.\r\n");
                }
            }
        }
        
        /// <summary>
        /// Update the main information RichTextBox (used as delegate in generates tabs).
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void UpdateMainInformationTextBox(Object o, MyStringEventArgs e)
        {
            richTextBoxInformation.AppendText(e.Value);
        }

        /// <summary>
        /// Delete tab page from tab control (via delegate method)
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void DeleteConnection(Object o, MyConnectionProfileEventArgs e)
        {
            // Remove the tab page from the tab control
            var localKey = e.Value.databaseConnectionKey;
            tabControlConnections.TabPages.RemoveByKey(localKey);

            comboBoxMetadataConnection.Items.Remove(new KeyValuePair<TeamConnectionProfile, string>(e.Value, e.Value.databaseConnectionKey));
        }

        /// <summary>
        /// Delete tab page from tab control and remove item from Combobox (via delegate method)
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void DeleteEnvironment(Object o, MyWorkingEnvironmentEventArgs e)
        {
            var localKey = e.Value.environmentName;
            tabControlEnvironments.TabPages.RemoveByKey(localKey);

            comboBoxEnvironments.Items.Remove(new KeyValuePair<TeamWorkingEnvironment, string>(e.Value, e.Value.environmentKey));
        }

        private void SaveEnvironment(object o, MyStringEventArgs e)
        {
            comboBoxEnvironments.Items.Clear();

            foreach (var environment in ConfigurationSettings.environmentDictionary)
            {
                comboBoxEnvironments.Items.Add(new KeyValuePair<TeamWorkingEnvironment, string>(environment.Value, environment.Value.environmentKey));
                comboBoxEnvironments.DisplayMember = "Value";
            }

            comboBoxEnvironments.SelectedIndex = comboBoxEnvironments.FindStringExact(GlobalParameters.WorkingEnvironment);
        }

        private void SaveConnection(object o, MyStringEventArgs e)
        {
            // Just adding is not enough as it can happen that the name has changed for an existing connection.
            comboBoxMetadataConnection.Items.Clear();

            foreach (var connection in ConfigurationSettings.connectionDictionary)
            {
                comboBoxMetadataConnection.Items.Add(new KeyValuePair<TeamConnectionProfile, string>(connection.Value, connection.Value.databaseConnectionKey));
                comboBoxMetadataConnection.ValueMember = "Key";
                comboBoxMetadataConnection.DisplayMember = "Value";

            }

            comboBoxMetadataConnection.SelectedIndex = comboBoxMetadataConnection.FindStringExact(ConfigurationSettings.MetadataConnection.databaseConnectionKey);
        }

        /// <summary>
        /// Prevent selecting the last tab in the connections tab control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControlConnections_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex == this.tabControlConnections.TabCount - 1)
                    e.Cancel = true;
        }

        /// <summary>
        /// Prevent selecting the last tab in the connections tab control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControlEnvironments_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex == this.tabControlEnvironments.TabCount - 1)
                e.Cancel = true;

        }

        /// <summary>
        /// OnMouseDown event on the Environments Tab, if New is clicked instantiate a new environment tab.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControlEnvironments_MouseDown(object sender, MouseEventArgs e)
        {
            var lastIndex = tabControlEnvironments.TabCount - 1;

            if (tabControlEnvironments.GetTabRect(lastIndex).Contains(e.Location))
            {
                TeamWorkingEnvironment workingEnvironment = new TeamWorkingEnvironment();
                workingEnvironment.environmentInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100)}, " % $@");
                workingEnvironment.environmentName = "New environment";
                workingEnvironment.environmentKey = "New";

                bool newTabExists = false;
                foreach (TabPage customTabPage in tabControlEnvironments.TabPages)
                {
                    if (customTabPage.Name == "New environment")
                    {
                        newTabExists = true;
                    }
                    else
                    {
                        // Do nothing
                    }
                }

                if (newTabExists == false)
                {
                    CustomTabPageEnvironment localCustomTabPage = new CustomTabPageEnvironment(workingEnvironment);
                    localCustomTabPage.OnDeleteEnvironment += DeleteEnvironment;
                    localCustomTabPage.OnSaveEnvironment += SaveEnvironment;
                    localCustomTabPage.OnChangeMainText += UpdateMainInformationTextBox;
                    tabControlEnvironments.TabPages.Insert(lastIndex, localCustomTabPage);
                    tabControlEnvironments.SelectedIndex = lastIndex;
                }
                else
                {
                    richTextBoxInformation.AppendText("There is already a 'new environment' tab open. Please close or save this first.\r\n");
                }
            }
        }

        /// <summary>
        /// Open the Root Path File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openRootPathFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(GlobalParameters.RootPath + GlobalParameters.PathFileName + GlobalParameters.FileExtension);

            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text =
                    "An error has occured while attempting to open the root path file. The error message is: " + ex;
            }
        }

        /// <summary>
        /// Open the active Configuration File.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openActiveConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigFileName + '_' +
                              GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension);

            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text =
                    "An error has occured while attempting to open the active configuration file. The error message is: " + ex;
            }
        }

        /// <summary>
        /// Manage the event when the environment selection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxEnvironments_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_formLoading == false)
            {
                // Retrieve the object from the event.
                var localComboBox = (ComboBox)sender;

                var localComboBoxSelection = (KeyValuePair<TeamWorkingEnvironment, string>) localComboBox.SelectedItem;

                var selectedItem = localComboBoxSelection.Key;

                // Get the full environment from the in-memory dictionary.
                var localEnvironment = ConfigurationSettings.environmentDictionary[selectedItem.environmentInternalId];

                // Set the working environment in memory.
                GlobalParameters.WorkingEnvironment = localEnvironment.environmentKey;

                // Update the root path file with the new working directory.
                UpdateRootPathFile();

                // Initialise new environment in configuration settings.
                UpdateEnvironment(localEnvironment);

                foreach (TabPage customTabPage in tabControlConnections.TabPages)
                {
                    if ((customTabPage.Name == "tabPageConnectionMain") || (customTabPage.Name == "tabPageConnectionNewTab"))
                    {
                        // Do nothing, as only the two standard Tab Pages exist.
                    }
                    else
                    {
                        // Remove the Tab Page from the Tab Control
                        tabControlConnections.Controls.Remove((customTabPage));
                    }
                }

                EnvironmentConfiguration.LoadConnectionFile();

                comboBoxMetadataConnection.Items.Clear();
                AddConnectionTabPages();

                try
                {
                    LocalInitialiseConnections(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigFileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension);
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.AppendText("Errors occured trying to load the configuration file, the message is " + ex + ". No default values were loaded. \r\n\r\n");
                }

                //var selectedItemComboBox = new KeyValuePair<TeamConnectionProfile, string>(ConfigurationSettings.MetadataConnection, ConfigurationSettings.MetadataConnection.databaseConnectionKey);

                comboBoxMetadataConnection.SelectedIndex = comboBoxMetadataConnection.FindStringExact(ConfigurationSettings.MetadataConnection.databaseConnectionKey);

                // Report back to the event log.
                GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The environment was changed to {localEnvironment.environmentName}."));
            }
        }
    }
}