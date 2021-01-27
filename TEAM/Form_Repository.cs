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
        Form_Alert _alertSampleData;
        Form_Alert _alertMetadata;

        public FormManageRepository()
        {
            InitializeComponent();

            foreach (var connection in TeamConfigurationSettings.ConnectionDictionary)
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

            if (TeamConfigurationSettings.MetadataConnection is null)
            {
                // Do nothing
            }
            else
            {
                comboBoxSourceConnection.SelectedIndex = comboBoxSourceConnection.FindStringExact(TeamConfigurationSettings.MetadataConnection.ConnectionKey);
                comboBoxStagingConnection.SelectedIndex = comboBoxStagingConnection.FindStringExact(TeamConfigurationSettings.MetadataConnection.ConnectionKey);
                comboBoxPsaConnection.SelectedIndex = comboBoxPsaConnection.FindStringExact(TeamConfigurationSettings.MetadataConnection.ConnectionKey);
                comboBoxIntegrationConnection.SelectedIndex = comboBoxIntegrationConnection.FindStringExact(TeamConfigurationSettings.MetadataConnection.ConnectionKey);
                comboBoxPresentationConnection.SelectedIndex = comboBoxIntegrationConnection.FindStringExact(TeamConfigurationSettings.MetadataConnection.ConnectionKey);
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
                    targetForm.SetTextLogging("An issue has occurred " + ex);
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
            if (TeamConfigurationSettings.MetadataRepositoryType == MetadataRepositoryStorageType.Json)
            {
                TeamJsonHandling.CreateDummyJsonFile(GlobalParameters.JsonTableMappingFileName);
                TeamJsonHandling.CreateDummyJsonFile(GlobalParameters.JsonAttributeMappingFileName);
                TeamJsonHandling.CreateDummyJsonFile(GlobalParameters.JsonModelMetadataFileName);
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
                    MessageBox.Show("An issue occurred creating the sample schemas. The error message is: " + ex, "An issue has occurred", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            var localSourceDatabaseName = localSourceConnectionObject.Key.DatabaseServer.DatabaseName;


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
                PopulateSqlCommandDictionaryFromFile(GlobalParameters.ScriptPath + "generateSampleSourceSchema.sql", commandDictionary, localSourceConnectionString);
            }
            #endregion

            #region Staging

            if (checkBoxCreateSampleStaging.Checked)
            {

                PopulateSqlCommandDictionaryFromFile(GlobalParameters.ScriptPath + @"generateSampleStagingSchema.sql",
                    commandDictionary, localStagingConnectionString);

            }

            #endregion

            #region Persistent Staging

            if (checkBoxCreateSamplePSA.Checked)
            {

                PopulateSqlCommandDictionaryFromFile(
                    GlobalParameters.ScriptPath + @"generateSamplePersistentStagingSchema.sql", commandDictionary,
                    localPsaConnectionString);

            }

            #endregion

            #region Integration Layer
            if (checkBoxCreateSampleIntegration.Checked)
            {

                
                    PopulateSqlCommandDictionaryFromFile(GlobalParameters.ScriptPath + @"generateSampleIntegrationSchema.sql", commandDictionary, localIntegrationConnectionString);
                
            }
            #endregion

            #region Presentation Layer
            if (checkBoxCreateSamplePresentation.Checked)
            {
                PopulateSqlCommandDictionaryFromFile(GlobalParameters.ScriptPath + @"generateSamplePresentationSchema.sql", commandDictionary, localPresentationConnectionString);

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

            }
        }

        private void SetStandardConfigurationSettings()
        {
            if (checkBoxConfigurationSettings.Checked)
            {
                TeamUtility.CreateFileBackup(GlobalParameters.ConfigurationPath +GlobalParameters.ConfigFileName + '_' + GlobalParameters.WorkingEnvironment + FormBase.GlobalParameters.FileExtension);

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


                persistentStagingAreaPrefix = "PSA";
                keyIdentifier = "SK";

                sourceRowId = "SOURCE_ROW_ID";
                eventDateTime = "EVENT_DATETIME";
                loadDateTime = "LOAD_DATETIME";
                expiryDateTime = "LOAD_END_DATETIME";
                changeDataIndicator = "CDC_OPERATION";
                recordSource = "RECORD_SOURCE";
                etlProcessId = "MODULE_INSTANCE_ID";
                etlUpdateProcessId = "MODULE_UPDATE_INSTANCE_ID";
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

                LocalTeamEnvironmentConfiguration.SaveConfigurationFile();
            }
        }
         
        private void buttonClick_GenerateMetadata(object sender, EventArgs e)
        {
            if (backgroundWorkerMetadata.IsBusy != true)
            {
                // create a new instance of the alert form
                _alertMetadata = new Form_Alert();
                // event handler for the Cancel button in AlertForm
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




        private void linkLabelSource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabelSource.LinkVisited = true;
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start("https://bit.ly/2ARcCTw");
        }

        private void linkLabelStaging_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabelStaging.LinkVisited = true;
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start("https://bit.ly/2VY4Os3");
        
        }

        private void linkLabelPsa_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabelPSA.LinkVisited = true;
            //Call the Process.Start method to open the default browser
            //with a URL:
            System.Diagnostics.Process.Start("https://bit.ly/2SX0Xth");
        
        }

        private void linkLabelIntegration_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Change the color of the link text by setting LinkVisited
            // to true.
            linkLabelIntegration.LinkVisited = true;
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


                                if (fileName.StartsWith("sample_") && (!fileName.StartsWith("sample_DIRECT")))
                                {
                                    fileName = fileName.Replace("sample_", GlobalParameters.WorkingEnvironment+"_");
                                    fileDictionary.Add(filePath, fileName);
                                }
                            

                        }

                        // And then process them
                        foreach (KeyValuePair<string, string> file in fileDictionary)
                        {
                            File.Copy(file.Key, GlobalParameters.ConfigurationPath + "\\" + file.Value, true);
                            _alertMetadata.SetTextLogging("Created sample JSON file " + file.Value + " in " + GlobalParameters.ConfigurationPath + "\r\n");
                        }

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


                PopulateSqlCommandDictionaryFromFile(
                    GlobalParameters.ScriptPath + @"generateSampleMappingMetadata.sql",
                    commandDictionary, TeamConfigurationSettings.MetadataConnection.CreateSqlServerConnectionString(false));
            

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
    }
}