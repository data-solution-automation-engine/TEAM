using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace TEAM
{
    public partial class FormBase : Form
    {
        protected FormMain MyParent;

        public FormBase()
        {
            InitializeComponent();
         //   GlobalParameters.TeamEventLog = new EventLog();
        }

        public FormBase(FormMain myParent)
        {
            MyParent = myParent;
            InitializeComponent();
            
        }

        /// <summary>
        /// Gets or sets the values from the most common configuration settings.
        /// </summary>
        internal static class ConfigurationSettings
        {
            #region Connectivity (connection objects, connection strings etc.)
            internal static Dictionary<string, TeamConnectionProfile> connectionDictionary { get; set; } = new Dictionary<string, TeamConnectionProfile>();
            #endregion

            internal static Dictionary<string, TeamWorkingEnvironment> environmentDictionary { get; set; } = new Dictionary<string, TeamWorkingEnvironment>();

            #region Prefixes
            //Prefixes
            internal static string StgTablePrefixValue { get; set; }
            internal static string PsaTablePrefixValue { get; set; }
            internal static string HubTablePrefixValue { get; set; }
            internal static string SatTablePrefixValue { get; set; }
            internal static string LinkTablePrefixValue { get; set; }
            internal static string LsatTablePrefixValue { get; set; }
            #endregion

            //Connection strings
            internal static string ConnectionStringSource { get; set; }
            internal static string ConnectionStringStg { get; set; }
            internal static string ConnectionStringHstg { get; set; }
            internal static string ConnectionStringInt { get; set; }
            internal static string ConnectionStringPres { get; set; }
            internal static string ConnectionStringOmd { get; set; }

            //Connection & authentication information
            internal static string MetadataSspi { get; set; }
            internal static string MetadataNamed { get; set; }
            internal static string MetadataUserName { get; set; }
            internal static string MetadataPassword { get; set; }

            internal static string PhysicalModelSspi { get; set; }
            internal static string PhysicalModelNamed { get; set; }
            internal static string PhysicalModelUserName { get; set; }
            internal static string PhysicalModelPassword { get; set; }


            internal static string DwhKeyIdentifier { get; set; }
            internal static string PsaKeyLocation { get; set; }
            internal static string SchemaName { get; set; }


            internal static string EventDateTimeAttribute { get; set; }

            internal static string LoadDateTimeAttribute { get; set; }

            internal static string ExpiryDateTimeAttribute { get; set; }

            internal static string ChangeDataCaptureAttribute { get; set; }

            internal static string RecordSourceAttribute { get; set; }

            internal static string EtlProcessAttribute { get; set; }

            internal static string EtlProcessUpdateAttribute { get; set; }

            internal static string RowIdAttribute { get; set; }

            internal static string RecordChecksumAttribute { get; set; }

            internal static string CurrentRowAttribute { get; set; }

            internal static string AlternativeRecordSourceAttribute { get; set; }

            internal static string AlternativeLoadDateTimeAttribute { get; set; }

            internal static string AlternativeSatelliteLoadDateTimeAttribute { get; set; }

            internal static string LogicalDeleteAttribute { get; set; }

            // Database names
            internal static string MetadataDatabaseName { get; set; }
            internal static string SourceDatabaseName { get; set; }
            internal static string StagingDatabaseName { get; set; }
            internal static string PsaDatabaseName { get; set; }
            internal static string IntegrationDatabaseName { get; set; }
            internal static string PresentationDatabaseName { get; set; }

            // Servers (instances)
            internal static string MetadataServerName { get; set; }
            internal static string PhysicalModelServerName { get; set; }

            // Prefixes and suffixes
            internal static string TableNamingLocation { get; set; } // The location if the table classification (i.e. HUB OR SAT) is a prefix (HUB_CUSTOMER) or suffix (CUSTOMER_HUB).
            internal static string KeyNamingLocation { get; set; } // The location if the key (i.e. HSH or SK), whether it is a prefix (SK_CUSTOMER) or a suffix (CUSTOMER_SK).

            internal static string EnableAlternativeSatelliteLoadDateTimeAttribute { get; set; }

            internal static string EnableAlternativeRecordSourceAttribute { get; set; }

            internal static string EnableAlternativeLoadDateTimeAttribute { get; set; }

            internal static string MetadataRepositoryType { get; set; }

            // File paths
            public static List<LoadPatternDefinition> patternDefinitionList { get; set; }
        }

        /// <summary>
        /// Gets or sets the values for the validation of the metadata.
        /// </summary>
        internal static class ValidationSettings
        {
            // Existence checks (in physical model or virtual representation of it)
            public static string SourceObjectExistence { get; set; }
            public static string TargetObjectExistence { get; set; }
            public static string SourceBusinessKeyExistence { get; set; }
            public static string SourceAttributeExistence { get; set; }
            public static string TargetAttributeExistence { get; set; }

            // Consistency of the unit of work
            public static string LogicalGroup { get; set; }
            public static string LinkKeyOrder { get; set; }

            // Syntax validation
            public static string BusinessKeySyntax { get; set; }
        }

        /// <summary>
        /// These parameters are used as global constants throughout the application.
        /// </summary>
        internal static class GlobalParameters
        {
            internal static EventLog TeamEventLog { get; set; } = new EventLog();
            // TEAM core path parameters
            public static string RootPath { get; } = Application.StartupPath + @"\";
            public static string ConfigurationPath { get; set; } = RootPath + @"Configuration\";
            public static string OutputPath { get; set; } = RootPath + @"Output\";

            public static string BackupPath { get; } = RootPath + @"Backup\";
            public static string ScriptPath { get; set; } = RootPath + @"Scripts\";
            public static string FilesPath { get; set; } = RootPath + @"Files\";
            internal static string LoadPatternPath { get; set; } = RootPath + @"LoadPatterns\";

            public static string ConfigFileName { get; set; } = "TEAM_configuration";
            public static string PathFileName { get; set; } = "TEAM_Path_configuration";
            public static string ValidationFileName { get; set; } = "TEAM_validation";
            public static string FileExtension { get; set; } = ".txt";
            internal static string WorkingEnvironment { get; set; } = "Development";
            public static string SandingElement { get; set; } = "^@#%7!";
            public static string DefaultSchema { get; set; } = "dbo";

            // Json file name parameters
            public static string JsonTableMappingFileName { get; } = "TEAM_Table_Mapping";
            public static string JsonAttributeMappingFileName { get; } = "TEAM_Attribute_Mapping";
            public static string JsonModelMetadataFileName { get; } = "TEAM_Model_Metadata";
            public static string JsonConnectionFileName { get; } = "TEAM_connections";
            public static string JsonEnvironmentFileName { get; } = "TEAM_environments";
            public static string JsonExtension { get;  } = ".json";

            public static string LoadPatternDefinitionFile { get; } = "loadPatternDefinition.json";

            // Version handling
            public static int CurrentVersionId { get; set; } = 0;
            public static int HighestVersionId { get; set; } = 0;
        }

        public KeyValuePair<int, int> GetVersion(int selectedVersion, SqlConnection sqlConnection)
        {
            var currentVersion = selectedVersion;
            var majorRelease = new int();
            var minorRelease = new int();

            var sqlStatementForVersion = new StringBuilder();

            sqlStatementForVersion.AppendLine("SELECT VERSION_ID, MAJOR_RELEASE_NUMBER, MINOR_RELEASE_NUMBER");
            sqlStatementForVersion.AppendLine("FROM MD_VERSION");
            sqlStatementForVersion.AppendLine("WHERE VERSION_ID = " + currentVersion);

            var versionList = Utility.GetDataTable(ref sqlConnection, sqlStatementForVersion.ToString());

            if (versionList != null)
            {
                foreach (DataRow version in versionList.Rows)
                {
                    majorRelease = (int) version["MAJOR_RELEASE_NUMBER"];
                    minorRelease = (int) version["MINOR_RELEASE_NUMBER"];
                }

                if (majorRelease.Equals(null))
                {
                    majorRelease = 0;
                }

                if (minorRelease.Equals(null))
                {
                    minorRelease = 0;
                }

                return new KeyValuePair<int, int>(majorRelease, minorRelease);
            }
            else
            {
                return new KeyValuePair<int, int>(0, 0);
            }
        }
        
        protected int GetMaxVersionId(SqlConnection sqlConnection)
        {
            var versionId = new int();

            try
            {
                sqlConnection.Open();
            }
            catch (Exception)
            {
               // richTextBoxInformation.Text += exception.Message;
            }

            var sqlStatementForVersion = new StringBuilder();

            sqlStatementForVersion.AppendLine("SELECT COALESCE(MAX(VERSION_ID),0) AS VERSION_ID");
            sqlStatementForVersion.AppendLine("FROM MD_VERSION");

            var versionList = Utility.GetDataTable(ref sqlConnection, sqlStatementForVersion.ToString());

            if (versionList!= null)
            {
                foreach (DataRow version in versionList.Rows)
                {
                    versionId = (int) version["VERSION_ID"];
                }
                return versionId;
            }
            else
            {
                return 0;
            }
         
        }

        protected int GetVersionCount()
        {
            var connOmd = new SqlConnection { ConnectionString = ConfigurationSettings.ConnectionStringOmd };
            var versionCount = new int();

            try
            {
                connOmd.Open();
            }
            catch (Exception)
            {
                //richTextBoxInformation.Text += exception.Message;
            }

            var sqlStatementForVersion = new StringBuilder();

            sqlStatementForVersion.AppendLine("SELECT COUNT(*) AS VERSION_COUNT");
            sqlStatementForVersion.AppendLine("FROM MD_VERSION");

            var versionList = Utility.GetDataTable(ref connOmd, sqlStatementForVersion.ToString());

            if (versionList != null)
            {
                if (versionList.Rows.Count == 0)
                {
                    //richTextBoxInformation.Text += "The version cannot be established.\r\n";
                }
                else
                {
                    foreach (DataRow version in versionList.Rows)
                    {
                        versionCount = (int) version["VERSION_COUNT"];
                    }
                }

                return versionCount;
            }
            else
            {
                return 0;
            }
        }

    }
}
