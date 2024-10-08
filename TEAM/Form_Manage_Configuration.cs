﻿using System;
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
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F); // for design in 96 DPI
            AutoScaleMode = AutoScaleMode.Dpi;
            parentFormMain = parent;
            InitializeComponent();

            //Paths
            textBoxConfigurationPath.Text = globalParameters.ConfigurationPath;
            textBoxTeamMetadataPath.Text = globalParameters.MetadataPath;

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
                comboBoxEnvironments.Items.Add(new KeyValuePair<TeamEnvironment, string>(environment.Value, environment.Value.environmentKey));
                comboBoxEnvironments.DisplayMember = "Value";
            }

            comboBoxEnvironments.SelectedIndex = comboBoxEnvironments.FindStringExact(globalParameters.ActiveEnvironmentKey);

            // Load the configuration file using the paths retrieved from the application root contents (configuration path)
            try
            {
                LocalInitialiseConnections(globalParameters.ConfigurationPath + globalParameters.ConfigFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.FileExtension);
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
            AutoScaleMode = AutoScaleMode.Dpi;
            IntPtr localHandle = tabControlConnections.Handle;
            foreach (var connection in TeamConfiguration.ConnectionDictionary)
            {
                // Adding tabs on the Tab Control
                var lastIndex = tabControlConnections.TabCount - 1;

                if (connection.Value.TechnologyConnectionType == TechnologyConnectionType.Snowflake)
                {
                    var localCustomSnowflakeTabPage = new TabPageSnowflakeConnection(connection.Value);
                    localCustomSnowflakeTabPage.OnDeleteConnection += DeleteConnection;
                    localCustomSnowflakeTabPage.OnChangeMainText += UpdateMainInformationTextBox;
                    localCustomSnowflakeTabPage.OnSaveConnection += SaveConnection;
                    //localCustomSnowflakeTabPage.Autscal
                    tabControlConnections.TabPages.Insert(lastIndex, localCustomSnowflakeTabPage);
                }
                else
                {
                    TabPageSqlServerConnection localCustomSqlServerTabPage = new TabPageSqlServerConnection(connection.Value);
                    localCustomSqlServerTabPage.OnDeleteConnection += DeleteConnection;
                    localCustomSqlServerTabPage.OnChangeMainText += UpdateMainInformationTextBox;
                    localCustomSqlServerTabPage.OnSaveConnection += SaveConnection;
                    tabControlConnections.TabPages.Insert(lastIndex, localCustomSqlServerTabPage);
                }

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

        public void UpdateEnvironment(TeamEnvironment environment)
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
                TeamConfiguration.CreateDummyTeamConfigurationFile(connectionFile);
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
                textBoxKeyIdentifier.Text = configList["KeyIdentifier"];
                textBoxKeyPattern.Text = configList["KeyPattern"];
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
                textBoxOtherExceptionColumns.Text = configList["OtherExceptionColumns"];

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
                UpdateDataWarehouseConfigurationInMemory();

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
                InitialDirectory = @""+ globalParameters.ConfigurationPath+""
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
                MessageBox.Show($@"Error: Could not read file from disk. Original error: {ex.Message}", @"An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var selectedTab = tabControlDefaultDetails.SelectedTab.Text;

            if (selectedTab == "Environments")
            {
                SaveTeamCoreFile();
                richTextBoxInformation.Text += $"The selected environment '{globalParameters.ActiveEnvironmentKey}' has been saved, and is now active.";

            }
            else if (selectedTab == "Connections")
            {
                UpdateMetadataConnectionConfigurationInMemory();
                LocalTeamEnvironmentConfiguration.SaveTeamConfigurationFile();
                richTextBoxInformation.Text += "\r\nThe metadata connection has been set.";
            }
            else if (selectedTab == "Data Object Types")
            {
                UpdateDataObjectTypesConfigurationInMemory();
                LocalTeamEnvironmentConfiguration.SaveTeamConfigurationFile();
                richTextBoxInformation.Text += "\r\nThe Data Object Type settings have been updated.";
            }
            else if (selectedTab == "Data Warehouse")
            {
                UpdateDataWarehouseConfigurationInMemory();
                LocalTeamEnvironmentConfiguration.SaveTeamConfigurationFile();
                richTextBoxInformation.Text += "\r\nThe Data Warehouse settings have been updated.";
            }
            else if (selectedTab == "Paths")
            {
                SaveEnvironmentPaths();
                richTextBoxInformation.Text += "\r\nThe paths have been updated.";
            }
            else
            {
                // Do nothing
            }
        }

        /// <summary>
        /// Save the TEAM Core file, that contains the working environment.
        /// </summary>
        private void SaveTeamCoreFile()
        {
            try
            {
                // Get the selected environment.
                var localEnvironment = (KeyValuePair<TeamEnvironment, string>)comboBoxEnvironments.SelectedItem;
                globalParameters.ActiveEnvironmentInternalId = localEnvironment.Key.environmentInternalId;
                globalParameters.ActiveEnvironmentKey = localEnvironment.Key.environmentKey;

                // Update the root path file, part of the core solution to be able to store the config and output path
                var rootPathConfigurationFile = new StringBuilder();
                rootPathConfigurationFile.AppendLine("/* TEAM Core */");
                rootPathConfigurationFile.AppendLine("WorkingEnvironment|" + globalParameters.ActiveEnvironmentInternalId + "");
                rootPathConfigurationFile.AppendLine("/* End of file */");


                using (var outfile = new StreamWriter(globalParameters.CorePath + globalParameters.PathFileName + globalParameters.FileExtension))
                {
                    outfile.Write(rootPathConfigurationFile.ToString());
                    outfile.Close();
                }
            }
            catch (Exception exception)
            {
                var errorMessage =
                    $"The TEAM Core configuration file {globalParameters.CorePath + globalParameters.PathFileName + globalParameters.FileExtension} could not be updated. The error message is: \r\n\r\b\n{exception.Message}";
                richTextBoxInformation.Text += errorMessage;
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, errorMessage));
            }
        }

        /// <summary>
        /// Make sure the information on the metadata repository is committed to memory, and saved.
        /// </summary>
        private void UpdateMetadataConnectionConfigurationInMemory()
        {
            if (comboBoxMetadataConnection.SelectedItem != null)
            {
                // Get the object in the Combobox into a Key Value Pair (object / id)
                var localConnectionKeyValuePair = (KeyValuePair<TeamConnection, string>)(comboBoxMetadataConnection.SelectedItem);

                // Lookup the object in the dictionary using the key (id)
                TeamConfiguration.MetadataConnection = TeamConfiguration.ConnectionDictionary[localConnectionKeyValuePair.Key.ConnectionInternalId];
            }
        }

        /// <summary>
        /// Make sure the information on the Data Object Types tab is committed to memory, and saved.
        /// </summary>
        private void UpdateDataObjectTypesConfigurationInMemory()
        {
            // Data Object Types tab.
            TeamConfiguration.StgTablePrefixValue = textBoxStagingAreaPrefix.Text;
            TeamConfiguration.PsaTablePrefixValue = textBoxPSAPrefix.Text;
            TeamConfiguration.PresentationLayerLabels = textBoxPresentationLayerLabels.Text;
            TeamConfiguration.TransformationLabels = textBoxTransformationLabels.Text;
            TeamConfiguration.HubTablePrefixValue = textBoxHubTablePrefix.Text;
            TeamConfiguration.SatTablePrefixValue = textBoxSatPrefix.Text;
            TeamConfiguration.LinkTablePrefixValue = textBoxLinkTablePrefix.Text;
            TeamConfiguration.LsatTablePrefixValue = textBoxLinkSatPrefix.Text;

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
                richTextBoxInformation.AppendText("Issues storing the PSA key location (Unique Index / Primary Key Index). Is one of the radio buttons checked?");
            }
        }

        /// <summary>
        /// Make sure the information on the Data Warehouse tab is committed to memory, and saved.
        /// </summary>
        private void UpdateDataWarehouseConfigurationInMemory()
        {
            // Data Warehouse tab.
            TeamConfiguration.KeyIdentifier = textBoxKeyIdentifier.Text;
            TeamConfiguration.KeyPattern = textBoxKeyPattern.Text;
            TeamConfiguration.RowIdAttribute = textBoxSourceRowId.Text;
            TeamConfiguration.EventDateTimeAttribute = textBoxEventDateTime.Text;
            TeamConfiguration.LoadDateTimeAttribute = textBoxLDST.Text;
            TeamConfiguration.ExpiryDateTimeAttribute = textBoxExpiryDateTimeName.Text;
            TeamConfiguration.ChangeDataCaptureAttribute = textBoxChangeDataCaptureIndicator.Text;
            TeamConfiguration.RecordSourceAttribute = textBoxRecordSource.Text;
            TeamConfiguration.EtlProcessAttribute = textBoxETLProcessID.Text;
            TeamConfiguration.EtlProcessUpdateAttribute = textBoxETLUpdateProcessID.Text;
            TeamConfiguration.LogicalDeleteAttribute = textBoxLogicalDeleteAttributeName.Text;
            TeamConfiguration.OtherExceptionColumns = textBoxOtherExceptionColumns.Text;

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
        }

        /// <summary>
        ///  Open the Windows Explorer (directory) using the value available as Configuration Directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openConfigurationDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBoxConfigurationPath.Text != "")
                {
                    var psi = new ProcessStartInfo() { FileName = textBoxConfigurationPath.Text, UseShellExecute = true };
                    Process.Start(psi);
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

            // Only do something when someone actually clicks the last tab.
            if (tabControlConnections.GetTabRect(lastIndex).Contains(e.Location))
            {
                var connectionDialog = new Form_Connection_Selection();
                var selectedConnectionType = "";

                if (connectionDialog.ShowDialog(this) == DialogResult.OK)
                {
                    selectedConnectionType = connectionDialog.GetValue();

                    connectionDialog.Dispose();

                    #region SQL Server

                    if (selectedConnectionType == "sqlserver")
                    {
                        TeamConnection sqlServerConnectionProfile = new TeamConnection();
                        sqlServerConnectionProfile.ConnectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100) }, " % $@");
                        sqlServerConnectionProfile.ConnectionName = "New connection";
                        sqlServerConnectionProfile.ConnectionKey = "New";
                        sqlServerConnectionProfile.CatalogConnectionType = CatalogConnectionTypes.Catalog;

                        TeamDatabaseConnection sqlServerDatabaseConnection = new TeamDatabaseConnection();
                        sqlServerDatabaseConnection.SchemaName = "<Schema Name>";
                        sqlServerDatabaseConnection.ServerName = "<Server Name>";
                        sqlServerDatabaseConnection.DatabaseName = "<Database Name>";
                        sqlServerDatabaseConnection.NamedUserName = "<User Name>";
                        sqlServerDatabaseConnection.NamedUserPassword = "<Password>";
                        sqlServerDatabaseConnection.AuthenticationType = ServerAuthenticationTypes.NamedUser;

                        sqlServerConnectionProfile.DatabaseServer = sqlServerDatabaseConnection;

                        bool sqlServerNewTabExists = false;
                        foreach (TabPage customTabPage in tabControlConnections.TabPages)
                        {
                            if (customTabPage.Name == "New")
                            {
                                sqlServerNewTabExists = true;
                            }
                            else
                            {
                                // Do nothing
                            }
                        }

                        if (!sqlServerNewTabExists)
                        {
                            // Create a new tab page using the connection profile (a TeamConnection class object) as input.
                            TabPageSqlServerConnection localCustomTabPage = new TabPageSqlServerConnection(sqlServerConnectionProfile);
                            localCustomTabPage.OnDeleteConnection += DeleteConnection;
                            localCustomTabPage.OnChangeMainText += UpdateMainInformationTextBox;
                            localCustomTabPage.OnSaveConnection += SaveConnection;
                            tabControlConnections.TabPages.Insert(lastIndex, localCustomTabPage);
                            tabControlConnections.SelectedIndex = lastIndex;

                            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"A new SQL Server connection was created."));
                        }
                        else
                        {
                            richTextBoxInformation.AppendText("There is already a 'new connection' tab open. Please close or save this first.\r\n");
                        }
                    }

                    #endregion

                    #region Snowflake

                    if (selectedConnectionType == "snowflake")
                    {
                        TeamConnection snowFlakeConnectionProfile = new TeamConnection();
                        snowFlakeConnectionProfile.ConnectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100) }, " % $@");
                        snowFlakeConnectionProfile.ConnectionName = "New connection";
                        snowFlakeConnectionProfile.ConnectionKey = "New";
                        snowFlakeConnectionProfile.TechnologyConnectionType = TechnologyConnectionType.Snowflake;
                        snowFlakeConnectionProfile.CatalogConnectionType = CatalogConnectionTypes.Catalog;
                        snowFlakeConnectionProfile.ConnectionNotes = "Snowflake connection";

                        TeamDatabaseConnection snowFlakeDatabaseConnection = new TeamDatabaseConnection();
                        snowFlakeDatabaseConnection.SchemaName = "<Schema e.g. PUBLIC>";
                        snowFlakeDatabaseConnection.MultiFactorAuthenticationUser = "<User>";
                        snowFlakeDatabaseConnection.Account = "<Snowflake account>";
                        snowFlakeDatabaseConnection.Role = "<Snowflake role>";
                        snowFlakeDatabaseConnection.Role = "<Role>";
                        snowFlakeDatabaseConnection.DatabaseName = "<Snowflake database>";
                        snowFlakeDatabaseConnection.Warehouse = "<Snowflake warehouse>";
                        snowFlakeDatabaseConnection.AuthenticationType = ServerAuthenticationTypes.SSO;

                        snowFlakeConnectionProfile.DatabaseServer = snowFlakeDatabaseConnection;

                        bool newSnowflakeTabExists = false;
                        foreach (TabPage customTabPage in tabControlConnections.TabPages)
                        {
                            if (customTabPage.Name == "New")
                            {
                                newSnowflakeTabExists = true;
                            }
                            else
                            {
                                // Do nothing
                            }
                        }

                        if (!newSnowflakeTabExists)
                        {
                            // Create a new tab page using the connection profile (a TeamConnection class object) as input.
                            TabPageSnowflakeConnection localCustomTabPage = new TabPageSnowflakeConnection(snowFlakeConnectionProfile);
                            localCustomTabPage.OnDeleteConnection += DeleteConnection;
                            localCustomTabPage.OnChangeMainText += UpdateMainInformationTextBox;
                            localCustomTabPage.OnSaveConnection += SaveConnection;
                            tabControlConnections.TabPages.Insert(lastIndex, localCustomTabPage);
                            tabControlConnections.SelectedIndex = lastIndex;

                            TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"A new Snowflake connection was created."));
                        }
                        else
                        {
                            richTextBoxInformation.AppendText("There is already a 'new connection' tab open. Please close or save this first.\r\n");
                        }
                    }

                    #endregion

                    else
                    {
                        richTextBoxInformation.AppendText("No valid connection type was selected.\r\n");
                    }
                }
                else
                {
                    richTextBoxInformation.AppendText("The creation of a new connection was cancelled.\r\n");
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

            comboBoxEnvironments.Items.Remove(new KeyValuePair<TeamEnvironment, string>(e.Value, e.Value.environmentKey));
        }

        private void SaveEnvironment(object o, MyStringEventArgs e)
        {
            comboBoxEnvironments.Items.Clear();

            foreach (var environment in TeamEnvironmentCollection.EnvironmentDictionary)
            {
                comboBoxEnvironments.Items.Add(new KeyValuePair<TeamEnvironment, string>(environment.Value, environment.Value.environmentKey));
                comboBoxEnvironments.DisplayMember = "Value";
            }

            comboBoxEnvironments.SelectedIndex = comboBoxEnvironments.FindStringExact(globalParameters.ActiveEnvironmentKey);
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
                TeamEnvironment workingEnvironment = new TeamEnvironment();
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
                Process.Start(globalParameters.CorePath + globalParameters.PathFileName + globalParameters.FileExtension);

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
                Process.Start(globalParameters.ConfigurationPath + globalParameters.ConfigFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.FileExtension);

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

                var localComboBoxSelection = (KeyValuePair<TeamEnvironment, string>) localComboBox.SelectedItem;

                var selectedItem = localComboBoxSelection.Key;

                // Get the full environment from the in-memory dictionary.
                var localEnvironment = TeamEnvironmentCollection.EnvironmentDictionary[selectedItem.environmentInternalId];

                // Set the working environment in memory.
                globalParameters.ActiveEnvironmentInternalId = localEnvironment.environmentInternalId;
                globalParameters.ActiveEnvironmentKey = localEnvironment.environmentKey;
                globalParameters.ConfigurationPath = localEnvironment.configurationPath;
                globalParameters.MetadataPath = localEnvironment.metadataPath;

                // Configuration Path
                FileHandling.InitialisePath(globalParameters.ConfigurationPath, TeamPathTypes.ConfigurationPath, TeamEventLog);
                // Metadata Path
                FileHandling.InitialisePath(globalParameters.MetadataPath, TeamPathTypes.MetadataPath, TeamEventLog);

                //Paths
                textBoxConfigurationPath.Text = globalParameters.ConfigurationPath;
                textBoxTeamMetadataPath.Text = globalParameters.MetadataPath;

                // Update the root path file with the new working directory.
                SaveTeamCoreFile();

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

                var connectionFileName = globalParameters.ConfigurationPath + globalParameters.JsonConnectionFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.JsonExtension;

                TeamConfiguration.ConnectionDictionary = TeamConnectionFile.LoadConnectionFile(connectionFileName, TeamEventLog);

                comboBoxMetadataConnection.Items.Clear();
                AddConnectionTabPages();

                try
                {
                    LocalInitialiseConnections(globalParameters.ConfigurationPath + globalParameters.ConfigFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.FileExtension);
                }
                catch (Exception ex)
                {
                    richTextBoxInformation.AppendText($"Errors occurred trying to load the configuration file, the message is {ex.Message}. No default values were loaded.\r\n\r\n");
                }

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

            fileBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
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

            fileBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
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
                    var psi = new ProcessStartInfo() { FileName = textBoxTeamMetadataPath.Text, UseShellExecute = true };
                    Process.Start(psi);
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

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBoxInformation.Clear();
            // Save the paths against the selected TEAM environment.
            SaveEnvironmentPaths();

            // Save the TEAM Core file to set the currently selected working environment.
            SaveTeamCoreFile();

            // Update the in-memory variables for use throughout the application, to commit the saved changes for runtime use. 
            // This is needed before saving to disk, as the EnvironmentConfiguration Class retrieves the values from memory.
            UpdateMetadataConnectionConfigurationInMemory();
            UpdateDataObjectTypesConfigurationInMemory();
            UpdateDataWarehouseConfigurationInMemory();

            // Save the information 
            LocalTeamEnvironmentConfiguration.SaveTeamConfigurationFile();
            parentFormMain.RevalidateFlag = true;

            richTextBoxInformation.Text += "The full configuration has been saved.";
        }

        /// <summary>
        /// Update the information on the 'paths' tab against the selected TEAM environment.
        /// </summary>
        private void SaveEnvironmentPaths()
        {
            // Add a backslash, if not present for some reason.
            if (!textBoxConfigurationPath.Text.EndsWith(@"\"))
            {
                textBoxConfigurationPath.Text += @"\";
            }

            if (!textBoxTeamMetadataPath.Text.EndsWith(@"\"))
            {
                textBoxTeamMetadataPath.Text += @"\";
            }

            // Set the parameters with the form values.
            globalParameters.ConfigurationPath = textBoxConfigurationPath.Text;
            globalParameters.MetadataPath = textBoxTeamMetadataPath.Text;

            if (comboBoxEnvironments.SelectedItem != null)
            {
                var localEnvironment = (KeyValuePair<TeamEnvironment, string>)comboBoxEnvironments.SelectedItem;
                globalParameters.ActiveEnvironmentInternalId = localEnvironment.Key.environmentInternalId;
                globalParameters.ActiveEnvironmentKey = localEnvironment.Key.environmentKey;

                // Make sure the new paths as updated are available upon save for backup etc.
                // Check if the paths and files are available, just to be sure.
                FileHandling.InitialisePath(globalParameters.ConfigurationPath, TeamPathTypes.ConfigurationPath, TeamEventLog);
                FileHandling.InitialisePath(globalParameters.MetadataPath, TeamPathTypes.MetadataPath, TeamEventLog);

                TeamConfiguration.CreateDummyTeamConfigurationFile(globalParameters.ConfigurationPath + globalParameters.ConfigFileName + '_' + globalParameters.ActiveEnvironmentKey +
                                                                   globalParameters.FileExtension);
                ValidationSetting.CreateDummyValidationFile(globalParameters.ConfigurationPath + globalParameters.ValidationFileName + '_' + globalParameters.ActiveEnvironmentKey +
                                                            globalParameters.FileExtension);
                JsonExportSetting.CreateDummyJsonConfigurationFile(
                    globalParameters.ConfigurationPath + globalParameters.JsonExportConfigurationFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.FileExtension, TeamEventLog);

                // Also updating the environments for paths etc.
                localEnvironment.Key.metadataPath = globalParameters.MetadataPath;
                localEnvironment.Key.configurationPath = globalParameters.ConfigurationPath;
                localEnvironment.Key.SaveTeamEnvironment(globalParameters.CorePath + globalParameters.JsonEnvironmentFileName + globalParameters.JsonExtension);
            }
            else
            {
                richTextBoxInformation.Text += "The selected environment has been deleted, and can not be saved.";
            }
        }
    }
}