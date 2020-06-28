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
        }

        public FormBase(FormMain myParent)
        {
            MyParent = myParent;
            InitializeComponent();
        }

        // TEAM configuration settings
        public static TeamConfiguration TeamConfigurationSettings { get; set; } = new TeamConfiguration();


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

            // File paths
            public static List<LoadPatternDefinition> PatternDefinitionList { get; set; }
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

            var conn = new SqlConnection { ConnectionString = TeamConfigurationSettings.MetadataConnection.CreateConnectionString(false) };
            var versionCount = new int();

            try
            {
                conn.Open();
            }
            catch (Exception)
            {
                //richTextBoxInformation.Text += exception.Message;
            }

            var sqlStatementForVersion = new StringBuilder();

            sqlStatementForVersion.AppendLine("SELECT COUNT(*) AS VERSION_COUNT");
            sqlStatementForVersion.AppendLine("FROM MD_VERSION");

            var versionList = Utility.GetDataTable(ref conn, sqlStatementForVersion.ToString());

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
