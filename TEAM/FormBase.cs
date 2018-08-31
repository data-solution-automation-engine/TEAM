using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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

        /// <summary>
        /// Generate a MD5 hash based on the string input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CreateMd5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        ///    Gets or sets the values from the most common configuration settings
        /// </summary>
        internal static class ConfigurationSettings
        {
            //Prefixes
            internal static string StgTablePrefixValue { get; set; }

            internal static string PsaTablePrefixValue { get; set; }

            internal static string HubTablePrefixValue { get; set; }

            internal static string SatTablePrefixValue { get; set; }

            internal static string LinkTablePrefixValue { get; set; }

            internal static string LsatPrefixValue { get; set; }

            //Connection strings
            internal static string ConnectionStringSource { get; set; }

            internal static string ConnectionStringStg { get; set; }

            internal static string ConnectionStringHstg { get; set; }

            internal static string ConnectionStringInt { get; set; }

            internal static string ConnectionStringPres { get; set; }

            internal static string ConnectionStringOmd { get; set; }


            internal static string DwhKeyIdentifier { get; set; }

            internal static string PsaKeyLocation { get; set; }

            internal static string SchemaName { get; set; }

            internal static string SourceSystemPrefix { get; set; }

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

            internal static string SourceDatabaseName { get; set; }

            internal static string StagingDatabaseName { get; set; }

            internal static string PsaDatabaseName { get; set; }

            internal static string IntegrationDatabaseName { get; set; }

            internal static string PresentationDatabaseName { get; set; }

            internal static string OutputPath { get; set; }

            internal static string ConfigurationPath { get; set; }

            internal static string LinkedServer { get; set; }

            internal static string TableNamingLocation { get; set; }

            internal static string KeyNamingLocation { get; set; }

            internal static string EnableAlternativeSatelliteLoadDateTimeAttribute { get; set; }

            internal static string EnableAlternativeRecordSourceAttribute { get; set; }

            internal static string EnableAlternativeLoadDateTimeAttribute { get; set; }

            internal static string MetadataRepositoryType { get; set; }

            internal static string WorkingEnvironment { get; set; } 
        }

        /// <summary>
        ///    These variables are used as global vairables throughout the applicatoin
        /// </summary>
        internal static class GlobalParameters
        {
            // TEAM core path parameters
            public static string ConfigurationPath { get; set; } = Application.StartupPath + @"\Configuration\";
            public static string OutputPath { get; set; } = Application.StartupPath + @"\Output\";

            public static string ConfigfileName { get; set; } = "TEAM_configuration";
            public static string PathfileName { get; set; } = "TEAM_Path_configuration";
            public static string FileExtension { get; set; } = ".txt";

            // Json file name parameters
            public static string JsonTableMappingFileName { get; set; } = "TEAM_Table_Mapping";
            public static string JsonAttributeMappingFileName { get; set; } = "TEAM_Attribute_Mapping";
            public static string JsonModelMetadataFileName { get; set; } = "TEAM_Model_Metadata";
            public static string JsonExtension { get; set; } = ".json";
        }


        public DataTable GetDataTable(ref SqlConnection sqlConnection, string sql)
        {
            // Pass the connection to a command object
            var sqlCommand = new SqlCommand(sql, sqlConnection);
            var sqlDataAdapter = new SqlDataAdapter { SelectCommand = sqlCommand };

            var dataTable = new DataTable();

            // Adds or refreshes rows in the DataSet to match those in the data source
            try
            {
                sqlDataAdapter.Fill(dataTable);
            }

            catch (Exception)
            {
                //  MessageBox.Show(@"SQL error: " + exception.Message + "\r\n\r\n The executed query was: " + sql + "\r\n\r\n The connection used was " + sqlConnection.ConnectionString, "An issue has been encountered", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return dataTable;

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

            var versionList = GetDataTable(ref sqlConnection, sqlStatementForVersion.ToString());

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


        public DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
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

            var versionList = GetDataTable(ref sqlConnection, sqlStatementForVersion.ToString());

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

            var versionList = GetDataTable(ref connOmd, sqlStatementForVersion.ToString());

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


        private void Form_Base_Load(object sender, EventArgs e)
        {

        }
    }
}
