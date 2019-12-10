using System;
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
                var commandVersion = new SqlCommand(createStatement.ToString(), connectionVersion);

                try
                {
                    connectionVersion.Open();
                    commandVersion.ExecuteNonQuery();

                    worker.ReportProgress(progressCounter);
                    _alertRepository.SetTextLogging(createStatement.ToString());
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
        private void RunSqlCommandSampleDataForm(string connString, StringBuilder createStatement, BackgroundWorker worker, int progressCounter)
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

            createStatement.Clear();
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

                    #region Source

                    if (checkBoxCreateSampleSource.Checked)
                    {
                        // Create sample data
                        StringBuilder createStatement = new StringBuilder();
                        var connString = ConfigurationSettings.ConnectionStringSource;

                        createStatement.AppendLine("/* Drop the tables, if they exist */");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.ESTIMATED_WORTH', 'U') IS NOT NULL DROP TABLE [ESTIMATED_WORTH]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.PERSONALISED_COSTING', 'U') IS NOT NULL DROP TABLE [PERSONALISED_COSTING]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.CUST_MEMBERSHIP', 'U') IS NOT NULL DROP TABLE [CUST_MEMBERSHIP]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.PLAN', 'U') IS NOT NULL DROP TABLE [PLAN]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.CUSTOMER_OFFER', 'U') IS NOT NULL DROP TABLE [CUSTOMER_OFFER]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.OFFER', 'U') IS NOT NULL DROP TABLE [OFFER]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.CUSTOMER_PERSONAL', 'U') IS NOT NULL DROP TABLE [CUSTOMER_PERSONAL]");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("/* Create the tables */");
                        createStatement.AppendLine("CREATE TABLE [CUST_MEMBERSHIP]");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [CustomerID] integer NOT NULL,");
                        createStatement.AppendLine("  [Plan_Code] varchar(100) NOT NULL,");
                        createStatement.AppendLine("  [Start_Date] datetime NULL,");
                        createStatement.AppendLine("  [End_Date] datetime NULL,");
                        createStatement.AppendLine("  [Status] varchar(10) NULL,");
                        createStatement.AppendLine("  [Comment] varchar(50) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_CUST_MEMBERSHIP] PRIMARY KEY CLUSTERED(CustomerID ASC, Plan_Code ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [CUSTOMER_OFFER]");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [CustomerID] integer NOT NULL,");
                        createStatement.AppendLine("  [OfferID] integer NOT NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_CUSTOMER_OFFER] PRIMARY KEY CLUSTERED (CustomerID ASC, OfferID ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [CUSTOMER_PERSONAL]");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [CustomerID] integer NOT NULL,");
                        createStatement.AppendLine("  [Given] varchar(100) NULL,");
                        createStatement.AppendLine("  [Surname] varchar(100) NULL,");
                        createStatement.AppendLine("  [Suburb] varchar(50) NULL,");
                        createStatement.AppendLine("  [State] varchar(3) NULL,");
                        createStatement.AppendLine("  [Postcode] varchar(6) NULL,");
                        createStatement.AppendLine("  [Country] varchar(100) NULL,");
                        createStatement.AppendLine("  [Gender] varchar(1) NULL,");
                        createStatement.AppendLine("  [DOB] date NULL,");
                        createStatement.AppendLine("  [Contact_Number] integer NULL,");
                        createStatement.AppendLine("  [Referee_Offer_Made] integer NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_CUSTOMER_PERSONAL] PRIMARY KEY CLUSTERED (CustomerID ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [ESTIMATED_WORTH]");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [Plan_Code] varchar(100) NOT NULL,");
                        createStatement.AppendLine("  [Date_effective] datetime NOT NULL,");
                        createStatement.AppendLine("  [Value_Amount] numeric NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_ESTIMATED_WORTH] PRIMARY KEY CLUSTERED(Plan_Code ASC, Date_effective ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [OFFER]");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [OfferID] integer NOT NULL,");
                        createStatement.AppendLine("  [Offer_Long_Description] varchar(100) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_OFFER] PRIMARY KEY CLUSTERED(OfferID ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [PERSONALISED_COSTING]");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [Member] integer NOT NULL,");
                        createStatement.AppendLine("  [Segment] varchar(100) NOT NULL,");
                        createStatement.AppendLine("  [Plan_Code] varchar(100) NOT NULL,");
                        createStatement.AppendLine("  [Date_effective] datetime NOT NULL,");
                        createStatement.AppendLine("  [Monthly_Cost] numeric NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_PERSONALISED_COSTING] PRIMARY KEY CLUSTERED(Member ASC, Segment ASC, Plan_Code ASC, Date_effective ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [PLAN]");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [Plan_Code] varchar(100) NOT NULL,");
                        createStatement.AppendLine("  [Plan_Desc]varchar(100) NULL,");
                        createStatement.AppendLine("  [Renewal_Plan_Code] varchar(100) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_PLAN] PRIMARY KEY CLUSTERED(Plan_Code ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("/* Create the content */");
                        createStatement.AppendLine("INSERT[dbo].[CUSTOMER_PERSONAL] ([CustomerID], [Given], [Surname], [Suburb], [State], [Postcode], [Country], [Gender], [DOB], [Contact_Number], [Referee_Offer_Made]) VALUES(235892, N'Simon', N'Vos', N'Sydney', N'NSW', N'1000', N'Australia', N'M', CAST(N'1960-12-10' AS Date), 9874634, 1)");
                        createStatement.AppendLine("INSERT[dbo].[CUSTOMER_PERSONAL] ([CustomerID], [Given], [Surname], [Suburb], [State], [Postcode], [Country], [Gender], [DOB], [Contact_Number], [Referee_Offer_Made]) VALUES(258279, N'John', N'Doe', N'Indooropilly', N'QLD', N'4000', N'Australia', N'M', CAST(N'1980-01-04' AS Date), 41234, 1)");
                        createStatement.AppendLine("INSERT[dbo].[CUSTOMER_PERSONAL] ([CustomerID], [Given], [Surname], [Suburb], [State], [Postcode], [Country], [Gender], [DOB], [Contact_Number], [Referee_Offer_Made]) VALUES(321799, N'Jonathan', N'Slimpy', N'London', N'N/A', N'0000', N'UK', N'M', CAST(N'1951-01-04' AS Date), 23555, 1)");
                        createStatement.AppendLine("INSERT[dbo].[CUSTOMER_PERSONAL] ([CustomerID], [Given], [Surname], [Suburb], [State], [Postcode], [Country], [Gender], [DOB], [Contact_Number], [Referee_Offer_Made]) VALUES(683492, N'Mary', N'Smith', N'Bulimba', N'QLD', N'3000', N'Australia', N'F', CAST(N'1977-04-12' AS Date), 41234, 0)");
                        createStatement.AppendLine("INSERT[dbo].[CUSTOMER_PERSONAL] ([CustomerID], [Given], [Surname], [Suburb], [State], [Postcode], [Country], [Gender], [DOB], [Contact_Number], [Referee_Offer_Made]) VALUES(885325, N'Michael', N'Evans', N'Bourke', N'NWS', N'2000', N'Australia', N'M', CAST(N'1985-04-19' AS Date), 89235, 0)");
                        createStatement.AppendLine("INSERT[dbo].[OFFER] ([OfferID], [Offer_Long_Description]) VALUES(450, N'20% off all future purchases')");
                        createStatement.AppendLine("INSERT[dbo].[OFFER] ([OfferID], [Offer_Long_Description]) VALUES(462, N'10% off all future purchases')");
                        createStatement.AppendLine("INSERT[dbo].[OFFER] ([OfferID], [Offer_Long_Description]) VALUES(469, N'Free movie tickets')");
                        createStatement.AppendLine("INSERT[dbo].[PLAN] ([Plan_Code], [Plan_Desc], [Renewal_Plan_Code]) VALUES(N'AVG', N'Average / Mix plan', 'SUPR')");
                        createStatement.AppendLine("INSERT[dbo].[PLAN] ([Plan_Code], [Plan_Desc], [Renewal_Plan_Code]) VALUES(N'HIGH', N'Highroller / risk embracing', 'SUPR')");
                        createStatement.AppendLine("INSERT[dbo].[PLAN] ([Plan_Code], [Plan_Desc], [Renewal_Plan_Code]) VALUES(N'LOW', N'Risk avoiding', 'MAXM')");
                        createStatement.AppendLine("INSERT[dbo].[CUST_MEMBERSHIP] ([CustomerID], [Plan_Code], [Start_Date], [End_Date], [Status], [Comment]) VALUES(235892, N'HIGH', CAST(N'2012-05-12T00:00:00.000' AS DateTime), CAST(N'2015-12-31T00:00:00.000' AS DateTime), N'High', N'Trial')");
                        createStatement.AppendLine("INSERT[dbo].[CUST_MEMBERSHIP] ([CustomerID], [Plan_Code], [Start_Date], [End_Date], [Status], [Comment]) VALUES(321799, N'AVG', CAST(N'2010-01-01T00:00:00.000' AS DateTime), CAST(N'2014-10-28T00:00:00.000' AS DateTime), N'Open', N'None')");
                        createStatement.AppendLine("INSERT[dbo].[CUST_MEMBERSHIP] ([CustomerID], [Plan_Code], [Start_Date], [End_Date], [Status], [Comment]) VALUES(683492, N'LOW', CAST(N'2012-12-12T00:00:00.000' AS DateTime), CAST(N'2020-02-27T00:00:00.000' AS DateTime), N'Active', N'None')");
                        createStatement.AppendLine("INSERT[dbo].[CUSTOMER_OFFER] ([CustomerID], [OfferID]) VALUES(235892, 450)");
                        createStatement.AppendLine("INSERT[dbo].[CUSTOMER_OFFER] ([CustomerID], [OfferID]) VALUES(258279, 450)");
                        createStatement.AppendLine("INSERT[dbo].[CUSTOMER_OFFER] ([CustomerID], [OfferID]) VALUES(321799, 469)");
                        createStatement.AppendLine("INSERT[dbo].[ESTIMATED_WORTH] ([Plan_Code], [Date_effective], [Value_Amount]) VALUES(N'AVG', CAST(N'2016-06-06T00:00:00.000' AS DateTime), CAST(10 AS Numeric(18, 0)))");
                        createStatement.AppendLine("INSERT[dbo].[ESTIMATED_WORTH] ([Plan_Code], [Date_effective], [Value_Amount]) VALUES(N'HIGH', CAST(N'2011-01-01T00:00:00.000' AS DateTime), CAST(1545000 AS Numeric(18, 0)))");
                        createStatement.AppendLine("INSERT[dbo].[ESTIMATED_WORTH] ([Plan_Code], [Date_effective], [Value_Amount]) VALUES(N'LOW', CAST(N'2012-05-04T00:00:00.000' AS DateTime), CAST(450000 AS Numeric(18, 0)))");
                        createStatement.AppendLine("INSERT[dbo].[ESTIMATED_WORTH] ([Plan_Code], [Date_effective], [Value_Amount]) VALUES(N'LOW', CAST(N'2013-06-19T00:00:00.000' AS DateTime), CAST(550000 AS Numeric(18, 0)))");
                        createStatement.AppendLine("INSERT[dbo].[PERSONALISED_COSTING] ([Member], [Segment], [Plan_Code], [Date_effective], [Monthly_Cost]) VALUES(258279, N'LOW', N'HIGH', CAST(N'2014-01-01T00:00:00.000' AS DateTime), CAST(150 AS Numeric(18, 0)))");
                        createStatement.AppendLine("INSERT[dbo].[PERSONALISED_COSTING] ([Member], [Segment], [Plan_Code], [Date_effective], [Monthly_Cost]) VALUES(683492, N'HIGH', N'AVG', CAST(N'2013-01-01T00:00:00.000' AS DateTime), CAST(450 AS Numeric(18, 0)))");
                        createStatement.AppendLine("INSERT[dbo].[PERSONALISED_COSTING] ([Member], [Segment], [Plan_Code], [Date_effective], [Monthly_Cost]) VALUES(885325, N'MED', N'AVG', CAST(N'2013-01-01T00:00:00.000' AS DateTime), CAST(475 AS Numeric(18, 0)))");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();
                    }

                    #endregion

                    #region Staging

                    if (checkBoxCreateSampleStaging.Checked)
                    {
                        // Create sample data
                        StringBuilder createStatement = new StringBuilder();

                        var connString = ConfigurationSettings.ConnectionStringStg;

                        createStatement.AppendLine("IF OBJECT_ID('dbo.STG_PROFILER_CUST_MEMBERSHIP', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_CUST_MEMBERSHIP]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.STG_PROFILER_CUSTOMER_OFFER', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_CUSTOMER_OFFER]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.STG_PROFILER_CUSTOMER_PERSONAL', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_CUSTOMER_PERSONAL]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.STG_PROFILER_ESTIMATED_WORTH', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_ESTIMATED_WORTH]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.STG_PROFILER_OFFER', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_OFFER]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.STG_PROFILER_PERSONALISED_COSTING', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_PERSONALISED_COSTING]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.STG_PROFILER_PLAN', 'U') IS NOT NULL DROP TABLE [dbo].[STG_PROFILER_PLAN]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.STG_USERMANAGED_SEGMENT', 'U') IS NOT NULL DROP TABLE [dbo].[STG_USERMANAGED_SEGMENT]");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("/* Create the tables */");
                        createStatement.AppendLine("CREATE TABLE [STG_PROFILER_CUST_MEMBERSHIP]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameworkIncludeStg);
                        createStatement.AppendLine("  [CustomerID] int NULL,");
                        createStatement.AppendLine("  [Plan_Code] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Start_Date] datetime2(7) NULL,");
                        createStatement.AppendLine("  [End_Date] datetime2(7) NULL,");
                        createStatement.AppendLine("  [Status] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Comment] nvarchar(100) NULL ");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_CUST_MEMBERSHIP',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'CustomerID'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_CUST_MEMBERSHIP',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'Plan_Code'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [STG_PROFILER_CUSTOMER_OFFER]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameworkIncludeStg);
                        createStatement.AppendLine("  [CustomerID] int NULL,");
                        createStatement.AppendLine("  [OfferID] int NULL ");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_CUSTOMER_OFFER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'CustomerID'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_CUSTOMER_OFFER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'OfferID'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [STG_PROFILER_CUSTOMER_PERSONAL]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameworkIncludeStg);
                        createStatement.AppendLine("  [CustomerID] int NULL,");
                        createStatement.AppendLine("  [Given] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Surname] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Suburb] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [State] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Postcode] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Country] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Gender] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [DOB] datetime2(7) NULL,");
                        createStatement.AppendLine("  [Contact_Number] int NULL,");
                        createStatement.AppendLine("  [Referee_Offer_Made] int NULL ");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_CUSTOMER_PERSONAL',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'CustomerID'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [STG_PROFILER_ESTIMATED_WORTH]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameworkIncludeStg);
                        createStatement.AppendLine("  [Plan_Code] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Date_effective] datetime2(7) NULL,");
                        createStatement.AppendLine("  [Value_Amount] numeric(38,20) NULL ");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_ESTIMATED_WORTH',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'Plan_Code'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_ESTIMATED_WORTH',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'Date_effective'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [STG_PROFILER_OFFER]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameworkIncludeStg);
                        createStatement.AppendLine("  [OfferID] int NULL,");
                        createStatement.AppendLine("  [Offer_Long_Description] nvarchar(100)   NULL ");
                        createStatement.AppendLine();
                        createStatement.AppendLine(")");
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_OFFER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'OfferID'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [STG_PROFILER_PERSONALISED_COSTING]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameworkIncludeStg);
                        createStatement.AppendLine("  [Member] int NULL,");
                        createStatement.AppendLine("  [Segment] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Plan_Code] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Date_effective] datetime2(7) NULL,");
                        createStatement.AppendLine("  [Monthly_Cost] numeric(38,20) NULL ");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_PERSONALISED_COSTING',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'Segment'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_PERSONALISED_COSTING',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'Plan_Code'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_PERSONALISED_COSTING',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'Date_effective'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [STG_PROFILER_PLAN]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameworkIncludeStg);
                        createStatement.AppendLine("  [Plan_Code] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Plan_Desc] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Renewal_Plan_Code] nvarchar(100) NULL");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_PROFILER_PLAN',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'Plan_Code'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [STG_USERMANAGED_SEGMENT]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameworkIncludeStg);
                        createStatement.AppendLine("  [Demographic_Segment_Code] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Demographic_Segment_Description] nvarchar(100) NULL ");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Natural_Key', @value = 'Yes',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'STG_USERMANAGED_SEGMENT',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'Demographic_Segment_Description'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();


                        createStatement.AppendLine("/* Create the content (for the User Managed Staging table) */");
                        if (checkBoxDIRECT.Checked)
                        {
                            createStatement.AppendLine("INSERT INTO[dbo].[STG_USERMANAGED_SEGMENT] (");
                            createStatement.AppendLine("  [OMD_INSERT_MODULE_INSTANCE_ID]");
                            createStatement.AppendLine(" ,[OMD_INSERT_DATETIME]");
                            createStatement.AppendLine(" ,[OMD_EVENT_DATETIME]");
                            createStatement.AppendLine(" ,[OMD_RECORD_SOURCE]");
                            createStatement.AppendLine(" ,[OMD_CDC_OPERATION]");
                            createStatement.AppendLine(" ,[OMD_HASH_FULL_RECORD]");
                            createStatement.AppendLine(" ,[Demographic_Segment_Code]");
                            createStatement.AppendLine(" ,[Demographic_Segment_Description])");
                            createStatement.AppendLine("VALUES");
                            createStatement.AppendLine(" ( -1, GETDATE(), GETDATE(), 'Data Warehouse','Insert',0x00, 'LOW', 'Lower SES'),");
                            createStatement.AppendLine(" ( -1,GETDATE(), GETDATE(), 'Data Warehouse','Insert',0x00, 'MED', 'Medium SES'),");
                            createStatement.AppendLine(" ( -1,GETDATE(), GETDATE(), 'Data Warehouse','Insert',0x00, 'HIGH','High SES')");
                        }
                        else
                        {
                            createStatement.AppendLine("INSERT INTO[dbo].[STG_USERMANAGED_SEGMENT] (");
                            createStatement.AppendLine("  [ETL_INSERT_RUN_ID]");
                            createStatement.AppendLine(" ,[LOAD_DATETIME]");
                            createStatement.AppendLine(" ,[EVENT_DATETIME]");
                            createStatement.AppendLine(" ,[RECORD_SOURCE]");
                            createStatement.AppendLine(" ,[CDC_OPERATION]");
                            createStatement.AppendLine(" ,[HASH_FULL_RECORD]");
                            createStatement.AppendLine(" ,[Demographic_Segment_Code]");
                            createStatement.AppendLine(" ,[Demographic_Segment_Description])");
                            createStatement.AppendLine("VALUES");
                            createStatement.AppendLine(" ( -1, GETDATE(), GETDATE(), 'Data Warehouse','Insert', (SELECT HASHBYTES('MD5', ISNULL(RTRIM(CONVERT(NVARCHAR(100),'N/A')),'NA')+'|')), CONVERT(NVARCHAR(100),'LOW'), CONVERT(NVARCHAR(100),'Lower SES')),");
                            createStatement.AppendLine(" ( -1, GETDATE(), GETDATE(), 'Data Warehouse','Insert', (SELECT HASHBYTES('MD5', ISNULL(RTRIM(CONVERT(NVARCHAR(100),'N/A')),'NA')+'|')), CONVERT(NVARCHAR(100),'MED'), CONVERT(NVARCHAR(100),'Medium SES')),");
                            createStatement.AppendLine(" ( -1, GETDATE(), GETDATE(), 'Data Warehouse','Insert', (SELECT HASHBYTES('MD5', ISNULL(RTRIM(CONVERT(NVARCHAR(100),'N/A')),'NA')+'|')), CONVERT(NVARCHAR(100),'HIGH'), CONVERT(NVARCHAR(100),'High SES'))");
                        }

                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();
                    }

                    #endregion

                    #region Persistent Staging

                    if (checkBoxCreateSamplePSA.Checked)
                    {
                        var connString = ConfigurationSettings.ConnectionStringHstg;

                        // Create sample data
                        StringBuilder createStatement = new StringBuilder();

                        createStatement.AppendLine("IF OBJECT_ID('dbo." + psaPrefixName + "_PROFILER_CUST_MEMBERSHIP', 'U') IS NOT NULL DROP TABLE[dbo].[" + psaPrefixName +"_PROFILER_CUST_MEMBERSHIP]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo." + psaPrefixName + "_PROFILER_CUSTOMER_OFFER', 'U') IS NOT NULL DROP TABLE[dbo].[" + psaPrefixName +"_PROFILER_CUSTOMER_OFFER]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo." + psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', 'U') IS NOT NULL DROP TABLE[dbo].[" + psaPrefixName +"_PROFILER_CUSTOMER_PERSONAL]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo." + psaPrefixName + "_PROFILER_ESTIMATED_WORTH', 'U') IS NOT NULL DROP TABLE[dbo].[" + psaPrefixName +"_PROFILER_ESTIMATED_WORTH]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo." + psaPrefixName + "_PROFILER_OFFER', 'U') IS NOT NULL DROP TABLE[dbo].[" + psaPrefixName +"_PROFILER_OFFER]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo." + psaPrefixName + "_PROFILER_PERSONALISED_COSTING', 'U') IS NOT NULL DROP TABLE[dbo].[" + psaPrefixName +"_PROFILER_PERSONALISED_COSTING]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo." + psaPrefixName + "_PROFILER_PLAN', 'U') IS NOT NULL DROP TABLE[dbo].[" + psaPrefixName +"_PROFILER_PLAN]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo." + psaPrefixName + "_USERMANAGED_SEGMENT', 'U') IS NOT NULL DROP TABLE[dbo].[" + psaPrefixName +"_USERMANAGED_SEGMENT]");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [" + psaPrefixName +"_PROFILER_CUST_MEMBERSHIP]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameWorkIncludePsa);
                        createStatement.AppendLine("  [CustomerID] integer NOT NULL,");
                        createStatement.AppendLine("  [Plan_Code] nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  [Start_Date] datetime2(7) NULL,");
                        createStatement.AppendLine("  [End_Date] datetime2(7) NULL,");
                        createStatement.AppendLine("  [Status] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Comment] nvarchar(100) NULL");
                        createStatement.AppendLine("  CONSTRAINT [PK_" + psaPrefixName +"_PROFILER_CUST_MEMBERSHIP] PRIMARY KEY NONCLUSTERED ([CustomerID] ASC, [Plan_Code] ASC, " + etlFrameworkIncludePsaKey+")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [" + psaPrefixName +"_PROFILER_CUSTOMER_OFFER]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameWorkIncludePsa);
                        createStatement.AppendLine("  [CustomerID] integer NOT NULL,");
                        createStatement.AppendLine("  [OfferID] integer NOT NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_" + psaPrefixName +"_PROFILER_CUSTOMER_OFFER] PRIMARY KEY NONCLUSTERED ([CustomerID] ASC, [OfferID] ASC, " + etlFrameworkIncludePsaKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [" + psaPrefixName +"_PROFILER_CUSTOMER_PERSONAL]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameWorkIncludePsa);
                        createStatement.AppendLine("  [CustomerID] integer NOT NULL,");
                        createStatement.AppendLine("  [Given] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Surname] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Suburb] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [State] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Postcode] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Country] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Gender] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [DOB] datetime2(7) NULL,");
                        createStatement.AppendLine("  [Contact_Number] integer NULL,");
                        createStatement.AppendLine("  [Referee_Offer_Made] integer NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_" + psaPrefixName +"_PROFILER_CUSTOMER_PERSONAL] PRIMARY KEY NONCLUSTERED([CustomerID] ASC, " + etlFrameworkIncludePsaKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [" + psaPrefixName +"_PROFILER_ESTIMATED_WORTH]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameWorkIncludePsa);
                        createStatement.AppendLine("  [Plan_Code] nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  [Date_effective] datetime2(7) NOT NULL,");
                        createStatement.AppendLine("  [Value_Amount] numeric(38,20) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_" + psaPrefixName +"_PROFILER_ESTIMATED_WORTH] PRIMARY KEY NONCLUSTERED([Plan_Code] ASC, [Date_effective] ASC, " + etlFrameworkIncludePsaKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [" + psaPrefixName +"_PROFILER_OFFER]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameWorkIncludePsa);
                        createStatement.AppendLine("  [OfferID] integer NOT NULL,");
                        createStatement.AppendLine("  [Offer_Long_Description] nvarchar(100) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_" + psaPrefixName +"_PROFILER_OFFER] PRIMARY KEY NONCLUSTERED([OfferID] ASC, " + etlFrameworkIncludePsaKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [" + psaPrefixName +"_PROFILER_PERSONALISED_COSTING]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameWorkIncludePsa);
                        createStatement.AppendLine("  [Member] integer NOT NULL,");
                        createStatement.AppendLine("  [Segment] nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  [Plan_Code] nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  [Date_effective] datetime2(7) NOT NULL,");
                        createStatement.AppendLine("  [Monthly_Cost] numeric(38,20) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_" + psaPrefixName +"_PROFILER_PERSONALISED_COSTING] PRIMARY KEY NONCLUSTERED([Member] ASC, [Segment] ASC, [Plan_Code] ASC, [Date_effective] ASC, " + etlFrameworkIncludePsaKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [" + psaPrefixName +"_PROFILER_PLAN]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameWorkIncludePsa);
                        createStatement.AppendLine("  [Plan_Code] nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  [Plan_Desc] nvarchar(100) NULL,");
                        createStatement.AppendLine("  [Renewal_Plan_Code] nvarchar(100) NULL");
                        createStatement.AppendLine("  CONSTRAINT [PK_" + psaPrefixName +"_PROFILER_PLAN] PRIMARY KEY NONCLUSTERED([Plan_Code] ASC, " + etlFrameworkIncludePsaKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [" + psaPrefixName +"_USERMANAGED_SEGMENT]");
                        createStatement.AppendLine("(");
                        createStatement.Append(etlFrameWorkIncludePsa);
                        createStatement.AppendLine("  [Demographic_Segment_Code] nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  [Demographic_Segment_Description] nvarchar(100) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_" + psaPrefixName +"_USERMANAGED_SEGMENT] PRIMARY KEY CLUSTERED([Demographic_Segment_Code] ASC, " + etlFrameworkIncludePsaKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();
                    }

                    #endregion

                    #region Integration Layer

                    if (checkBoxCreateSampleDV.Checked)
                    {
                        var connString = ConfigurationSettings.ConnectionStringInt;

                        // Create sample data
                        StringBuilder createStatement = new StringBuilder();

                        createStatement.AppendLine("IF OBJECT_ID('dbo.HUB_CUSTOMER', 'U') IS NOT NULL DROP TABLE dbo.HUB_CUSTOMER");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.HUB_INCENTIVE_OFFER', 'U') IS NOT NULL DROP TABLE dbo.HUB_INCENTIVE_OFFER");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.HUB_MEMBERSHIP_PLAN', 'U') IS NOT NULL DROP TABLE dbo.HUB_MEMBERSHIP_PLAN");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.HUB_SEGMENT', 'U') IS NOT NULL DROP TABLE dbo.HUB_SEGMENT");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.LNK_CUSTOMER_COSTING', 'U') IS NOT NULL DROP TABLE dbo.LNK_CUSTOMER_COSTING");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.LNK_CUSTOMER_OFFER', 'U') IS NOT NULL DROP TABLE dbo.LNK_CUSTOMER_OFFER");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.LNK_MEMBERSHIP', 'U') IS NOT NULL DROP TABLE dbo.LNK_MEMBERSHIP");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.LNK_RENEWAL_MEMBERSHIP', 'U') IS NOT NULL DROP TABLE dbo.LNK_RENEWAL_MEMBERSHIP");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.LSAT_CUSTOMER_COSTING', 'U') IS NOT NULL DROP TABLE dbo.LSAT_CUSTOMER_COSTING");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.LSAT_CUSTOMER_OFFER', 'U') IS NOT NULL DROP TABLE dbo.LSAT_CUSTOMER_OFFER");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.LSAT_MEMBERSHIP', 'U') IS NOT NULL DROP TABLE dbo.LSAT_MEMBERSHIP");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.SAT_CUSTOMER', 'U') IS NOT NULL DROP TABLE dbo.SAT_CUSTOMER");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.SAT_CUSTOMER_ADDITIONAL_DETAILS', 'U') IS NOT NULL DROP TABLE dbo.SAT_CUSTOMER_ADDITIONAL_DETAILS");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.SAT_INCENTIVE_OFFER', 'U') IS NOT NULL DROP TABLE dbo.SAT_INCENTIVE_OFFER");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.SAT_MEMBERSHIP_PLAN_DETAIL', 'U') IS NOT NULL DROP TABLE dbo.SAT_MEMBERSHIP_PLAN_DETAIL");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.SAT_MEMBERSHIP_PLAN_VALUATION', 'U') IS NOT NULL DROP TABLE dbo.SAT_MEMBERSHIP_PLAN_VALUATION");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.SAT_SEGMENT', 'U') IS NOT NULL DROP TABLE dbo.SAT_SEGMENT");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.BR_MEMBERSHIP_OFFER', 'U') IS NOT NULL DROP TABLE dbo.BR_MEMBERSHIP_OFFER");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- HUB CUSTOMER");
                        createStatement.AppendLine("CREATE TABLE dbo.HUB_CUSTOMER");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  CUSTOMER_" + dwhKeyName+ " binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeHubLink);
                        createStatement.AppendLine("  CUSTOMER_ID nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_HUB_CUSTOMER] PRIMARY KEY NONCLUSTERED (CUSTOMER_" + dwhKeyName +" ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE UNIQUE CLUSTERED INDEX IX_HUB_CUSTOMER ON dbo.HUB_CUSTOMER");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  CUSTOMER_ID ASC");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- HUB INCENTIVE OFFER");
                        createStatement.AppendLine("CREATE TABLE dbo.HUB_INCENTIVE_OFFER");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  INCENTIVE_OFFER_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeHubLink);
                        createStatement.AppendLine("  OFFER_ID nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_HUB_INCENTIVE_OFFER] PRIMARY KEY NONCLUSTERED (INCENTIVE_OFFER_" + dwhKeyName +" ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE UNIQUE CLUSTERED INDEX IX_HUB_INCENTIVE_OFFER ON dbo.HUB_INCENTIVE_OFFER");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("    OFFER_ID ASC");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- HUB MEMBERSHIP PLAN");
                        createStatement.AppendLine("CREATE TABLE dbo.HUB_MEMBERSHIP_PLAN");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  MEMBERSHIP_PLAN_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeHubLink);
                        createStatement.AppendLine("  PLAN_CODE nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  PLAN_SUFFIX nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_HUB_MEMBERSHIP_PLAN] PRIMARY KEY NONCLUSTERED (MEMBERSHIP_PLAN_" + dwhKeyName +" ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE UNIQUE CLUSTERED INDEX IX_HUB_MEMBERSHIP_PLAN ON dbo.HUB_MEMBERSHIP_PLAN");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  PLAN_CODE ASC,");
                        createStatement.AppendLine("  PLAN_SUFFIX ASC");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- HUB SEGMENT");
                        createStatement.AppendLine("CREATE TABLE dbo.HUB_SEGMENT");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  SEGMENT_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeHubLink);
                        createStatement.AppendLine("  SEGMENT_CODE nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_HUB_SEGMENT] PRIMARY KEY NONCLUSTERED (SEGMENT_" + dwhKeyName +" ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE UNIQUE CLUSTERED INDEX IX_HUB_SEGMENT ON dbo.HUB_SEGMENT");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  SEGMENT_CODE ASC");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- LNK CUSTOMER COSTING");
                        createStatement.AppendLine("CREATE TABLE dbo.LNK_CUSTOMER_COSTING");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  CUSTOMER_COSTING_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeHubLink);
                        createStatement.AppendLine("  MEMBERSHIP_PLAN_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.AppendLine("  CUSTOMER_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.AppendLine("  SEGMENT_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_LNK_CUSTOMER_COSTING] PRIMARY KEY NONCLUSTERED (CUSTOMER_COSTING_" + dwhKeyName +" ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE UNIQUE CLUSTERED INDEX IX_LNK_CUSTOMER_COSTING ON dbo.LNK_CUSTOMER_COSTING");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  CUSTOMER_" + dwhKeyName +" ASC,");
                        createStatement.AppendLine("  MEMBERSHIP_PLAN_" + dwhKeyName +" ASC,");
                        createStatement.AppendLine("  SEGMENT_" + dwhKeyName +" ASC");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();
                        
                        createStatement.AppendLine("-- LNK CUSTOMER OFFER");
                        createStatement.AppendLine("CREATE TABLE dbo.LNK_CUSTOMER_OFFER");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  CUSTOMER_OFFER_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeHubLink);
                        createStatement.AppendLine("  CUSTOMER_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.AppendLine("  INCENTIVE_OFFER_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_LNK_CUSTOMER_OFFER] PRIMARY KEY NONCLUSTERED(CUSTOMER_OFFER_" + dwhKeyName +" ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE UNIQUE CLUSTERED INDEX IX_LNK_CUSTOMER_OFFER ON dbo.LNK_CUSTOMER_OFFER");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  CUSTOMER_" + dwhKeyName +" ASC,");
                        createStatement.AppendLine("  INCENTIVE_OFFER_" + dwhKeyName +" ASC");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'Driving_Key_Indicator', @value = 'True',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'LNK_CUSTOMER_OFFER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'CUSTOMER_" + dwhKeyName +"'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- LNK MEMBERSHIP");
                        createStatement.AppendLine("CREATE TABLE dbo.LNK_MEMBERSHIP");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  MEMBERSHIP_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeHubLink);
                        createStatement.AppendLine("  CUSTOMER_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.AppendLine("  MEMBERSHIP_PLAN_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.AppendLine("  SALES_CHANNEL nvarchar(100) NOT NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_LNK_MEMBERSHIP] PRIMARY KEY NONCLUSTERED(MEMBERSHIP_" + dwhKeyName +" ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE UNIQUE CLUSTERED INDEX IX_LNK_MEMBERSHIP ON dbo.LNK_MEMBERSHIP");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  CUSTOMER_" + dwhKeyName +" ASC,");
                        createStatement.AppendLine("  MEMBERSHIP_PLAN_" + dwhKeyName +" ASC,");
                        createStatement.AppendLine("  SALES_CHANNEL ASC");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- LNK RENEWAL_MEMBERSHIP");
                        createStatement.AppendLine("CREATE TABLE[dbo].[LNK_RENEWAL_MEMBERSHIP]");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [RENEWAL_MEMBERSHIP_" + dwhKeyName +"][binary](16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeHubLink);
                        createStatement.AppendLine("  [MEMBERSHIP_PLAN_" + dwhKeyName +"] [binary] (16) NOT NULL,");
                        createStatement.AppendLine("  [RENEWAL_PLAN_" + dwhKeyName +"] [binary] (16) NOT NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_LNK_RENEWAL_MEMBERSHIP] PRIMARY KEY NONCLUSTERED ([RENEWAL_MEMBERSHIP_" + dwhKeyName +"] ASC)");
                        createStatement.AppendLine(") ON [PRIMARY]");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE UNIQUE CLUSTERED INDEX IX_LNK_RENEWAL_MEMBERSHIP ON dbo.LNK_RENEWAL_MEMBERSHIP");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [MEMBERSHIP_PLAN_" + dwhKeyName +"] ASC,");
                        createStatement.AppendLine("  [RENEWAL_PLAN_" + dwhKeyName +"] ASC");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- LSAT CUSTOMER COSTING");
                        createStatement.AppendLine("CREATE TABLE dbo.LSAT_CUSTOMER_COSTING");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  CUSTOMER_COSTING_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.AppendLine("  COSTING_EFFECTIVE_DATE datetime2(7) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeSat);
                        createStatement.AppendLine("  PERSONAL_MONTHLY_COST numeric(38,20) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_LSAT_CUSTOMER_COSTING] PRIMARY KEY CLUSTERED (CUSTOMER_COSTING_" + dwhKeyName +" ASC, " + etlFrameworkIncludeSatKey + ", COSTING_EFFECTIVE_DATE ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();
                        
                        createStatement.AppendLine("-- LSAT CUSTOMER OFFER");
                        createStatement.AppendLine("CREATE TABLE dbo.LSAT_CUSTOMER_OFFER");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  CUSTOMER_OFFER_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeSat);
                        createStatement.AppendLine("  CONSTRAINT [PK_LSAT_CUSTOMER_OFFER] PRIMARY KEY CLUSTERED (CUSTOMER_OFFER_" + dwhKeyName +" ASC, " + etlFrameworkIncludeSatKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- LSAT MEMBERSHIP");
                        createStatement.AppendLine("CREATE TABLE dbo.LSAT_MEMBERSHIP");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  MEMBERSHIP_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeSat);
                        createStatement.AppendLine("  MEMBERSHIP_START_DATE datetime2(7) NULL,");
                        createStatement.AppendLine("  MEMBERSHIP_END_DATE datetime2(7) NULL,");
                        createStatement.AppendLine("  MEMBERSHIP_STATUS nvarchar(100) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_LSAT_MEMBERSHIP] PRIMARY KEY CLUSTERED (MEMBERSHIP_" + dwhKeyName +" ASC, " + etlFrameworkIncludeSatKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- SAT CUSTOMER");
                        createStatement.AppendLine("CREATE TABLE dbo.SAT_CUSTOMER");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  CUSTOMER_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeSat);
                        createStatement.AppendLine("  GIVEN_NAME nvarchar(100) NULL,");
                        createStatement.AppendLine("  SURNAME nvarchar(100) NULL,");
                        createStatement.AppendLine("  SUBURB nvarchar(100) NULL,");
                        createStatement.AppendLine("  POSTCODE nvarchar(100) NULL,");
                        createStatement.AppendLine("  COUNTRY nvarchar(100) NULL,");
                        createStatement.AppendLine("  GENDER nvarchar(100) NULL,");
                        createStatement.AppendLine("  DATE_OF_BIRTH datetime2(7) NULL,");
                        createStatement.AppendLine("  REFERRAL_OFFER_MADE_INDICATOR nvarchar(100) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_SAT_CUSTOMER] PRIMARY KEY CLUSTERED (CUSTOMER_" + dwhKeyName +" ASC, " + etlFrameworkIncludeSatKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- SAT CUSTOMER ADDITIONAL DETAILS");
                        createStatement.AppendLine("CREATE TABLE dbo.SAT_CUSTOMER_ADDITIONAL_DETAILS");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  CUSTOMER_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeSat);
                        createStatement.AppendLine("  CONTACT_NUMBER nvarchar(100) NULL,");
                        createStatement.AppendLine("  [STATE] nvarchar(100) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_SAT_CUSTOMER_ADDITIONAL_DETAILS] PRIMARY KEY CLUSTERED (CUSTOMER_" + dwhKeyName +" ASC, " + etlFrameworkIncludeSatKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- SAT INCENTIVE OFFER");
                        createStatement.AppendLine("CREATE TABLE dbo.SAT_INCENTIVE_OFFER");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  INCENTIVE_OFFER_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeSat);
                        createStatement.AppendLine("  OFFER_DESCRIPTION nvarchar(100) NULL,"); 
                        createStatement.AppendLine("  CONSTRAINT [PK_SAT_INCENTIVE_OFFER] PRIMARY KEY CLUSTERED(INCENTIVE_OFFER_" + dwhKeyName +" ASC, " + etlFrameworkIncludeSatKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- SAT MEMBERSHIP PLAN DETAIL");
                        createStatement.AppendLine("CREATE TABLE dbo.SAT_MEMBERSHIP_PLAN_DETAIL");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  MEMBERSHIP_PLAN_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeSat);
                        createStatement.AppendLine("  PLAN_DESCRIPTION nvarchar(100) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_SAT_MEMBERSHIP_PLAN_DETAIL] PRIMARY KEY CLUSTERED(MEMBERSHIP_PLAN_" + dwhKeyName +" ASC, " + etlFrameworkIncludeSatKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- SAT MEMBERSHIP PLAN VALUATION");
                        createStatement.AppendLine("CREATE TABLE dbo.SAT_MEMBERSHIP_PLAN_VALUATION");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  MEMBERSHIP_PLAN_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.AppendLine("  PLAN_VALUATION_DATE datetime2(7) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeSat);
                        createStatement.AppendLine("  PLAN_VALUATION_AMOUNT numeric(38,20) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_SAT_MEMBERSHIP_PLAN_VALUATION] PRIMARY KEY CLUSTERED(MEMBERSHIP_PLAN_" + dwhKeyName +" ASC, " + etlFrameworkIncludeSatKey + ", PLAN_VALUATION_DATE ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- SAT SEGMENT");
                        createStatement.AppendLine("CREATE TABLE dbo.SAT_SEGMENT");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  SEGMENT_" + dwhKeyName +" binary(16) NOT NULL,");
                        createStatement.Append(etlFrameworkIncludeSat);
                        createStatement.AppendLine("  SEGMENT_DESCRIPTION nvarchar(100) NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_SAT_SEGMENT] PRIMARY KEY CLUSTERED (SEGMENT_" + dwhKeyName +" ASC, " + etlFrameworkIncludeSatKey + ")");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("-- BR MEMBERSHIP OFFER");
                        createStatement.AppendLine("CREATE TABLE[dbo].[BR_MEMBERSHIP_OFFER]");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [SNAPSHOT_DATETIME][datetime2](7) NOT NULL,");
                        createStatement.AppendLine("  [CUSTOMER_OFFER_" + dwhKeyName +"] binary(16) NOT NULL,");
                        createStatement.AppendLine("  [MEMBERSHIP_" + dwhKeyName +"] binary(16) NOT NULL,");
                        createStatement.AppendLine("  [CUSTOMER_" + dwhKeyName +"] binary(16) NOT NULL,");
                        createStatement.AppendLine("  [MEMBERSHIP_PLAN_" + dwhKeyName +"] binary(16) NOT NULL,");
                        createStatement.AppendLine("  [SALES_CHANNEL] [nvarchar] (100) NOT NULL,");
                        createStatement.AppendLine("  CONSTRAINT [PK_BR_MEMBERSHIP_OFFER] PRIMARY KEY CLUSTERED([SNAPSHOT_DATETIME] ASC, [CUSTOMER_OFFER_" + dwhKeyName +"] ASC, [MEMBERSHIP_" + dwhKeyName +"] ASC, [CUSTOMER_" + dwhKeyName +"] ASC, [MEMBERSHIP_PLAN_" + dwhKeyName +"] ASC, [SALES_CHANNEL] ASC)");
                        createStatement.AppendLine(") ON [PRIMARY]");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();
                    }

                    #endregion

                    #region Presentation Layer

                    if (checkBoxCreateSamplePresLayer.Checked)
                    {
                        var connString = ConfigurationSettings.ConnectionStringPres;

                        // Create sample data
                        StringBuilder createStatement = new StringBuilder();

                        createStatement.AppendLine("IF OBJECT_ID('dbo.DIM_CUSTOMER', 'U') IS NOT NULL DROP TABLE [DIM_CUSTOMER]");
                        createStatement.AppendLine("IF OBJECT_ID('temp.DIM_CUSTOMER_TMP', 'U') IS NOT NULL DROP TABLE [temp].[DIM_CUSTOMER_TMP]");
                        createStatement.AppendLine("IF OBJECT_ID('dbo.DIM_CUSTOMER_VW', 'V') IS NOT NULL DROP VIEW [dbo].[DIM_CUSTOMER_VW]");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        // Create the schemas
                        createStatement.AppendLine("-- Creating the schema");
                        createStatement.AppendLine("IF EXISTS ( SELECT schema_name FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'temp')");
                        createStatement.AppendLine("DROP SCHEMA [temp]");
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE SCHEMA [temp]");
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        // Create the tables
                        createStatement.AppendLine("/*");
                        createStatement.AppendLine("Create tables");
                        createStatement.AppendLine("*/");
                        createStatement.AppendLine("CREATE TABLE [DIM_CUSTOMER]");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [DIM_CUSTOMER_" + dwhKeyName +"]          binary(16)  NOT NULL,");
                        createStatement.AppendLine("  [ETL_INSERT_RUN_ID]         integer NOT NULL ,");
                        createStatement.AppendLine("  [ETL_UPDATE_RUN_ID] integer NOT NULL ,");
                        createStatement.AppendLine("  [CHECKSUM_TYPE1] varchar(100)  NOT NULL,");
                        createStatement.AppendLine("  [CHECKSUM_TYPE2]            varchar(100)  NOT NULL,");
                        createStatement.AppendLine("  [EFFECTIVE_DATETIME]        datetime2(7)  NOT NULL,");
                        createStatement.AppendLine("  [EXPIRY_DATETIME]           datetime2(7)  NOT NULL,");
                        createStatement.AppendLine("  [CURRENT_RECORD_INDICATOR]  varchar(100)  NOT NULL,");
                        createStatement.AppendLine("  [CUSTOMER_" + dwhKeyName +"]              binary(16) NOT NULL,");
                        createStatement.AppendLine("  [CUSTOMER_ID]               numeric(38,20)  NOT NULL,");
                        createStatement.AppendLine("  [GIVEN_NAME]                varchar(100)  NOT NULL,");
                        createStatement.AppendLine("  [SURNAME]                   varchar(100)  NOT NULL,");
                        createStatement.AppendLine("  [GENDER]                    varchar(100)  NOT NULL,");
                        createStatement.AppendLine("  [SUBURB]                    varchar(100)  NOT NULL,");
                        createStatement.AppendLine("  [POSTCODE]                  varchar(100)  NOT NULL,");
                        createStatement.AppendLine("  [COUNTRY]                   varchar(100)  NOT NULL,");
                        createStatement.AppendLine("  [DATE_OF_BIRTH]             datetime2(7)  NOT NULL,");
                        createStatement.AppendLine("  PRIMARY KEY CLUSTERED([DIM_CUSTOMER_" + dwhKeyName +"] ASC)");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("CREATE TABLE [temp].[DIM_CUSTOMER_TMP]");
                        createStatement.AppendLine("(");
                        createStatement.AppendLine("  [ETL_INSERT_RUN_ID][int] NOT NULL,");
                        createStatement.AppendLine("  [ETL_UPDATE_RUN_ID] [int] NOT NULL,");
                        createStatement.AppendLine("  [EFFECTIVE_DATETIME] [datetime2] (7) NOT NULL,");
                        createStatement.AppendLine("  [EXPIRY_DATETIME] [datetime2] (7) NOT NULL,");
                        createStatement.AppendLine("  [CURRENT_RECORD_INDICATOR] [varchar] (100) NOT NULL,");
                        createStatement.AppendLine("  [CUSTOMER_" + dwhKeyName +"] [char](32) NOT NULL,");
                        createStatement.AppendLine("  [CUSTOMER_ID] [numeric] (38, 20) NOT NULL,");
                        createStatement.AppendLine("  [GIVEN_NAME] [varchar] (100) NOT NULL,");
                        createStatement.AppendLine("  [SURNAME] [varchar] (100) NOT NULL,");
                        createStatement.AppendLine("  [GENDER] [varchar] (100) NOT NULL,");
                        createStatement.AppendLine("  [SUBURB] [varchar] (100) NOT NULL,");
                        createStatement.AppendLine("  [POSTCODE] [varchar] (100) NOT NULL,");
                        createStatement.AppendLine("  [COUNTRY] [varchar] (100) NOT NULL,");
                        createStatement.AppendLine("  [DATE_OF_BIRTH] [datetime2] (7) NOT NULL");
                        createStatement.AppendLine(")");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();


                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'HistoryType', @value = 'Type1',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'GIVEN_NAME'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'HistoryType', @value = 'Type1',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'SURNAME'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'HistoryType', @value = 'Type2',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'SUBURB'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'HistoryType', @value = 'None',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'POSTCODE'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'HistoryType', @value = 'None',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'GENDER'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'HistoryType', @value = 'None',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'DATE_OF_BIRTH'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'HistoryType', @value = 'Type1',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'CUSTOMER_ID'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("EXEC sp_addextendedproperty");
                        createStatement.AppendLine("@name = 'HistoryType', @value = 'None',");
                        createStatement.AppendLine("@level0type = 'SCHEMA', @level0name = 'dbo',");
                        createStatement.AppendLine("@level1type = 'TABLE', @level1name = 'DIM_CUSTOMER',");
                        createStatement.AppendLine("@level2type = 'COLUMN', @level2name = 'DIM_CUSTOMER_" + dwhKeyName +"'");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();

                        createStatement.AppendLine("/*");
                        createStatement.AppendLine("	Create the Views");
                        createStatement.AppendLine("*/");
                        createStatement.AppendLine("CREATE VIEW DIM_CUSTOMER_VW AS");
                        createStatement.AppendLine("SELECT ");
                        createStatement.AppendLine("  -1 AS[ETL_INSERT_RUN_ID],");
                        createStatement.AppendLine("  -1 AS[ETL_UPDATE_RUN_ID],");
                        createStatement.AppendLine("  scu.[LOAD_DATETIME] AS EFFECTIVE_DATETIME,");
                        createStatement.AppendLine("  scu.[LOAD_END_DATETIME] AS EXPIRY_DATETIME,");
                        createStatement.AppendLine("  scu.[CURRENT_RECORD_INDICATOR],");
                        createStatement.AppendLine("  --scu.[DELETED_RECORD_INDICATOR],");
                        createStatement.AppendLine("  hcu.CUSTOMER_" + dwhKeyName +", ");
                        createStatement.AppendLine("  hcu.CUSTOMER_ID,");
                        createStatement.AppendLine("  scu.GIVEN_NAME,");
                        createStatement.AppendLine("  scu.SURNAME,");
                        createStatement.AppendLine("  CASE scu.GENDER");
                        createStatement.AppendLine("    WHEN 'M' THEN 'Male'");
                        createStatement.AppendLine("    WHEN 'F' THEN 'Female'");
                        createStatement.AppendLine("    ELSE 'Unknown'");
                        createStatement.AppendLine("  END AS GENDER,");
                        createStatement.AppendLine("  scu.SUBURB,");
                        createStatement.AppendLine("  scu.POSTCODE,");
                        createStatement.AppendLine("  scu.COUNTRY,");
                        createStatement.AppendLine("  CAST(scu.DATE_OF_BIRTH AS DATE) AS DATE_OF_BIRTH");
                        createStatement.AppendLine("FROM");
                        createStatement.AppendLine("  " + ConfigurationSettings.IntegrationDatabaseName + "." + ConfigurationSettings.SchemaName + ".HUB_CUSTOMER AS hcu INNER JOIN");
                        createStatement.AppendLine("  " + ConfigurationSettings.IntegrationDatabaseName + "." + ConfigurationSettings.SchemaName + ".SAT_CUSTOMER AS scu ON hcu.CUSTOMER_" + dwhKeyName +" = scu.CUSTOMER_" + dwhKeyName +"");
                        createStatement.AppendLine("WHERE");
                        createStatement.AppendLine("  (ISNULL(scu.CURRENT_RECORD_INDICATOR, 'Y') = 'Y') ");
                        createStatement.AppendLine();
                        RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                        createStatement.Clear();
                    }

                    #endregion

                    #region Metadadata

                    if (checkBoxCreateMetadataMapping.Checked)
                    {
                        if (!checkBoxRetainManualMapping.Checked)
                        {
                            // Create sample mapping data
                            var connString = ConfigurationSettings.ConnectionStringOmd;
                            var createStatement = new StringBuilder();

                            createStatement.AppendLine("DELETE FROM [MD_TABLE_MAPPING];");
                            createStatement.AppendLine("DELETE FROM [MD_ATTRIBUTE_MAPPING];");
                            createStatement.AppendLine("DELETE FROM [MD_PHYSICAL_MODEL];");
                            createStatement.AppendLine("TRUNCATE TABLE [MD_VERSION_ATTRIBUTE];");
                            createStatement.AppendLine("TRUNCATE TABLE [MD_VERSION];");
                            createStatement.AppendLine();
                            RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                            createStatement.Clear();

                            // Staging Layer
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUST_MEMBERSHIP', N'COMPOSITE(CustomerID; Plan_Code)', N'"+ psaPrefixName + "_PROFILER_CUST_MEMBERSHIP', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'CUST_MEMBERSHIP', N'COMPOSITE(CustomerID; Plan_Code)', N'STG_PROFILER_CUST_MEMBERSHIP', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUSTOMER_OFFER', N'COMPOSITE(CustomerID; OfferID)', N'"+ psaPrefixName + "_PROFILER_CUSTOMER_OFFER', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'CUSTOMER_OFFER', N'COMPOSITE(CustomerID; OfferID)', N'STG_PROFILER_CUSTOMER_OFFER', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'CustomerID', N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'CUSTOMER_PERSONAL', N'CustomerID', N'STG_PROFILER_CUSTOMER_PERSONAL', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_ESTIMATED_WORTH', N'COMPOSITE(Plan_Code; Date_effective)', N'"+ psaPrefixName + "_PROFILER_ESTIMATED_WORTH', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'ESTIMATED_WORTH', N'COMPOSITE(Plan_Code; Date_effective)', N'STG_PROFILER_ESTIMATED_WORTH', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_OFFER', N'OfferID', N'"+ psaPrefixName + "_PROFILER_OFFER', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'OFFER', N'OfferID', N'STG_PROFILER_OFFER', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PERSONALISED_COSTING', N'COMPOSITE(Member; Segment; Plan_Code; Date_effective)', N'"+ psaPrefixName + "_PROFILER_PERSONALISED_COSTING', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'PERSONALISED_COSTING', N'COMPOSITE(Member; Segment; Plan_Code; Date_effective)', N'STG_PROFILER_PERSONALISED_COSTING', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PLAN', N'Plan_Code', N'"+ psaPrefixName + "_PROFILER_PLAN', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'PLAN', N'Plan_Code', N'STG_PROFILER_PLAN', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_USERMANAGED_SEGMENT', N'Demographic_Segment_Code', N'"+ psaPrefixName + "_USERMANAGED_SEGMENT', NULL, NULL, 'Y');");

                            // Integration Layer
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_ESTIMATED_WORTH', N'COMPOSITE(Plan_Code;''XYZ'')', N'HUB_MEMBERSHIP_PLAN', N'12=12', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUST_MEMBERSHIP', N'COMPOSITE(Plan_Code;''XYZ'')', N'HUB_MEMBERSHIP_PLAN', N'14=14', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_OFFER', N'CustomerID, OfferID', N'LNK_CUSTOMER_OFFER', N'7=7', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_PERSONALISED_COSTING', N'Member', N'HUB_CUSTOMER', N'', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_PLAN', N'COMPOSITE(Plan_Code;''XYZ'')', N'HUB_MEMBERSHIP_PLAN', N'10=10', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_PERSONALISED_COSTING', N'CONCATENATE(Segment;''TEST'')', N'HUB_SEGMENT', N'', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_USERMANAGED_SEGMENT', N'CONCATENATE(Demographic_Segment_Code;''TEST'')', N'SAT_SEGMENT', N'9=9', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_OFFER', N'CustomerID, OfferID', N'LSAT_CUSTOMER_OFFER', N'7=7', 'CustomerID', 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUST_MEMBERSHIP', N'CustomerID', N'HUB_CUSTOMER', N'15=15', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUST_MEMBERSHIP', N'CustomerID, COMPOSITE(Plan_Code;''XYZ'')', N'LNK_MEMBERSHIP', N'16=16', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_PLAN', N'COMPOSITE(Plan_Code;''XYZ'')', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'11=11', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_OFFER', N'CustomerID', N'HUB_CUSTOMER', N'5=5', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_OFFER', N'OfferID', N'HUB_INCENTIVE_OFFER', N'3=3', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_PERSONALISED_COSTING', N'COMPOSITE(Plan_Code;''XYZ'')', N'HUB_MEMBERSHIP_PLAN', N'18=18', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_OFFER', N'OfferID', N'SAT_INCENTIVE_OFFER', N'4=4', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'CustomerID', N'HUB_CUSTOMER', N'1=1', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'CustomerID', N'SAT_CUSTOMER', N'2=2', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'CustomerID', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_USERMANAGED_SEGMENT', N'CONCATENATE(Demographic_Segment_Code;''TEST'')', N'HUB_SEGMENT', N'8=8', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_PERSONALISED_COSTING', N'COMPOSITE(Plan_Code;''XYZ''), Member, CONCATENATE(Segment;''TEST'')', N'LSAT_CUSTOMER_COSTING', N'', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_OFFER', N'OfferID', N'HUB_INCENTIVE_OFFER', N'6=6', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_PERSONALISED_COSTING', N'COMPOSITE(Plan_Code;''XYZ''), Member, CONCATENATE(Segment;''TEST'')', N'LNK_CUSTOMER_COSTING', N'', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_ESTIMATED_WORTH', N'COMPOSITE(Plan_Code;''XYZ'')', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'13=13', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUST_MEMBERSHIP', N'CustomerID, COMPOSITE(Plan_Code;''XYZ'')', N'LSAT_MEMBERSHIP', N'17=17', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_PLAN', N'COMPOSITE(Plan_Code;''XYZ''),COMPOSITE(Renewal_Plan_Code;''XYZ'')', N'LNK_RENEWAL_MEMBERSHIP', N'1=1', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'" + psaPrefixName + "_PROFILER_PLAN', N'COMPOSITE(Renewal_Plan_Code;''XYZ'')', N'HUB_MEMBERSHIP_PLAN', N'1=1', NULL, 'Y');");
                            createStatement.AppendLine();
                            RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                            createStatement.Clear();

                            // Attribute mapping details
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_PERSONALISED_COSTING', N'Monthly_Cost', N'LSAT_CUSTOMER_COSTING', N'PERSONAL_MONTHLY_COST', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_PERSONALISED_COSTING', N'Date_effective', N'LSAT_CUSTOMER_COSTING', N'COSTING_EFFECTIVE_DATE', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUST_MEMBERSHIP', N'Status', N'LNK_MEMBERSHIP', N'SALES_CHANNEL', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUST_MEMBERSHIP', N'End_Date', N'LSAT_MEMBERSHIP', N'MEMBERSHIP_END_DATE', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_ESTIMATED_WORTH', N'Date_effective', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'PLAN_VALUATION_DATE', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_ESTIMATED_WORTH', N'Value_Amount', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'PLAN_VALUATION_AMOUNT', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_PLAN', N'Plan_Desc', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'PLAN_DESCRIPTION', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_USERMANAGED_SEGMENT', N'Demographic_Segment_Description', N'SAT_SEGMENT', N'SEGMENT_DESCRIPTION', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_OFFER', N'Offer_Long_Description', N'SAT_INCENTIVE_OFFER', N'OFFER_DESCRIPTION', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUST_MEMBERSHIP', N'Start_Date', N'LSAT_MEMBERSHIP', N'MEMBERSHIP_START_DATE', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUST_MEMBERSHIP', N'Status', N'LSAT_MEMBERSHIP', N'MEMBERSHIP_STATUS', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'Contact_Number', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'CONTACT_NUMBER', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'State', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'STATE', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'DOB', N'SAT_CUSTOMER', N'DATE_OF_BIRTH', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'Surname', N'SAT_CUSTOMER', N'SURNAME', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'Gender', N'SAT_CUSTOMER', N'GENDER', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'Given', N'SAT_CUSTOMER', N'GIVEN_NAME', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'Country', N'SAT_CUSTOMER', N'COUNTRY', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'Referee_Offer_Made', N'SAT_CUSTOMER', N'REFERRAL_OFFER_MADE_INDICATOR', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'"+ psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'Suburb', N'SAT_CUSTOMER', N'SUBURB', N'');");
                            createStatement.AppendLine("INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'" + psaPrefixName + "_PROFILER_CUSTOMER_PERSONAL', N'Postcode', N'SAT_CUSTOMER', N'POSTCODE', N'');");
                            createStatement.AppendLine();
                            RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                            createStatement.Clear();

                            // Physical  model
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'Comment', N'nvarchar', 100, 0, 13, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'CustomerID', N'int', 4, 10, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'End_Date', N'datetime2', 8, 27, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'Plan_Code', N'nvarchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'Start_Date', N'datetime2', 8, 27, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUST_MEMBERSHIP', N'Status', N'nvarchar', 100, 0, 12, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_OFFER', N'CustomerID', N'int', 4, 10, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_OFFER', N'OfferID', N'int', 4, 10, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'Contact_Number', N'int', 4, 10, 17, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'Country', N'nvarchar', 100, 0, 14, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'CustomerID', N'int', 4, 10, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'DOB', N'datetime2', 8, 27, 16, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'Gender', N'nvarchar', 100, 0, 15, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'Given', N'nvarchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'Postcode', N'nvarchar', 100, 0, 13, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'Referee_Offer_Made', N'int', 4, 10, 18, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'State', N'nvarchar', 100, 0, 12, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'Suburb', N'nvarchar', 100, 0, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_CUSTOMER_PERSONAL', N'Surname', N'nvarchar', 100, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_ESTIMATED_WORTH', N'Date_effective', N'datetime2', 8, 27, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_ESTIMATED_WORTH', N'Plan_Code', N'nvarchar', 100, 0, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_ESTIMATED_WORTH', N'Value_Amount', N'numeric', 17, 38, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_OFFER', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_OFFER', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_OFFER', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_OFFER', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_OFFER', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_OFFER', N'Offer_Long_Description', N'nvarchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_OFFER', N'OfferID', N'int', 4, 10, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_OFFER', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_OFFER', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'Date_effective', N'datetime2', 8, 27, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'Member', N'int', 4, 10, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'Monthly_Cost', N'numeric', 17, 38, 12, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'Plan_Code', N'nvarchar', 100, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'Segment', N'nvarchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PLAN', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PLAN', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PLAN', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PLAN', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PLAN', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PLAN', N'Plan_Code', N'nvarchar', 100, 0, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PLAN', N'Plan_Desc', N'nvarchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PLAN', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PLAN', N'Renewal_Plan_Code', N'nvarchar', 100, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_PROFILER_PLAN', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_USERMANAGED_SEGMENT', N'Demographic_Segment_Code', N'nvarchar', 100, 0, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_USERMANAGED_SEGMENT', N'Demographic_Segment_Description', N'nvarchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'100_Staging_Area', N'dbo', N'STG_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'Comment', N'nvarchar', 100, 0, 13, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'CustomerID', N'int', 4, 10, 8, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'End_Date', N'datetime2', 8, 27, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'Plan_Code', N'nvarchar', 100, 0, 9, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'Start_Date', N'datetime2', 8, 27, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUST_MEMBERSHIP', N'Status', N'nvarchar', 100, 0, 12, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_OFFER', N'CustomerID', N'int', 4, 10, 8, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_OFFER', N'OfferID', N'int', 4, 10, 9, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_OFFER', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'Contact_Number', N'int', 4, 10, 17, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'Country', N'nvarchar', 100, 0, 14, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'CustomerID', N'int', 4, 10, 8, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'DOB', N'datetime2', 8, 27, 16, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'Gender', N'nvarchar', 100, 0, 15, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'Given', N'nvarchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'Postcode', N'nvarchar', 100, 0, 13, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'Referee_Offer_Made', N'int', 4, 10, 18, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'State', N'nvarchar', 100, 0, 12, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'Suburb', N'nvarchar', 100, 0, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_CUSTOMER_PERSONAL', N'Surname', N'nvarchar', 100, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_ESTIMATED_WORTH', N'Date_effective', N'datetime2', 8, 27, 9, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_ESTIMATED_WORTH', N'Plan_Code', N'nvarchar', 100, 0, 8, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_ESTIMATED_WORTH', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_ESTIMATED_WORTH', N'Value_Amount', N'numeric', 17, 38, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_OFFER', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_OFFER', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_OFFER', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_OFFER', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_OFFER', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_OFFER', N'Offer_Long_Description', N'nvarchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_OFFER', N'OfferID', N'int', 4, 10, 8, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_OFFER', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_OFFER', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'Date_effective', N'datetime2', 8, 27, 11, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'Member', N'int', 4, 10, 8, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'Monthly_Cost', N'numeric', 17, 38, 12, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'Plan_Code', N'nvarchar', 100, 0, 10, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'Segment', N'nvarchar', 100, 0, 9, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PERSONALISED_COSTING', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PLAN', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PLAN', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PLAN', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PLAN', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PLAN', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PLAN', N'Plan_Code', N'nvarchar', 100, 0, 8, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PLAN', N'Plan_Desc', N'nvarchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PLAN', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PLAN', N'Renewal_Plan_Code', N'nvarchar', 100, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_PROFILER_PLAN', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_USERMANAGED_SEGMENT', N'Demographic_Segment_Code', N'nvarchar', 100, 0, 8, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_USERMANAGED_SEGMENT', N'Demographic_Segment_Description', N'nvarchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 1, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.EventDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'150_Persistent_Staging_Area', N'dbo', N'PSA_USERMANAGED_SEGMENT', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 5, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_CUSTOMER', N'CUSTOMER_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_CUSTOMER', N'CUSTOMER_ID', N'nvarchar', 100, 0, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_CUSTOMER', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_CUSTOMER', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_CUSTOMER', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_INCENTIVE_OFFER', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_INCENTIVE_OFFER', N'INCENTIVE_OFFER_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_INCENTIVE_OFFER', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_INCENTIVE_OFFER', N'OFFER_ID', N'nvarchar', 100, 0, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_INCENTIVE_OFFER', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_MEMBERSHIP_PLAN', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_MEMBERSHIP_PLAN', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_MEMBERSHIP_PLAN', N'MEMBERSHIP_PLAN_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_MEMBERSHIP_PLAN', N'PLAN_CODE', N'nvarchar', 100, 0, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_MEMBERSHIP_PLAN', N'PLAN_SUFFIX', N'nvarchar', 100, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_MEMBERSHIP_PLAN', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_SEGMENT', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_SEGMENT', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_SEGMENT', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_SEGMENT', N'SEGMENT_CODE', N'nvarchar', 100, 0, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'HUB_SEGMENT', N'SEGMENT_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_COSTING', N'CUSTOMER_COSTING_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_COSTING', N'CUSTOMER_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_COSTING', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_COSTING', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_COSTING', N'MEMBERSHIP_PLAN_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_COSTING', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_COSTING', N'SEGMENT_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_OFFER', N'CUSTOMER_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_OFFER', N'CUSTOMER_OFFER_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_OFFER', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_OFFER', N'INCENTIVE_OFFER_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_OFFER', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_CUSTOMER_OFFER', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_MEMBERSHIP', N'CUSTOMER_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_MEMBERSHIP', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_MEMBERSHIP', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_MEMBERSHIP', N'MEMBERSHIP_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_MEMBERSHIP', N'MEMBERSHIP_PLAN_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_MEMBERSHIP', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_MEMBERSHIP', N'SALES_CHANNEL', N'nvarchar', 100, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_RENEWAL_MEMBERSHIP', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 2, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_RENEWAL_MEMBERSHIP', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_RENEWAL_MEMBERSHIP', N'MEMBERSHIP_PLAN_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_RENEWAL_MEMBERSHIP', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_RENEWAL_MEMBERSHIP', N'RENEWAL_MEMBERSHIP_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LNK_RENEWAL_MEMBERSHIP', N'RENEWAL_PLAN_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'COSTING_EFFECTIVE_DATE', N'datetime2', 8, 27, 2, N'Y' ,N'Y')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'"+ ConfigurationSettings.CurrentRowAttribute + "', N'varchar', 100, 0, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'CUSTOMER_COSTING_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'"+ ConfigurationSettings.EtlProcessUpdateAttribute + "', N'int', 4, 10, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'"+ ConfigurationSettings.ExpiryDateTimeAttribute + "', N'datetime2', 8, 27, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'PERSONAL_MONTHLY_COST', N'numeric', 17, 38, 12, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_COSTING', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_OFFER', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_OFFER', N'"+ ConfigurationSettings.CurrentRowAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_OFFER', N'CUSTOMER_OFFER_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_OFFER', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_OFFER', N'"+ ConfigurationSettings.EtlProcessUpdateAttribute + "', N'int', 4, 10, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_OFFER', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_OFFER', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_OFFER', N'"+ ConfigurationSettings.ExpiryDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_OFFER', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_CUSTOMER_OFFER', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'"+ ConfigurationSettings.CurrentRowAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'"+ ConfigurationSettings.EtlProcessUpdateAttribute + "', N'int', 4, 10, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'"+ ConfigurationSettings.ExpiryDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'MEMBERSHIP_END_DATE', N'datetime2', 8, 27, 12, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'MEMBERSHIP_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'MEMBERSHIP_START_DATE', N'datetime2', 8, 27, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'MEMBERSHIP_STATUS', N'nvarchar', 100, 0, 13, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'LSAT_MEMBERSHIP', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'COUNTRY', N'nvarchar', 100, 0, 15, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'"+ ConfigurationSettings.CurrentRowAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'CUSTOMER_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'DATE_OF_BIRTH', N'datetime2', 8, 27, 17, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'"+ ConfigurationSettings.EtlProcessUpdateAttribute + "', N'int', 4, 10, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'GENDER', N'nvarchar', 100, 0, 16, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'GIVEN_NAME', N'nvarchar', 100, 0, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'"+ ConfigurationSettings.ExpiryDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'POSTCODE', N'nvarchar', 100, 0, 14, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'REFERRAL_OFFER_MADE_INDICATOR', N'nvarchar', 100, 0, 18, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'SUBURB', N'nvarchar', 100, 0, 13, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER', N'SURNAME', N'nvarchar', 100, 0, 12, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'CONTACT_NUMBER', N'nvarchar', 100, 0, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'"+ ConfigurationSettings.CurrentRowAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'CUSTOMER_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'"+ ConfigurationSettings.EtlProcessUpdateAttribute + "', N'int', 4, 10, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'"+ ConfigurationSettings.ExpiryDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'STATE', N'nvarchar', 100, 0, 12, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_INCENTIVE_OFFER', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_INCENTIVE_OFFER', N'"+ ConfigurationSettings.CurrentRowAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_INCENTIVE_OFFER', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_INCENTIVE_OFFER', N'"+ ConfigurationSettings.EtlProcessUpdateAttribute + "', N'int', 4, 10, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_INCENTIVE_OFFER', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_INCENTIVE_OFFER', N'INCENTIVE_OFFER_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_INCENTIVE_OFFER', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_INCENTIVE_OFFER', N'"+ ConfigurationSettings.ExpiryDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_INCENTIVE_OFFER', N'OFFER_DESCRIPTION', N'nvarchar', 100, 0, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_INCENTIVE_OFFER', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_INCENTIVE_OFFER', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'"+ ConfigurationSettings.CurrentRowAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'"+ ConfigurationSettings.EtlProcessUpdateAttribute + "', N'int', 4, 10, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'"+ ConfigurationSettings.ExpiryDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'MEMBERSHIP_PLAN_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'PLAN_DESCRIPTION', N'nvarchar', 100, 0, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 8, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'"+ ConfigurationSettings.CurrentRowAttribute + "', N'varchar', 100, 0, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'"+ ConfigurationSettings.EtlProcessUpdateAttribute + "', N'int', 4, 10, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'"+ ConfigurationSettings.ExpiryDateTimeAttribute + "', N'datetime2', 8, 27, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'MEMBERSHIP_PLAN_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'PLAN_VALUATION_AMOUNT', N'numeric', 17, 38, 12, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'PLAN_VALUATION_DATE', N'datetime2', 8, 27, 2, N'Y' ,N'Y')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_SEGMENT', N'"+ ConfigurationSettings.ChangeDataCaptureAttribute + "', N'varchar', 100, 0, 7, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_SEGMENT', N'"+ ConfigurationSettings.CurrentRowAttribute + "', N'varchar', 100, 0, 4, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_SEGMENT', N'"+ ConfigurationSettings.EtlProcessAttribute + "', N'int', 4, 10, 5, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_SEGMENT', N'"+ ConfigurationSettings.EtlProcessUpdateAttribute + "', N'int', 4, 10, 6, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_SEGMENT', N'"+ ConfigurationSettings.RecordChecksumAttribute + "', N'binary', 16, 0, 10, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_SEGMENT', N'"+ ConfigurationSettings.LoadDateTimeAttribute + "', N'datetime2', 8, 27, 2, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_SEGMENT', N'"+ ConfigurationSettings.ExpiryDateTimeAttribute + "', N'datetime2', 8, 27, 3, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_SEGMENT', N'"+ ConfigurationSettings.RecordSourceAttribute + "', N'varchar', 100, 0, 9, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_SEGMENT', N'SEGMENT_DESCRIPTION', N'nvarchar', 100, 0, 11, N'N' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_SEGMENT', N'SEGMENT_"+ ConfigurationSettings.DwhKeyIdentifier + "', N'binary', 16, 0, 1, N'Y' ,N'N')");
                            createStatement.AppendLine("INSERT[dbo].[MD_VERSION_ATTRIBUTE] ([VERSION_ID], [DATABASE_NAME], [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [DATA_TYPE], [CHARACTER_MAXIMUM_LENGTH], [NUMERIC_PRECISION], [ORDINAL_POSITION], [PRIMARY_KEY_INDICATOR], [MULTI_ACTIVE_INDICATOR]) VALUES(0, N'200_Integration_Layer', N'dbo', N'SAT_SEGMENT', N'"+ ConfigurationSettings.RowIdAttribute + "', N'int', 4, 10, 8, N'N' ,N'N')");
                            RunSqlCommandSampleDataForm(connString, createStatement, worker, 6);
                            createStatement.Clear();

                        }
                        else
                        {
                            _alertSampleData.SetTextLogging("The option to retain the mapping metadata is checked, so new mapping metadata has not been added.");
                        }
                    }

                    #endregion

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