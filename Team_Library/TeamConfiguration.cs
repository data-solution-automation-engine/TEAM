using System.Collections.Generic;
using System.IO;

namespace TEAM
{
    public enum MetadataRepositoryStorageType
    {
        Json
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
        public Dictionary<string, TeamConnectionProfile> ConnectionDictionary { get; set; }

        public static TeamConnectionProfile GetTeamConnectionByInternalId(string connectionInternalId, Dictionary<string, TeamConnectionProfile> connectionDictionary)
        {
            var returnConnectionProfile = new TeamConnectionProfile();

            connectionDictionary.TryGetValue(connectionInternalId, out returnConnectionProfile);

            return returnConnectionProfile;
        }

        /// <summary>
        /// Dictionary to contain the available environments within TEAM.
        /// </summary>
        //public Dictionary<string, TeamWorkingEnvironment> EnvironmentDictionary { get; set; } = new Dictionary<string, TeamWorkingEnvironment>();
        public TeamConnectionProfile MetadataConnection { get; set; }
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
        public MetadataRepositoryStorageType MetadataRepositoryType { get; set; }

        public TeamConfiguration()
        {
            ConnectionDictionary = new Dictionary<string, TeamConnectionProfile>();
            MetadataConnection = new TeamConnectionProfile();
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

                string lookUpValue;
                string value;


                lookUpValue = "MetadataConnectionId";
                if (configList.TryGetValue(lookUpValue, out value))
                {
                    if (value != null)
                    {
                        if (ConnectionDictionary.Count == 0)
                        {
                            ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Warning,
                                $"The connection dictionary is empty, so the value for the metadata connection cannot be set. Is the path to the connections file correct?"));
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
                    ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The key/value pair {lookUpValue} was not found in the configuration file"));
                }

                MetadataRepositoryType = MetadataRepositoryStorageType.Json;

                StgTablePrefixValue = configList["StagingAreaPrefix"];
                PsaTablePrefixValue = configList["PersistentStagingAreaPrefix"];
                PresentationLayerLabels = configList["PresentationLayerLabels"];
                HubTablePrefixValue = configList["HubTablePrefix"];
                SatTablePrefixValue = configList["SatTablePrefix"];
                LinkTablePrefixValue = configList["LinkTablePrefix"];
                LsatTablePrefixValue = configList["LinkSatTablePrefix"];

                TableNamingLocation = configList["TableNamingLocation"];
                KeyNamingLocation = configList["KeyNamingLocation"];

                DwhKeyIdentifier = configList["KeyIdentifier"];
                PsaKeyLocation = configList["PSAKeyLocation"];
                SchemaName = configList["SchemaName"];

                RowIdAttribute = configList["RowID"];
                EventDateTimeAttribute = configList["EventDateTimeStamp"];
                LoadDateTimeAttribute = configList["LoadDateTimeStamp"];
                ExpiryDateTimeAttribute = configList["ExpiryDateTimeStamp"];
                ChangeDataCaptureAttribute = configList["ChangeDataIndicator"];
                RecordSourceAttribute = configList["RecordSourceAttribute"];

                EtlProcessAttribute = configList["ETLProcessID"];
                EtlProcessUpdateAttribute = configList["ETLUpdateProcessID"];
                LogicalDeleteAttribute = configList["LogicalDeleteAttribute"];

                RecordChecksumAttribute = configList["RecordChecksum"];
                CurrentRowAttribute = configList["CurrentRecordAttribute"];
                AlternativeRecordSourceAttribute = configList["AlternativeRecordSource"];
                AlternativeLoadDateTimeAttribute = configList["AlternativeHubLDTS"];
                EnableAlternativeRecordSourceAttribute = configList["AlternativeRecordSourceFunction"];
                EnableAlternativeLoadDateTimeAttribute = configList["AlternativeHubLDTSFunction"];

                EnableAlternativeSatelliteLoadDateTimeAttribute = configList["AlternativeSatelliteLDTSFunction"];
                AlternativeSatelliteLoadDateTimeAttribute = configList["AlternativeSatelliteLDTS"];
            }
            else
            {
                ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"No valid TEAM configuration file was found. Please select a valid TEAM configuration file (settings tab => TEAM configuration file)"));
            }

            ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"TEAM configuration updated in memory" + $"."));
        }
    }
}
