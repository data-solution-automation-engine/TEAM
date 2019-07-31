using System;
using System.ComponentModel;
using System.Data.SqlClient;
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

  



        private void RunSqlCommandRepositoryForm(string connString, StringBuilder createStatement, BackgroundWorker worker, int progressCounter)
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
                }
            }

            createStatement.Clear();
        }


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
                    _alertSampleData.SetTextLogging("This occurred with the following query: " + createStatement +"\r\n\r\n");
                    ErrorHandlingParameters.ErrorCatcher++;
                }
            }

            createStatement.Clear();
        }

        private void buttonTruncate_Click(object sender, EventArgs e)
        {
            // Retrieving the required parameters

            // Truncating the entire repository
            StringBuilder commandText = new StringBuilder();

            commandText.AppendLine("DELETE FROM [MD_SOURCE_LINK_ATTRIBUTE_XREF];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF];");
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
            commandText.AppendLine("DELETE FROM [MD_SOURCE_STAGING_XREF];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_STAGING_ATTRIBUTE_XREF];");
            commandText.AppendLine("DELETE FROM [MD_PERSISTENT_STAGING];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_PERSISTENT_STAGING_XREF];");
            commandText.AppendLine("DELETE FROM [MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF];");
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

                var createStatement = new StringBuilder();

                // Drop any existing Foreign Key Constraints
                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_BUSINESS_KEY_COMPONENT_MD_SOURCE_HUB_XREF]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_BUSINESS_KEY_COMPONENT] DROP CONSTRAINT [FK_MD_BUSINESS_KEY_COMPONENT_MD_SOURCE_HUB_XREF]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 0);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_BUSINESS_KEY_COMPONENT_PART_MD_ATTRIBUTE]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_BUSINESS_KEY_COMPONENT_PART] DROP CONSTRAINT [FK_MD_BUSINESS_KEY_COMPONENT_PART_MD_ATTRIBUTE]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 1);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_BUSINESS_KEY_COMPONENT_PART_MD_BUSINESS_KEY_COMPONENT]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_BUSINESS_KEY_COMPONENT_PART] DROP CONSTRAINT [FK_MD_BUSINESS_KEY_COMPONENT_PART_MD_BUSINESS_KEY_COMPONENT]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 2);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_DRIVING_KEY_XREF_MD_HUB]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_DRIVING_KEY_XREF] DROP CONSTRAINT [FK_MD_DRIVING_KEY_XREF_MD_HUB]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_DRIVING_KEY_XREF_MD_SATELLITE]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_DRIVING_KEY_XREF] DROP CONSTRAINT [FK_MD_DRIVING_KEY_XREF_MD_SATELLITE]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_HUB_LINK_XREF_MD_HUB]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_HUB_LINK_XREF] DROP CONSTRAINT [FK_MD_HUB_LINK_XREF_MD_HUB]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_HUB_LINK_XREF_MD_HUB]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_HUB_LINK_XREF] DROP CONSTRAINT [FK_MD_HUB_LINK_XREF_MD_LINK]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_HUB_LINK_XREF_MD_HUB]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_HUB_LINK_XREF] DROP CONSTRAINT [FK_MD_HUB_LINK_XREF_MD_LINK]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SATELLITE_MD_HUB]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_SATELLITE] DROP CONSTRAINT [FK_MD_SATELLITE_MD_HUB]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SATELLITE_MD_LINK]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_SATELLITE] DROP CONSTRAINT [FK_MD_SATELLITE_MD_LINK]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_HUB_XREF_MD_HUB]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_HUB_XREF] DROP CONSTRAINT [FK_MD_SOURCE_HUB_XREF_MD_HUB]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_HUB_XREF_MD_SOURCE]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_HUB_XREF] DROP CONSTRAINT [FK_MD_SOURCE_HUB_XREF_MD_SOURCE]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_ATTRIBUTE_FROM]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_LINK_ATTRIBUTE_XREF] DROP CONSTRAINT [FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_ATTRIBUTE_FROM]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_ATTRIBUTE_TO]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_LINK_ATTRIBUTE_XREF] DROP CONSTRAINT [FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_ATTRIBUTE_TO]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_SOURCE]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_LINK_ATTRIBUTE_XREF] DROP CONSTRAINT [FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_SOURCE]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_LINK]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_LINK_ATTRIBUTE_XREF] DROP CONSTRAINT [FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_LINK]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine(
                    "IF OBJECT_ID('[FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_ATTRIBUTERIBUTE_FROM]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF] DROP CONSTRAINT [FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_ATTRIBUTERIBUTE_FROM]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_ATTRIBUTERIBUTE_TO]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF] DROP CONSTRAINT [FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_ATTRIBUTERIBUTE_TO]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_SATELLITE]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF] DROP CONSTRAINT [FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_SATELLITE]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_SOURCE]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF] DROP CONSTRAINT [FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_SOURCE]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_SATELLITE_XREF_MD_SATELLITE]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_SATELLITE_XREF] DROP CONSTRAINT [FK_MD_SOURCE_SATELLITE_XREF_MD_SATELLITE]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_SATELLITE_XREF_MD_SOURCE]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_SATELLITE_XREF] DROP CONSTRAINT [FK_MD_SOURCE_SATELLITE_XREF_MD_SOURCE]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_SATELLITE_XREF_MD_SOURCE]', 'F') IS NOT NULL");
                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_SATELLITE_XREF] DROP CONSTRAINT [FK_MD_SOURCE_SATELLITE_XREF_MD_SOURCE]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_LINK_XREF_MD_LINK]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_LINK_XREF] DROP CONSTRAINT [FK_MD_SOURCE_LINK_XREF_MD_LINK]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_LINK_XREF_MD_SOURCE]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_LINK_XREF] DROP CONSTRAINT [FK_MD_SOURCE_LINK_XREF_MD_SOURCE]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 3);
                createStatement.Clear();

                //createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_STAGING_XREF_MD_SOURCE_DATASET]', 'F') IS NOT NULL");
                //createStatement.AppendLine("ALTER TABLE [MD_SOURCE_STAGING_XREF] DROP CONSTRAINT [FK_MD_SOURCE_STAGING_XREF_MD_SOURCE_DATASET]");
                //createStatement.AppendLine();
                //RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 4);
                //createStatement.Clear();

                createStatement.AppendLine("IF OBJECT_ID('[FK_MD_SOURCE_DATASET_MD_SOURCE_SYSTEM]', 'F') IS NOT NULL");
                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_DATASET] DROP CONSTRAINT [FK_MD_SOURCE_DATASET_MD_SOURCE_SYSTEM]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 4);
                createStatement.Clear();

                // Metadata, well, metadata
                createStatement.AppendLine();
                createStatement.AppendLine("--Model metadata");
                createStatement.AppendLine("IF OBJECT_ID('[MD_MODEL_METADATA]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_MODEL_METADATA]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_MODEL_METADATA]");
                createStatement.AppendLine("(");
                createStatement.AppendLine("    [VERSION_NAME]       varchar(100)  NOT NULL ,");
                createStatement.AppendLine("    [ACTIVATION_DATETIME]     datetime2(7) NOT NULL,");
                createStatement.AppendLine("    CONSTRAINT[PK_MD_MODEL_METADATA] PRIMARY KEY CLUSTERED ( [VERSION_NAME] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 5);
                createStatement.Clear();

                // Repository version
                createStatement.AppendLine();
                createStatement.AppendLine("--Repository version");
                createStatement.AppendLine("IF OBJECT_ID('[MD_REPOSITORY_VERSION]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_REPOSITORY_VERSION]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_REPOSITORY_VERSION]");
                createStatement.AppendLine("(");
                createStatement.AppendLine("    [REPOSITORY_VERSION]       varchar(100)  NOT NULL ,");
                createStatement.AppendLine("    [REPOSITORY_VERSION_NOTES]       varchar(4000)  NOT NULL ,");
                createStatement.AppendLine("    [REPOSITORY_CREATION_DATETIME]     datetime2(7) NOT NULL,");
                createStatement.AppendLine("    [REPOSITORY_UPDATE_DATETIME]     datetime2(7) NOT NULL,");
                createStatement.AppendLine("    CONSTRAINT[PK_MD_REPOSITORY_VERSION] PRIMARY KEY CLUSTERED ( [REPOSITORY_VERSION] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 5);
                createStatement.Clear();

                createStatement.AppendLine();
                createStatement.AppendLine("--Repository version insert");
                createStatement.AppendLine("INSERT INTO [MD_REPOSITORY_VERSION]");
                createStatement.AppendLine("(");
                createStatement.AppendLine("    [REPOSITORY_VERSION],");
                createStatement.AppendLine("    [REPOSITORY_VERSION_NOTES],");
                createStatement.AppendLine("    [REPOSITORY_CREATION_DATETIME],");
                createStatement.AppendLine("    [REPOSITORY_UPDATE_DATETIME]");
                createStatement.AppendLine(")");
                createStatement.AppendLine("VALUES");
                createStatement.AppendLine("(");
                createStatement.AppendLine("'1.6',");
                createStatement.AppendLine("'Changed STG to SOURCE and removed the TABLE part in attribute names.',");
                createStatement.AppendLine("SYSDATETIME(),");
                createStatement.AppendLine("SYSDATETIME()");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 5);
                createStatement.Clear();

                // Attribute 
                createStatement.AppendLine();
                createStatement.AppendLine("--Attribute");
                createStatement.AppendLine("IF OBJECT_ID('[MD_ATTRIBUTE]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_ATTRIBUTE]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_ATTRIBUTE]");
                createStatement.AppendLine("(");
                createStatement.AppendLine("    [ATTRIBUTE_ID]       integer NOT NULL ,");
                createStatement.AppendLine("    [ATTRIBUTE_NAME]     varchar(100) NOT NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_ATTRIBUTE] PRIMARY KEY CLUSTERED ( [ATTRIBUTE_ID] ASC)");
                createStatement.AppendLine(")");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE UNIQUE NONCLUSTERED INDEX [IX_MD_ATTRIBUTE] ON [MD_ATTRIBUTE]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [ATTRIBUTE_NAME] ASC");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 5);
                createStatement.Clear();

                if (!checkBoxRetainManualMapping.Checked)
                {
                    // Attribute Mapping 
                    createStatement.AppendLine();
                    createStatement.AppendLine("-- Attribute mapping");
                    createStatement.AppendLine("IF OBJECT_ID('[MD_ATTRIBUTE_MAPPING]', 'U') IS NOT NULL");
                    createStatement.AppendLine(" DROP TABLE [MD_ATTRIBUTE_MAPPING]");
                    createStatement.AppendLine("");
                    createStatement.AppendLine("CREATE TABLE [MD_ATTRIBUTE_MAPPING]");
                    createStatement.AppendLine("( ");
                    createStatement.AppendLine("    [ATTRIBUTE_MAPPING_HASH] AS(");
                    createStatement.AppendLine("                CONVERT([CHAR](32),HASHBYTES('MD5',");
                    createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[TARGET_TABLE])),'NA')+'|'+");
                    createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[TARGET_COLUMN])),'NA')+'|'+");
                    createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[SOURCE_TABLE])),'NA')+'|'+");
                    createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[SOURCE_COLUMN])),'NA')+'|' +");
                    createStatement.AppendLine("                ISNULL(RTRIM(CONVERT(VARCHAR(100),[TRANSFORMATION_RULE])),'NA')+'|'");
                    createStatement.AppendLine("			),(2)");
                    createStatement.AppendLine("			)");
                    createStatement.AppendLine("		) PERSISTED NOT NULL,");
                    createStatement.AppendLine("	[VERSION_ID]          integer NOT NULL,");
                    createStatement.AppendLine("	[SOURCE_TABLE]        varchar(100)  NULL,");
                    createStatement.AppendLine("	[SOURCE_COLUMN]       varchar(100)  NULL,");
                    createStatement.AppendLine("	[TARGET_TABLE]        varchar(100)  NULL,");
                    createStatement.AppendLine("	[TARGET_COLUMN]       varchar(100)  NULL,");
                    createStatement.AppendLine("	[TRANSFORMATION_RULE] varchar(4000)  NULL,");
                    createStatement.AppendLine(
                        "    CONSTRAINT[PK_MD_ATTRIBUTE_MAPPING] PRIMARY KEY CLUSTERED ([ATTRIBUTE_MAPPING_HASH] ASC, [VERSION_ID] ASC)");
                    createStatement.AppendLine(")");

                    RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 10);
                    createStatement.Clear();
                }

                // Business Key Component
                createStatement.AppendLine();
                createStatement.AppendLine("-- Business Key Component");
                createStatement.AppendLine("IF OBJECT_ID('[MD_BUSINESS_KEY_COMPONENT]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_BUSINESS_KEY_COMPONENT]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_BUSINESS_KEY_COMPONENT]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [SOURCE_ID]        integer NOT NULL,");
                createStatement.AppendLine("	[HUB_ID]      integer NOT NULL,");
                createStatement.AppendLine("	[BUSINESS_KEY_DEFINITION] [varchar](4000) NOT NULL,");
                createStatement.AppendLine("	[COMPONENT_ID]       integer NOT NULL,");
                createStatement.AppendLine("	[COMPONENT_ORDER]       integer NOT NULL,");
                createStatement.AppendLine("	[COMPONENT_VALUE]    varchar(100)  NOT NULL,");
                createStatement.AppendLine("    [COMPONENT_TYPE]     varchar(100)  NOT NULL,");
                createStatement.AppendLine(
                    "    CONSTRAINT[PK_MD_BUSINESS_KEY_COMPONENT] PRIMARY KEY CLUSTERED ([SOURCE_ID] ASC, [HUB_ID] ASC, [BUSINESS_KEY_DEFINITION] ASC, [COMPONENT_ID] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 15);
                createStatement.Clear();

                // Business Key Component Part
                createStatement.AppendLine();
                createStatement.AppendLine("-- Business Key Component Part");
                createStatement.AppendLine("IF OBJECT_ID('[MD_BUSINESS_KEY_COMPONENT_PART]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_BUSINESS_KEY_COMPONENT_PART]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_BUSINESS_KEY_COMPONENT_PART]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("");
                createStatement.AppendLine("    [SOURCE_ID]  integer NOT NULL,");
                createStatement.AppendLine("	[HUB_ID]       integer NOT NULL,");
                createStatement.AppendLine("	[COMPONENT_ID]      integer NOT NULL,");
                createStatement.AppendLine("	[BUSINESS_KEY_DEFINITION] [varchar](4000) NOT NULL, ");
                createStatement.AppendLine("	[COMPONENT_ELEMENT_ID]     integer NOT NULL,");
                createStatement.AppendLine("	[COMPONENT_ELEMENT_ORDER]      integer NULL,");
                createStatement.AppendLine("    [COMPONENT_ELEMENT_VALUE] varchar(1000)  NULL,");
                createStatement.AppendLine("	[COMPONENT_ELEMENT_TYPE] varchar(100)  NOT NULL,");
                createStatement.AppendLine("    [ATTRIBUTE_ID]       integer NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_BUSINESS_KEY_COMPONENT_PART] PRIMARY KEY CLUSTERED ([SOURCE_ID] ASC, [HUB_ID] ASC, [BUSINESS_KEY_DEFINITION] ASC, [COMPONENT_ID] ASC, [COMPONENT_ELEMENT_ID] ASC)");
                createStatement.AppendLine(")");

                try
                {
                    RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 20);
                }
                catch (Exception ex)
                {
                    _alertRepository.SetTextLogging("An issue has occured creating the Business Key Component Part. The full error message is: " + ex);
                }

                createStatement.Clear();

                // Driving Key Xref
                createStatement.AppendLine();
                createStatement.AppendLine("-- Driving Key Xref");
                createStatement.AppendLine("IF OBJECT_ID ('[MD_DRIVING_KEY_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_DRIVING_KEY_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_DRIVING_KEY_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [SATELLITE_ID]  integer NOT NULL,");
                createStatement.AppendLine("	[HUB_ID]       integer NOT NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_DRIVING_KEY_XREF] PRIMARY KEY CLUSTERED ([SATELLITE_ID] ASC, [HUB_ID] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 25);
                createStatement.Clear();

                // Staging
                createStatement.AppendLine();
                createStatement.AppendLine("-- Staging");
                createStatement.AppendLine("IF OBJECT_ID ('[MD_STAGING]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_STAGING]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_STAGING]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("	[STAGING_NAME] varchar(100) NOT NULL,");
                createStatement.AppendLine("	[SCHEMA_NAME]  varchar(100)  NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_STAGING] PRIMARY KEY CLUSTERED ([STAGING_NAME] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 27);
                createStatement.Clear();

                // Source-Staging XREF
                createStatement.AppendLine();
                createStatement.AppendLine("-- Source Staging XREF");
                createStatement.AppendLine("IF OBJECT_ID ('[MD_SOURCE_STAGING_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_SOURCE_STAGING_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_SOURCE_STAGING_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("	[SOURCE_NAME] varchar(100) NOT NULL,");
                createStatement.AppendLine("	[STAGING_NAME] varchar(100) NOT NULL,");
                createStatement.AppendLine("	[CHANGE_DATETIME_DEFINITION]  varchar(4000) NULL,");
                createStatement.AppendLine("	[CHANGE_DATA_CAPTURE_DEFINITION]  varchar(100) NULL,");
                createStatement.AppendLine("	[KEY_DEFINITION]  varchar(4000) NULL,");
                createStatement.AppendLine("	[FILTER_CRITERIA]  varchar(4000) NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_SOURCE_STAGING_XREF] PRIMARY KEY CLUSTERED ([SOURCE_NAME],[STAGING_NAME] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 27);
                createStatement.Clear();

                // Persistent Staging
                createStatement.AppendLine();
                createStatement.AppendLine("-- Persistent Staging");
                createStatement.AppendLine("IF OBJECT_ID ('[MD_PERSISTENT_STAGING]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_PERSISTENT_STAGING]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_PERSISTENT_STAGING]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("	[PERSISTENT_STAGING_NAME] varchar(100) NOT NULL,");
                createStatement.AppendLine("	[SCHEMA_NAME]  varchar(100)  NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_PERSISTENT_STAGING] PRIMARY KEY CLUSTERED ([PERSISTENT_STAGING_NAME] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 28);
                createStatement.Clear();

                // Source-Persistent Staging XREF
                createStatement.AppendLine();
                createStatement.AppendLine("-- Source Persistent Staging XREF");
                createStatement.AppendLine("IF OBJECT_ID ('[MD_SOURCE_PERSISTENT_STAGING_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_SOURCE_PERSISTENT_STAGING_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_SOURCE_PERSISTENT_STAGING_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("	[SOURCE_NAME] varchar(100) NOT NULL,");
                createStatement.AppendLine("	[PERSISTENT_STAGING_NAME] varchar(100) NOT NULL,");
                createStatement.AppendLine("	[CHANGE_DATETIME_DEFINITION]  varchar(4000) NULL,");
                createStatement.AppendLine("	[KEY_DEFINITION]  varchar(4000) NULL,");
                createStatement.AppendLine("	[FILTER_CRITERIA]  varchar(4000) NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_SOURCE_PERSISTENT_STAGING_XREF] PRIMARY KEY CLUSTERED ([SOURCE_NAME],[PERSISTENT_STAGING_NAME] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 27);
                createStatement.Clear();

                // Source Staging Attribute XREF
                createStatement.AppendLine();
                createStatement.AppendLine("-- Source Staging Attribute XREF");
                createStatement.AppendLine("IF OBJECT_ID ('[MD_SOURCE_STAGING_ATTRIBUTE_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_SOURCE_STAGING_ATTRIBUTE_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_SOURCE_STAGING_ATTRIBUTE_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("	[SOURCE_NAME] varchar(100) NOT NULL,");
                createStatement.AppendLine("	[STAGING_NAME] varchar(100) NOT NULL,");
                createStatement.AppendLine("	[ATTRIBUTE_NAME_FROM]  varchar(100) NOT NULL,");
                createStatement.AppendLine("	[ATTRIBUTE_NAME_TO]  varchar(100) NOT NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_SOURCE_STAGING_ATTRIBUTE_XREF] PRIMARY KEY CLUSTERED ([SOURCE_NAME],[STAGING_NAME], [ATTRIBUTE_NAME_FROM] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 27);
                createStatement.Clear();

                // Source Persistent Staging Attribute XREF
                createStatement.AppendLine();
                createStatement.AppendLine("-- Source Persistent Staging Attribute XREF");
                createStatement.AppendLine("IF OBJECT_ID ('[MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("	[SOURCE_NAME] varchar(100) NOT NULL,");
                createStatement.AppendLine("	[PERSISTENT_STAGING_NAME] varchar(100) NOT NULL,");
                createStatement.AppendLine("	[ATTRIBUTE_NAME_FROM]  varchar(100) NOT NULL,");
                createStatement.AppendLine("	[ATTRIBUTE_NAME_TO]  varchar(100) NOT NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_SOURCE_STAGING_PERSISTENT_ATTRIBUTE_XREF] PRIMARY KEY CLUSTERED ([SOURCE_NAME],[PERSISTENT_STAGING_NAME], [ATTRIBUTE_NAME_FROM] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 27);
                createStatement.Clear();

                // Hub
                createStatement.AppendLine();
                createStatement.AppendLine("-- Hub");
                createStatement.AppendLine("IF OBJECT_ID('[MD_HUB]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_HUB]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_HUB]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [HUB_ID]       integer NOT NULL ,");
                createStatement.AppendLine("	[HUB_NAME]     varchar(100)  NOT NULL,");
                createStatement.AppendLine("	[SCHEMA_NAME]  varchar(100)  NULL,");
                createStatement.AppendLine("	[BUSINESS_KEY] varchar(100)  NULL,");
                createStatement.AppendLine("	[SURROGATE_KEY] varchar(100)  NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_HUB] PRIMARY KEY CLUSTERED ([HUB_ID] ASC)");
                createStatement.AppendLine(")");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE UNIQUE NONCLUSTERED INDEX [IX_MD_HUB] ON [MD_HUB]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [HUB_NAME] ASC");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 30);
                createStatement.Clear();

                // Hub Link Xref
                createStatement.AppendLine();
                createStatement.AppendLine("-- Hub Link XREF");
                createStatement.AppendLine("IF OBJECT_ID('[MD_HUB_LINK_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE[MD_HUB_LINK_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE[MD_HUB_LINK_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [HUB_ID]          integer NOT NULL,");
                createStatement.AppendLine("	[LINK_ID]         integer NOT NULL,");
                createStatement.AppendLine("	[HUB_ORDER]             integer NOT NULL,");
                createStatement.AppendLine("	[HUB_TARGET_KEY_NAME_IN_LINK]  varchar(4000) NOT NULL,");
                createStatement.AppendLine(
                    "    CONSTRAINT[PK_MD_HUB_LINK_XREF] PRIMARY KEY CLUSTERED ( [LINK_ID] ASC, [HUB_ID] ASC, [HUB_ORDER] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 35);
                createStatement.Clear();

                // Link
                createStatement.AppendLine();
                createStatement.AppendLine("-- Link");
                createStatement.AppendLine("IF OBJECT_ID('[MD_LINK]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_LINK]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_LINK]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [LINK_ID]      integer NOT NULL,");
                createStatement.AppendLine("	[LINK_NAME]    varchar(100)  NOT NULL,");
                createStatement.AppendLine("	[SCHEMA_NAME]  varchar(100)  NULL,");
                createStatement.AppendLine("	[SURROGATE_KEY]  varchar(100)  NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_LINK] PRIMARY KEY CLUSTERED ( [LINK_ID] ASC)");
                createStatement.AppendLine(")");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE UNIQUE NONCLUSTERED INDEX [IX_MD_LINK] ON [MD_LINK]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [LINK_NAME] ASC");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 40);
                createStatement.Clear();

                // Satellite
                createStatement.AppendLine();
                createStatement.AppendLine("-- Sat");
                createStatement.AppendLine("IF OBJECT_ID('[MD_SATELLITE]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE[MD_SATELLITE]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE[MD_SATELLITE]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [SATELLITE_ID]  integer NOT NULL ,");
                createStatement.AppendLine("	[SATELLITE_NAME] varchar(100)  NOT NULL,");
                createStatement.AppendLine("    [SATELLITE_TYPE]     varchar(100)  NOT NULL,");
                createStatement.AppendLine("	[SCHEMA_NAME]  varchar(100)  NULL,");
                createStatement.AppendLine("	[HUB_ID]  integer NOT NULL,");
                createStatement.AppendLine("	[LINK_ID] integer NOT NULL ,");
                createStatement.AppendLine(
                    "    CONSTRAINT [PK_MD_SATELLITE] PRIMARY KEY CLUSTERED ([SATELLITE_ID] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 41);
                createStatement.Clear();

                // Source CDC Xref
                createStatement.AppendLine();
                createStatement.AppendLine("-- Source / CDC Xref");
                createStatement.AppendLine("IF OBJECT_ID('[MD_SOURCE_CDC_TYPE_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_SOURCE_CDC_TYPE_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_SOURCE_CDC_TYPE_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("	[SOURCE_NAME] varchar(100)  NOT NULL,");
                createStatement.AppendLine("    [CHANGE_DATA_CAPTURE_TYPE] varchar(100)  NOT NULL,");
                createStatement.AppendLine("    [CHANGE_DATETIME_DEFINITION] varchar(4000) NULL,");
                createStatement.AppendLine("    [PROCESS_INDICATOR] varchar(1) NULL,");
                createStatement.AppendLine(
                    "    CONSTRAINT [PK_MD_SOURCE_CDC_TYPE_XREF] PRIMARY KEY CLUSTERED ( [CHANGE_DATA_CAPTURE_TYPE], [SOURCE_NAME] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 42);
                createStatement.Clear();

                // Source Datasest
                createStatement.AppendLine();
                createStatement.AppendLine("-- Source dataset");
                createStatement.AppendLine("IF OBJECT_ID('[MD_SOURCE_DATASET]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_SOURCE_DATASET]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_SOURCE_DATASET]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("        [SOURCE_DATASET_ID] [int] IDENTITY(1,1) NOT NULL,");
                createStatement.AppendLine("        [SOURCE_DATASET_NAME] [varchar] (100) NOT NULL, ");
                createStatement.AppendLine("        [SOURCE_DATASET_TYPE] [varchar] (100) NULL,");
                createStatement.AppendLine("    	[SCHEMA_NAME] [varchar] (100) NULL,");
                createStatement.AppendLine("    	[FILE_BASE_NAME] [varchar] (100) NULL,");
                createStatement.AppendLine("    	[FILE_EXTENSION] [varchar] (100) NULL,");
                createStatement.AppendLine("    	[FILE_HEADER_INCLUDED_INDICATOR] [varchar] (1) NULL,");
                createStatement.AppendLine("    	[FILE_COLUMN_DELIMITER_NAME] [varchar] (100) NULL,");
                createStatement.AppendLine("    	[FILE_COLUMN_DELIMITER_TYPE] [varchar] (100) NULL,");
                createStatement.AppendLine("    	[FILE_ROW_DELIMITER_NAME] [varchar] (100) NULL,");
                createStatement.AppendLine("    	[FILE_ROW_DELIMITER_TYPE] [varchar] (100) NULL,");
                createStatement.AppendLine("    	[FILE_LAST_ROW_DELIMITER_TYPE] [varchar] (100) NULL,");
                createStatement.AppendLine("    	[ARCHIVE_REQUIRED_INDICATOR] [varchar] (1) NULL,");
                createStatement.AppendLine("    	[SOURCE_SYSTEM_ID] [int] NULL,");
                createStatement.AppendLine("    CONSTRAINT [PK_MD_SOURCE_DATASET] PRIMARY KEY CLUSTERED ( [SOURCE_DATASET_ID] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 43);
                createStatement.Clear();

                // Source System
                createStatement.AppendLine();
                createStatement.AppendLine("-- Source system");
                createStatement.AppendLine("IF OBJECT_ID('[MD_SOURCE_SYSTEM]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_SOURCE_SYSTEM]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_SOURCE_SYSTEM]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("  [SOURCE_SYSTEM_ID] [int] NOT NULL,");
                createStatement.AppendLine("  [SOURCE_SYSTEM_NAME] [varchar] (100) NOT NULL,");
                createStatement.AppendLine("  [SOURCE_SYSTEM_NAME_SHORT] [varchar] (100) NULL,");
                createStatement.AppendLine("  [SOURCE_SYSTEM_DESCRIPTION] [varchar] (4000) NULL,");
                createStatement.AppendLine("  [DEFAULT_FILE_EXTENSION] [varchar] (100) NULL,");
                createStatement.AppendLine("  [DEFAULT_FILE_HEADER_INCLUDED_INDICATOR] [varchar] (1) NULL,");
                createStatement.AppendLine("  [DEFAULT_FILE_COLUMN_DELIMITER_NAME] [varchar] (100) NULL,");
                createStatement.AppendLine("  [DEFAULT_FILE_COLUMN_DELIMITER_TYPE] [varchar] (100) NULL,");
                createStatement.AppendLine("  [DEFAULT_FILE_ROW_DELIMITER_NAME] [varchar] (100) NULL,");
                createStatement.AppendLine("  [DEFAULT_FILE_ROW_DELIMITER_TYPE] [varchar] (100) NULL,");
                createStatement.AppendLine("  [DEFAULT_FILE_LAST_ROW_DELIMITER_TYPE] [varchar] (100) NULL,");
                createStatement.AppendLine("  [DEFAULT_ARCHIVE_REQUIRED_INDICATOR] [varchar] (1) NULL");
                createStatement.AppendLine("  CONSTRAINT [PK_MD_SOURCE_SYSTEM] PRIMARY KEY CLUSTERED ( [SOURCE_SYSTEM_ID] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 43);
                createStatement.Clear();

                // Staging
                createStatement.AppendLine();
                createStatement.AppendLine("-- Staging");
                createStatement.AppendLine("IF OBJECT_ID ('[MD_SOURCE]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_SOURCE]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_SOURCE]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [SOURCE_ID] integer NOT NULL,");
                createStatement.AppendLine("	[SOURCE_NAME] varchar(100) NOT NULL,");
                createStatement.AppendLine("	[SCHEMA_NAME] varchar(100) NULL,");
                createStatement.AppendLine(
                    "    CONSTRAINT[PK_MD_SOURCE] PRIMARY KEY CLUSTERED ([SOURCE_ID] ASC)");
                createStatement.AppendLine(")");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE UNIQUE NONCLUSTERED INDEX [IX_MD_SOURCE] ON [MD_SOURCE]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [SOURCE_NAME]   ASC");
                createStatement.AppendLine(")");

                try
                {
                    RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 44);
                }
                catch (Exception ex)
                {
                    _alertRepository.SetTextLogging(
                        "An issue has occured creating the Staging metadata table. The full error message is: " + ex);
                }

                createStatement.Clear();

                // Staging Hub Xref
                createStatement.AppendLine();
                createStatement.AppendLine("-- Staging Hub XREF");
                createStatement.AppendLine("IF OBJECT_ID('[MD_SOURCE_HUB_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE[MD_SOURCE_HUB_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_SOURCE_HUB_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [SOURCE_ID]  integer NOT NULL,");
                createStatement.AppendLine("	[HUB_ID] integer NOT NULL,");
                createStatement.AppendLine("	[BUSINESS_KEY_DEFINITION] varchar(4000) NOT NULL,");
                createStatement.AppendLine("	[FILTER_CRITERIA] varchar(4000)  NULL,");
                createStatement.AppendLine("	[LOAD_VECTOR] varchar(100) NULL,");
                createStatement.AppendLine("    CONSTRAINT[PK_MD_SOURCE_HUB_XREF] PRIMARY KEY CLUSTERED([SOURCE_ID] ASC, [HUB_ID] ASC, [BUSINESS_KEY_DEFINITION] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 45);
                createStatement.Clear();

                // Staging Link Attribute Xref
                createStatement.AppendLine();
                createStatement.AppendLine("-- Staging Link Attribute XREF");
                createStatement.AppendLine("IF OBJECT_ID('[MD_SOURCE_LINK_ATTRIBUTE_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE[MD_SOURCE_LINK_ATTRIBUTE_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE[MD_SOURCE_LINK_ATTRIBUTE_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [SOURCE_ID] integer NOT NULL,");
                createStatement.AppendLine("	[LINK_ID] integer NOT NULL,");
                createStatement.AppendLine("	[ATTRIBUTE_ID_FROM] integer NOT NULL,");
                createStatement.AppendLine("	[ATTRIBUTE_ID_TO] integer NOT NULL,");
                createStatement.AppendLine("    CONSTRAINT[PK_MD_SOURCE_LINK_ATTRIBUTE_XREF] PRIMARY KEY CLUSTERED([SOURCE_ID] ASC, [LINK_ID] ASC, [ATTRIBUTE_ID_FROM] ASC, [ATTRIBUTE_ID_TO] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 46);
                createStatement.Clear();

                // Staging Link  Xref
                createStatement.AppendLine();
                createStatement.AppendLine("-- Staging Link  XREF");
                createStatement.AppendLine("IF OBJECT_ID('[MD_SOURCE_LINK_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE[MD_SOURCE_LINK_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE[MD_SOURCE_LINK_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [SOURCE_ID] integer NOT NULL,");
                createStatement.AppendLine("	[LINK_ID] integer NOT NULL,");
                createStatement.AppendLine("	[FILTER_CRITERIA] varchar(4000) NULL,");
                createStatement.AppendLine("	[BUSINESS_KEY_DEFINITION] varchar(4000) NOT NULL,");
                createStatement.AppendLine("	[LOAD_VECTOR] varchar(100)  NULL,");
                createStatement.AppendLine("    CONSTRAINT[PK_MD_SOURCE_LINK_XREF] PRIMARY KEY CLUSTERED([SOURCE_ID] ASC, [LINK_ID] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 47);
                createStatement.Clear();

                // Staging / Satellite Attribute Xref
                createStatement.AppendLine();
                createStatement.AppendLine("-- Staging Attribute XREF");
                createStatement.AppendLine("IF OBJECT_ID('[MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE[MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE[MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [SOURCE_ID] integer NOT NULL,");
                createStatement.AppendLine("	[SATELLITE_ID] integer NOT NULL,");
                createStatement.AppendLine("	[ATTRIBUTE_ID_FROM]  integer NOT NULL,");
                createStatement.AppendLine("	[ATTRIBUTE_ID_TO] integer NOT NULL,");
                createStatement.AppendLine("	[MULTI_ACTIVE_KEY_INDICATOR] varchar(100)  NOT NULL,");
                createStatement.AppendLine("    CONSTRAINT[PK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF] PRIMARY KEY CLUSTERED([SOURCE_ID] ASC, [SATELLITE_ID] ASC, [ATTRIBUTE_ID_FROM] ASC, [ATTRIBUTE_ID_TO] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 48);
                createStatement.Clear();

                // Staging / Satellite  Xref
                createStatement.AppendLine();
                createStatement.AppendLine("-- Staging Satellite XREF");
                createStatement.AppendLine("IF OBJECT_ID('[MD_SOURCE_SATELLITE_XREF]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_SOURCE_SATELLITE_XREF]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_SOURCE_SATELLITE_XREF]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("	[SATELLITE_ID] integer NOT NULL,");
                createStatement.AppendLine("    [SOURCE_ID] integer NOT NULL,");
                createStatement.AppendLine("    [BUSINESS_KEY_DEFINITION] [varchar](1000) NOT NULL,");
                createStatement.AppendLine("	[FILTER_CRITERIA] varchar(4000)  NULL,");
                createStatement.AppendLine("	[LOAD_VECTOR] varchar(100)  NULL,");
                createStatement.AppendLine("    CONSTRAINT[PK_MD_SOURCE_SATELLITE_XREF] PRIMARY KEY CLUSTERED([SATELLITE_ID] ASC, [SOURCE_ID] ASC)");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 50);
                createStatement.Clear();

                if (!checkBoxRetainManualMapping.Checked)
                {
                    // Table Mapping
                    createStatement.AppendLine();
                    createStatement.AppendLine("-- Table Mapping");
                    createStatement.AppendLine("IF OBJECT_ID('[MD_TABLE_MAPPING]', 'U') IS NOT NULL");
                    createStatement.AppendLine(" DROP TABLE [MD_TABLE_MAPPING]");
                    createStatement.AppendLine("");
                    createStatement.AppendLine("CREATE TABLE [MD_TABLE_MAPPING]");
                    createStatement.AppendLine("( ");
                    createStatement.AppendLine("    [TABLE_MAPPING_HASH] AS(");
                    createStatement.AppendLine("    CONVERT([CHAR](32),HASHBYTES('MD5',");
                    createStatement.AppendLine("       ISNULL(RTRIM(CONVERT(VARCHAR(100),[TARGET_TABLE])),'NA')+'|'+");
                    createStatement.AppendLine("       ISNULL(RTRIM(CONVERT(VARCHAR(100),[SOURCE_TABLE])),'NA')+'|'+");
                    createStatement.AppendLine("       ISNULL(RTRIM(CONVERT(VARCHAR(100),[BUSINESS_KEY_ATTRIBUTE])),'NA')+'|'+");
                    createStatement.AppendLine("       ISNULL(RTRIM(CONVERT(VARCHAR(100),[DRIVING_KEY_ATTRIBUTE])),'NA')+'|'+");
                    createStatement.AppendLine("       ISNULL(RTRIM(CONVERT(VARCHAR(100),[FILTER_CRITERIA])),'NA')+'|'");
                    createStatement.AppendLine("),(2)");
                    createStatement.AppendLine(")");
                    createStatement.AppendLine(") PERSISTED NOT NULL ,");
                    createStatement.AppendLine("	[VERSION_ID] integer NOT NULL ,");
                    createStatement.AppendLine("	[SOURCE_TABLE] varchar(100)  NULL,");
                    createStatement.AppendLine("	[BUSINESS_KEY_ATTRIBUTE] varchar(4000)  NULL,");
                    createStatement.AppendLine("	[DRIVING_KEY_ATTRIBUTE] varchar(4000)  NULL,");
                    createStatement.AppendLine("	[TARGET_TABLE] varchar(100)  NULL,");
                    createStatement.AppendLine("	[FILTER_CRITERIA] varchar(4000)  NULL,");
                    createStatement.AppendLine("	[PROCESS_INDICATOR] varchar(1)  NULL,");
                    createStatement.AppendLine("    CONSTRAINT[PK_MD_TABLE_MAPPING] PRIMARY KEY CLUSTERED([TABLE_MAPPING_HASH] ASC, [VERSION_ID] ASC)");
                    createStatement.AppendLine(")");

                    RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 55);
                    createStatement.Clear();
                }

                // Version
                createStatement.AppendLine();
                createStatement.AppendLine("-- Version");
                createStatement.AppendLine("IF OBJECT_ID('[MD_VERSION]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE[MD_VERSION]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE[MD_VERSION]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("    [VERSION_ID] integer NOT NULL IDENTITY( 1,1 ) ,");
                createStatement.AppendLine("	[VERSION_NAME] varchar(100)  NOT NULL,");
                createStatement.AppendLine("");
                createStatement.AppendLine("    [VERSION_NOTES]      varchar(1000)  NULL ,");
                createStatement.AppendLine("	[MAJOR_RELEASE_NUMBER] integer NULL,");
                createStatement.AppendLine("    [MINOR_RELEASE_NUMBER] integer NULL ");
                createStatement.AppendLine(")");
                createStatement.AppendLine("");
                createStatement.AppendLine("ALTER TABLE [MD_VERSION]");
                createStatement.AppendLine("  ADD CONSTRAINT[PK_MD_VERSION] PRIMARY KEY CLUSTERED([VERSION_ID] ASC)");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE UNIQUE NONCLUSTERED INDEX[IX_MD_VERSION] ON[MD_VERSION]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("");
                createStatement.AppendLine("  [MAJOR_RELEASE_NUMBER] ASC,");
                createStatement.AppendLine("  [MINOR_RELEASE_NUMBER] ASC");
                createStatement.AppendLine(")");

                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 58);
                createStatement.Clear();

                // Version Attribute
                createStatement.AppendLine();
                createStatement.AppendLine("-- Version Attribute");
                createStatement.AppendLine("IF OBJECT_ID('[MD_VERSION_ATTRIBUTE]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_VERSION_ATTRIBUTE]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_VERSION_ATTRIBUTE]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("  [VERSION_ATTRIBUTE_HASH] AS (");
                createStatement.AppendLine("  CONVERT([CHAR](32),HASHBYTES('MD5',");
                createStatement.AppendLine("    ISNULL(RTRIM(CONVERT(VARCHAR(100),[DATABASE_NAME])),'NA')+'|'+");
                createStatement.AppendLine("    ISNULL(RTRIM(CONVERT(VARCHAR(100),[SCHEMA_NAME])),'NA')+'|'+");
                createStatement.AppendLine("    ISNULL(RTRIM(CONVERT(VARCHAR(100),[TABLE_NAME])),'NA')+'|'+");
                createStatement.AppendLine("    ISNULL(RTRIM(CONVERT(VARCHAR(100),[COLUMN_NAME])),'NA')+'|'+");
                createStatement.AppendLine("    ISNULL(RTRIM(CONVERT(VARCHAR(100),[VERSION_ID])),'NA')+'|'");
                createStatement.AppendLine("  ),(2)");
                createStatement.AppendLine(")");
                createStatement.AppendLine(") PERSISTED NOT NULL ,");
                createStatement.AppendLine("  [VERSION_ID] integer NOT NULL ,");
                createStatement.AppendLine("  [DATABASE_NAME] varchar(100) NOT NULL ,");
                createStatement.AppendLine("  [SCHEMA_NAME] varchar(100) NOT NULL ,");
                createStatement.AppendLine("  [TABLE_NAME] varchar(100)  NOT NULL ,");
                createStatement.AppendLine("  [COLUMN_NAME] varchar(100)  NOT NULL,");
                createStatement.AppendLine("  [DATA_TYPE] varchar(100)  NOT NULL ,");
                createStatement.AppendLine("  [CHARACTER_MAXIMUM_LENGTH] integer NULL,");
                createStatement.AppendLine("  [NUMERIC_PRECISION] integer NULL,");
                createStatement.AppendLine("  [ORDINAL_POSITION] integer NULL,");
                createStatement.AppendLine("  [PRIMARY_KEY_INDICATOR] varchar(1)  NULL ,");
                createStatement.AppendLine("  [MULTI_ACTIVE_INDICATOR] varchar(1)  NULL ");
                createStatement.AppendLine(")");
                createStatement.AppendLine("");
                createStatement.AppendLine("ALTER TABLE[MD_VERSION_ATTRIBUTE]");
                createStatement.AppendLine("  ADD CONSTRAINT[PK_MD_VERSION_ATTRIBUTE] PRIMARY KEY CLUSTERED([DATABASE_NAME] ASC, [SCHEMA_NAME], [TABLE_NAME], [COLUMN_NAME], [VERSION_ID] ASC)");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 59);
                createStatement.Clear();


                // Physical Model
                createStatement.AppendLine();
                createStatement.AppendLine("-- Version Attribute");
                createStatement.AppendLine("IF OBJECT_ID('[MD_PHYSICAL_MODEL]', 'U') IS NOT NULL");
                createStatement.AppendLine(" DROP TABLE [MD_PHYSICAL_MODEL]");
                createStatement.AppendLine("");
                createStatement.AppendLine("CREATE TABLE [MD_PHYSICAL_MODEL]");
                createStatement.AppendLine("( ");
                createStatement.AppendLine("  [DATABASE_NAME] varchar(100)  NOT NULL ,");
                createStatement.AppendLine("  [SCHEMA_NAME] varchar(100)  NOT NULL ,");
                createStatement.AppendLine("  [TABLE_NAME] varchar(100)  NOT NULL ,");
                createStatement.AppendLine("  [COLUMN_NAME] varchar(100)  NOT NULL,");
                createStatement.AppendLine("  [DATA_TYPE] varchar(100)  NULL ,");
                createStatement.AppendLine("  [CHARACTER_MAXIMUM_LENGTH] integer NULL,");
                createStatement.AppendLine("  [NUMERIC_PRECISION] integer NULL,");
                createStatement.AppendLine("  [ORDINAL_POSITION] integer NULL,");
                createStatement.AppendLine("  [PRIMARY_KEY_INDICATOR] varchar(1)  NULL");
                createStatement.AppendLine(")");
                createStatement.AppendLine("");
                createStatement.AppendLine("ALTER TABLE[MD_PHYSICAL_MODEL]");
                createStatement.AppendLine("  ADD CONSTRAINT [PK_MD_PHYSICAL_MODEL] PRIMARY KEY CLUSTERED ([SCHEMA_NAME] ASC, [TABLE_NAME] ASC, [COLUMN_NAME] ASC)");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 59);
                createStatement.Clear();



                // Create existing Foreign Key Constraints
                createStatement.AppendLine("ALTER TABLE [MD_BUSINESS_KEY_COMPONENT] WITH CHECK ADD CONSTRAINT [FK_MD_BUSINESS_KEY_COMPONENT_MD_SOURCE_HUB_XREF] FOREIGN KEY([SOURCE_ID], [HUB_ID], [BUSINESS_KEY_DEFINITION])");
                createStatement.AppendLine("REFERENCES [MD_SOURCE_HUB_XREF] ([SOURCE_ID], [HUB_ID], [BUSINESS_KEY_DEFINITION])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 60);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_BUSINESS_KEY_COMPONENT_PART] WITH CHECK ADD CONSTRAINT [FK_MD_BUSINESS_KEY_COMPONENT_PART_MD_ATTRIBUTE] FOREIGN KEY([ATTRIBUTE_ID])");
                createStatement.AppendLine("REFERENCES  [MD_ATTRIBUTE] ([ATTRIBUTE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 61);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_BUSINESS_KEY_COMPONENT_PART] WITH CHECK ADD CONSTRAINT [FK_MD_BUSINESS_KEY_COMPONENT_PART_MD_BUSINESS_KEY_COMPONENT] FOREIGN KEY([SOURCE_ID], [HUB_ID], [BUSINESS_KEY_DEFINITION], [COMPONENT_ID])");
                createStatement.AppendLine("REFERENCES  [MD_BUSINESS_KEY_COMPONENT]([SOURCE_ID], [HUB_ID], [BUSINESS_KEY_DEFINITION], [COMPONENT_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 62);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_DRIVING_KEY_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_DRIVING_KEY_XREF_MD_HUB] FOREIGN KEY([HUB_ID])");
                createStatement.AppendLine("REFERENCES  [MD_HUB] ([HUB_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 63);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_DRIVING_KEY_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_DRIVING_KEY_XREF_MD_SATELLITE] FOREIGN KEY([SATELLITE_ID])");
                createStatement.AppendLine("REFERENCES  [MD_SATELLITE] ([SATELLITE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 64);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_HUB_LINK_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_HUB_LINK_XREF_MD_HUB] FOREIGN KEY([HUB_ID])");
                createStatement.AppendLine("REFERENCES  [MD_HUB] ([HUB_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 65);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_HUB_LINK_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_HUB_LINK_XREF_MD_LINK] FOREIGN KEY([LINK_ID])");
                createStatement.AppendLine("REFERENCES  [MD_LINK] ([LINK_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 66);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SATELLITE]  WITH CHECK ADD  CONSTRAINT [FK_MD_SATELLITE_MD_HUB] FOREIGN KEY([HUB_ID])");
                createStatement.AppendLine("REFERENCES  [MD_HUB] ([HUB_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 67);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SATELLITE]  WITH CHECK ADD  CONSTRAINT [FK_MD_SATELLITE_MD_LINK] FOREIGN KEY([LINK_ID])");
                createStatement.AppendLine("REFERENCES  [MD_LINK] ([LINK_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 68);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_HUB_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_HUB_XREF_MD_HUB] FOREIGN KEY([HUB_ID])");
                createStatement.AppendLine("REFERENCES  [MD_HUB] ([HUB_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 69);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_HUB_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_HUB_XREF_MD_SOURCE] FOREIGN KEY([SOURCE_ID])");
                createStatement.AppendLine("REFERENCES  [MD_SOURCE] ([SOURCE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 70);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_LINK_ATTRIBUTE_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_ATTRIBUTE_FROM] FOREIGN KEY([ATTRIBUTE_ID_FROM])");
                createStatement.AppendLine("REFERENCES  [MD_ATTRIBUTE] ([ATTRIBUTE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 71);
                createStatement.Clear();

                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_LINK_ATTRIBUTE_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_ATTRIBUTE_TO] FOREIGN KEY([ATTRIBUTE_ID_TO])");
                createStatement.AppendLine("REFERENCES  [MD_ATTRIBUTE] ([ATTRIBUTE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 71);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_LINK_ATTRIBUTE_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_LINK] FOREIGN KEY([LINK_ID])");
                createStatement.AppendLine("REFERENCES  [MD_LINK] ([LINK_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 72);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_LINK_ATTRIBUTE_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_LINK_ATTRIBUTE_XREF_MD_SOURCE] FOREIGN KEY([SOURCE_ID])");
                createStatement.AppendLine("REFERENCES  [MD_SOURCE] ([SOURCE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 73);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_LINK_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_LINK_XREF_MD_LINK] FOREIGN KEY([LINK_ID])");
                createStatement.AppendLine("REFERENCES  [MD_LINK] ([LINK_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 74);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_LINK_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_LINK_XREF_MD_SOURCE] FOREIGN KEY([SOURCE_ID])");
                createStatement.AppendLine("REFERENCES  [MD_SOURCE] ([SOURCE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 75);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_ATTRIBUTERIBUTE_FROM] FOREIGN KEY([ATTRIBUTE_ID_FROM])");
                createStatement.AppendLine("REFERENCES  [MD_ATTRIBUTE] ([ATTRIBUTE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 76);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_ATTRIBUTERIBUTE_TO] FOREIGN KEY([ATTRIBUTE_ID_TO])");
                createStatement.AppendLine("REFERENCES  [MD_ATTRIBUTE] ([ATTRIBUTE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 77);
                createStatement.Clear();

                createStatement.AppendLine("ALTER TABLE [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_SATELLITE] FOREIGN KEY([SATELLITE_ID])");
                createStatement.AppendLine("REFERENCES  [MD_SATELLITE] ([SATELLITE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 78);
                createStatement.Clear();

                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_SATELLITE_ATTRIBUTE_XREF_MD_SOURCE] FOREIGN KEY([SOURCE_ID])");
                createStatement.AppendLine("REFERENCES  [MD_SOURCE] ([SOURCE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 78);
                createStatement.Clear();

                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_SATELLITE_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_SATELLITE_XREF_MD_SATELLITE] FOREIGN KEY([SATELLITE_ID])");
                createStatement.AppendLine("REFERENCES  [MD_SATELLITE] ([SATELLITE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 79);
                createStatement.Clear();

                createStatement.AppendLine(
                    "ALTER TABLE [MD_SOURCE_SATELLITE_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_SATELLITE_XREF_MD_SOURCE] FOREIGN KEY([SOURCE_ID])");
                createStatement.AppendLine("REFERENCES  [MD_SOURCE] ([SOURCE_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 79);
                createStatement.Clear();

                //createStatement.AppendLine(
                //    "ALTER TABLE [dbo].[MD_SOURCE_STAGING_XREF]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_STAGING_XREF_MD_SOURCE_DATASET] FOREIGN KEY([SOURCE_DATASET_ID])");
                //createStatement.AppendLine("REFERENCES [dbo].[MD_SOURCE_DATASET] ([SOURCE_DATASET_ID])");
                //createStatement.AppendLine();
                //RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 79);
                //createStatement.Clear();

                createStatement.AppendLine(
                    "ALTER TABLE [dbo].[MD_SOURCE_DATASET]  WITH CHECK ADD  CONSTRAINT [FK_MD_SOURCE_DATASET_MD_SOURCE_SYSTEM] FOREIGN KEY([SOURCE_SYSTEM_ID])");
                createStatement.AppendLine("REFERENCES [dbo].[MD_SOURCE_SYSTEM] ([SOURCE_SYSTEM_ID])");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 79);
                createStatement.Clear();


                // Drop the views

                createStatement.AppendLine("-- INTERFACE_BUSINESS_KEY_COMPONENT");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_BUSINESS_KEY_COMPONENT]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_BUSINESS_KEY_COMPONENT]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 78);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_BUSINESS_KEY_COMPONENT_PART");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_BUSINESS_KEY_COMPONENT_PART]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_BUSINESS_KEY_COMPONENT_PART]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 78);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_SOURCE_SATELLITE_ATTRIBUTE_XREF");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_SOURCE_SATELLITE_ATTRIBUTE_XREF]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_SOURCE_SATELLITE_ATTRIBUTE_XREF]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 79);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_SOURCE_LINK_ATTRIBUTE_XREF");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_SOURCE_LINK_ATTRIBUTE_XREF]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_SOURCE_LINK_ATTRIBUTE_XREF]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 79);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_HUB_LINK_XREF");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_HUB_LINK_XREF]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_HUB_LINK_XREF]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 79);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_DRIVING_KEY");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_DRIVING_KEY]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_DRIVING_KEY]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 80);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_SOURCE_STAGING_XREF");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_SOURCE_STAGING_XREF]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_SOURCE_STAGING_XREF]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 80);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_SOURCE_STAGING_ATTRIBUTE_XREF");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_SOURCE_STAGING_ATTRIBUTE_XREF]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_SOURCE_STAGING_ATTRIBUTE_XREF]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 80);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_SOURCE_PERSISTENT_STAGING_XREF");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_SOURCE_PERSISTENT_STAGING_XREF]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_SOURCE_PERSISTENT_STAGING_XREF]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 80);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 80);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_SOURCE_SATELLITE_XREF");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_SOURCE_SATELLITE_XREF]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_SOURCE_SATELLITE_XREF]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 81);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_SOURCE_HUB_XREF");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_SOURCE_HUB_XREF]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_SOURCE_HUB_XREF]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 81);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_SOURCE_LINK_XREF");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_SOURCE_LINK_XREF]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_SOURCE_LINK_XREF]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 82);
                createStatement.Clear();

                createStatement.AppendLine("-- INTERFACE_PHYSICAL_MODEL");
                createStatement.AppendLine("IF OBJECT_ID('[interface].[INTERFACE_PHYSICAL_MODEL]', 'V') IS NOT NULL");
                createStatement.AppendLine(" DROP VIEW [interface].[INTERFACE_PHYSICAL_MODEL]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 82);
                createStatement.Clear();

                // Create the schemas

                createStatement.AppendLine("-- Creating the schema");
                createStatement.AppendLine("IF EXISTS ( SELECT schema_name FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'interface')");
                createStatement.AppendLine("DROP SCHEMA [interface]");
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 82);
                createStatement.Clear();

                createStatement.AppendLine("CREATE SCHEMA [interface]");
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 83);
                createStatement.Clear();


                // Create the views

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_BUSINESS_KEY_COMPONENT]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine(" xref.SOURCE_ID,");
                createStatement.AppendLine(" SOURCE_NAME,");
                createStatement.AppendLine(" stg.SCHEMA_NAME AS SOURCE_SCHEMA_NAME,");
                createStatement.AppendLine(" xref.HUB_ID,");
                createStatement.AppendLine(" HUB_NAME,");
                createStatement.AppendLine(" BUSINESS_KEY_DEFINITION,");
                createStatement.AppendLine(" COMPONENT_ID AS BUSINESS_KEY_COMPONENT_ID,");
                createStatement.AppendLine(" COMPONENT_ORDER AS BUSINESS_KEY_COMPONENT_ORDER,");
                createStatement.AppendLine(" COMPONENT_VALUE AS BUSINESS_KEY_COMPONENT_VALUE");
                createStatement.AppendLine("FROM MD_BUSINESS_KEY_COMPONENT xref");
                createStatement.AppendLine("JOIN MD_SOURCE stg ON xref.SOURCE_ID = stg.SOURCE_ID");
                createStatement.AppendLine("JOIN MD_HUB hub ON xref.HUB_ID = hub.HUB_ID");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 85);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_BUSINESS_KEY_COMPONENT_PART]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine("  comp.SOURCE_ID, ");
                createStatement.AppendLine("  stg.SOURCE_NAME,");
                //createStatement.AppendLine("  stg.SCHEMA_NAME,");
                createStatement.AppendLine("  comp.HUB_ID, ");
                createStatement.AppendLine("  hub.HUB_NAME,");
                createStatement.AppendLine("  comp.BUSINESS_KEY_DEFINITION,");
                createStatement.AppendLine("  comp.COMPONENT_ID AS BUSINESS_KEY_COMPONENT_ID, ");
                createStatement.AppendLine("  comp.COMPONENT_ORDER AS BUSINESS_KEY_COMPONENT_ORDER,");
                createStatement.AppendLine("  elem.COMPONENT_ELEMENT_ID AS BUSINESS_KEY_COMPONENT_ELEMENT_ID, ");
                createStatement.AppendLine("  elem.COMPONENT_ELEMENT_ORDER AS BUSINESS_KEY_COMPONENT_ELEMENT_ORDER,");
                createStatement.AppendLine("  elem.COMPONENT_ELEMENT_VALUE AS BUSINESS_KEY_COMPONENT_ELEMENT_VALUE,");
                createStatement.AppendLine("  elem.COMPONENT_ELEMENT_TYPE AS BUSINESS_KEY_COMPONENT_ELEMENT_TYPE,");
                createStatement.AppendLine("  elem.ATTRIBUTE_ID AS BUSINESS_KEY_COMPONENT_ELEMENT_ATTRIBUTE_ID,");
                createStatement.AppendLine(
                    "  COALESCE(att.ATTRIBUTE_NAME, 'Not applicable') AS BUSINESS_KEY_COMPONENT_ELEMENT_ATTRIBUTE_NAME");
                createStatement.AppendLine("FROM MD_BUSINESS_KEY_COMPONENT comp");
                createStatement.AppendLine("JOIN MD_BUSINESS_KEY_COMPONENT_PART elem");
                createStatement.AppendLine("  ON comp.SOURCE_ID = elem.SOURCE_ID");
                createStatement.AppendLine(" AND comp.HUB_ID = elem.HUB_ID");
                createStatement.AppendLine(" AND comp.BUSINESS_KEY_DEFINITION = elem.BUSINESS_KEY_DEFINITION");
                createStatement.AppendLine(" AND comp.COMPONENT_ID = elem.COMPONENT_ID");
                createStatement.AppendLine("JOIN MD_SOURCE stg ON comp.SOURCE_ID = stg.SOURCE_ID");
                createStatement.AppendLine("JOIN MD_HUB hub ON comp.HUB_ID = hub.HUB_ID");
                createStatement.AppendLine("LEFT JOIN MD_ATTRIBUTE att ON elem.ATTRIBUTE_ID = att.ATTRIBUTE_ID");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 87);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW[interface].[INTERFACE_DRIVING_KEY]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine("  MD.SATELLITE_ID, ");
                createStatement.AppendLine("  sat.SATELLITE_NAME,");
                createStatement.AppendLine("  MD.HUB_ID,");
                createStatement.AppendLine("  hub.HUB_NAME");
                createStatement.AppendLine("FROM MD_DRIVING_KEY_XREF MD");
                createStatement.AppendLine(
                    "LEFT OUTER JOIN dbo.MD_SATELLITE sat ON MD.SATELLITE_ID = sat.SATELLITE_ID");
                createStatement.AppendLine("LEFT OUTER JOIN dbo.MD_HUB hub ON MD.HUB_ID = hub.HUB_ID");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 89);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW[interface].[INTERFACE_HUB_LINK_XREF]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine(" slxref.LINK_ID,");
                createStatement.AppendLine(" lnk.LINK_NAME,");
                createStatement.AppendLine(" slxref.SOURCE_ID,");
                createStatement.AppendLine(" stg.SOURCE_NAME,");
                createStatement.AppendLine(" stg.SCHEMA_NAME AS SOURCE_SCHEMA_NAME,");
                createStatement.AppendLine(" hub.HUB_ID,");
                createStatement.AppendLine(" hub.HUB_NAME,");
                createStatement.AppendLine(" hlxref.HUB_ORDER,");
                createStatement.AppendLine(" BUSINESS_KEY_PART_SOURCE AS BUSINESS_KEY_DEFINITION");
                createStatement.AppendLine("FROM --Base table that selects the Links to generate. This is the basis for the Link generation");
                createStatement.AppendLine("(");
                createStatement.AppendLine("	SELECT");
                createStatement.AppendLine("	  LINK_ID,");
                createStatement.AppendLine("	  SOURCE_ID,");
                createStatement.AppendLine("	  LTRIM(Split.a.value('.', 'VARCHAR(4000)')) AS BUSINESS_KEY_PART_SOURCE,");
                createStatement.AppendLine("	  ROW_NUMBER() OVER(PARTITION BY LINK_ID, SOURCE_ID ORDER BY LINK_ID, SOURCE_ID) AS HUB_ORDER");
                createStatement.AppendLine("	FROM");
                createStatement.AppendLine("	(");
                createStatement.AppendLine("	  SELECT");
                createStatement.AppendLine("		LINK_ID,");
                createStatement.AppendLine("		SOURCE_ID,");
                createStatement.AppendLine("		CAST('<M>' + REPLACE(BUSINESS_KEY_DEFINITION, ',', '</M><M>') + '</M>' AS XML) AS BUSINESS_KEY_SOURCE_XML");
                createStatement.AppendLine("		FROM [MD_SOURCE_LINK_XREF]");
                createStatement.AppendLine("	) AS A CROSS APPLY BUSINESS_KEY_SOURCE_XML.nodes('/M') AS Split(a)");
                createStatement.AppendLine(") slxref");
                createStatement.AppendLine("JOIN MD_SOURCE stg ");
                createStatement.AppendLine("	ON slxref.SOURCE_ID = stg.SOURCE_ID -- Adding the Staging Area table name and schema");
                createStatement.AppendLine("JOIN MD_LINK lnk ");
                createStatement.AppendLine("	ON slxref.LINK_ID = lnk.LINK_ID -- Adding the Link table name");
                createStatement.AppendLine("JOIN MD_HUB_LINK_XREF hlxref ");
                createStatement.AppendLine("	ON slxref.LINK_ID = hlxref.LINK_ID -- Adding the Hubs that relate to the Link, from a target perspective");
                createStatement.AppendLine("  AND slxref.HUB_ORDER = hlxref.HUB_ORDER");
                createStatement.AppendLine("JOIN MD_HUB hub");
                createStatement.AppendLine("    ON hlxref.HUB_ID = hub.HUB_ID -- Adding the Hub name");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 91);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_SOURCE_STAGING_XREF]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("/*");
                createStatement.AppendLine("This view combines the staging area listing / interfaces with the exceptions where a single source file/table is mapped to more than one Staging Area tables.");
                createStatement.AppendLine("This is the default source for source-to-staging interfaces.");
                createStatement.AppendLine("*/");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine("    NULL AS [SOURCE_ID],");
                createStatement.AppendLine("src.[SCHEMA_NAME] AS [SOURCE_SCHEMA_NAME],");
                createStatement.AppendLine("xref.[SOURCE_NAME],");
                createStatement.AppendLine("xref.[KEY_DEFINITION] AS [SOURCE_BUSINESS_KEY_DEFINITION],");
                createStatement.AppendLine("NULL AS[TARGET_ID],");
                createStatement.AppendLine("tgt.[SCHEMA_NAME] AS[TARGET_SCHEMA_NAME],");
                createStatement.AppendLine("xref.[STAGING_NAME] AS[TARGET_NAME],");
                createStatement.AppendLine("NULL AS [TARGET_BUSINESS_KEY_DEFINITION],");
                createStatement.AppendLine("'PersistentStagingArea' AS [TARGET_TYPE],");
                createStatement.AppendLine("NULL AS [SURROGATE_KEY],");
                createStatement.AppendLine("    [FILTER_CRITERIA],");
                createStatement.AppendLine("'Raw' AS[LOAD_VECTOR]");
                createStatement.AppendLine("FROM[MD_SOURCE_STAGING_XREF] xref");
                createStatement.AppendLine("JOIN[MD_SOURCE] src ON xref.[SOURCE_NAME] = src.[SOURCE_NAME]");
                createStatement.AppendLine("JOIN[MD_STAGING] tgt ON xref.[STAGING_NAME] = tgt.[STAGING_NAME]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 93);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_SOURCE_STAGING_ATTRIBUTE_XREF]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine("  NULL AS[SOURCE_ID]");
                createStatement.AppendLine("  ,src.SCHEMA_NAME AS SOURCE_SCHEMA_NAME");
                createStatement.AppendLine("  ,src.SOURCE_NAME");
                createStatement.AppendLine("  ,NULL AS TARGET_ID");
                createStatement.AppendLine("  ,tgt.SCHEMA_NAME AS TARGET_SCHEMA_NAME");
                createStatement.AppendLine("  ,xref.[STAGING_NAME] AS TARGET_NAME");
                createStatement.AppendLine("  ,NULL AS SOURCE_ATTRIBUTE_ID");
                createStatement.AppendLine("  ,att_from.[ATTRIBUTE_NAME] AS SOURCE_ATTRIBUTE_NAME");
                createStatement.AppendLine("  ,NULL AS TARGET_ATTRIBUTE_ID");
                createStatement.AppendLine("  ,att_to.[ATTRIBUTE_NAME] AS TARGET_ATTRIBUTE_NAME");
                createStatement.AppendLine("FROM[MD_SOURCE_STAGING_ATTRIBUTE_XREF] xref");
                createStatement.AppendLine("JOIN MD_SOURCE src ON xref.SOURCE_NAME = src.SOURCE_NAME");
                createStatement.AppendLine("JOIN MD_STAGING tgt ON xref.STAGING_NAME = tgt.STAGING_NAME");
                createStatement.AppendLine("JOIN MD_ATTRIBUTE att_from ON xref.ATTRIBUTE_NAME_FROM = att_from.ATTRIBUTE_NAME");
                createStatement.AppendLine("JOIN MD_ATTRIBUTE att_to ON xref.ATTRIBUTE_NAME_TO = att_to.ATTRIBUTE_NAME");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 92);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_SOURCE_PERSISTENT_STAGING_XREF]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine("  NULL AS[SOURCE_ID],");
                createStatement.AppendLine("src.[SCHEMA_NAME] AS[SOURCE_SCHEMA_NAME],");
                createStatement.AppendLine("xref.[SOURCE_NAME],");
                createStatement.AppendLine("xref.[KEY_DEFINITION] AS [SOURCE_BUSINESS_KEY_DEFINITION],");
                createStatement.AppendLine("NULL AS [TARGET_ID],");
                createStatement.AppendLine("tgt.[SCHEMA_NAME] AS[TARGET_SCHEMA_NAME],");
                createStatement.AppendLine("xref.[PERSISTENT_STAGING_NAME] AS[TARGET_NAME],");
                createStatement.AppendLine("NULL AS[TARGET_BUSINESS_KEY_DEFINITION],");
                createStatement.AppendLine("'StagingArea' AS[TARGET_TYPE],");
                createStatement.AppendLine("NULL AS[SURROGATE_KEY],");
                createStatement.AppendLine("    [FILTER_CRITERIA],");
                createStatement.AppendLine("'Raw' AS[LOAD_VECTOR]");
                createStatement.AppendLine("FROM[MD_SOURCE_PERSISTENT_STAGING_XREF] xref");
                createStatement.AppendLine("JOIN[MD_SOURCE] src ON xref.[SOURCE_NAME] = src.[SOURCE_NAME]");
                createStatement.AppendLine("JOIN[MD_PERSISTENT_STAGING] tgt ON xref.[PERSISTENT_STAGING_NAME] = tgt.[PERSISTENT_STAGING_NAME]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 94);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine("  NULL AS[SOURCE_ID]");
                createStatement.AppendLine("  ,src.SCHEMA_NAME AS SOURCE_SCHEMA_NAME");
                createStatement.AppendLine("  ,src.SOURCE_NAME");
                createStatement.AppendLine("  ,NULL AS TARGET_ID");
                createStatement.AppendLine("  ,tgt.SCHEMA_NAME AS TARGET_SCHEMA_NAME");
                createStatement.AppendLine("  ,xref.[PERSISTENT_STAGING_NAME] AS TARGET_NAME");
                createStatement.AppendLine("  ,NULL AS SOURCE_ATTRIBUTE_ID");
                createStatement.AppendLine("  ,att_from.[ATTRIBUTE_NAME] AS SOURCE_ATTRIBUTE_NAME");
                createStatement.AppendLine("  ,NULL AS TARGET_ATTRIBUTE_ID");
                createStatement.AppendLine("  ,att_to.[ATTRIBUTE_NAME] AS TARGET_ATTRIBUTE_NAME");
                createStatement.AppendLine("FROM[MD_SOURCE_PERSISTENT_STAGING_ATTRIBUTE_XREF] xref");
                createStatement.AppendLine("JOIN MD_SOURCE src ON xref.SOURCE_NAME = src.SOURCE_NAME");
                createStatement.AppendLine("JOIN MD_PERSISTENT_STAGING tgt ON xref.PERSISTENT_STAGING_NAME = tgt.PERSISTENT_STAGING_NAME");
                createStatement.AppendLine("JOIN MD_ATTRIBUTE att_from ON xref.ATTRIBUTE_NAME_FROM = att_from.ATTRIBUTE_NAME");
                createStatement.AppendLine("JOIN MD_ATTRIBUTE att_to ON xref.ATTRIBUTE_NAME_TO = att_to.ATTRIBUTE_NAME");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 92);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_SOURCE_HUB_XREF]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine(" xref.[SOURCE_ID],");
                createStatement.AppendLine(" stg.[SCHEMA_NAME] AS [SOURCE_SCHEMA_NAME],");
                createStatement.AppendLine(" stg.[SOURCE_NAME],");
                createStatement.AppendLine(" xref.[BUSINESS_KEY_DEFINITION] AS [SOURCE_BUSINESS_KEY_DEFINITION],");
                createStatement.AppendLine(" xref.[HUB_ID] AS [TARGET_ID],");
                createStatement.AppendLine(" hub.[SCHEMA_NAME] AS [TARGET_SCHEMA_NAME],");
                createStatement.AppendLine(" [HUB_NAME] AS [TARGET_NAME],");
                createStatement.AppendLine(" hub.[BUSINESS_KEY] AS [TARGET_BUSINESS_KEY_DEFINITION],");
                createStatement.AppendLine(" 'Hub' AS [TARGET_TYPE],");
                createStatement.AppendLine(" [SURROGATE_KEY],");
                createStatement.AppendLine(" [FILTER_CRITERIA],");
                createStatement.AppendLine(" xref.[LOAD_VECTOR]");
                createStatement.AppendLine("FROM [MD_SOURCE_HUB_XREF] xref");
                createStatement.AppendLine("JOIN [MD_SOURCE] stg ON xref.[SOURCE_ID] = stg.[SOURCE_ID]");
                createStatement.AppendLine("JOIN [MD_HUB] hub ON xref.HUB_ID = hub.[HUB_ID]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 95);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_SOURCE_LINK_XREF]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine("  xref.[SOURCE_ID]");
                createStatement.AppendLine(" ,stg.[SOURCE_NAME]");
                createStatement.AppendLine(" ,xref.[LINK_ID]");
                createStatement.AppendLine(" ,lnk.[LINK_NAME]");
                createStatement.AppendLine(" ,[FILTER_CRITERIA]");
                createStatement.AppendLine(" ,xref.[LOAD_VECTOR]");
                createStatement.AppendLine("FROM[MD_SOURCE_LINK_XREF] xref");
                createStatement.AppendLine("JOIN MD_SOURCE stg ON xref.SOURCE_ID = stg.SOURCE_ID");
                createStatement.AppendLine("JOIN MD_LINK lnk ON xref.LINK_ID = lnk.LINK_ID");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 97);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_SOURCE_SATELLITE_ATTRIBUTE_XREF]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine("  xref.[SOURCE_ID]");
                createStatement.AppendLine(" ,stg.SOURCE_NAME");
                createStatement.AppendLine(" ,stg.SCHEMA_NAME AS SOURCE_SCHEMA_NAME");
                createStatement.AppendLine(" ,xref.[SATELLITE_ID]");
                createStatement.AppendLine(" ,[SATELLITE_NAME]");
                createStatement.AppendLine(" ,[ATTRIBUTE_ID_FROM] AS SOURCE_ATTRIBUTE_ID");
                createStatement.AppendLine(" ,att_from.[ATTRIBUTE_NAME] AS SOURCE_ATTRIBUTE_NAME");
                createStatement.AppendLine(" ,[ATTRIBUTE_ID_TO] AS SATELLITE_ATTRIBUTE_ID");
                createStatement.AppendLine(" ,UPPER(att_to.[ATTRIBUTE_NAME]) AS SATELLITE_ATTRIBUTE_NAME");
                createStatement.AppendLine(" ,[MULTI_ACTIVE_KEY_INDICATOR]");
                createStatement.AppendLine("FROM [MD_SOURCE_SATELLITE_ATTRIBUTE_XREF] xref");
                createStatement.AppendLine("JOIN MD_SOURCE stg ON xref.SOURCE_ID = stg.SOURCE_ID");
                createStatement.AppendLine("JOIN MD_SATELLITE sat ON xref.SATELLITE_ID = sat.SATELLITE_ID");
                createStatement.AppendLine("JOIN MD_ATTRIBUTE att_from ON xref.ATTRIBUTE_ID_FROM = att_from.ATTRIBUTE_ID");
                createStatement.AppendLine("JOIN MD_ATTRIBUTE att_to ON xref.ATTRIBUTE_ID_TO = att_to.ATTRIBUTE_ID");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 98);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_SOURCE_LINK_ATTRIBUTE_XREF]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine("  xref.[SOURCE_ID]");
                createStatement.AppendLine(" ,stg.SOURCE_NAME");
                createStatement.AppendLine(" ,stg.SCHEMA_NAME AS SOURCE_SCHEMA_NAME");
                createStatement.AppendLine(" ,xref.[LINK_ID]");
                createStatement.AppendLine(" ,[LINK_NAME]");
                createStatement.AppendLine(" ,[ATTRIBUTE_ID_FROM] AS SOURCE_ATTRIBUTE_ID");
                createStatement.AppendLine(" , att_from.[ATTRIBUTE_NAME] AS SOURCE_ATTRIBUTE_NAME");
                createStatement.AppendLine(" ,[ATTRIBUTE_ID_TO] AS LINK_ATTRIBUTE_ID");
                createStatement.AppendLine(" , UPPER(att_to.[ATTRIBUTE_NAME]) AS LINK_ATTRIBUTE_NAME");
                createStatement.AppendLine("FROM MD_SOURCE_LINK_ATTRIBUTE_XREF xref");
                createStatement.AppendLine("JOIN MD_SOURCE stg ON xref.SOURCE_ID = stg.SOURCE_ID");
                createStatement.AppendLine("JOIN MD_LINK lnk ON xref.LINK_ID = lnk.LINK_ID");
                createStatement.AppendLine("JOIN MD_ATTRIBUTE att_from ON xref.ATTRIBUTE_ID_FROM = att_from.ATTRIBUTE_ID");
                createStatement.AppendLine("JOIN MD_ATTRIBUTE att_to ON xref.ATTRIBUTE_ID_TO = att_to.ATTRIBUTE_ID");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 98);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_PHYSICAL_MODEL]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine("  [DATABASE_NAME]");
                createStatement.AppendLine(" ,[SCHEMA_NAME]");
                createStatement.AppendLine(" ,[TABLE_NAME]");
                createStatement.AppendLine(" ,[COLUMN_NAME]");
                createStatement.AppendLine(" ,[DATA_TYPE]");
                createStatement.AppendLine(" ,[CHARACTER_MAXIMUM_LENGTH]");
                createStatement.AppendLine(" ,[NUMERIC_PRECISION]");
                createStatement.AppendLine(" ,[ORDINAL_POSITION]");
                createStatement.AppendLine(" ,[PRIMARY_KEY_INDICATOR]");
                createStatement.AppendLine("FROM [MD_PHYSICAL_MODEL]");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 99);
                createStatement.Clear();

                createStatement.AppendLine("CREATE VIEW [interface].[INTERFACE_SOURCE_SATELLITE_XREF]");
                createStatement.AppendLine("AS");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine(" xref.SOURCE_ID,");
                createStatement.AppendLine(" stg.[SCHEMA_NAME] AS SOURCE_SCHEMA_NAME,");
                createStatement.AppendLine(" SOURCE_NAME,");
                createStatement.AppendLine(" xref.BUSINESS_KEY_DEFINITION AS SOURCE_BUSINESS_KEY_DEFINITION,");
                createStatement.AppendLine(" xref.SATELLITE_ID AS[TARGET_ID],");
                createStatement.AppendLine(" sat.[SCHEMA_NAME] AS[TARGET_SCHEMA_NAME],");
                createStatement.AppendLine(" sat.SATELLITE_NAME AS[TARGET_NAME],");
                createStatement.AppendLine(" NULL AS[TARGET_BUSINESS_KEY_DEFINITION],");
                createStatement.AppendLine(" sat.SATELLITE_TYPE AS[TARGET_TYPE],");
                createStatement.AppendLine(" hub.SURROGATE_KEY,");
                createStatement.AppendLine(" xref.FILTER_CRITERIA,");
                createStatement.AppendLine(" xref.[LOAD_VECTOR]");
                createStatement.AppendLine("FROM MD_SOURCE_SATELLITE_XREF xref");
                createStatement.AppendLine("JOIN MD_SOURCE stg ON xref.SOURCE_ID = stg.SOURCE_ID");
                createStatement.AppendLine("JOIN MD_SATELLITE sat ON xref.SATELLITE_ID = sat.SATELLITE_ID");
                createStatement.AppendLine("JOIN MD_HUB hub ON sat.HUB_ID = hub.HUB_ID");
                createStatement.AppendLine("LEFT JOIN MD_SOURCE_HUB_XREF stghubxref");
                createStatement.AppendLine("  ON xref.SOURCE_ID = stghubxref.SOURCE_ID");
                createStatement.AppendLine("  AND hub.HUB_ID = stghubxref.HUB_ID");
                createStatement.AppendLine("  AND xref.BUSINESS_KEY_DEFINITION = stghubxref.BUSINESS_KEY_DEFINITION");
                createStatement.AppendLine("WHERE sat.SATELLITE_TYPE= 'Normal'");
                createStatement.AppendLine("");
                createStatement.AppendLine("UNION");
                createStatement.AppendLine("");
                createStatement.AppendLine("SELECT");
                createStatement.AppendLine(" xref.SOURCE_ID,");
                createStatement.AppendLine(" stg.[SCHEMA_NAME] AS SOURCE_SCHEMA_NAME,");
                createStatement.AppendLine(" SOURCE_NAME,");
                createStatement.AppendLine(" xref.BUSINESS_KEY_DEFINITION AS SOURCE_BUSINESS_KEY_DEFINITION,"); 
                createStatement.AppendLine(" xref.SATELLITE_ID AS [TARGET_ID],");
                createStatement.AppendLine(" sat.[SCHEMA_NAME] AS [TARGET_SCHEMA_NAME],");
                createStatement.AppendLine(" sat.SATELLITE_NAME AS [TARGET_NAME],");
                createStatement.AppendLine(" NULL AS [TARGET_BUSINESS_KEY_DEFINITION],");
                createStatement.AppendLine(" sat.SATELLITE_TYPE AS [TARGET_TYPE],");
                createStatement.AppendLine(" lnk.SURROGATE_KEY,");
                createStatement.AppendLine(" xref.FILTER_CRITERIA,");
                createStatement.AppendLine(" xref.[LOAD_VECTOR]");
                createStatement.AppendLine("FROM MD_SOURCE_SATELLITE_XREF xref");
                createStatement.AppendLine("JOIN MD_SOURCE stg ON xref.SOURCE_ID = stg.SOURCE_ID");
                createStatement.AppendLine("JOIN MD_SATELLITE sat ON xref.SATELLITE_ID = sat.SATELLITE_ID");
                createStatement.AppendLine("JOIN MD_HUB hub ON sat.HUB_ID = hub.HUB_ID");
                createStatement.AppendLine("JOIN MD_LINK lnk ON sat.LINK_ID = lnk.LINK_ID");
                createStatement.AppendLine("LEFT JOIN MD_SOURCE_HUB_XREF stghubxref");
                createStatement.AppendLine("  ON xref.SOURCE_ID = stghubxref.SOURCE_ID");
                createStatement.AppendLine("  AND hub.HUB_ID = stghubxref.HUB_ID");
                createStatement.AppendLine("  AND xref.BUSINESS_KEY_DEFINITION = stghubxref.BUSINESS_KEY_DEFINITION");
                createStatement.AppendLine("WHERE sat.SATELLITE_TYPE= 'Link Satellite'");
                createStatement.AppendLine();
                RunSqlCommandRepositoryForm(connOmdString, createStatement, worker, 100);
                createStatement.Clear();
                
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
        }

        private void backgroundWorkerSampleData_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            ErrorHandlingParameters.ErrorCatcher = 0;

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

                    string dwhKeyName = "";
                    string psaPrefixName = "";

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
                            createStatement.AppendLine("TRUNCATE TABLE [MD_VERSION_ATTRIBUTE];");
                            createStatement.AppendLine("TRUNCATE TABLE [MD_VERSION];");
                            createStatement.AppendLine();
                            RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                            createStatement.Clear();

                            // Staging Layer
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUST_MEMBERSHIP', N'COMPOSITE(CustomerID; Plan_Code)', N'PSA_PROFILER_CUST_MEMBERSHIP', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'CUST_MEMBERSHIP', N'COMPOSITE(CustomerID; Plan_Code)', N'STG_PROFILER_CUST_MEMBERSHIP', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUSTOMER_OFFER', N'COMPOSITE(CustomerID; OfferID)', N'PSA_PROFILER_CUSTOMER_OFFER', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'CUSTOMER_OFFER', N'COMPOSITE(CustomerID; OfferID)', N'STG_PROFILER_CUSTOMER_OFFER', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'CustomerID', N'PSA_PROFILER_CUSTOMER_PERSONAL', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'CUSTOMER_PERSONAL', N'CustomerID', N'STG_PROFILER_CUSTOMER_PERSONAL', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_ESTIMATED_WORTH', N'COMPOSITE(Plan_Code; Date_effective)', N'PSA_PROFILER_ESTIMATED_WORTH', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'ESTIMATED_WORTH', N'COMPOSITE(Plan_Code; Date_effective)', N'STG_PROFILER_ESTIMATED_WORTH', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_OFFER', N'OfferID', N'PSA_PROFILER_OFFER', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'OFFER', N'OfferID', N'STG_PROFILER_OFFER', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PERSONALISED_COSTING', N'COMPOSITE(Member; Segment; Plan_Code; Date_effective)', N'PSA_PROFILER_PERSONALISED_COSTING', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'PERSONALISED_COSTING', N'COMPOSITE(Member; Segment; Plan_Code; Date_effective)', N'STG_PROFILER_PERSONALISED_COSTING', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PLAN', N'Plan_Code', N'PSA_PROFILER_PLAN', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'PLAN', N'Plan_Code', N'STG_PROFILER_PLAN', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_USERMANAGED_SEGMENT', N'Demographic_Segment_Code', N'PSA_USERMANAGED_SEGMENT', NULL, NULL, 'Y');");

                            // Integration Layer
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_ESTIMATED_WORTH', N'COMPOSITE(Plan_Code;''XYZ'')', N'HUB_MEMBERSHIP_PLAN', N'12=12', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUST_MEMBERSHIP', N'COMPOSITE(Plan_Code;''XYZ'')', N'HUB_MEMBERSHIP_PLAN', N'14=14', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUSTOMER_OFFER', N'CustomerID, OfferID', N'LNK_CUSTOMER_OFFER', N'7=7', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PERSONALISED_COSTING', N'Member', N'HUB_CUSTOMER', N'', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PLAN', N'COMPOSITE(Plan_Code;''XYZ'')', N'HUB_MEMBERSHIP_PLAN', N'10=10', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PERSONALISED_COSTING', N'CONCATENATE(Segment;''TEST'')', N'HUB_SEGMENT', N'', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_USERMANAGED_SEGMENT', N'CONCATENATE(Demographic_Segment_Code;''TEST'')', N'SAT_SEGMENT', N'9=9', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUSTOMER_OFFER', N'CustomerID, OfferID', N'LSAT_CUSTOMER_OFFER', N'7=7', 'CustomerID', 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUST_MEMBERSHIP', N'CustomerID', N'HUB_CUSTOMER', N'15=15', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUST_MEMBERSHIP', N'CustomerID, COMPOSITE(Plan_Code;''XYZ'')', N'LNK_MEMBERSHIP', N'16=16', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PLAN', N'COMPOSITE(Plan_Code;''XYZ'')', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'11=11', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUSTOMER_OFFER', N'CustomerID', N'HUB_CUSTOMER', N'5=5', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_OFFER', N'OfferID', N'HUB_INCENTIVE_OFFER', N'3=3', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PERSONALISED_COSTING', N'COMPOSITE(Plan_Code;''XYZ'')', N'HUB_MEMBERSHIP_PLAN', N'18=18', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_OFFER', N'OfferID', N'SAT_INCENTIVE_OFFER', N'4=4', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'CustomerID', N'HUB_CUSTOMER', N'1=1', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'CustomerID', N'SAT_CUSTOMER', N'2=2', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'CustomerID', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', NULL, NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_USERMANAGED_SEGMENT', N'CONCATENATE(Demographic_Segment_Code;''TEST'')', N'HUB_SEGMENT', N'8=8', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PERSONALISED_COSTING', N'COMPOSITE(Plan_Code;''XYZ''), Member, CONCATENATE(Segment;''TEST'')', N'LSAT_CUSTOMER_COSTING', N'', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUSTOMER_OFFER', N'OfferID', N'HUB_INCENTIVE_OFFER', N'6=6', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PERSONALISED_COSTING', N'COMPOSITE(Plan_Code;''XYZ''), Member, CONCATENATE(Segment;''TEST'')', N'LNK_CUSTOMER_COSTING', N'', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_ESTIMATED_WORTH', N'COMPOSITE(Plan_Code;''XYZ'')', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'13=13', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_CUST_MEMBERSHIP', N'CustomerID, COMPOSITE(Plan_Code;''XYZ'')', N'LSAT_MEMBERSHIP', N'17=17', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PLAN', N'COMPOSITE(Plan_Code;''XYZ''),COMPOSITE(Renewal_Plan_Code;''XYZ'')', N'LNK_RENEWAL_MEMBERSHIP', N'1=1', NULL, 'Y');");
                            createStatement.AppendLine("INSERT[dbo].[MD_TABLE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [BUSINESS_KEY_ATTRIBUTE], [TARGET_TABLE], [FILTER_CRITERIA], [DRIVING_KEY_ATTRIBUTE], [PROCESS_INDICATOR]) VALUES(0, N'STG_PROFILER_PLAN', N'COMPOSITE(Renewal_Plan_Code;''XYZ'')', N'HUB_MEMBERSHIP_PLAN', N'1=1', NULL, 'Y');");
                            createStatement.AppendLine();
                            RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
                            createStatement.Clear();

                            // Attribute mapping details
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_PERSONALISED_COSTING', N'Monthly_Cost', N'LSAT_CUSTOMER_COSTING', N'PERSONAL_MONTHLY_COST', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_PERSONALISED_COSTING', N'Date_effective', N'LSAT_CUSTOMER_COSTING', N'COSTING_EFFECTIVE_DATE', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUST_MEMBERSHIP', N'Status', N'LNK_MEMBERSHIP', N'SALES_CHANNEL', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUST_MEMBERSHIP', N'End_Date', N'LSAT_MEMBERSHIP', N'MEMBERSHIP_END_DATE', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_ESTIMATED_WORTH', N'Date_effective', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'PLAN_VALUATION_DATE', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_ESTIMATED_WORTH', N'Value_Amount', N'SAT_MEMBERSHIP_PLAN_VALUATION', N'PLAN_VALUATION_AMOUNT', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_PLAN', N'Plan_Desc', N'SAT_MEMBERSHIP_PLAN_DETAIL', N'PLAN_DESCRIPTION', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_USERMANAGED_SEGMENT', N'Demographic_Segment_Description', N'SAT_SEGMENT', N'SEGMENT_DESCRIPTION', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_OFFER', N'Offer_Long_Description', N'SAT_INCENTIVE_OFFER', N'OFFER_DESCRIPTION', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUST_MEMBERSHIP', N'Start_Date', N'LSAT_MEMBERSHIP', N'MEMBERSHIP_START_DATE', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUST_MEMBERSHIP', N'Status', N'LSAT_MEMBERSHIP', N'MEMBERSHIP_STATUS', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'Contact_Number', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'CONTACT_NUMBER', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'State', N'SAT_CUSTOMER_ADDITIONAL_DETAILS', N'STATE', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'DOB', N'SAT_CUSTOMER', N'DATE_OF_BIRTH', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'Surname', N'SAT_CUSTOMER', N'SURNAME', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'Gender', N'SAT_CUSTOMER', N'GENDER', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'Given', N'SAT_CUSTOMER', N'GIVEN_NAME', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'Country', N'SAT_CUSTOMER', N'COUNTRY', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'Referee_Offer_Made', N'SAT_CUSTOMER', N'REFERRAL_OFFER_MADE_INDICATOR', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'Suburb', N'SAT_CUSTOMER', N'SUBURB', N'');");
                            createStatement.AppendLine(
                                "INSERT[dbo].[MD_ATTRIBUTE_MAPPING] ([VERSION_ID], [SOURCE_TABLE], [SOURCE_COLUMN], [TARGET_TABLE], [TARGET_COLUMN], [TRANSFORMATION_RULE]) VALUES(0, N'STG_PROFILER_CUSTOMER_PERSONAL', N'Postcode', N'SAT_CUSTOMER', N'POSTCODE', N'');");
                            createStatement.AppendLine();
                            RunSqlCommandSampleDataForm(connString, createStatement, worker, 5);
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
                backgroundWorkerSampleData.RunWorkerAsync();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetStandardConfigurationSettings();
        }


    }
}

