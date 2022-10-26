using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TEAM_Library
{
    /// <summary>
    /// Allowed repository types for the metadata repository.
    /// </summary>
    public enum MetadataRepositoryStorageType
    {
        Json
    }

    public enum EnvironmentModes
    {
        PhysicalMode,
        VirtualMode
    }


    /// <summary>
    /// These settings are driven by the TEAM application.
    /// They have to be updated through TEAM, i.e. via the Team Configuration / Settings file in the designated directory.
    /// </summary>
    public class TeamConfiguration
    {
        public EventLog ConfigurationSettingsEventLog { get; set; }

        #region Connectivity (connection objects, connection strings etc.)
        /// <summary>
        /// Dictionary to contain the connection internal ID and the corresponding object.
        /// </summary>
        public Dictionary<string, TeamConnection> ConnectionDictionary { get; set; }

        /// <summary>
        /// Retrieve the connection based on the internal id.
        /// </summary>
        /// <param name="connectionInternalId"></param>
        /// <param name="connectionDictionary"></param>
        /// <returns></returns>
        public static TeamConnection GetTeamConnectionByInternalId(string connectionInternalId, Dictionary<string, TeamConnection> connectionDictionary)
        {
            connectionDictionary.TryGetValue(connectionInternalId, out var returnConnectionProfile);

            return returnConnectionProfile;
        }

        /// <summary>
        /// Dictionary to contain the available environments within TEAM.
        /// </summary>
        //public Dictionary<string, TeamWorkingEnvironment> EnvironmentDictionary { get; set; } = new Dictionary<string, TeamWorkingEnvironment>();
        public TeamConnection MetadataConnection { get; set; }
        #endregion

        #region Configuration values
        //Prefixes
        public string StgTablePrefixValue { get; set; }
        public string PsaTablePrefixValue { get; set; }
        public string HubTablePrefixValue { get; set; }
        public string SatTablePrefixValue { get; set; }
        public string LinkTablePrefixValue { get; set; }
        public string LsatTablePrefixValue { get; set; }
        public string PresentationLayerLabels { get; set; }
        public string TransformationLabels { get; set; }
        #endregion

        public string DwhKeyIdentifier { get; set; }
        public string PsaKeyLocation { get; set; }
        public string SchemaName { get; set; }

        public string EventDateTimeAttribute { get; set; }
        public string LoadDateTimeAttribute { get; set; }
        public string ExpiryDateTimeAttribute { get; set; }
        public string ChangeDataCaptureAttribute { get; set; }
        public string RecordSourceAttribute { get; set; }

        public string EtlProcessAttribute { get; set; }
        public string EtlProcessUpdateAttribute { get; set; }
        public string RowIdAttribute { get; set; }
        public string RecordChecksumAttribute { get; set; }
        public string CurrentRowAttribute { get; set; }
        public string AlternativeRecordSourceAttribute { get; set; }
        public string AlternativeLoadDateTimeAttribute { get; set; }
        public string AlternativeSatelliteLoadDateTimeAttribute { get; set; }
        public  string LogicalDeleteAttribute { get; set; }
        
        // Prefixes and suffixes
        public string TableNamingLocation { get; set; } // The location if the table classification (i.e. HUB OR SAT) is a prefix (HUB_CUSTOMER) or suffix (CUSTOMER_HUB).
        public string KeyNamingLocation { get; set; } // The location if the key (i.e. HSH or SK), whether it is a prefix (SK_CUSTOMER) or a suffix (CUSTOMER_SK).

        public string EnableAlternativeSatelliteLoadDateTimeAttribute { get; set; }
        public string EnableAlternativeRecordSourceAttribute { get; set; }
        public string EnableAlternativeLoadDateTimeAttribute { get; set; }
        public MetadataRepositoryStorageType MetadataRepositoryType { get; } = MetadataRepositoryStorageType.Json;
        public EnvironmentModes EnvironmentMode { get; set; } 

        public TeamConfiguration()
        {
            ConnectionDictionary = new Dictionary<string, TeamConnection>();
            MetadataConnection = new TeamConnection();
            ConfigurationSettingsEventLog = new EventLog();
        }

        /// <summary>
        /// Load the information from the TEAM configuration file into memory.
        /// </summary>
        public void LoadTeamConfigurationFile(string fileName)
        {
            // Load the rest of the (TEAM) configurations, from wherever they may be according to the VDW settings (the TEAM configuration file).
            ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"Retrieving TEAM configuration details from {fileName}."));

            if (File.Exists(fileName))
            {
                var configList = FileHandling.LoadConfigurationFile(fileName);

                if (configList.Count == 0)
                {
                    ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"No lines detected in file {fileName}. Is it empty?"));
                }

                string[] configurationArray = new[]
                {
                    "StagingAreaPrefix", 
                    "PersistentStagingAreaPrefix", 
                    "PresentationLayerLabels",
                    "TransformationLabels",
                    "HubTablePrefix",
                    "SatTablePrefix",
                    "LinkTablePrefix",
                    "LinkSatTablePrefix",
                    "TableNamingLocation",
                    "KeyNamingLocation",
                    "KeyIdentifier",
                    "PSAKeyLocation",
                    "SchemaName",
                    "RowID",
                    "EventDateTimeStamp",
                    "LoadDateTimeStamp",
                    "ExpiryDateTimeStamp",
                    "ChangeDataIndicator",
                    "RecordSourceAttribute",
                    "ETLProcessID",
                    "ETLUpdateProcessID",
                    "LogicalDeleteAttribute",
                    "RecordChecksum",
                    "CurrentRecordAttribute",
                    "AlternativeRecordSource",
                    "AlternativeHubLDTS",
                    "AlternativeRecordSourceFunction",
                    "AlternativeHubLDTSFunction",
                    "AlternativeSatelliteLDTSFunction",
                    "AlternativeSatelliteLDTS",
                    "EnvironmentMode"
                };

                foreach (string configuration in configurationArray)
                {
                    if (configList.ContainsKey(configuration))
                    {
                        switch (configuration)
                        {
                            case "EnvironmentMode":
                                Enum.TryParse(configList[configuration], out EnvironmentModes localEnvironmentMode);
                                EnvironmentMode = localEnvironmentMode;
                                break;
                            case "StagingAreaPrefix":
                                StgTablePrefixValue = configList[configuration];
                                break;
                            case "PersistentStagingAreaPrefix":
                                PsaTablePrefixValue = configList[configuration];
                                break;
                            case "PresentationLayerLabels":
                                PresentationLayerLabels = configList[configuration];
                                break;
                            case "TransformationLabels":
                                TransformationLabels = configList[configuration];
                                break;
                            case "HubTablePrefix":
                                HubTablePrefixValue = configList[configuration];
                                break;
                            case "SatTablePrefix":
                                SatTablePrefixValue = configList[configuration];
                                break;
                            case "LinkTablePrefix":
                                LinkTablePrefixValue = configList[configuration];
                                break;
                            case "TableNamingLocation":
                                TableNamingLocation = configList[configuration];
                                break;
                            case "LinkSatTablePrefix":
                                LsatTablePrefixValue = configList[configuration];
                                break;
                            case "KeyNamingLocation":
                                KeyNamingLocation = configList[configuration];
                                break;
                            case "KeyIdentifier":
                                DwhKeyIdentifier = configList[configuration];
                                break;
                            case "PSAKeyLocation":
                                PsaKeyLocation = configList[configuration];
                                break;
                            case "SchemaName":
                                SchemaName = configList[configuration];
                                break;
                            case "RowID":
                                RowIdAttribute = configList[configuration];
                                break;
                            case "EventDateTimeStamp":
                                EventDateTimeAttribute = configList[configuration];
                                break;
                            case "LoadDateTimeStamp":
                                LoadDateTimeAttribute = configList[configuration];
                                break;
                            case "ExpiryDateTimeStamp":
                                ExpiryDateTimeAttribute = configList[configuration];
                                break;
                            case "ChangeDataIndicator":
                                ChangeDataCaptureAttribute = configList[configuration];
                                break;
                            case "RecordSourceAttribute":
                                RecordSourceAttribute = configList[configuration];
                                break;
                            case "ETLProcessID":
                                EtlProcessAttribute = configList[configuration];
                                break;
                            case "ETLUpdateProcessID":
                                EtlProcessUpdateAttribute = configList[configuration];
                                break;
                            case "LogicalDeleteAttribute":
                                LogicalDeleteAttribute = configList[configuration];
                                break;
                            case "RecordChecksum":
                                RecordChecksumAttribute = configList[configuration];
                                break;
                            case "CurrentRecordAttribute":
                                CurrentRowAttribute = configList[configuration];
                                break;
                            case "AlternativeRecordSource":
                                AlternativeRecordSourceAttribute = configList[configuration];
                                break;
                            case "AlternativeHubLDTS":
                                AlternativeLoadDateTimeAttribute = configList[configuration];
                                break;
                            case "AlternativeRecordSourceFunction":
                                EnableAlternativeRecordSourceAttribute = configList[configuration];
                                break;
                            case "AlternativeHubLDTSFunction":
                                EnableAlternativeLoadDateTimeAttribute = configList[configuration];
                                break;
                            case "AlternativeSatelliteLDTSFunction":
                                EnableAlternativeSatelliteLoadDateTimeAttribute = configList[configuration];
                                break;
                            case "AlternativeSatelliteLDTS":
                                AlternativeSatelliteLoadDateTimeAttribute = configList[configuration];
                                break;
                            default:
                                ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"Incorrect configuration '{configuration}' encountered."));
                                break;
                        }

                        ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The entry '{configuration}' was loaded from the configuration file with value '{configList[configuration]}'."));

                    }
                    else
                    {
                        ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"* The entry '{configuration}' was not found in the configuration file. Please make sure an entry exists ({configuration}|<value>)."));
                        break;
                    }
                }

                ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"TEAM configuration updated in memory" + $"."));

                var lookUpValue = "MetadataConnectionId";
                if (configList.TryGetValue(lookUpValue, out var value))
                {
                    if (value != null)
                    {
                        if (ConnectionDictionary.Count == 0)
                        {
                            ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The connection dictionary is empty, so the value for the metadata connection cannot be set. Is the path to the connections file correct?"));
                            MetadataConnection = null;
                        }
                        else
                        {
                            MetadataConnection = ConnectionDictionary[configList["MetadataConnectionId"]];
                        }
                    }
                    else
                    {

                        MetadataConnection = null;
                    }
                }
                else 
                {
                    ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The key/value pair {lookUpValue} was not found in the configuration file."));
                }
            }
            else // No file found, report error.
            {
                ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"No valid TEAM configuration file was found. Please select a valid TEAM configuration file (settings tab => TEAM configuration file)"));
            }
        }

        /// <summary>
        /// Method to create a new configuration file with default values at the default location.
        /// Checks if the file already exists. If it does, nothing will happen.
        /// </summary>
        public void CreateDummyEnvironmentConfigurationFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                // Create a completely new file
                var initialConfigurationFile = new StringBuilder();

                initialConfigurationFile.AppendLine("/* TEAM Configuration Settings */");

                initialConfigurationFile.AppendLine("MetadataConnectionId|MetadataConnectionInternalId");

                initialConfigurationFile.AppendLine("StagingAreaPrefix|STG");
                initialConfigurationFile.AppendLine("PersistentStagingAreaPrefix|PSA");
                initialConfigurationFile.AppendLine("PresentationLayerLabels|DIM, FACT");
                initialConfigurationFile.AppendLine("TransformationLabels|BDV");
                initialConfigurationFile.AppendLine("HubTablePrefix|HUB_");
                initialConfigurationFile.AppendLine("SatTablePrefix|SAT_");
                initialConfigurationFile.AppendLine("LinkTablePrefix|LNK_");
                initialConfigurationFile.AppendLine("LinkSatTablePrefix|LSAT_");
                initialConfigurationFile.AppendLine("KeyIdentifier|_SK");
                initialConfigurationFile.AppendLine("SchemaName|dbo");
                initialConfigurationFile.AppendLine("RowID|SOURCE_ROW_ID");
                initialConfigurationFile.AppendLine("EventDateTimeStamp|EVENT_DATETIME");
                initialConfigurationFile.AppendLine("LoadDateTimeStamp|LOAD_DATETIME");
                initialConfigurationFile.AppendLine("ExpiryDateTimeStamp|LOAD_END_DATETIME");
                initialConfigurationFile.AppendLine("ChangeDataIndicator|CDC_OPERATION");
                initialConfigurationFile.AppendLine("RecordSourceAttribute|RECORD_SOURCE");
                initialConfigurationFile.AppendLine("ETLProcessID|ETL_INSERT_RUN_ID");
                initialConfigurationFile.AppendLine("ETLUpdateProcessID|ETL_UPDATE_RUN_ID");
                initialConfigurationFile.AppendLine("LogicalDeleteAttribute|DELETED_RECORD_INDICATOR");
                initialConfigurationFile.AppendLine("TableNamingLocation|Prefix");
                initialConfigurationFile.AppendLine("KeyNamingLocation|Suffix");
                initialConfigurationFile.AppendLine("RecordChecksum|HASH_FULL_RECORD");
                initialConfigurationFile.AppendLine("CurrentRecordAttribute|CURRENT_RECORD_INDICATOR");
                initialConfigurationFile.AppendLine("AlternativeRecordSource|N/A");
                initialConfigurationFile.AppendLine("AlternativeHubLDTS|N/A");
                initialConfigurationFile.AppendLine("AlternativeSatelliteLDTS|N/A");
                initialConfigurationFile.AppendLine("AlternativeRecordSourceFunction|False");
                initialConfigurationFile.AppendLine("AlternativeHubLDTSFunction|False");
                initialConfigurationFile.AppendLine("AlternativeSatelliteLDTSFunction|False");
                initialConfigurationFile.AppendLine("PSAKeyLocation|PrimaryKey"); //Can be PrimaryKey or UniqueIndex
                initialConfigurationFile.AppendLine("MetadataRepositoryType|Json");
                initialConfigurationFile.AppendLine("EnvironmentMode|PhysicalMode");

                initialConfigurationFile.AppendLine("/* End of file */");

                try
                {
                    using (var outfile = new StreamWriter(fileName))
                    {
                        outfile.Write(initialConfigurationFile.ToString());
                        outfile.Close();
                    }
                }
                catch
                {
                    MessageBox.Show($"An issue was encountered creating the new configuration file. This is usually due to insufficient privileges. Please consider starting the application as Administrator or make sure the directories exist. The file that was attempted to be created was '{fileName}'.");
                }
            }
        }
    }
}
