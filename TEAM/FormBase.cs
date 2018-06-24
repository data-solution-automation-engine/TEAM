﻿using System;
using System.Collections.Generic;
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

            try
            {
                InitialiseConnections(GlobalParameters.ConfigurationPath + GlobalParameters.ConfigfileName);
            }
            catch (Exception)
            {
              //  richTextBoxInformation.AppendText("Errors occured trying to load the configuration file, the message is " + ex + ". No default values were loaded. \r\n\r\n");
            }

        }

        public static void InitialiseConnections(string chosenFile)
        {
            var configList = new Dictionary<string, string>();
            var fs = new FileStream(chosenFile, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs);

            try
            {
                string textline;
                while ((textline = sr.ReadLine()) != null)
                {
                    if (textline.IndexOf(@"/*", StringComparison.Ordinal) == -1)
                    {
                        var line = textline.Split('|');
                        configList.Add(line[0], line[1]);
                    }
                }

                sr.Close();
                fs.Close();

                var connectionStringOmd = configList["connectionStringMetadata"];
                connectionStringOmd = connectionStringOmd.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringSource = configList["connectionStringSource"];
                connectionStringSource = connectionStringSource.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringStg = configList["connectionStringStaging"];
                connectionStringStg = connectionStringStg.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringHstg = configList["connectionStringPersistentStaging"];
                connectionStringHstg = connectionStringHstg.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringInt = configList["connectionStringIntegration"];
                connectionStringInt = connectionStringInt.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                var connectionStringPres = configList["connectionStringPresentation"];
                connectionStringPres = connectionStringPres.Replace("Provider=SQLNCLI10;", "").Replace("Provider=SQLNCLI11;", "").Replace("Provider=SQLNCLI12;", "");

                // These variables are used as global vairables throughout the application
                // They will be set once after startup
                var configurationSettingObject = new ConfigurationSettings();

                configurationSettingObject.ConnectionStringSource = connectionStringSource;
                configurationSettingObject.ConnectionStringStg = connectionStringStg;
                configurationSettingObject.ConnectionStringHstg = connectionStringHstg;
                configurationSettingObject.ConnectionStringInt = connectionStringInt;
                configurationSettingObject.ConnectionStringOmd = connectionStringOmd;
                configurationSettingObject.ConnectionStringPres = connectionStringPres;

                configurationSettingObject.metadataRepositoryType = configList["metadataRepositoryType"];

                configurationSettingObject.StgTablePrefixValue = configList["StagingAreaPrefix"];
                configurationSettingObject.PsaTablePrefixValue = configList["PersistentStagingAreaPrefix"];
                configurationSettingObject.HubTablePrefixValue = configList["HubTablePrefix"];
                configurationSettingObject.SatTablePrefixValue = configList["SatTablePrefix"];
                configurationSettingObject.LinkTablePrefixValue = configList["LinkTablePrefix"];
                configurationSettingObject.LsatPrefixValue = configList["LinkSatTablePrefix"];

                configurationSettingObject.DwhKeyIdentifier = configList["KeyIdentifier"];
                configurationSettingObject.PsaKeyLocation = configList["PSAKeyLocation"];
                configurationSettingObject.TableNamingLocation = configList["TableNamingLocation"];
                configurationSettingObject.KeyNamingLocation = configList["KeyNamingLocation"];

                configurationSettingObject.SchemaName = configList["SchemaName"];
                configurationSettingObject.SourceSystemPrefix = configList["SourceSystemPrefix"];

                configurationSettingObject.EventDateTimeAttribute = configList["EventDateTimeStamp"];
                configurationSettingObject.LoadDateTimeAttribute = configList["LoadDateTimeStamp"];
                configurationSettingObject.ExpiryDateTimeAttribute = configList["ExpiryDateTimeStamp"];
                configurationSettingObject.ChangeDataCaptureAttribute = configList["ChangeDataIndicator"];
                configurationSettingObject.RecordSourceAttribute = configList["RecordSourceAttribute"];
                configurationSettingObject.EtlProcessAttribute = configList["ETLProcessID"];
                configurationSettingObject.EtlProcessUpdateAttribute = configList["ETLUpdateProcessID"];
                configurationSettingObject.RowIdAttribute = configList["RowID"];
                configurationSettingObject.RecordChecksumAttribute  = configList["RecordChecksum"];
                configurationSettingObject.CurrentRowAttribute = configList["CurrentRecordAttribute"];
                configurationSettingObject.LogicalDeleteAttribute = configList["LogicalDeleteAttribute"];

                configurationSettingObject.EnableAlternativeRecordSourceAttribute = configList["AlternativeRecordSourceFunction"];
                configurationSettingObject.AlternativeRecordSourceAttribute = configList["AlternativeRecordSource"];

                configurationSettingObject.EnableAlternativeLoadDateTimeAttribute = configList["AlternativeHubLDTSFunction"];
                configurationSettingObject.AlternativeLoadDateTimeAttribute = configList["AlternativeHubLDTS"];

                configurationSettingObject.EnableAlternativeSatelliteLoadDateTimeAttribute = configList["AlternativeSatelliteLDTSFunction"];
                configurationSettingObject.AlternativeSatelliteLoadDateTimeAttribute = configList["AlternativeSatelliteLDTS"];


                configurationSettingObject.SourceDatabaseName = configList["SourceDatabase"];
                configurationSettingObject.StagingDatabaseName = configList["StagingDatabase"];
                configurationSettingObject.PsaDatabaseName = configList["PersistentStagingDatabase"];
                configurationSettingObject.IntegrationDatabaseName = configList["IntegrationDatabase"];
                configurationSettingObject.PresentationDatabaseName = configList["PresentationDatabase"];

                configurationSettingObject.OutputPath = configList["OutputPath"];
                configurationSettingObject.LinkedServer = configList["LinkedServerName"];


            }
            catch (Exception)
            {
                // richTextBoxInformation.AppendText("\r\n\r\nAn error occured while interpreting the configuration file. The original error is: '" + ex.Message + "'");
            }
        }

        public class ConfigurationSettings
        {

            //Prefixes
            private static string _localStgPrefix;
            public string StgTablePrefixValue
            {
                get { return _localStgPrefix; }
                set { _localStgPrefix = value; }
            }

            private static string _localPsaPrefix;
            public string PsaTablePrefixValue
            {
                get { return _localPsaPrefix; }
                set { _localPsaPrefix = value; }
            }

            private static string _localHubPrefix;
            public string HubTablePrefixValue
            {
                get { return _localHubPrefix; }
                set { _localHubPrefix = value; }
            }

            private static string _localSatPrefix;
            public string SatTablePrefixValue
            {
                get { return _localSatPrefix; }
                set { _localSatPrefix = value; }
            }

            private static string _localLnkPrefix;
            public string LinkTablePrefixValue
            {
                get { return _localLnkPrefix; }
                set { _localLnkPrefix = value; }
            }

            private static string _localLsatPrefix;
            public string LsatPrefixValue
            {
                get { return _localLsatPrefix; }
                set { _localLsatPrefix = value; }
            }

            //Connection strings
            private static string _connectionStringSource;
            public string ConnectionStringSource
            {
                get { return _connectionStringSource; }
                set { _connectionStringSource = value; }
            }

            private static string _connectionStringStg;
            public string ConnectionStringStg
            {
                get { return _connectionStringStg; }
                set { _connectionStringStg = value; }
            }

            private static string _connectionStringHstg;
            public string ConnectionStringHstg
            {
                get { return _connectionStringHstg; }
                set { _connectionStringHstg = value; }
            }

            private static string _connectionStringInt;
            public string ConnectionStringInt
            {
                get { return _connectionStringInt; }
                set { _connectionStringInt = value; }
            }

            private static string _connectionStringPres;
            public string ConnectionStringPres
            {
                get { return _connectionStringPres; }
                set { _connectionStringPres = value; }
            }

            private static string _connectionStringOmd;
            public string ConnectionStringOmd
            {
                get { return _connectionStringOmd; }
                set { _connectionStringOmd = value; }
            }



            private static string _DwhKeyIdentifier;
            public string DwhKeyIdentifier
            {
                get { return _DwhKeyIdentifier; }
                set { _DwhKeyIdentifier = value; }
            }

            private static string _PsaKeyLocation;
            public string PsaKeyLocation
            {
                get { return _PsaKeyLocation; }
                set { _PsaKeyLocation = value; }
            }

            private static string _SchemaName;
            public string SchemaName
            {
                get { return _SchemaName; }
                set { _SchemaName = value; }
            }

            private static string _SourceSystemPrefix;
            public string SourceSystemPrefix
            {
                get { return _SourceSystemPrefix; }
                set { _SourceSystemPrefix = value; }
            }

            private static string _EventDateTimeAttribute;
            public string EventDateTimeAttribute
            {
                get { return _EventDateTimeAttribute; }
                set { _EventDateTimeAttribute = value; }
            }

            private static string _LoadDateTimeAttribute;
            public string LoadDateTimeAttribute
            {
                get { return _LoadDateTimeAttribute; }
                set { _LoadDateTimeAttribute = value; }
            }

            private static string _ExpiryDateTimeAttribute;
            public string ExpiryDateTimeAttribute
            {
                get { return _ExpiryDateTimeAttribute; }
                set { _ExpiryDateTimeAttribute = value; }
            }

            private static string _ChangeDataCaptureAttribute;
            public string ChangeDataCaptureAttribute
            {
                get { return _ChangeDataCaptureAttribute; }
                set { _ChangeDataCaptureAttribute = value; }
            }

            private static string _RecordSourceAttribute;
            public string RecordSourceAttribute
            {
                get { return _RecordSourceAttribute; }
                set { _RecordSourceAttribute = value; }
            }

            private static string _EtlProcessAttribute;
            public string EtlProcessAttribute
            {
                get { return _EtlProcessAttribute; }
                set { _EtlProcessAttribute = value; }
            }


            private static string _EtlProcessUpdateAttribute;
            public string EtlProcessUpdateAttribute
            {
                get { return _EtlProcessUpdateAttribute; }
                set { _EtlProcessUpdateAttribute = value; }
            }

            private static string _RowIdAttribute;
            public string RowIdAttribute
            {
                get { return _RowIdAttribute; }
                set { _RowIdAttribute = value; }
            }

            private static string _RecordChecksumAttribute;
            public string RecordChecksumAttribute
            {
                get { return _RecordChecksumAttribute; }
                set { _RecordChecksumAttribute = value; }
            }

            private static string _CurrentRowAttribute;
            public string CurrentRowAttribute
            {
                get { return _CurrentRowAttribute; }
                set { _CurrentRowAttribute = value; }
            }


            private static string _AlternativeRecordSourceAttribute;
            public string AlternativeRecordSourceAttribute
            {
                get { return _AlternativeRecordSourceAttribute; }
                set { _AlternativeRecordSourceAttribute = value; }
            }

            private static string _AlternativeLoadDateTimeAttribute;
            public string AlternativeLoadDateTimeAttribute
            {
                get { return _AlternativeLoadDateTimeAttribute; }
                set { _AlternativeLoadDateTimeAttribute = value; }
            }

            private static string _AlternativeSatelliteLoadDateTimeAttribute;
            public string AlternativeSatelliteLoadDateTimeAttribute
            {
                get { return _AlternativeSatelliteLoadDateTimeAttribute; }
                set { _AlternativeSatelliteLoadDateTimeAttribute = value; }
            }

            private static string _LogicalDeleteAttribute;
            public string LogicalDeleteAttribute
            {
                get { return _LogicalDeleteAttribute; }
                set { _LogicalDeleteAttribute = value; }
            }

            private static string _SourceDatabaseName;
            public string SourceDatabaseName
            {
                get { return _SourceDatabaseName; }
                set { _SourceDatabaseName = value; }
            }

            private static string _StagingDatabaseName;
            public string StagingDatabaseName
            {
                get { return _StagingDatabaseName; }
                set { _StagingDatabaseName = value; }
            }

            private static string _PsaDatabaseName;
            public string PsaDatabaseName
            {
                get { return _PsaDatabaseName; }
                set { _PsaDatabaseName = value; }
            }

            private static string _IntegrationDatabaseName;
            public string IntegrationDatabaseName
            {
                get { return _IntegrationDatabaseName; }
                set { _IntegrationDatabaseName = value; }
            }

            private static string _PresentationDatabaseName;
            public string PresentationDatabaseName
            {
                get { return _PresentationDatabaseName; }
                set { _PresentationDatabaseName = value; }
            }



            private static string _OutputPath;
            public string OutputPath
            {
                get { return _OutputPath; }
                set { _OutputPath = value; }
            }

            private static string _LinkedServer;
            public string LinkedServer
            {
                get { return _LinkedServer; }
                set { _LinkedServer = value; }
            }



            private static string _TableNamingLocation;
            public string TableNamingLocation
            {
                get { return _TableNamingLocation; }
                set { _TableNamingLocation = value; }
            }

            private static string _KeyNamingLocation;
            public string KeyNamingLocation
            {
                get { return _KeyNamingLocation; }
                set { _KeyNamingLocation = value; }
            }



            private static string _EnableAlternativeSatelliteLoadDateTimeAttribute;
            public string EnableAlternativeSatelliteLoadDateTimeAttribute
            {
                get { return _EnableAlternativeSatelliteLoadDateTimeAttribute; }
                set { _EnableAlternativeSatelliteLoadDateTimeAttribute = value; }
            }
            private static string _EnableAlternativeRecordSourceAttribute;
            public string EnableAlternativeRecordSourceAttribute
            {
                get { return _EnableAlternativeRecordSourceAttribute; }
                set { _EnableAlternativeRecordSourceAttribute = value; }
            }

            private static string _EnableAlternativeLoadDateTimeAttribute;
            public string EnableAlternativeLoadDateTimeAttribute
            {
                get { return _EnableAlternativeLoadDateTimeAttribute; }
                set { _EnableAlternativeLoadDateTimeAttribute = value; }
            }

            private static string _metadataRepositoryType;
            public string metadataRepositoryType
            {
                get { return _metadataRepositoryType; }
                set { _metadataRepositoryType = value; }
            }
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

        public class GlobalParameters
        {
            // These variables are used as global vairables throughout the applicatoin
            private static string _configurationLocalPath = Application.StartupPath + @"\Configuration\";
            private static string _outputLocalPath = Application.StartupPath + @"\Output\";
            private static string _fileLocalName = "Virtual_EDW_configuration.txt";
            private static string _jsonTableMappingFileName = "TEAM_Table_Mapping.json";
            private static string _jsonAttributeMappingFileName = "TEAM_Attribute_Mapping.json";

            public static string ConfigurationPath
            {
                get { return _configurationLocalPath; }
                set { _configurationLocalPath = value; }
            }

            public static string OutputPath
            {
                get { return _outputLocalPath; }
                set { _outputLocalPath = value; }
            }

            public static string ConfigfileName
            {
                get { return _fileLocalName; }
                set { _fileLocalName = value; }
            }

            public static string jsonTableMappingFileName
            {
                get { return _jsonTableMappingFileName; }
                set { _jsonTableMappingFileName = value; }
            }
            public static string jsonAttributeMappingFileName
            {
                get { return _jsonAttributeMappingFileName; }
                set { _jsonAttributeMappingFileName = value; }
            }
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
            var configurationSettings = new ConfigurationSettings();

            var connOmd = new SqlConnection { ConnectionString = configurationSettings.ConnectionStringOmd };
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