using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TEAM
{
    public partial class FormManageRepository : FormBase
    {
        Form_Alert _alertRepository;
        Form_Alert _alertSampleData;
        Form_Alert _alertMetadata;

        public FormManageRepository()
        {
            InitializeComponent();

            labelMetadataRepository.Text = "Repository type in configuration is set to " + TeamConfigurationSettings.MetadataRepositoryType;

            foreach (var connection in TeamConfigurationSettings.ConnectionDictionary)
            {
                // Adding items in the drop down list
                comboBoxSourceConnection.Items.Add(new KeyValuePair<TeamConnectionProfile, string>(connection.Value, connection.Value.databaseConnectionKey));
                comboBoxStagingConnection.Items.Add(new KeyValuePair<TeamConnectionProfile, string>(connection.Value, connection.Value.databaseConnectionKey));
                comboBoxPsaConnection.Items.Add(new KeyValuePair<TeamConnectionProfile, string>(connection.Value, connection.Value.databaseConnectionKey));
                comboBoxIntegrationConnection.Items.Add(new KeyValuePair<TeamConnectionProfile, string>(connection.Value, connection.Value.databaseConnectionKey));
            }

            comboBoxSourceConnection.ValueMember = "Key";
            comboBoxSourceConnection.DisplayMember = "Value";
            comboBoxStagingConnection.ValueMember = "Key";
            comboBoxStagingConnection.DisplayMember = "Value";
            comboBoxPsaConnection.ValueMember = "Key";
            comboBoxPsaConnection.DisplayMember = "Value";
            comboBoxIntegrationConnection.ValueMember = "Key";
            comboBoxIntegrationConnection.DisplayMember = "Value";

            comboBoxSourceConnection.SelectedIndex = comboBoxSourceConnection.FindStringExact(TeamConfigurationSettings.MetadataConnection.databaseConnectionKey);
            comboBoxStagingConnection.SelectedIndex = comboBoxStagingConnection.FindStringExact(TeamConfigurationSettings.MetadataConnection.databaseConnectionKey);
            comboBoxPsaConnection.SelectedIndex = comboBoxPsaConnection.FindStringExact(TeamConfigurationSettings.MetadataConnection.databaseConnectionKey);
            comboBoxIntegrationConnection.SelectedIndex = comboBoxIntegrationConnection.FindStringExact(TeamConfigurationSettings.MetadataConnection.databaseConnectionKey);
        }

        private void buttonDeploy_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerRepository.IsBusy != true)
            {
                // create a new instance of the alert form
                _alertRepository = new Form_Alert();
                // event handler for the Cancel button in AlertForm
                _alertRepository.Canceled += buttonCancel_Click;
                _alertRepository.Show();
                // Start the asynchronous operation.
                backgroundWorkerRepository.RunWorkerAsync();
            }
        }

        // This event handler cancels the backgroundworker, fired from Cancel button in AlertForm.
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerRepository.WorkerSupportsCancellation)
            {
                // Cancel the asynchronous operation.
                backgroundWorkerRepository.CancelAsync();
                // Close the AlertForm
                _alertRepository.Close();
            }
        }

        /// <summary>
        /// Run a SQL command against the provided database connection, capture any errors and report feedback to the Repository screen.
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="createStatement"></param>
        /// <param name="worker"></param>
        /// <param name="progressCounter"></param>
        private void RunSqlCommandRepositoryForm(string connString, string createStatement, BackgroundWorker worker, int progressCounter)
        {
            using (var connectionVersion = new SqlConnection(connString))
            {
                var commandVersion = new SqlCommand(createStatement, connectionVersion);

                try
                {
                    connectionVersion.Open();
                    commandVersion.ExecuteNonQuery();

                    worker.ReportProgress(progressCounter);
                    _alertRepository.SetTextLogging(createStatement);
                }
                catch (Exception ex)
                {
                    _alertRepository.SetTextLogging("An issue has occured " + ex);
                    _alertRepository.SetTextLogging("This occurred with the following query: " + createStatement + "\r\n\r\n");
                    ErrorHandlingParameters.ErrorCatcher++;
                    ErrorHandlingParameters.ErrorLog.AppendLine("An error occurred with the following query: " + createStatement + "\r\n\r\n)");
                }
            }
        }

        /// <summary>
        /// /// Run a SQL command against the provided database connection, capture any errors and report feedback to the Sample data screen.
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="createStatement"></param>
        /// <param name="worker"></param>
        /// <param name="progressCounter"></param>
        private void RunSqlCommandSampleDataForm(string connString, string createStatement, BackgroundWorker worker, int progressCounter, Form_Alert targetForm)
        {
            using (var connectionVersion = new SqlConnection(connString))
            {
                var commandVersion = new SqlCommand(createStatement, connectionVersion);

                try
                {
                    connectionVersion.Open();
                    commandVersion.ExecuteNonQuery();

                    worker.ReportProgress(progressCounter);
                    targetForm.SetTextLogging(createStatement);
                }
                catch (Exception ex)
                {
                    targetForm.SetTextLogging("An issue has occured " + ex);
                    targetForm.SetTextLogging("This occurred with the following query: " + createStatement + "\r\n\r\n");
                    ErrorHandlingParameters.ErrorCatcher++;
                    ErrorHandlingParameters.ErrorLog.AppendLine("An error occurred with the following query: " + createStatement + "\r\n\r\n)");
                }
            }
        }

        /// <summary>
        /// Truncate the existing metdata (MD) schema.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonTruncate_Click(object sender, EventArgs e)
        {
            // Retrieving the required parameters

            // Truncating the entire repository
            StringBuilder commandText = new StringBuilder();

            commandText.AppendLine("DELETE FROM [MD_SOURCE_STAGING_ATTRIBUTE_XREF];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_LINK_ATTRIBUTE_XREF];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_STAGING_XREF];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_PERSISTENT_STAGING_XREF];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_LINK_XREF];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_SATELLITE_XREF];");
            commandText.AppendLine("DELETE FROM [MD_DRIVING_KEY_XREF];");
            commandText.AppendLine("DELETE FROM [MD_HUB_LINK_XREF];");
            commandText.AppendLine("DELETE FROM [MD_SATELLITE];");
            commandText.AppendLine("DELETE FROM [MD_BUSINESS_KEY_COMPONENT_PART];");
            commandText.AppendLine("DELETE FROM [MD_BUSINESS_KEY_COMPONENT];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_HUB_XREF];");
            commandText.AppendLine("DELETE FROM [MD_ATTRIBUTE];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE];");
            commandText.AppendLine("DELETE FROM [MD_STAGING];");
            commandText.AppendLine("DELETE FROM [MD_PERSISTENT_STAGING];");
            commandText.AppendLine("DELETE FROM [MD_HUB];");
            commandText.AppendLine("DELETE FROM [MD_LINK];");

            commandText.AppendLine("TRUNCATE TABLE [MD_VERSION];");

            using (var connection = new SqlConnection(TeamConfigurationSettings.MetadataConnection.CreateConnectionString(false)))
            {
                var command = new SqlCommand(commandText.ToString(), connection);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("The metadata tables have been truncated.", "Completed", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "An issue occurred when truncating the metadata tables. The error message is: " + ex,
                        "An issue has occured", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (TeamConfigurationSettings.MetadataRepositoryType == MetadataRepositoryStorageType.Json)
            {
                JsonHandling.CreateDummyJsonFile(GlobalParameters.JsonTableMappingFileName);
                JsonHandling.CreateDummyJsonFile(GlobalParameters.JsonAttributeMappingFileName);
                JsonHandling.CreateDummyJsonFile(GlobalParameters.JsonModelMetadataFileName);
            }
        }

        private void backgroundWorkerRepository_DoWork(object sender, DoWorkEventArgs e)
        {
            // Instantiate the thread / background worker
            BackgroundWorker worker = sender as BackgroundWorker;

            ErrorHandlingParameters.ErrorCatcher = 0;
            ErrorHandlingParameters.ErrorLog = new StringBuilder();
            
            var localMetadataConnectionString = TeamConfigurationSettings.MetadataConnection.CreateConnectionString(false);

            // Handle multi-threading
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                // Create the repository
                _alertRepository.SetTextLogging("--Commencing metadata repository creation.\r\n\r\n");

                try
                {  
                    using (StreamReader sr = new StreamReader(GlobalParameters.ScriptPath+"generateRepository.sql"))
                    {
                        var sqlCommands = sr.ReadToEnd().Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                        int counter = 0;

                        foreach (var command in sqlCommands)
                        {
                            // Normalise all values in array against a 0-100 scale to support the progress bar relative to the number of commands to execute.                        
                            var normalisedValue = 1 + (counter - 0) * (100 - 1) / (sqlCommands.Length - 0);

                            RunSqlCommandRepositoryForm(localMetadataConnectionString, command+"\r\n\r\n", worker, normalisedValue);
                            counter++;
                        }
                        worker.ReportProgress(100);
                    }
                }
                catch (Exception ex)
                {
                    _alertRepository.SetTextLogging("An issue has occured executing the repository creation logic. The reported error was: " + ex);
                }

                // Error handling
                if (ErrorHandlingParameters.ErrorCatcher > 0)
                {
                    _alertRepository.SetTextLogging("\r\nWarning! There were " + ErrorHandlingParameters.ErrorCatcher + " error(s) found while processing the metadata.\r\n");
                    _alertRepository.SetTextLogging("Please check the Error Log for details \r\n");
                    _alertRepository.SetTextLogging("\r\n");

                    using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + @"\Error_Log.txt"))
                    {
                        outfile.Write(ErrorHandlingParameters.ErrorLog);
                        outfile.Close();
                    }
                }
                else
                {
                    _alertRepository.SetTextLogging("\r\nNo errors were detected.\r\n");
                }

            }
        }

        private void backgroundWorkerRepository_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Show the progress in main form (GUI)
            labelResult.Text = (e.ProgressPercentage + "%");

            // Pass the progress to AlertForm label and progressbar
            _alertRepository.Message = "In progress, please wait... " + e.ProgressPercentage + "%";
            _alertRepository.ProgressValue = e.ProgressPercentage;
        }

        private void backgroundWorkerRepository_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                labelResult.Text = "Cancelled!";
            }
            else if (e.Error != null)
            {
                labelResult.Text = "Error: " + e.Error.Message;
            }
            else
            {
                labelResult.Text = "Done!";
                if (ErrorHandlingParameters.ErrorCatcher == 0)
                {
                    MessageBox.Show("The metadata repository has been created.", "Completed", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("The metadata repository has been created with " + ErrorHandlingParameters.ErrorCatcher + " errors. Please review the results.", "Completed", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    ErrorHandlingParameters.ErrorCatcher = 0;
                }
            }
        }

        internal static class ErrorHandlingParameters
        {
            public static int ErrorCatcher { get; set; }
            public static StringBuilder ErrorLog { get; set; }
        }

        private void backgroundWorkerSampleData_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            ErrorHandlingParameters.ErrorCatcher = 0;
            ErrorHandlingParameters.ErrorLog = new StringBuilder();

            // Handle multi-threading
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                backgroundWorkerSampleData.ReportProgress(0);

                // Create the sample data
                _alertSampleData.SetTextLogging("Commencing sample data set creation.\r\n\r\n");

                try
                {

                    GenerateDatabaseSample(worker);
                    SetStandardConfigurationSettings();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An issue occurred creating the sample schemas. The error message is: " + ex, "An issue has occured", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Error handling
                if (ErrorHandlingParameters.ErrorCatcher > 0)
                {
                    _alertSampleData.SetTextLogging("\r\nWarning! There were " + ErrorHandlingParameters.ErrorCatcher + " error(s) found while processing the sample data.\r\n");
                    _alertSampleData.SetTextLogging("Please check the Error Log for details \r\n");
                    _alertSampleData.SetTextLogging("\r\n");

                    using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + @"\Error_Log.txt"))
                    {
                        outfile.Write(ErrorHandlingParameters.ErrorLog);
                        outfile.Close();
                    }
                }
                else
                {
                    _alertSampleData.SetTextLogging("\r\nNo errors were detected.\r\n");
                }

                backgroundWorkerSampleData.ReportProgress(100);
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
            var localSourceConnectionObject = new KeyValuePair<TeamConnectionProfile, string>();
            var localPsaConnectionObject = new KeyValuePair<TeamConnectionProfile, string>();
            var localStagingConnectionObject = new KeyValuePair<TeamConnectionProfile, string>();
            var localIntegrationConnectionObject = new KeyValuePair<TeamConnectionProfile, string>();
            var localPresentationConnectionObject = new KeyValuePair<TeamConnectionProfile, string>();


            comboBoxSourceConnection.Invoke((MethodInvoker)delegate
            {
                localSourceConnectionObject = (KeyValuePair<TeamConnectionProfile, string>)comboBoxSourceConnection.SelectedItem;
            });

            var localSourceConnectionString = localSourceConnectionObject.Key.CreateConnectionString(false);
            var localSourceDatabaseName = localSourceConnectionObject.Key.databaseServer.databaseName;


            comboBoxStagingConnection.Invoke((MethodInvoker)delegate
            {
                localStagingConnectionObject = (KeyValuePair<TeamConnectionProfile, string>)comboBoxStagingConnection.SelectedItem;
            });

            var localStagingConnectionString = localStagingConnectionObject.Key.CreateConnectionString(false);
            var localStagingDatabaseName = localStagingConnectionObject.Key.databaseServer.databaseName;


            comboBoxPsaConnection.Invoke((MethodInvoker)delegate
            {
                localPsaConnectionObject = (KeyValuePair<TeamConnectionProfile, string>)comboBoxPsaConnection.SelectedItem;
            });
            var localPsaConnectionString = localPsaConnectionObject.Key.CreateConnectionString(false);
            var localPsaDatabaseName = localPsaConnectionObject.Key.databaseServer.databaseName;


            comboBoxIntegrationConnection.Invoke((MethodInvoker)delegate
            {
                localIntegrationConnectionObject = (KeyValuePair<TeamConnectionProfile, string>)comboBoxIntegrationConnection.SelectedItem;
            });
            var localIntegrationConnectionString = localIntegrationConnectionObject.Key.CreateConnectionString(false);
            var localIntegrationDatabaseName = localIntegrationConnectionObject.Key.databaseServer.databaseName;


            //comboBoxPresentationConnection.Invoke((MethodInvoker)delegate
            //{
            //    localPresentationConnectionObject = (KeyValuePair<TeamConnectionProfile, string>)comboBoxPresentationConnection.SelectedItem;
            //});
            //var localPresentationConnectionString = localPresentationConnectionObject.Key.CreateConnectionString(false);
            //var localPresentationDatabaseName = localPresentationConnectionObject.Key.databaseServer.databaseName;

            #region Source
            if (checkBoxCreateSampleSource.Checked)
            {
                PopulateSqlCommandDictionaryFromFile(GlobalParameters.ScriptPath + "generateSampleSourceSchema.sql", commandDictionary, localSourceConnectionString);
            }
            #endregion

            #region Staging
            if (checkBoxCreateSampleStaging.Checked)
            {
                if (checkBoxDIRECT.Checked)
                {
                    PopulateSqlCommandDictionaryFromFile(GlobalParameters.ScriptPath + @"generateSampleStagingSchemaDIRECT.sql", commandDictionary, localStagingConnectionString);
                }
                else
                {
                    PopulateSqlCommandDictionaryFromFile(GlobalParameters.ScriptPath + @"generateSampleStagingSchema.sql", commandDictionary, localStagingConnectionString);
                }
            }
            #endregion

            #region Persistent Staging
            if (checkBoxCreateSamplePSA.Checked)
            {
                if (checkBoxDIRECT.Checked)
                {
                    PopulateSqlCommandDictionaryFromFile(GlobalParameters.ScriptPath + @"generateSamplePersistentStagingSchemaDIRECT.sql", commandDictionary, localPsaConnectionString);
                }
                else
                {
                    PopulateSqlCommandDictionaryFromFile(GlobalParameters.ScriptPath + @"generateSamplePersistentStagingSchema.sql", commandDictionary, localPsaConnectionString);
                }
            }
            #endregion

            #region Integration Layer
            if (checkBoxCreateSampleDV.Checked)
            {
                if (checkBoxDIRECT.Checked)
                {
                    PopulateSqlCommandDictionaryFromFile(GlobalParameters.ScriptPath + @"generateSampleIntegrationSchemaDIRECT.sql", commandDictionary, localIntegrationConnectionString);
                }
                else
                {
                    PopulateSqlCommandDictionaryFromFile(GlobalParameters.ScriptPath + @"generateSampleIntegrationSchema.sql", commandDictionary, localIntegrationConnectionString);
                }
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

                //if (sqlCommand.Contains("N'300_Presentation_Layer',"))
                //{
                //    sqlCommand = sqlCommand.Replace("N'300_Presentation_Layer',", "N'" + localPresentationDatabaseName + "',");
                //}

                // Normalise all values in array against a 0-100 scale to support the progress bar relative to the number of commands to execute.                        
                var normalisedValue = 1 + (counter - 0) * (100 - 1) / (commandDictionary.Count - 0);

                RunSqlCommandSampleDataForm(individualSQlCommand.Value, sqlCommand + "\r\n\r\n", worker, normalisedValue, _alertSampleData);
                counter++;

                worker.ReportProgress(100);
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
                    var sqlCommands = sr.ReadToEnd()
                        .Split(new string[] {Environment.NewLine + Environment.NewLine},
                            StringSplitOptions.RemoveEmptyEntries);

                    foreach (var command in sqlCommands)
                    {
                        commandDictionary.Add(command, connString);
                    }
                }
            }
            catch (Exception ex)
            {
                _alertRepository.SetTextLogging(
                    "An issue has occured interpreting a file containing the SQL commands. The reported error was: " + ex);
            }
        }

        private void SetStandardConfigurationSettings()
        {
            if (checkBoxConfigurationSettings.Checked)
            {
                EnvironmentConfiguration.CreateFileBackup(GlobalParameters.ConfigurationPath +GlobalParameters.ConfigFileName + '_' + GlobalParameters.WorkingEnvironment + FormBase.GlobalParameters.FileExtension);

                // Shared values (same for all samples)
                //var metadataRepositoryType = "SqlServer";
                var stagingAreaPrefix = "STG";
                var hubTablePrefix = "HUB";
                var satTablePrefix = "SAT";
                var linkTablePrefix = "LNK";
                var linkSatTablePrefix = "LSAT";
                string psaKeyLocation = "PrimaryKey";

                string persistentStagingAreaPrefix;
                string keyIdentifier;
                string sourceRowId;
                string eventDateTime;
                string loadDateTime;
                string expiryDateTime;
                string changeDataIndicator;
                string recordSource;
                string etlProcessId;
                string etlUpdateProcessId;
                string logicalDeleteAttribute;
                string tableNamingLocation;
                string keyNamingLocation;
                string recordChecksum;
                string currentRecordIndicator;
                string alternativeRecordSource;
                string alternativeHubLoadDateTime;
                string alternativeSatelliteLoadDateTime;
                string alternativeRecordSourceFunction;
                string alternativeHubLoadDateTimeFunction;
                string alternativeSatelliteLoadDateTimeFunction;


                // Update the values using the DIRECT information
                if (checkBoxDIRECT.Checked)
                {
                    persistentStagingAreaPrefix = "HSTG";
                    keyIdentifier = "SK";

                    sourceRowId = "OMD_SOURCE_ROW_ID";
                    eventDateTime = "OMD_EVENT_DATETIME";
                    loadDateTime = "OMD_INSERT_DATETIME";
                    expiryDateTime = "OMD_EXPIRY_DATETIME";
                    changeDataIndicator = "OMD_CDC_OPERATION";
                    recordSource = "OMD_RECORD_SOURCE";
                    etlProcessId = "OMD_INSERT_MODULE_INSTANCE_ID";
                    etlUpdateProcessId = "OMD_UPDATE_MODULE_INSTANCE_ID";
                    logicalDeleteAttribute = "OMD_DELETED_RECORD_INDICATOR";
                    tableNamingLocation = "Prefix";
                    keyNamingLocation = "Suffix";
                    recordChecksum = "OMD_HASH_FULL_RECORD";
                    currentRecordIndicator = "OMD_CURRENT_RECORD_INDICATOR";
                    alternativeRecordSource = "OMD_RECORD_SOURCE_ID";
                    alternativeHubLoadDateTime = "OMD_FIRST_SEEN_DATETIME";
                    alternativeSatelliteLoadDateTime = "OMD_EFFECTIVE_DATETIME";
                    alternativeRecordSourceFunction = "True";
                    alternativeHubLoadDateTimeFunction = "True";
                    alternativeSatelliteLoadDateTimeFunction = "True";
                }
                else  // Use the standard (profiler) sample
                {
                    persistentStagingAreaPrefix = "PSA";
                    keyIdentifier = "HSH";

                    sourceRowId = "SOURCE_ROW_ID";
                    eventDateTime = "EVENT_DATETIME";
                    loadDateTime = "LOAD_DATETIME";
                    expiryDateTime = "LOAD_END_DATETIME";
                    changeDataIndicator = "CDC_OPERATION";
                    recordSource = "RECORD_SOURCE";
                    etlProcessId = "ETL_INSERT_RUN_ID";
                    etlUpdateProcessId = "ETL_UPDATE_RUN_ID";
                    logicalDeleteAttribute = "DELETED_RECORD_INDICATOR";
                    tableNamingLocation = "Prefix";
                    keyNamingLocation = "Suffix";
                    recordChecksum = "HASH_FULL_RECORD";
                    currentRecordIndicator = "CURRENT_RECORD_INDICATOR";
                    alternativeRecordSource = "N/A";
                    alternativeHubLoadDateTime = "N/A";
                    alternativeSatelliteLoadDateTime = "N/A";
                    alternativeRecordSourceFunction = "False";
                    alternativeHubLoadDateTimeFunction = "False";
                    alternativeSatelliteLoadDateTimeFunction = "False";
                }

                //TeamConfigurationSettings.MetadataRepositoryType = metadataRepositoryType;

                TeamConfigurationSettings.StgTablePrefixValue = stagingAreaPrefix;
                TeamConfigurationSettings.PsaTablePrefixValue = persistentStagingAreaPrefix;

                TeamConfigurationSettings.HubTablePrefixValue = hubTablePrefix;
                TeamConfigurationSettings.SatTablePrefixValue = satTablePrefix;
                TeamConfigurationSettings.LinkTablePrefixValue = linkTablePrefix;
                TeamConfigurationSettings.LsatTablePrefixValue = linkSatTablePrefix;
                TeamConfigurationSettings.DwhKeyIdentifier = keyIdentifier;
                TeamConfigurationSettings.PsaKeyLocation = psaKeyLocation;
                TeamConfigurationSettings.TableNamingLocation = tableNamingLocation;
                TeamConfigurationSettings.KeyNamingLocation = keyNamingLocation;

                TeamConfigurationSettings.EventDateTimeAttribute = eventDateTime;
                TeamConfigurationSettings.LoadDateTimeAttribute = loadDateTime;
                TeamConfigurationSettings.ExpiryDateTimeAttribute = expiryDateTime;
                TeamConfigurationSettings.ChangeDataCaptureAttribute = changeDataIndicator;
                TeamConfigurationSettings.RecordSourceAttribute = recordSource;
                TeamConfigurationSettings.EtlProcessAttribute = etlProcessId;
                TeamConfigurationSettings.EtlProcessUpdateAttribute = etlUpdateProcessId;
                TeamConfigurationSettings.RowIdAttribute = sourceRowId;
                TeamConfigurationSettings.RecordChecksumAttribute = recordChecksum;
                TeamConfigurationSettings.CurrentRowAttribute = currentRecordIndicator;
                TeamConfigurationSettings.LogicalDeleteAttribute = logicalDeleteAttribute;
                TeamConfigurationSettings.EnableAlternativeRecordSourceAttribute = alternativeRecordSourceFunction;
                TeamConfigurationSettings.AlternativeRecordSourceAttribute = alternativeRecordSource;
                TeamConfigurationSettings.EnableAlternativeLoadDateTimeAttribute = alternativeHubLoadDateTimeFunction;
                TeamConfigurationSettings.AlternativeLoadDateTimeAttribute = alternativeHubLoadDateTime;
                TeamConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute = alternativeSatelliteLoadDateTimeFunction;
                TeamConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute = alternativeSatelliteLoadDateTime;

                EnvironmentConfiguration.SaveConfigurationFile();
            }
        }

        private void backgroundWorkerSampleData_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Show the progress in main form (GUI)
            labelResult.Text = (e.ProgressPercentage + "%");

            // Pass the progress to AlertForm label and progressbar
            _alertSampleData.Message = "In progress, please wait... " + e.ProgressPercentage + "%";
            _alertSampleData.ProgressValue = e.ProgressPercentage;

            // Manage the logging
        }

        private void backgroundWorkerSampleData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                labelResult.Text = "Cancelled!";
            }
            else if (e.Error != null)
            {
                labelResult.Text = "Error: " + e.Error.Message;
            }
            else
            {
                labelResult.Text = "Done!";
                if (ErrorHandlingParameters.ErrorCatcher == 0)
                {
                    MessageBox.Show("The sample schema / data has been created.", "Completed", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("The sample schema / data has been created with "+ ErrorHandlingParameters.ErrorCatcher + " errors. Please review the results.", "Completed", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    ErrorHandlingParameters.ErrorCatcher = 0;
                }
            }
        }

        private void buttonClick_GenerateMetadata(object sender, EventArgs e)
        {
            if (backgroundWorkerMetadata.IsBusy != true)
            {
                // create a new instance of the alert form
                _alertMetadata = new Form_Alert();
                // event handler for the Cancel button in AlertForm
                _alertMetadata.Canceled += buttonCancel_Click;
                _alertMetadata.Show();
                // Start the asynchronous operation.
                SetStandardConfigurationSettings();

                backgroundWorkerMetadata.RunWorkerAsync();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetStandardConfigurationSettings();
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabelRepositoryModel.LinkVisited = true;
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start("https://bit.ly/2YF5tTr");
        }


        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabelRepositoryModel.LinkVisited = true;
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start("https://bit.ly/2ARcCTw");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabelRepositoryModel.LinkVisited = true;
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start("https://bit.ly/2VY4Os3");
        
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabelRepositoryModel.LinkVisited = true;
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start("https://bit.ly/2SX0Xth");
        
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabelRepositoryModel.LinkVisited = true;
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start("https://bit.ly/2FuWBq5");
        
        }

        private void buttonGenerateDatabaseSamples(object sender, EventArgs e)
        {
            if (backgroundWorkerSampleData.IsBusy != true)
            {
                // create a new instance of the alert form
                _alertSampleData = new Form_Alert();
                // event handler for the Cancel button in AlertForm
                _alertSampleData.Canceled += buttonCancel_Click;
                _alertSampleData.Show();
                // Start the asynchronous operation.
                SetStandardConfigurationSettings();
                backgroundWorkerSampleData.RunWorkerAsync();
            }
        }

        private void backgroundWorkerMetadata_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            ErrorHandlingParameters.ErrorCatcher = 0;
            ErrorHandlingParameters.ErrorLog = new StringBuilder();

            // Handle multi-threading
            if (worker != null && worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                backgroundWorkerMetadata.ReportProgress(0);

                // Create the sample data
                _alertMetadata.SetTextLogging("Commencing sample source-to-target metadata creation.\r\n\r\n");

                try
                {
                    if (TeamConfigurationSettings.MetadataRepositoryType == MetadataRepositoryStorageType.Json)
                    {
                        Dictionary<string, string> fileDictionary = new Dictionary<string, string>();

                        // First, figure out which files to process
                        foreach (var filePath in Directory.EnumerateFiles(GlobalParameters.FilesPath, "*.json"))
                        {
                            var fileName = Path.GetFileName(filePath);

                            if (checkBoxDIRECT.Checked)
                            {
                                if (fileName.StartsWith("sample_DIRECT_"))
                                {
                                    fileName = fileName.Replace("sample_DIRECT_", GlobalParameters.WorkingEnvironment+"_");
                                    fileDictionary.Add(filePath, fileName);
                                }
                            }
                            else if (!checkBoxDIRECT.Checked)
                            {
                                if (fileName.StartsWith("sample_") && (!fileName.StartsWith("sample_DIRECT")))
                                {
                                    fileName = fileName.Replace("sample_", GlobalParameters.WorkingEnvironment+"_");
                                    fileDictionary.Add(filePath, fileName);
                                }
                            }
                            else
                            {
                                ErrorHandlingParameters.ErrorLog.AppendLine("There was an issue detecting the type of sample mapping data to be created. Either both DIRECT and regular were checked (or none).\r\n\r\n)");
                            }
                        }

                        // And then process them
                        foreach (KeyValuePair<string, string> file in fileDictionary)
                        {
                            File.Copy(file.Key, GlobalParameters.ConfigurationPath + "\\" + file.Value, true);
                            _alertMetadata.SetTextLogging("Created sample JSON file " + file.Value + " in " + GlobalParameters.ConfigurationPath + "\r\n");
                        }

                    }
                    else
                    {
                        ErrorHandlingParameters.ErrorLog.AppendLine("There was an issue detecting the repository type (SQL Server or JSON). It appears neither was selected. \r\n\r\n)");
                    }



                    #region Configuration Settings

                    SetStandardConfigurationSettings();

                    #endregion
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An issue occurred creating the sample metadata. The error message is: " + ex, "An issue has occured", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Error handling
                if (ErrorHandlingParameters.ErrorCatcher > 0)
                {
                    _alertMetadata.SetTextLogging("\r\nWarning! There were " + ErrorHandlingParameters.ErrorCatcher + " error(s) found while processing the sample data.\r\n");
                    _alertMetadata.SetTextLogging("Please check the Error Log for details \r\n");
                    _alertMetadata.SetTextLogging("\r\n");

                    using (var outfile = new StreamWriter(GlobalParameters.ConfigurationPath + @"\Error_Log.txt"))
                    {
                        outfile.Write(ErrorHandlingParameters.ErrorLog);
                        outfile.Close();
                    }
                }
                else
                {
                    _alertMetadata.SetTextLogging("\r\nNo errors were detected.\r\n");
                }

                backgroundWorkerMetadata.ReportProgress(100);
            }
        }

        private void GenerateMetadataInDatabase(BackgroundWorker worker)
        {
            // Create a dictionary for all SQL files to execute
            Dictionary<string, string> commandDictionary = new Dictionary<string, string>();

            if (checkBoxDIRECT.Checked)
            {
                PopulateSqlCommandDictionaryFromFile(
                    GlobalParameters.ScriptPath + @"generateSampleMappingMetadataDIRECT.sql",
                    commandDictionary, TeamConfigurationSettings.MetadataConnection.CreateConnectionString(false));
            }
            else
            {
                PopulateSqlCommandDictionaryFromFile(
                    GlobalParameters.ScriptPath + @"generateSampleMappingMetadata.sql",
                    commandDictionary, TeamConfigurationSettings.MetadataConnection.CreateConnectionString(false));
            }

            // Execute the SQL statements
            int counter = 0;
            foreach (var individualSQlCommand in commandDictionary)
            {
                var sqlCommand = individualSQlCommand.Key;

                // Normalise all values in array against a 0-100 scale to support the progress bar relative to the number of commands to execute.                        
                var normalisedValue = 1 + (counter - 0) * (100 - 1) / (commandDictionary.Count - 0);

                RunSqlCommandSampleDataForm(individualSQlCommand.Value, sqlCommand + "\r\n\r\n", worker, normalisedValue, _alertMetadata);
                counter++;

                worker.ReportProgress(100);
            }
        }

        private void backgroundWorkerMetadata_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Show the progress in main form (GUI)
            labelResult.Text = (e.ProgressPercentage + "%");

            // Pass the progress to AlertForm label and progressbar
            _alertMetadata.Message = "In progress, please wait... " + e.ProgressPercentage + "%";
            _alertMetadata.ProgressValue = e.ProgressPercentage;

            // Manage the logging
        }

        private void backgroundWorkerMetadata_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                labelResult.Text = "Cancelled!";
            }
            else if (e.Error != null)
            {
                labelResult.Text = "Error: " + e.Error.Message;
            }
            else
            {
                labelResult.Text = "Done!";
                if (ErrorHandlingParameters.ErrorCatcher == 0)
                {
                    MessageBox.Show("The sample metadata has been created.", "Completed", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("The sample metadata has been created with " + ErrorHandlingParameters.ErrorCatcher + " errors. Please review the results.", "Completed", MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    ErrorHandlingParameters.ErrorCatcher = 0;
                }
            }
        }
    }
}