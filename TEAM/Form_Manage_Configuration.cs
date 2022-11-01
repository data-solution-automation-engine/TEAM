using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.IO;
using TEAM_Library;

namespace TEAM
{
    public partial class FormManageConfiguration : FormBase
    {
        private bool _formLoading = true;
        private FormMain parentFormMain;

        public FormManageConfiguration(FormMain parent) : base(parent)
        {
            parentFormMain = parent;
            InitializeComponent();

            //Paths
            textBoxConfigurationPath.Text = GlobalParameters.ConfigurationPath;
            textBoxTeamMetadataPath.Text = GlobalParameters.MetadataPath;

            // Adding tab pages to the Environment tabs.
            IntPtr localHandle = tabControlEnvironments.Handle;
            foreach (var environment in TeamEnvironmentCollection.EnvironmentDictionary)
            {
                // Adding tabs on the Tab Control
                var lastIndex = tabControlEnvironments.TabCount - 1;
                TabPageEnvironments localCustomTabPage = new TabPageEnvironments(environment.Value);
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

            // Load the configuration file using the paths retrieved from the application root contents (configuration path)
            try
            {
                LocalInitialiseConnections(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigFileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension);
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText("Errors occurred trying to load the configuration file, the message is " + ex + ". No default values were loaded. \r\n\r\n");
            }

            // Connection tabs for the specific environment.
            AddConnectionTabPages();

            if (TeamConfiguration.MetadataConnection is null)
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"No metadata connection is set."));
            }
            else
            {
                comboBoxMetadataConnection.SelectedIndex = comboBoxMetadataConnection.FindStringExact(TeamConfiguration.MetadataConnection.ConnectionKey);
            }

