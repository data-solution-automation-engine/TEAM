using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using TEAM_Library;

namespace TEAM
{
    public partial class FormManageRepository : FormBase
    {
        Form_Alert _alertSampleDataCreationInDatabase;
        Form_Alert _alertSampleJsonMetadata;

        public FormManageRepository()
        {
            InitializeComponent();

            foreach (var connection in TeamConfiguration.ConnectionDictionary)
            {
                // Adding items in the drop down list
                comboBoxSourceConnection.Items.Add(new KeyValuePair<TeamConnection, string>(connection.Value, connection.Value.ConnectionKey));
                comboBoxStagingConnection.Items.Add(new KeyValuePair<TeamConnection, string>(connection.Value, connection.Value.ConnectionKey));
                comboBoxPsaConnection.Items.Add(new KeyValuePair<TeamConnection, string>(connection.Value, connection.Value.ConnectionKey));
                comboBoxIntegrationConnection.Items.Add(new KeyValuePair<TeamConnection, string>(connection.Value, connection.Value.ConnectionKey));
                comboBoxPresentationConnection.Items.Add(new KeyValuePair<TeamConnection, string>(connection.Value, connection.Value.ConnectionKey));
            }

            comboBoxSourceConnection.ValueMember = "Key";
            comboBoxSourceConnection.DisplayMember = "Value";
            comboBoxStagingConnection.ValueMember = "Key";
            comboBoxStagingConnection.DisplayMember = "Value";
            comboBoxPsaConnection.ValueMember = "Key";
            comboBoxPsaConnection.DisplayMember = "Value";
            comboBoxIntegrationConnection.ValueMember = "Key";
            comboBoxIntegrationConnection.DisplayMember = "Value";
            comboBoxPresentationConnection.ValueMember = "Key";
            comboBoxPresentationConnection.DisplayMember = "Value";

            if (TeamConfiguration.MetadataConnection is null)
            {
                // Do nothing
            }
            else
            {
                comboBoxSourceConnection.SelectedIndex = comboBoxSourceConnection.FindStringExact(TeamConfiguration.MetadataConnection.ConnectionKey);
                comboBoxStagingConnection.SelectedIndex = comboBoxStagingConnection.FindStringExact(TeamConfiguration.MetadataConnection.ConnectionKey);
                comboBoxPsaConnection.SelectedIndex = comboBoxPsaConnection.FindStringExact(TeamConfiguration.MetadataConnection.ConnectionKey);
                comboBoxIntegrationConnection.SelectedIndex = comboBoxIntegrationConnection.FindStringExact(TeamConfiguration.MetadataConnection.ConnectionKey);
                comboBoxPresentationConnection.SelectedIndex = comboBoxIntegrationConnection.FindStringExact(TeamConfiguration.MetadataConnection.ConnectionKey);
            }
        }

