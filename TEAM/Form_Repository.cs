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
        FormAlert _alertRepository;
        FormAlert _alertSampleData;

        public FormManageRepository()
        {
            InitializeComponent();

            labelMetadataRepository.Text = "Repository type in configuration is set to " + ConfigurationSettings.MetadataRepositoryType;
        }

        private void buttonDeploy_Click(object sender, EventArgs e)
        {
            if (backgroundWorkerRepository.IsBusy != true)
            {
                // create a new instance of the alert form
                _alertRepository = new FormAlert();
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
        private void RunSqlCommandSampleDataForm(string connString, string createStatement, BackgroundWorker worker, int progressCounter)
        {
            using (var connectionVersion = new SqlConnection(connString))
            {
                var commandVersion = new SqlCommand(createStatement.ToString(), connectionVersion);

                try
                {
                    connectionVersion.Open();
                    commandVersion.ExecuteNonQuery();

                    worker.ReportProgress(progressCounter);
                    _alertSampleData.SetTextLogging(createStatement.ToString());
                }
                catch (Exception ex)
                {
                    _alertSampleData.SetTextLogging("An issue has occured " + ex);
                    _alertSampleData.SetTextLogging("This occurred with the following query: " + createStatement + "\r\n\r\n");
                    ErrorHandlingParameters.ErrorCatcher++;
                    ErrorHandlingParameters.ErrorLog.AppendLine("An error occurred with the following query: " + createStatement + "\r\n\r\n)");
                }
            }
        }

        /// <summary>
        /// Event to truncate the existing MD schema
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

            if (!checkBoxRetainManualMapping.Checked)
            {
                commandText.AppendLine("DELETE FROM [MD_TABLE_MAPPING];");
                commandText.AppendLine("DELETE FROM [MD_ATTRIBUTE_MAPPING];");
                commandText.AppendLine("TRUNCATE TABLE [MD_VERSION_ATTRIBUTE];");
                commandText.AppendLine("TRUNCATE TABLE [MD_VERSION];");
            }


            using (var connection = new SqlConnection(ConfigurationSettings.ConnectionStringOmd))
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
        }

        private void backgroundWorkerRepository_DoWork(object sender, DoWorkEventArgs e)
        {
            // Instantiate the thread / background worker
            BackgroundWorker worker = sender as BackgroundWorker;

            ErrorHandlingParameters.ErrorCatcher = 0;
            ErrorHandlingParameters.ErrorLog = new StringBuilder();
            
            var connOmdString = ConfigurationSettings.ConnectionStringOmd;

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
                    using (StreamReader sr = new StreamReader(FormBase.GlobalParameters.RootPath + @"..\..\..\Scripts\generateRepository.sql"))
                    {
                        var sqlCommands = sr.ReadToEnd().Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                        int counter = 0;

                        foreach (var command in sqlCommands)
                        {
                            // Normalise all values in array against a 0-100 scale to support the progress bar relative to the number of commands to execute.                        
                            var normalisedValue = 1 + (counter - 0) * (100 - 1) / (sqlCommands.Length - 0);

                            RunSqlCommandRepositoryForm(connOmdString, command+"\r\n\r\n", worker, normalisedValue);
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

                    using (var outfile = new System.IO.StreamWriter(GlobalParameters.ConfigurationPath + @"\Error_Log.txt"))
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

                // Create the repository
                _alertSampleData.SetTextLogging("Commencing sample data set creation.\r\n\r\n");

                try
                {
                    #region Framework Default Attributes
                    var etlFrameworkIncludeStg = new StringBuilder();
                    var etlFrameWorkIncludePsa = new StringBuilder();
                    var etlFrameworkIncludePsaKey = new StringBuilder();

                    var etlFrameworkIncludeHubLink = new StringBuilder();
                    var etlFrameworkIncludeSat = new StringBuilder();
                    var etlFrameworkIncludeSatKey = new StringBuilder();

                    string dwhKeyName;
                    string psaPrefixName;

                    if (checkBoxDIRECT.Checked)
                    {
                        dwhKeyName = "SK";
                        psaPrefixName = "HSTG";

                        etlFrameworkIncludeStg.AppendLine("  [OMD_INSERT_MODULE_INSTANCE_ID] [int] NOT NULL,");
                        etlFrameworkIncludeStg.AppendLine("  [OMD_INSERT_DATETIME] [datetime2] (7) NOT NULL DEFAULT SYSDATETIME(),");
                        etlFrameworkIncludeStg.AppendLine("  [OMD_EVENT_DATETIME] [datetime2] (7) NOT NULL,");
                        etlFrameworkIncludeStg.AppendLine("  [OMD_RECORD_SOURCE] [varchar] (100) NOT NULL,");
                        etlFrameworkIncludeStg.AppendLine("  [OMD_SOURCE_ROW_ID] [int] IDENTITY(1,1) NOT NULL,");
                        etlFrameworkIncludeStg.AppendLine("  [OMD_CDC_OPERATION] [varchar] (100) NOT NULL,");
                        etlFrameworkIncludeStg.AppendLine("  [OMD_HASH_FULL_RECORD] [binary] (16) NOT NULL,");

                        etlFrameWorkIncludePsa.AppendLine("  [OMD_INSERT_MODULE_INSTANCE_ID][int] NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [OMD_INSERT_DATETIME] [datetime2] (7) NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [OMD_EVENT_DATETIME] [datetime2] (7) NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [OMD_RECORD_SOURCE] [varchar] (100) NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [OMD_SOURCE_ROW_ID] [int] NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [OMD_CDC_OPERATION] [varchar] (100) NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [OMD_HASH_FULL_RECORD] [binary] (16) NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [OMD_CURRENT_RECORD_INDICATOR] [varchar] (1) NOT NULL DEFAULT 'Y',");

                        etlFrameworkIncludePsaKey.AppendLine("[OMD_INSERT_DATETIME] ASC, [OMD_SOURCE_ROW_ID] ASC");

                        etlFrameworkIncludeHubLink.AppendLine("  OMD_INSERT_MODULE_INSTANCE_ID integer NOT NULL,");
                        etlFrameworkIncludeHubLink.AppendLine("  OMD_FIRST_SEEN_DATETIME datetime2(7) NOT NULL,");
                        etlFrameworkIncludeHubLink.AppendLine("  [OMD_RECORD_SOURCE_ID] [int] NOT NULL,");

                        etlFrameworkIncludeSat.AppendLine("  OMD_EFFECTIVE_DATETIME datetime2(7) NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  OMD_EXPIRY_DATETIME datetime2(7) NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  OMD_CURRENT_RECORD_INDICATOR varchar(100) NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  OMD_INSERT_MODULE_INSTANCE_ID integer NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  OMD_UPDATE_MODULE_INSTANCE_ID integer NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  OMD_CDC_OPERATION varchar(100) NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  OMD_SOURCE_ROW_ID integer NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  [OMD_RECORD_SOURCE_ID] [int] NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  OMD_HASH_FULL_RECORD binary(16) NOT NULL,");

                        etlFrameworkIncludeSatKey.AppendLine("OMD_EFFECTIVE_DATETIME ASC");
                    }
                    else
                    {
                        dwhKeyName = "HSH";
                        psaPrefixName = "PSA";

                        etlFrameworkIncludeStg.AppendLine("  [ETL_INSERT_RUN_ID] int NOT NULL,");
                        etlFrameworkIncludeStg.AppendLine("  [LOAD_DATETIME] datetime2(7) NOT NULL DEFAULT SYSDATETIME(),");
                        etlFrameworkIncludeStg.AppendLine("  [EVENT_DATETIME] datetime2(7) NOT NULL,");
                        etlFrameworkIncludeStg.AppendLine("  [RECORD_SOURCE] varchar(100) NOT NULL,");
                        etlFrameworkIncludeStg.AppendLine("  [SOURCE_ROW_ID] int NOT NULL IDENTITY( 1,1 ),");
                        etlFrameworkIncludeStg.AppendLine("  [CDC_OPERATION] varchar(100) NOT NULL,");
                        etlFrameworkIncludeStg.AppendLine("  [HASH_FULL_RECORD] binary(16) NOT NULL,");

                        etlFrameWorkIncludePsa.AppendLine("  [ETL_INSERT_RUN_ID] integer NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [LOAD_DATETIME] datetime2(7) NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [EVENT_DATETIME] datetime2(7) NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [RECORD_SOURCE] varchar(100) NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [SOURCE_ROW_ID] integer NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [CDC_OPERATION] varchar(100) NOT NULL,");
                        etlFrameWorkIncludePsa.AppendLine("  [HASH_FULL_RECORD] binary(16) NOT NULL,");

                        etlFrameworkIncludePsaKey.AppendLine("[LOAD_DATETIME] ASC, [SOURCE_ROW_ID] ASC");

                        etlFrameworkIncludeHubLink.AppendLine("  ETL_INSERT_RUN_ID integer NOT NULL,");
                        etlFrameworkIncludeHubLink.AppendLine("  LOAD_DATETIME datetime2(7) NOT NULL,");
                        etlFrameworkIncludeHubLink.AppendLine("  RECORD_SOURCE varchar(100) NOT NULL,");

                        etlFrameworkIncludeSat.AppendLine("  LOAD_DATETIME datetime2(7) NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  LOAD_END_DATETIME datetime2(7) NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  CURRENT_RECORD_INDICATOR varchar(100) NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  ETL_INSERT_RUN_ID integer NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  ETL_UPDATE_RUN_ID integer NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  CDC_OPERATION varchar(100) NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  SOURCE_ROW_ID integer NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  RECORD_SOURCE varchar(100) NOT NULL,");
                        etlFrameworkIncludeSat.AppendLine("  HASH_FULL_RECORD binary(16) NOT NULL,");

                        etlFrameworkIncludeSatKey.AppendLine("LOAD_DATETIME ASC");

                    }
                    #endregion


                    Dictionary<string, string> commandDictionary = new Dictionary<string, string>();

                    #region Source
                    if (checkBoxCreateSampleSource.Checked)
                    {
                        PopulateSqlCommandDictionaryFromFile(FormBase.GlobalParameters.RootPath + @"..\..\..\Scripts\generateSampleSourceSchema.sql", commandDictionary, ConfigurationSettings.ConnectionStringSource);
                    }
                    #endregion

                    #region Staging
                    if (checkBoxCreateSampleStaging.Checked)
                    {
                        if (checkBoxDIRECT.Checked)
                        {
                            PopulateSqlCommandDictionaryFromFile(FormBase.GlobalParameters.RootPath + @"..\..\..\Scripts\generateSampleStagingSchemaDIRECT.sql", commandDictionary, ConfigurationSettings.ConnectionStringStg);
                        }
                        else
                        {
                            PopulateSqlCommandDictionaryFromFile(FormBase.GlobalParameters.RootPath + @"..\..\..\Scripts\generateSampleStagingSchema.sql", commandDictionary, ConfigurationSettings.ConnectionStringStg);
                        }
                    }
                    #endregion

                    #region Persistent Staging
                    if (checkBoxCreateSamplePSA.Checked)
                    {
                        if (checkBoxDIRECT.Checked)
                        {
                            PopulateSqlCommandDictionaryFromFile(FormBase.GlobalParameters.RootPath + @"..\..\..\Scripts\generateSamplePersistentStagingSchemaDIRECT.sql", commandDictionary, ConfigurationSettings.ConnectionStringHstg);
                        }
                        else
                        {
                            PopulateSqlCommandDictionaryFromFile(FormBase.GlobalParameters.RootPath + @"..\..\..\Scripts\generateSamplePersistentStagingSchema.sql", commandDictionary, ConfigurationSettings.ConnectionStringHstg);
                        }
                    }
                    #endregion

                    #region Integration Layer
                    if (checkBoxCreateSampleDV.Checked)
                    {
                        if (checkBoxDIRECT.Checked)
                        {
                            PopulateSqlCommandDictionaryFromFile(FormBase.GlobalParameters.RootPath + @"..\..\..\Scripts\generateSampleIntegrationSchemaDIRECT.sql", commandDictionary, ConfigurationSettings.ConnectionStringInt);
                        }
                        else
                        {
                            PopulateSqlCommandDictionaryFromFile(FormBase.GlobalParameters.RootPath + @"..\..\..\Scripts\generateSampleIntegrationSchema.sql", commandDictionary, ConfigurationSettings.ConnectionStringInt);
                        }
                    }
                    #endregion

                    //#region Presentation Layer
                    //if (checkBoxCreateSamplePresLayer.Checked)
                    //{
                    //    var connString = ConfigurationSettings.ConnectionStringPres;

                    //    // Create sample data
                    //    StringBuilder createStatement = new StringBuilder();

                    //    createStatement.AppendLine("IF OBJECT_ID('dbo.DIM_CUSTOMER', 'U') IS NOT NULL DROP TABLE [DIM_CUSTOMER]");
                    //    createStatement.AppendLine("IF OBJECT_ID('temp.DIM_CUSTOMER_TMP', 'U') IS NOT NULL DROP TABLE [temp].[DIM_CUSTOMER_TMP]");
                    //    createStatement.AppendLine("IF OBJECT_ID('dbo.DIM_CUSTOMER_VW', 'V') IS NOT NULL DROP VIEW [dbo].[DIM_CUSTOMER_VW]");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    // Create the schemas
                    //    createStatement.AppendLine("-- Creating the schema");
                    //    createStatement.AppendLine("IF EXISTS ( SELECT schema_name FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'temp')");
                    //    createStatement.AppendLine("DROP SCHEMA [temp]");
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    createStatement.AppendLine("CREATE SCHEMA [temp]");
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    // Create the tables
                    //    createStatement.AppendLine("/*");
                    //    createStatement.AppendLine("Create tables");
                    //    createStatement.AppendLine("*/");
                    //    createStatement.AppendLine("CREATE TABLE [DIM_CUSTOMER]");
                    //    createStatement.AppendLine("(");
                    //    createStatement.AppendLine("  [DIM_CUSTOMER_" + dwhKeyName +"]          binary(16)  NOT NULL,");
                    //    createStatement.AppendLine("  [ETL_INSERT_RUN_ID]         integer NOT NULL ,");
                    //    createStatement.AppendLine("  [ETL_UPDATE_RUN_ID] integer NOT NULL ,");
                    //    createStatement.AppendLine("  [CHECKSUM_TYPE1] varchar(100)  NOT NULL,");
                    //    createStatement.AppendLine("  [CHECKSUM_TYPE2]            varchar(100)  NOT NULL,");
                    //    createStatement.AppendLine("  [EFFECTIVE_DATETIME]        datetime2(7)  NOT NULL,");
                    //    createStatement.AppendLine("  [EXPIRY_DATETIME]           datetime2(7)  NOT NULL,");
                    //    createStatement.AppendLine("  [CURRENT_RECORD_INDICATOR]  varchar(100)  NOT NULL,");
                    //    createStatement.AppendLine("  [CUSTOMER_" + dwhKeyName +"]              binary(16) NOT NULL,");
                    //    createStatement.AppendLine("  [CUSTOMER_ID]               numeric(38,20)  NOT NULL,");
                    //    createStatement.AppendLine("  [GIVEN_NAME]                varchar(100)  NOT NULL,");
                    //    createStatement.AppendLine("  [SURNAME]                   varchar(100)  NOT NULL,");
                    //    createStatement.AppendLine("  [GENDER]                    varchar(100)  NOT NULL,");
                    //    createStatement.AppendLine("  [SUBURB]                    varchar(100)  NOT NULL,");
                    //    createStatement.AppendLine("  [POSTCODE]                  varchar(100)  NOT NULL,");
                    //    createStatement.AppendLine("  [COUNTRY]                   varchar(100)  NOT NULL,");
                    //    createStatement.AppendLine("  [DATE_OF_BIRTH]             datetime2(7)  NOT NULL,");
                    //    createStatement.AppendLine("  PRIMARY KEY CLUSTERED([DIM_CUSTOMER_" + dwhKeyName +"] ASC)");
                    //    createStatement.AppendLine(")");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    createStatement.AppendLine("CREATE TABLE [temp].[DIM_CUSTOMER_TMP]");
                    //    createStatement.AppendLine("(");
                    //    createStatement.AppendLine("  [ETL_INSERT_RUN_ID][int] NOT NULL,");
                    //    createStatement.AppendLine("  [ETL_UPDATE_RUN_ID] [int] NOT NULL,");
                    //    createStatement.AppendLine("  [EFFECTIVE_DATETIME] [datetime2] (7) NOT NULL,");
                    //    createStatement.AppendLine("  [EXPIRY_DATETIME] [datetime2] (7) NOT NULL,");
                    //    createStatement.AppendLine("  [CURRENT_RECORD_INDICATOR] [varchar] (100) NOT NULL,");
                    //    createStatement.AppendLine("  [CUSTOMER_" + dwhKeyName +"] [char](32) NOT NULL,");
                    //    createStatement.AppendLine("  [CUSTOMER_ID] [numeric] (38, 20) NOT NULL,");
                    //    createStatement.AppendLine("  [GIVEN_NAME] [varchar] (100) NOT NULL,");
                    //    createStatement.AppendLine("  [SURNAME] [varchar] (100) NOT NULL,");
                    //    createStatement.AppendLine("  [GENDER] [varchar] (100) NOT NULL,");
                    //    createStatement.AppendLine("  [SUBURB] [varchar] (100) NOT NULL,");
                    //    createStatement.AppendLine("  [POSTCODE] [varchar] (100) NOT NULL,");
                    //    createStatement.AppendLine("  [COUNTRY] [varchar] (100) NOT NULL,");
                    //    createStatement.AppendLine("  [DATE_OF_BIRTH] [datetime2] (7) NOT NULL");
                    //    createStatement.AppendLine(")");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();


                    //    createStatement.AppendLine("EXEC sp_addextendedproperty");
                    //    createStatement.AppendLine("@name = 'HistoryType', @value = 'Type1',");
                    //    createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                    //    createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                    //    createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'GIVEN_NAME'");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    createStatement.AppendLine("EXEC sp_addextendedproperty");
                    //    createStatement.AppendLine("@name = 'HistoryType', @value = 'Type1',");
                    //    createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                    //    createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                    //    createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'SURNAME'");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    createStatement.AppendLine("EXEC sp_addextendedproperty");
                    //    createStatement.AppendLine("@name = 'HistoryType', @value = 'Type2',");
                    //    createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                    //    createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                    //    createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'SUBURB'");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    createStatement.AppendLine("EXEC sp_addextendedproperty");
                    //    createStatement.AppendLine("@name = 'HistoryType', @value = 'None',");
                    //    createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                    //    createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                    //    createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'POSTCODE'");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    createStatement.AppendLine("EXEC sp_addextendedproperty");
                    //    createStatement.AppendLine("@name = 'HistoryType', @value = 'None',");
                    //    createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                    //    createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                    //    createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'GENDER'");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    createStatement.AppendLine("EXEC sp_addextendedproperty");
                    //    createStatement.AppendLine("@name = 'HistoryType', @value = 'None',");
                    //    createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                    //    createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                    //    createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'DATE_OF_BIRTH'");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    createStatement.AppendLine("EXEC sp_addextendedproperty");
                    //    createStatement.AppendLine("@name = 'HistoryType', @value = 'Type1',");
                    //    createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                    //    createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                    //    createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'CUSTOMER_ID'");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    createStatement.AppendLine("EXEC sp_addextendedproperty");
                    //    createStatement.AppendLine("@name = 'HistoryType', @value = 'None',");
                    //    createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                    //    createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                    //    createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'DIM_CUSTOMER_" + dwhKeyName +"'");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();

                    //    createStatement.AppendLine("/*");
                    //    createStatement.AppendLine("	Create the Views");
                    //    createStatement.AppendLine("*/");
                    //    createStatement.AppendLine("CREATE VIEW DIM_CUSTOMER_VW AS");
                    //    createStatement.AppendLine("SELECT ");
                    //    createStatement.AppendLine("  -1 AS[ETL_INSERT_RUN_ID],");
                    //    createStatement.AppendLine("  -1 AS[ETL_UPDATE_RUN_ID],");
                    //    createStatement.AppendLine("  scu.[LOAD_DATETIME] AS EFFECTIVE_DATETIME,");
                    //    createStatement.AppendLine("  scu.[LOAD_END_DATETIME] AS EXPIRY_DATETIME,");
                    //    createStatement.AppendLine("  scu.[CURRENT_RECORD_INDICATOR],");
                    //    createStatement.AppendLine("  --scu.[DELETED_RECORD_INDICATOR],");
                    //    createStatement.AppendLine("  hcu.CUSTOMER_" + dwhKeyName +", ");
                    //    createStatement.AppendLine("  hcu.CUSTOMER_ID,");
                    //    createStatement.AppendLine("  scu.GIVEN_NAME,");
                    //    createStatement.AppendLine("  scu.SURNAME,");
                    //    createStatement.AppendLine("  CASE scu.GENDER");
                    //    createStatement.AppendLine("    WHEN 'M' THEN 'Male'");
                    //    createStatement.AppendLine("    WHEN 'F' THEN 'Female'");
                    //    createStatement.AppendLine("    ELSE 'Unknown'");
                    //    createStatement.AppendLine("  END AS GENDER,");
                    //    createStatement.AppendLine("  scu.SUBURB,");
                    //    createStatement.AppendLine("  scu.POSTCODE,");
                    //    createStatement.AppendLine("  scu.COUNTRY,");
                    //    createStatement.AppendLine("  CAST(scu.DATE_OF_BIRTH AS DATE) AS DATE_OF_BIRTH");
                    //    createStatement.AppendLine("FROM");
                    //    createStatement.AppendLine("  " + ConfigurationSettings.IntegrationDatabaseName + "." + ConfigurationSettings.SchemaName + ".HUB_CUSTOMER AS hcu INNER JOIN");
                    //    createStatement.AppendLine("  " + ConfigurationSettings.IntegrationDatabaseName + "." + ConfigurationSettings.SchemaName + ".SAT_CUSTOMER AS scu ON hcu.CUSTOMER_" + dwhKeyName +" = scu.CUSTOMER_" + dwhKeyName +"");
                    //    createStatement.AppendLine("WHERE");
                    //    createStatement.AppendLine("  (ISNULL(scu.CURRENT_RECORD_INDICATOR, 'Y') = 'Y') ");
                    //    createStatement.AppendLine();
                    //    RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                    //    createStatement.Clear();
                    //}
                    //#endregion

                    #region Metadadata
                    if (checkBoxCreateMetadataMapping.Checked)
                    {
                        if (!checkBoxRetainManualMapping.Checked)
                        {
                            if (checkBoxDIRECT.Checked)
                            {
                                PopulateSqlCommandDictionaryFromFile(FormBase.GlobalParameters.RootPath + @"..\..\..\Scripts\generateSampleMappingMetadataDIRECT.sql", commandDictionary, ConfigurationSettings.ConnectionStringOmd);
                            }
                            else
                            {
                                PopulateSqlCommandDictionaryFromFile(FormBase.GlobalParameters.RootPath + @"..\..\..\Scripts\generateSampleMappingMetadata.sql", commandDictionary, ConfigurationSettings.ConnectionStringOmd);
                            }
                        }
                        else
                        {
                            _alertSampleData.SetTextLogging("The option to retain the mapping metadata is checked, so new mapping metadata has not been added.");
                        }
                    }
                    #endregion

                    // Execute the SQL statements
                    foreach (var individualSQlCommand in commandDictionary)
                    {
                        int counter = 0;

                        // Normalise all values in array against a 0-100 scale to support the progress bar relative to the number of commands to execute.                        
                        var normalisedValue = 1 + (counter - 0) * (100 - 1) / (commandDictionary.Count - 0);

                        RunSqlCommandSampleDataForm(individualSQlCommand.Value, individualSQlCommand.Key + "\r\n\r\n", worker, normalisedValue);
                        counter++;

                        worker.ReportProgress(100);
                    }

                    #region Configuration Settings

                    SetStandardConfigurationSettings();

                    #endregion
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

                    using (var outfile = new System.IO.StreamWriter(GlobalParameters.ConfigurationPath + @"\Error_Log.txt"))
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
                ClassEnvironmentConfiguration.CreateEnvironmentConfigurationBackupFile();

                // Shared values (same for all samples)
                var metadataRepositoryType = "SQLServer";
                var sourceSystemPrefix = "PROFILER";
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

                ConfigurationSettings.MetadataRepositoryType = metadataRepositoryType;

                ConfigurationSettings.StgTablePrefixValue = stagingAreaPrefix;
                ConfigurationSettings.PsaTablePrefixValue = persistentStagingAreaPrefix;

                ConfigurationSettings.HubTablePrefixValue = hubTablePrefix;
                ConfigurationSettings.SatTablePrefixValue = satTablePrefix;
                ConfigurationSettings.LinkTablePrefixValue = linkTablePrefix;
                ConfigurationSettings.LsatTablePrefixValue = linkSatTablePrefix;
                ConfigurationSettings.DwhKeyIdentifier = keyIdentifier;
                ConfigurationSettings.PsaKeyLocation = psaKeyLocation;
                ConfigurationSettings.TableNamingLocation = tableNamingLocation;
                ConfigurationSettings.KeyNamingLocation = keyNamingLocation;

                ConfigurationSettings.SourceSystemPrefix = sourceSystemPrefix;
                ConfigurationSettings.EventDateTimeAttribute = eventDateTime;
                ConfigurationSettings.LoadDateTimeAttribute = loadDateTime;
                ConfigurationSettings.ExpiryDateTimeAttribute = expiryDateTime;
                ConfigurationSettings.ChangeDataCaptureAttribute = changeDataIndicator;
                ConfigurationSettings.RecordSourceAttribute = recordSource;
                ConfigurationSettings.EtlProcessAttribute = etlProcessId;
                ConfigurationSettings.EtlProcessUpdateAttribute = etlUpdateProcessId;
                ConfigurationSettings.RowIdAttribute = sourceRowId;
                ConfigurationSettings.RecordChecksumAttribute = recordChecksum;
                ConfigurationSettings.CurrentRowAttribute = currentRecordIndicator;
                ConfigurationSettings.LogicalDeleteAttribute = logicalDeleteAttribute;
                ConfigurationSettings.EnableAlternativeRecordSourceAttribute = alternativeRecordSourceFunction;
                ConfigurationSettings.AlternativeRecordSourceAttribute = alternativeRecordSource;
                ConfigurationSettings.EnableAlternativeLoadDateTimeAttribute = alternativeHubLoadDateTimeFunction;
                ConfigurationSettings.AlternativeLoadDateTimeAttribute = alternativeHubLoadDateTime;
                ConfigurationSettings.EnableAlternativeSatelliteLoadDateTimeAttribute = alternativeSatelliteLoadDateTimeFunction;
                ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute = alternativeSatelliteLoadDateTime;

                ClassEnvironmentConfiguration.SaveConfigurationFile();
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

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (backgroundWorkerSampleData.IsBusy != true)
            {
                // create a new instance of the alert form
                _alertSampleData = new FormAlert();
                // event handler for the Cancel button in AlertForm
                _alertSampleData.Canceled += buttonCancel_Click;
                _alertSampleData.Show();
                // Start the asynchronous operation.
                SetStandardConfigurationSettings();
                backgroundWorkerSampleData.RunWorkerAsync();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetStandardConfigurationSettings();
        }


    }
}