            _formLoading = false;
        }

        /// <summary>
        /// Add Tabs to the Connections Tab Control based on the in-memory values (connection dictionary).
        /// </summary>
        private void AddConnectionTabPages()
        {
            IntPtr localHandle = tabControlConnections.Handle;
            foreach (var connection in TeamConfiguration.ConnectionDictionary)
            {
                // Adding tabs on the Tab Control
                var lastIndex = tabControlConnections.TabCount - 1;
                TabPageConnections localCustomTabPage = new TabPageConnections(connection.Value);
                localCustomTabPage.OnDeleteConnection += DeleteConnection;
                localCustomTabPage.OnChangeMainText += UpdateMainInformationTextBox;
                localCustomTabPage.OnSaveConnection += SaveConnection;
                tabControlConnections.TabPages.Insert(lastIndex, localCustomTabPage);
                tabControlConnections.SelectedIndex = 0;

                // Adding items in the drop down list
                comboBoxMetadataConnection.Items.Add(new KeyValuePair<TeamConnection, string>(connection.Value, connection.Value.ConnectionKey));
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
        /// <param name="connectionFile"></param>
        private void LocalInitialiseConnections(string connectionFile)
        {
            // If the config file does not exist yet, create it by calling the EnvironmentConfiguration Class.
            if (!File.Exists(connectionFile))
            {
                TeamConfiguration.CreateDummyEnvironmentConfigurationFile(connectionFile);
            }

            // Open the configuration file
            var configList = new Dictionary<string, string>();
            var fs = new FileStream(connectionFile, FileMode.Open, FileAccess.Read);
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
                    var metadataKey = TeamConfiguration.ConnectionDictionary[configList["MetadataConnectionId"]];
                    comboBoxMetadataConnection.SelectedIndex = comboBoxMetadataConnection.FindStringExact(metadataKey.ConnectionKey);
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
                textBoxPresentationLayerLabels.Text = configList["PresentationLayerLabels"];
                textBoxTransformationLabels.Text = configList["TransformationLabels"];
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

                richTextBoxInformation.AppendText(@"The file " + connectionFile + " was uploaded successfully.\r\n");
            }
            catch (Exception ex)
            {
                richTextBoxInformation.AppendText("\r\n\r\nAn error occurred while loading the configuration file. The original error is: '" + ex.Message + "'");
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
                MessageBox.Show($@"Error: Could not read file from disk. Original error: {ex.Message}", @"An issues has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        private void SaveConfigurationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            #region root path file
            // Update the paths in memory
            GlobalParameters.ConfigurationPath = textBoxConfigurationPath.Text;
            GlobalParameters.MetadataPath = textBoxTeamMetadataPath.Text;

            var localEnvironment = (KeyValuePair<TeamWorkingEnvironment, string>) comboBoxEnvironments.SelectedItem;
            GlobalParameters.WorkingEnvironment = localEnvironment.Key.environmentKey;
            GlobalParameters.WorkingEnvironmentInternalId = localEnvironment.Key.environmentInternalId;

            // Save the paths from memory to disk.
            UpdateRootPathFile();
            #endregion

            // Make sure the new paths as updated are available upon save for backup etc.
            // Check if the paths and files are available, just to be sure.
            FileHandling.InitialisePath(GlobalParameters.ConfigurationPath, TeamPathTypes.ConfigurationPath, TeamEventLog);
            FileHandling.InitialisePath(GlobalParameters.MetadataPath, TeamPathTypes.MetadataPath, TeamEventLog);

            TeamConfiguration.CreateDummyEnvironmentConfigurationFile(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigFileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension);
            ValidationSetting.CreateDummyValidationFile(GlobalParameters.ConfigurationPath + GlobalParameters.ValidationFileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension);
            JsonExportSetting.CreateDummyJsonConfigurationFile(GlobalParameters.ConfigurationPath + GlobalParameters.JsonExportConfigurationFileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension);

            // Create a file backup for the configuration file
            try
            {
                FileHandling.CreateFileBackup(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigFileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension, GlobalParameters.BackupPath);
                richTextBoxInformation.Text = $@"A backup of the current configuration was made at {DateTime.Now} in {textBoxConfigurationPath.Text}.";
            }
            catch (Exception)
            {
                richTextBoxInformation.Text = @"TEAM was unable to create a backup of the configuration file.";
            }

            
            // Update the in-memory variables for use throughout the application, to commit the saved changes for runtime use. 
            // This is needed before saving to disk, as the EnvironmentConfiguration Class retrieves the values from memory.
            UpdateConfigurationInMemory();


            // Save the information 
            LocalTeamEnvironmentConfiguration.SaveConfigurationFile();
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
            rootPathConfigurationFile.AppendLine("MetadataPath|" + GlobalParameters.MetadataPath + "");
            rootPathConfigurationFile.AppendLine("WorkingEnvironment|" + GlobalParameters.WorkingEnvironment + "");
            rootPathConfigurationFile.AppendLine("/* End of file */");

            try
            {
                using (var outfile = new StreamWriter(GlobalParameters.CorePath + GlobalParameters.PathFileName + GlobalParameters.FileExtension))
                {
                    outfile.Write(rootPathConfigurationFile.ToString());
                    outfile.Close();
                }
            }
            catch (Exception ex)
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The configuration file {GlobalParameters.CorePath +GlobalParameters.PathFileName + GlobalParameters.FileExtension} could not be updated. The error message is: \r\n\r\b\n{ex}"));
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
                var localConnectionKeyValuePair = (KeyValuePair<TeamConnection, string>)(comboBoxMetadataConnection.SelectedItem);

                // Lookup the object in the dictionary using the key (id)
                TeamConfiguration.MetadataConnection = TeamConfiguration.ConnectionDictionary[localConnectionKeyValuePair.Key.ConnectionInternalId];
            }

            GlobalParameters.MetadataPath = textBoxTeamMetadataPath.Text;
            GlobalParameters.ConfigurationPath = textBoxConfigurationPath.Text;

            TeamConfiguration.StgTablePrefixValue = textBoxStagingAreaPrefix.Text;
            TeamConfiguration.PsaTablePrefixValue = textBoxPSAPrefix.Text;
            TeamConfiguration.PresentationLayerLabels = textBoxPresentationLayerLabels.Text;
            TeamConfiguration.TransformationLabels = textBoxTransformationLabels.Text;
            TeamConfiguration.HubTablePrefixValue = textBoxHubTablePrefix.Text;
            TeamConfiguration.SatTablePrefixValue = textBoxSatPrefix.Text;
            TeamConfiguration.LinkTablePrefixValue = textBoxLinkTablePrefix.Text;
            TeamConfiguration.LsatTablePrefixValue = textBoxLinkSatPrefix.Text;

            if (keyPrefixRadiobutton.Checked)
            {
                TeamConfiguration.KeyNamingLocation = "Prefix";
            }
            else if (keySuffixRadiobutton.Checked)
            {
                TeamConfiguration.KeyNamingLocation = "Suffix";
            }
            else
            {
                richTextBoxInformation.AppendText("Issues storing the key location (prefix/suffix). Is one of the radio buttons checked?");
            }

            TeamConfiguration.DwhKeyIdentifier = textBoxDWHKeyIdentifier.Text;
            TeamConfiguration.RowIdAttribute = textBoxSourceRowId.Text;
            TeamConfiguration.EventDateTimeAttribute = textBoxEventDateTime.Text;
            TeamConfiguration.LoadDateTimeAttribute = textBoxLDST.Text;
            TeamConfiguration.ExpiryDateTimeAttribute = textBoxExpiryDateTimeName.Text;
            TeamConfiguration.ChangeDataCaptureAttribute = textBoxChangeDataCaptureIndicator.Text;
            TeamConfiguration.RecordSourceAttribute = textBoxRecordSource.Text;
            TeamConfiguration.EtlProcessAttribute = textBoxETLProcessID.Text;
            TeamConfiguration.EtlProcessUpdateAttribute = textBoxETLUpdateProcessID.Text;
            TeamConfiguration.LogicalDeleteAttribute = textBoxLogicalDeleteAttributeName.Text;

            TeamConfiguration.RecordChecksumAttribute = textBoxRecordChecksum.Text;
            TeamConfiguration.CurrentRowAttribute = textBoxCurrentRecordAttributeName.Text;

            // Alternative attributes
            if (checkBoxAlternativeHubLDTS.Checked)
            {
                TeamConfiguration.EnableAlternativeLoadDateTimeAttribute = "True";
                TeamConfiguration.AlternativeLoadDateTimeAttribute = textBoxHubAlternativeLDTSAttribute.Text;
            }
            else
            {
                TeamConfiguration.EnableAlternativeLoadDateTimeAttribute = "False";
            }

            if (checkBoxAlternativeRecordSource.Checked)
            {
                TeamConfiguration.EnableAlternativeRecordSourceAttribute = "True";
                TeamConfiguration.AlternativeRecordSourceAttribute = textBoxAlternativeRecordSource.Text;
            }
            else
            {
                TeamConfiguration.EnableAlternativeRecordSourceAttribute = "False";
            }

            if (checkBoxAlternativeSatLDTS.Checked)
            {
                TeamConfiguration.EnableAlternativeSatelliteLoadDateTimeAttribute = "True";
                TeamConfiguration.AlternativeSatelliteLoadDateTimeAttribute = textBoxSatelliteAlternativeLDTSAttribute.Text;
            }
            else
            {
                TeamConfiguration.EnableAlternativeSatelliteLoadDateTimeAttribute = "False";
            }

            // Prefix radio buttons
            if (tablePrefixRadiobutton.Checked)
            {
                TeamConfiguration.TableNamingLocation = "Prefix";
            }
            else if (tableSuffixRadiobutton.Checked)
            {
                TeamConfiguration.TableNamingLocation = "Suffix";
            }
            else
            {
                richTextBoxInformation.AppendText("Issues storing the table prefix location (prefix/suffix). Is one of the radio buttons checked?");
            }

            if (radioButtonPSABusinessKeyIndex.Checked)
            {
                TeamConfiguration.PsaKeyLocation = "UniqueIndex";
            }
            else if (radioButtonPSABusinessKeyPK.Checked)
            {
                TeamConfiguration.PsaKeyLocation = "PrimaryKey";
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
                    richTextBoxInformation.Text = @"There is no value given for the Configuration Path. Please enter a valid path name.";
                }
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = $@"An error has occurred while attempting to open the configuration directory. The error message is: {ex.Message}";
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
                TeamConnection connectionProfile = new TeamConnection();
                connectionProfile.ConnectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100) }, " % $@");
                connectionProfile.ConnectionName = "New connection";
                connectionProfile.ConnectionKey = "New";
                connectionProfile.ConnectionType = ConnectionTypes.Database;

                TeamDatabaseConnection connectionDatabase = new TeamDatabaseConnection();
                connectionDatabase.SchemaName = "<Schema Name>";
                connectionDatabase.ServerName = "<Server Name>";
                connectionDatabase.DatabaseName = "<Database Name>";
                connectionDatabase.NamedUserName = "<User Name>";
                connectionDatabase.NamedUserPassword = "<Password>";
                connectionDatabase.authenticationType = ServerAuthenticationTypes.NamedUser;

                TeamFileConnection connectionFile = new TeamFileConnection();
                connectionFile.FilePath = @"<File Path>";
                connectionFile.FileName = @"<File Name>";

                connectionProfile.DatabaseServer = connectionDatabase;
                connectionProfile.FileConnection = connectionFile;

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
                    // Create a new tab page using the connection profile (a TeamConnection class object) as input.
                    TabPageConnections localCustomTabPage = new TabPageConnections(connectionProfile);
                    localCustomTabPage.OnDeleteConnection += DeleteConnection;
                    localCustomTabPage.OnChangeMainText += UpdateMainInformationTextBox;
                    localCustomTabPage.OnSaveConnection += SaveConnection;
                    tabControlConnections.TabPages.Insert(lastIndex, localCustomTabPage);
                    tabControlConnections.SelectedIndex = lastIndex;

                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"A new connection was created."));
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
            richTextBoxInformation.ScrollToCaret();
        }

        /// <summary>
        /// Delete tab page from tab control (via delegate method)
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void DeleteConnection(Object o, MyConnectionProfileEventArgs e)
        {
            // Remove the tab page from the tab control
            var localKey = e.Value.ConnectionKey;
            tabControlConnections.TabPages.RemoveByKey(localKey);

            comboBoxMetadataConnection.Items.Remove(new KeyValuePair<TeamConnection, string>(e.Value, e.Value.ConnectionKey));
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

            foreach (var environment in TeamEnvironmentCollection.EnvironmentDictionary)
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

            foreach (var connection in TeamConfiguration.ConnectionDictionary)
            {
                comboBoxMetadataConnection.Items.Add(new KeyValuePair<TeamConnection, string>(connection.Value, connection.Value.ConnectionKey));
                comboBoxMetadataConnection.ValueMember = "Key";
                comboBoxMetadataConnection.DisplayMember = "Value";

            }

            comboBoxMetadataConnection.SelectedIndex = comboBoxMetadataConnection.FindStringExact(TeamConfiguration.MetadataConnection.ConnectionKey);
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
                    TabPageEnvironments localCustomTabPage = new TabPageEnvironments(workingEnvironment);
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
                Process.Start(GlobalParameters.CorePath + GlobalParameters.PathFileName + GlobalParameters.FileExtension);

            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = $@"An error has occurred while attempting to open the root path file. The error message is: {ex.Message}";
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
                richTextBoxInformation.Text = $@"An error has occurred while attempting to open the active configuration file. The error message is: {ex.Message}";
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
                var localEnvironment = TeamEnvironmentCollection.EnvironmentDictionary[selectedItem.environmentInternalId];

                // Set the working environment in memory.
                GlobalParameters.WorkingEnvironment = localEnvironment.environmentKey;
                GlobalParameters.WorkingEnvironmentInternalId = localEnvironment.environmentInternalId;

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

                var connectionFileName =
                    GlobalParameters.ConfigurationPath +
                    GlobalParameters.JsonConnectionFileName + '_' +
                    GlobalParameters.WorkingEnvironment +
                    GlobalParameters.JsonExtension;

                TeamConfiguration.ConnectionDictionary = TeamConnectionFile.LoadConnectionFile(connectionFileName);

                comboBoxMetadataConnection.Items.Clear();
                AddConnectionTabPages();

                
                try
                {
                    LocalInitialiseConnections(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigFileName + '_' + GlobalParameters.WorkingEnvironment + GlobalParameters.FileExtension);
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.AppendText("Errors occurred trying to load the configuration file, the message is " + ex + ". No default values were loaded. \r\n\r\n");
                }



                //var selectedItemComboBox = new KeyValuePair<TeamConnectionProfile, string>(TeamConfigurationSettings.MetadataConnection, TeamConfigurationSettings.MetadataConnection.ConnectionKey);

                    if (TeamConfiguration.MetadataConnection is null)
                {
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"No metadata connection is set."));
                }
                else
                {
                    comboBoxMetadataConnection.SelectedIndex = comboBoxMetadataConnection.FindStringExact(TeamConfiguration.MetadataConnection.ConnectionKey);
                }

                // Report back to the event log.
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The environment was changed to {localEnvironment.environmentName}."));
            }
        }

        private void pictureBoxMetadataPath_Click(object sender, EventArgs e)
        {
            var fileBrowserDialog = new FolderBrowserDialog();
            fileBrowserDialog.SelectedPath = textBoxTeamMetadataPath.Text;

            DialogResult result = fileBrowserDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fileBrowserDialog.SelectedPath))
            {

                string finalPath;
                if (fileBrowserDialog.SelectedPath.EndsWith(@"\"))
                {
                    finalPath = fileBrowserDialog.SelectedPath;
                }
                else
                {
                    finalPath = fileBrowserDialog.SelectedPath + @"\";
                }


                textBoxTeamMetadataPath.Text = finalPath;
                richTextBoxInformation.Text = $@"The metadata path is set to {finalPath}. Don't forget to save!'";
            }
        }

        private void pictureBoxConfigurationPath_Click(object sender, EventArgs e)
        {
            var fileBrowserDialog = new FolderBrowserDialog();
            fileBrowserDialog.SelectedPath = textBoxConfigurationPath.Text;

            DialogResult result = fileBrowserDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fileBrowserDialog.SelectedPath))
            {

                string finalPath;
                if (fileBrowserDialog.SelectedPath.EndsWith(@"\"))
                {
                    finalPath = fileBrowserDialog.SelectedPath;
                }
                else
                {
                    finalPath = fileBrowserDialog.SelectedPath + @"\";
                }


                textBoxConfigurationPath.Text = finalPath;
                richTextBoxInformation.Text = $@"The configuration path is set to {finalPath}. Don't forget to save!'";
            }
        }

        private void openMetadataDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBoxTeamMetadataPath.Text != "")
                {
                    Process.Start(textBoxTeamMetadataPath.Text);
                }
                else
                {
                    richTextBoxInformation.Text = @"There is no value given for the Metadata Path. Please enter a valid path name.";
                }
            }
            catch (Exception ex)
            {
                richTextBoxInformation.Text = $@"An error has occurred while attempting to open the metadata directory. The error message is: {ex.Message}";
            }
        }
    }
}