        /// <summary>
        /// /// Run a SQL command against the provided database connection, capture any errors and report feedback.
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="createStatement"></param>
        /// <param name="worker"></param>
        /// <param name="progressCounter"></param>
        /// <param name="targetForm"></param>
        private static void RunSqlCommandSampleDataForm(string connString, string createStatement, BackgroundWorker worker, int progressCounter, Form_Alert targetForm)
        {
            using (var connection = new SqlConnection(connString))
            {
                var sqlCommand = new SqlCommand(createStatement, connection);

                try
                {
                    connection.Open();
                    sqlCommand.ExecuteNonQuery();

                    worker.ReportProgress(progressCounter);
                    targetForm.SetTextLogging(createStatement);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"An error has occurred with the following query: \r\n\r\n{createStatement}.\r\n\r\nThe error message is {ex.Message}.";
                    TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, errorMessage));

                    targetForm.SetTextLogging(errorMessage+"\r\n\r\n");
                    targetForm.SetTextLogging($"This occurred with the following query: {createStatement}.\r\n\r\n");
                }
            }
        }

        private void ButtonGenerateDatabaseSamples(object sender, EventArgs e)
        {
            if (backgroundWorkerSampleData.IsBusy != true)
            {
                _alertSampleDataCreationInDatabase = new Form_Alert();
                _alertSampleDataCreationInDatabase.Show();
                _alertSampleDataCreationInDatabase.ShowLogButton(false);

                backgroundWorkerSampleData.RunWorkerAsync();
            }
        }

        private void backgroundWorkerSampleData_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // Handle multi-threading
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                if (worker != null)
                {
                    worker.ReportProgress(0);

                    // Create the sample data
                    _alertSampleDataCreationInDatabase.SetTextLogging("Commencing sample data set creation.\r\n\r\n");

                    try
                    {
                        GenerateDatabaseSample(worker);

                        _alertSampleDataCreationInDatabase.SetTextLogging("\r\n\r\nThe configurations (configuration screen) have also been reset to the TEAM defaults to match the sample source-target mapping metadata.");
                        SetStandardConfigurationSettings();
                        worker.ReportProgress(100);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show($@"An issue occurred creating the sample schemas. The error message is: {exception.Message}", @"An issue has occurred", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        /// <summary>
        /// Create the sample code by running database scripts
        /// </summary>
        /// <param name="worker"></param>
        private void GenerateDatabaseSample(BackgroundWorker worker)
        {
            // Create a dictionary for all SQL files to execute
            Dictionary<string, string> commandDictionary = new Dictionary<string, string>();

            // Retrieve the connection strings from the ComboBox objects.
            var localSourceConnectionObject = new KeyValuePair<TeamConnection, string>();
            var localPsaConnectionObject = new KeyValuePair<TeamConnection, string>();
            var localStagingConnectionObject = new KeyValuePair<TeamConnection, string>();
            var localIntegrationConnectionObject = new KeyValuePair<TeamConnection, string>();
            var localPresentationConnectionObject = new KeyValuePair<TeamConnection, string>();


            comboBoxSourceConnection.Invoke((MethodInvoker)delegate
            {
                localSourceConnectionObject = (KeyValuePair<TeamConnection, string>)comboBoxSourceConnection.SelectedItem;
            });

            var localSourceConnectionString = localSourceConnectionObject.Key.CreateSqlServerConnectionString(false);
            comboBoxStagingConnection.Invoke((MethodInvoker)delegate
            {
                localStagingConnectionObject = (KeyValuePair<TeamConnection, string>)comboBoxStagingConnection.SelectedItem;
            });

            var localStagingConnectionString = localStagingConnectionObject.Key.CreateSqlServerConnectionString(false);
            var localStagingDatabaseName = localStagingConnectionObject.Key.DatabaseServer.DatabaseName;


            comboBoxPsaConnection.Invoke((MethodInvoker)delegate
            {
                localPsaConnectionObject = (KeyValuePair<TeamConnection, string>)comboBoxPsaConnection.SelectedItem;
            });
            var localPsaConnectionString = localPsaConnectionObject.Key.CreateSqlServerConnectionString(false);
            var localPsaDatabaseName = localPsaConnectionObject.Key.DatabaseServer.DatabaseName;


            comboBoxIntegrationConnection.Invoke((MethodInvoker)delegate
            {
                localIntegrationConnectionObject = (KeyValuePair<TeamConnection, string>)comboBoxIntegrationConnection.SelectedItem;
            });
            var localIntegrationConnectionString = localIntegrationConnectionObject.Key.CreateSqlServerConnectionString(false);
            var localIntegrationDatabaseName = localIntegrationConnectionObject.Key.DatabaseServer.DatabaseName;


            comboBoxPresentationConnection.Invoke((MethodInvoker)delegate
            {
                localPresentationConnectionObject = (KeyValuePair<TeamConnection, string>)comboBoxPresentationConnection.SelectedItem;
            });
            var localPresentationConnectionString = localPresentationConnectionObject.Key.CreateSqlServerConnectionString(false);
            var localPresentationDatabaseName = localPresentationConnectionObject.Key.DatabaseServer.DatabaseName;

            #region Source
            if (checkBoxCreateSampleSource.Checked)
            {
                PopulateSqlCommandDictionaryFromFile(globalParameters.ScriptPath + "generateSampleSourceSchema.sql", commandDictionary, localSourceConnectionString);
            }
            #endregion

            #region Staging

            if (checkBoxCreateSampleStaging.Checked)
            {

                PopulateSqlCommandDictionaryFromFile(globalParameters.ScriptPath + @"generateSampleStagingSchema.sql",
                    commandDictionary, localStagingConnectionString);

            }

            #endregion

            #region Persistent Staging

            if (checkBoxCreateSamplePSA.Checked)
            {

                PopulateSqlCommandDictionaryFromFile(
                    globalParameters.ScriptPath + @"generateSamplePersistentStagingSchema.sql", commandDictionary,
                    localPsaConnectionString);

            }

            #endregion

            #region Integration Layer
            if (checkBoxCreateSampleIntegration.Checked)
            {

                
                    PopulateSqlCommandDictionaryFromFile(globalParameters.ScriptPath + @"generateSampleIntegrationSchema.sql", commandDictionary, localIntegrationConnectionString);
                
            }
            #endregion

            #region Presentation Layer
            if (checkBoxCreateSamplePresentation.Checked)
            {
                PopulateSqlCommandDictionaryFromFile(globalParameters.ScriptPath + @"generateSamplePresentationSchema.sql", commandDictionary, localPresentationConnectionString);

            }
            #endregion
            
            // Execute the SQL statements
            int counter = 0;
            foreach (var individualSQlCommand in commandDictionary)
            {
                //Replace some of the database connections with runtime values from the configuration settings
                var sqlCommand = individualSQlCommand.Key;

                if (sqlCommand.Contains("N'100_Staging_Area',"))
                {
                    sqlCommand = sqlCommand.Replace("N'100_Staging_Area',", "N'" + localStagingDatabaseName + "',");
                }

                if (sqlCommand.Contains("N'150_Persistent_Staging_Area',"))
                {
                    sqlCommand = sqlCommand.Replace("N'150_Persistent_Staging_Area',", "N'" + localPsaDatabaseName + "',");
                }

                if (sqlCommand.Contains("N'200_Integration_Layer',"))
                {
                    sqlCommand = sqlCommand.Replace("N'200_Integration_Layer',", "N'" + localIntegrationDatabaseName + "',");
                }

                if (sqlCommand.Contains("N'300_Presentation_Layer',"))
                {
                    sqlCommand = sqlCommand.Replace("N'300_Presentation_Layer',", "N'" + localPresentationDatabaseName + "',");
                }

                // Normalise all values in array against a 0-100 scale to support the progress bar relative to the number of commands to execute.                        
                var normalisedValue = 1 + (counter - 0) * (100 - 1) / (commandDictionary.Count - 0);

                RunSqlCommandSampleDataForm(individualSQlCommand.Value, sqlCommand + "\r\n\r\n", worker, normalisedValue, _alertSampleDataCreationInDatabase);
                counter++;

                worker.ReportProgress(normalisedValue);
            }
        }

        /// <summary>
        /// This method reads a file (using filePath) and populates a Dictionary collection using the individual commands and provided Connection String
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="commandDictionary"></param>
        /// <param name="connString"></param>
        private void PopulateSqlCommandDictionaryFromFile(string filePath, Dictionary<string, string> commandDictionary, string connString)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    var sqlCommands = sr.ReadToEnd().Split(new[] {Environment.NewLine + Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var command in sqlCommands)
                    {
                        commandDictionary.Add(command, connString);
                    }
                }
            }
            catch (Exception ex)
            {
                TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error occurred: {ex}"));
            }
        }

        private void SetStandardConfigurationSettings()
        {
            if (checkBoxConfigurationSettings.Checked)
            {
                FileHandling.CreateFileBackup(globalParameters.ConfigurationPath + globalParameters.ConfigFileName + '_' + globalParameters.ActiveEnvironmentKey + globalParameters.FileExtension, globalParameters.BackupPath);

                // Shared values (same for all samples)
                var stagingAreaPrefix = "STG_";
                var persistentStagingAreaPrefix = "PSA_";
                
                var hubTablePrefix = "HUB_";
                var satTablePrefix = "SAT_";
                var linkTablePrefix = "LNK_";
                var linkSatTablePrefix = "LSAT_";
                string psaKeyLocation = "PrimaryKey";

                var keyIdentifier = "SK";
                var keyPattern = "{dataObject.baseName}_{keyIdentifier}";

                var sourceRowId = "SOURCE_ROW_ID";
                var eventDateTime = "EVENT_DATETIME";
                var loadDateTime = "LOAD_DATETIME";
                var expiryDateTime = "LOAD_END_DATETIME";
                var changeDataIndicator = "CDC_OPERATION";
                var recordSource = "RECORD_SOURCE";
                var etlProcessId = "MODULE_INSTANCE_ID";
                var etlUpdateProcessId = "MODULE_UPDATE_INSTANCE_ID";
                var logicalDeleteAttribute = "DELETED_RECORD_INDICATOR";
                var otherExceptionColumns = "";
                var tableNamingLocation = "Prefix";
                var recordChecksum = "HASH_FULL_RECORD";
                var currentRecordIndicator = "CURRENT_RECORD_INDICATOR";
                var alternativeRecordSource = "N/A";
                var alternativeHubLoadDateTime = "N/A";
                var alternativeSatelliteLoadDateTime = "N/A";
                var alternativeRecordSourceFunction = "False";
                var alternativeHubLoadDateTimeFunction = "False";
                var alternativeSatelliteLoadDateTimeFunction = "False";

                TeamConfiguration.StgTablePrefixValue = stagingAreaPrefix;
                TeamConfiguration.PsaTablePrefixValue = persistentStagingAreaPrefix;

                TeamConfiguration.HubTablePrefixValue = hubTablePrefix;
                TeamConfiguration.SatTablePrefixValue = satTablePrefix;
                TeamConfiguration.LinkTablePrefixValue = linkTablePrefix;
                TeamConfiguration.LsatTablePrefixValue = linkSatTablePrefix;
                TeamConfiguration.KeyIdentifier = keyIdentifier;
                TeamConfiguration.KeyPattern = keyPattern;
                TeamConfiguration.PsaKeyLocation = psaKeyLocation;
                TeamConfiguration.TableNamingLocation = tableNamingLocation;

                TeamConfiguration.EventDateTimeAttribute = eventDateTime;
                TeamConfiguration.LoadDateTimeAttribute = loadDateTime;
                TeamConfiguration.ExpiryDateTimeAttribute = expiryDateTime;
                TeamConfiguration.ChangeDataCaptureAttribute = changeDataIndicator;
                TeamConfiguration.RecordSourceAttribute = recordSource;
                TeamConfiguration.EtlProcessAttribute = etlProcessId;
                TeamConfiguration.EtlProcessUpdateAttribute = etlUpdateProcessId;
                TeamConfiguration.RowIdAttribute = sourceRowId;
                TeamConfiguration.RecordChecksumAttribute = recordChecksum;
                TeamConfiguration.CurrentRowAttribute = currentRecordIndicator;
                TeamConfiguration.LogicalDeleteAttribute = logicalDeleteAttribute;
                TeamConfiguration.OtherExceptionColumns = otherExceptionColumns;
                TeamConfiguration.EnableAlternativeRecordSourceAttribute = alternativeRecordSourceFunction;
                TeamConfiguration.AlternativeRecordSourceAttribute = alternativeRecordSource;
                TeamConfiguration.EnableAlternativeLoadDateTimeAttribute = alternativeHubLoadDateTimeFunction;
                TeamConfiguration.AlternativeLoadDateTimeAttribute = alternativeHubLoadDateTime;
                TeamConfiguration.EnableAlternativeSatelliteLoadDateTimeAttribute = alternativeSatelliteLoadDateTimeFunction;
                TeamConfiguration.AlternativeSatelliteLoadDateTimeAttribute = alternativeSatelliteLoadDateTime;

                LocalTeamEnvironmentConfiguration.SaveTeamConfigurationFile();
            }
        }
         
        private void buttonClick_GenerateMetadata(object sender, EventArgs e)
        {
            if (backgroundWorkerMetadata.IsBusy != true)
            {
                // Create a new instance of the alert form, disabling most stuff.
                _alertSampleJsonMetadata = new Form_Alert();
                _alertSampleJsonMetadata.ShowLogButton(false);
                _alertSampleJsonMetadata.ShowCancelButton(false);
                _alertSampleJsonMetadata.ShowProgressBar(false);
                _alertSampleJsonMetadata.ShowProgressLabel(false);
                _alertSampleJsonMetadata.Show();
                
                // Start the asynchronous operation to create / move the sample Json files.
                backgroundWorkerMetadata.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Background worker to populate the metadata grids (by creating sample Json files).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerMetadata_DoWork(object sender, DoWorkEventArgs e)
        {
            // Handle multi-threading
            if (sender is BackgroundWorker worker && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                // Create the sample data
                _alertSampleJsonMetadata.SetTextLogging("Commencing sample source-to-target metadata creation.\r\n\r\n");

                try
                {
                    Dictionary<string, string> fileDictionary = new Dictionary<string, string>();

                    // First, figure out which files to process
                    foreach (var filePath in Directory.EnumerateFiles(globalParameters.FilesPath, "*.json"))
                    {
                        var fileName = Path.GetFileName(filePath);

                        if (fileName.StartsWith("sample_") && (!fileName.StartsWith("sample_DIRECT")))
                        {
                            fileName = fileName.Replace("sample_", globalParameters.ActiveEnvironmentKey + "_");
                        }

                        fileDictionary.Add(filePath, fileName);
                    }

                    // And then process them
                    foreach (KeyValuePair<string, string> file in fileDictionary)
                    {
                        File.Copy(file.Key, globalParameters.MetadataPath + "\\" + file.Value, true);
                        _alertSampleJsonMetadata.SetTextLogging($"Created sample Json file '{file.Value}' in {globalParameters.MetadataPath}.");
                        _alertSampleJsonMetadata.SetTextLogging("\r\n"); // Empty line
                    }

                    _alertSampleJsonMetadata.SetTextLogging("\r\nThis metadata will populate the data grids in the 'metadata mapping' screen, but not create any data structures in a database.");
                    _alertSampleJsonMetadata.SetTextLogging("\r\nIn other words, this is just the source-target mapping metadata (dataObjectsMappings and dataItemMappings).");

                    #region Configuration Settings

                    _alertSampleJsonMetadata.SetTextLogging("\r\n\r\nThe configurations (configuration screen) have also been reset to the TEAM defaults to match the sample source-target mapping metadata.");
                    SetStandardConfigurationSettings();

                    #endregion
                }
                catch (Exception ex)
                {
                    MessageBox.Show($@"An issue occurred creating the sample metadata. The error message is: {ex.Message}", @"An issue has occurred", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ButtonSetStandardConfiguration(object sender, EventArgs e)
        {
            SetStandardConfigurationSettings();
        }

        private void linkLabelSource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabelSource.LinkVisited = true;
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start("http://roelantvos.com/blog/team-sample-data/");
        }

 private void backgroundWorkerSampleData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Pass the progress to AlertForm label and progressbar
            _alertSampleDataCreationInDatabase.Message = "In progress, please wait... " + e.ProgressPercentage + "%";
            _alertSampleDataCreationInDatabase.ProgressValue = e.ProgressPercentage;
        }
    }
}