using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace TEAM_Library
{
    /// <summary>
    /// Possible ways to authenticate (SQL Server).
    /// </summary>
    public enum ServerAuthenticationTypes
    {
        NamedUser,
        SSPI
    }

    /// <summary>
    /// Connection types
    /// </summary>
    public enum ConnectionTypes
    {
        Database,
        File
    }
    
    public class TeamConnection
    {
        public string ConnectionInternalId { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectionKey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ConnectionTypes ConnectionType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ConnectionNotes { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TeamDatabaseConnection DatabaseServer { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TeamFileConnection FileConnection { get; set; }

        /// <summary>
        /// Generate a SQL Server connection string from available information.
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public string CreateSqlServerConnectionString(bool mask)
        {
            // Initialise the return variable
            var outputConnectionString = "";

            if (DatabaseServer != null)
            {
                var localDatabaseConnection = DatabaseServer;

                var localServerName = localDatabaseConnection.ServerName ?? "<>";
                var localPortNumber = localDatabaseConnection.PortNumber ?? "<>";
                var localDatabaseName = localDatabaseConnection.DatabaseName ?? "<>";
                var localNamedUserName = localDatabaseConnection.NamedUserName ?? "<>";
                var localNamedUserPassword = localDatabaseConnection.NamedUserPassword ?? "<>";

                var connectionString = new StringBuilder();

                connectionString.Append("Server=" + localServerName);

                if (localPortNumber != "<>" && localPortNumber != "")
                {
                    connectionString.Append("," + localPortNumber);
                }

                connectionString.Append(";Initial Catalog=" + localDatabaseName);

                if (DatabaseServer.authenticationType == ServerAuthenticationTypes.SSPI)
                {
                    connectionString.Append(";Integrated Security=SSPI");
                }
                else if (DatabaseServer.authenticationType == ServerAuthenticationTypes.NamedUser)
                {
                    connectionString.Append(";user id=" + localNamedUserName);
                    connectionString.Append(";password=" + localNamedUserPassword);
                }

                if (localNamedUserPassword.Length > 0 && mask)
                {
                    outputConnectionString = connectionString.ToString().Replace(localNamedUserPassword, "*****");
                }
                else
                {
                    outputConnectionString = connectionString.ToString();
                }
            }
            else
            {
                outputConnectionString = "Server=<undefined>,<undefined>;Initial Catalog=<undefined>;user id=<>; password=<>";
            }

            return outputConnectionString;
        }

        public static TeamConnection GetTeamConnectionByConnectionKey(string connectionKey, TeamConfiguration teamConfiguration)
        {
            TeamConnection returnTeamConnection = new TeamConnection();

            foreach (var teamConnection in teamConfiguration.ConnectionDictionary)
            {
                if (teamConnection.Value.ConnectionKey == connectionKey)
                {
                    returnTeamConnection = teamConnection.Value;
                }
            }

            return returnTeamConnection;
        }
    }

    public class TeamFileConnection
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }

    /// <summary>
    /// Specification of a database connection within TEAM.
    /// </summary>
    public class TeamDatabaseConnection
    {
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }
        public string ServerName { get; set; }
        public string PortNumber { get; set; }
        public ServerAuthenticationTypes authenticationType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NamedUserName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NamedUserPassword { get; set; }
        public bool IntegratedSecuritySelectionEvaluation()
        {
            bool returnValue;
            if (authenticationType == ServerAuthenticationTypes.SSPI)
            {
                returnValue = true;
            }
            else
            {
                returnValue = false;
            }

            return returnValue;
        }
        public bool NamedUserSecuritySelectionEvaluation()
        {
            bool returnValue;
            if (authenticationType == ServerAuthenticationTypes.NamedUser)
            {
                returnValue = true;
            }
            else
            {
                returnValue = false;
            }

            return returnValue;
        }
    }

    [Serializable]
    public class LocalTeamConnection
    {
        public string ConnectionKey { get; }
        public string ConnectionId { get; }

        public LocalTeamConnection(string connectionKey, string connectionId)
        {
            ConnectionKey = connectionKey;
            ConnectionId = connectionId;
        }

        /// <summary>
        /// Used to populate combo boxes in a key/value setting.
        /// </summary>
        /// <param name="localConnectionDictionary"></param>
        /// <returns></returns>
        public static List<LocalTeamConnection> GetConnections(Dictionary<string, TeamConnection> localConnectionDictionary)
        {
            var possibleConnections = new List<LocalTeamConnection>();
            foreach (var connection in localConnectionDictionary)
            {
                possibleConnections.Add(new LocalTeamConnection(connection.Value.ConnectionKey, connection.Value.ConnectionInternalId));

            }

            return possibleConnections;
        }

        /// <summary>
        /// List of unique connection keys derived from the TEAM connections dictionary.
        /// </summary>
        /// <returns></returns>
        public static List<string> TeamConnectionKeyList(Dictionary<string, TeamConnection> connectionDictionary)
        {
            List<string> returnList = new List<string>();

            foreach (var connection in connectionDictionary)
            {
                if (!returnList.Contains(connection.Value.ConnectionInternalId))
                {
                    returnList.Add(connection.Value.ConnectionInternalId);
                }
            }

            return returnList;
        }
    }


    public class TeamConnectionFile
    {
        /// <summary>
        /// Load in to memory (deserialise) a TEAM connection file.
        /// </summary>
        public static Dictionary<string, TeamConnection> LoadConnectionFile(string connectionFileName)
        {
            var localConnectionDictionary = new Dictionary<string, TeamConnection>();

            // Load the connections
            try
            {
                // Create a new empty file if it doesn't exist.
                if (!File.Exists(connectionFileName))
                {
                    File.Create(connectionFileName).Close();

                    // Generate the sample connection dictionary and commit to memory.
                    localConnectionDictionary = CreateDummyConnectionDictionary();

                    // There was no key in the file for this connection, so it's new.
                    var list = new List<TeamConnection>();

                    foreach (var connection in localConnectionDictionary)
                    {
                        list.Add(connection.Value);
                    }

                    string output = JsonConvert.SerializeObject(list.ToArray(), Formatting.Indented);
                    File.WriteAllText(connectionFileName, output);

                    //localEvent = Event.CreateNewEvent(EventTypes.Information, $"A new (dummy) connections file {connectionFileName} was created.");
                }
                else
                {
                    // Clear the in-memory dictionary and load the file.
                    localConnectionDictionary.Clear();
                    TeamConnection[] connectionJson = JsonConvert.DeserializeObject<TeamConnection[]>(File.ReadAllText(connectionFileName));

                    if (connectionJson != null)
                        foreach (var connection in connectionJson)
                        {
                            localConnectionDictionary.Add(connection.ConnectionInternalId, connection);
                        }
                }

                //localEvent = Event.CreateNewEvent(EventTypes.Information, $"The connections file {connectionFileName} was loaded successfully.");
            }
            catch
            {
                //localEvent = Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered loading the connections file {connectionFileName}.");
            }

            return localConnectionDictionary;
        }

        /// <summary>
        /// Create the sample / start dictionary of connections and commit to memory (global parameter connection dictionary).
        /// </summary>
        internal static Dictionary<string, TeamConnection> CreateDummyConnectionDictionary()
        {
            var localDictionary = new Dictionary<string, TeamConnection>();

            // Metadata
            var newTeamConnectionProfileMetadata = new TeamConnection
            {
                ConnectionInternalId = "MetadataConnectionInternalId",
                ConnectionKey = "Metadata",
                ConnectionName = "Metadata Repository",
                ConnectionType = ConnectionTypes.Database,
                ConnectionNotes = "Default metadata repository connection."
            };

            var newTeamDatabaseConnectionMetadata = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "900_Metadata",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfileMetadata.DatabaseServer = newTeamDatabaseConnectionMetadata;

            // Source
            var newTeamConnectionProfileSource = new TeamConnection
            {
                //connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Source" }, "%$@"),
                ConnectionInternalId =  "SourceConnectionInternalId",
                ConnectionKey = "Source",
                ConnectionName = "Source System",
                ConnectionType = ConnectionTypes.Database,
                ConnectionNotes = "Sample source system connection."
            };

            var newTeamDatabaseConnectionSource = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "000_Source",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfileSource.DatabaseServer = newTeamDatabaseConnectionSource;

            // Staging
            var newTeamConnectionProfileStaging = new TeamConnection
            {
                //connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Staging" }, "%$@"),
                ConnectionInternalId =  "StagingConnectionInternalId",
                ConnectionKey = "Staging",
                ConnectionName = "Staging / Landing Area",
                ConnectionType = ConnectionTypes.Database,
                ConnectionNotes = ""
            };

            var newTeamDatabaseConnectionStaging = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "100_Staging_Area",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfileStaging.DatabaseServer = newTeamDatabaseConnectionStaging;

            // PSA
            var newTeamConnectionProfilePsa = new TeamConnection
            {
                //connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "PersistentStagingArea" }, "%$@"),
                ConnectionInternalId =  "PsaConnectionInternalId",
                ConnectionKey = "PSA",
                ConnectionName = "Persistent Staging Area",
                ConnectionType = ConnectionTypes.Database,
                ConnectionNotes = ""
            };

            var newTeamDatabaseConnectionPsa = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "150_Persistent_Staging_Area",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfilePsa.DatabaseServer = newTeamDatabaseConnectionPsa;

            // Integration
            var newTeamConnectionProfileIntegration = new TeamConnection
            {
                //connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Integration" }, "%$@"),
                ConnectionInternalId =  "IntegrationConnectionInternalId",
                ConnectionKey = "Integration",
                ConnectionName = "Integration Layer",
                ConnectionType = ConnectionTypes.Database,
                ConnectionNotes = ""
            };

            var newTeamDatabaseConnectionIntegration = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "200_Integration_Layer",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfileIntegration.DatabaseServer = newTeamDatabaseConnectionIntegration;

            // Presentation
            var newTeamConnectionProfilePresentation = new TeamConnection
            {
                //connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Presentation" }, "%$@"),
                ConnectionInternalId = "PresentationConnectionInternalId",
                ConnectionKey = "Presentation",
                ConnectionName = "Presentation Layer",
                ConnectionType = ConnectionTypes.Database,
                ConnectionNotes = ""
            };

            var newTeamDatabaseConnectionPresentation = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "300_Presentation_Layer",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfilePresentation.DatabaseServer = newTeamDatabaseConnectionPresentation;

            // Compile the dictionary
            localDictionary.Add(newTeamConnectionProfileMetadata.ConnectionInternalId, newTeamConnectionProfileMetadata);
            localDictionary.Add(newTeamConnectionProfileSource.ConnectionInternalId, newTeamConnectionProfileSource);
            localDictionary.Add(newTeamConnectionProfileStaging.ConnectionInternalId, newTeamConnectionProfileStaging);
            localDictionary.Add(newTeamConnectionProfilePsa.ConnectionInternalId, newTeamConnectionProfilePsa);
            localDictionary.Add(newTeamConnectionProfileIntegration.ConnectionInternalId, newTeamConnectionProfileIntegration);
            localDictionary.Add(newTeamConnectionProfilePresentation.ConnectionInternalId, newTeamConnectionProfilePresentation);

            // Commit to memory.
            //FormBase.ConfigurationSettings.connectionDictionary = localDictionary;
            return localDictionary;
        }
    }

    public class LocalConnectionDictionary
    {
        //public Dictionary<string, TeamConnection> ConnectionDictionary { get; set; }

        //public string connectionKey;
        //public string connectionString;

        //public LocalConnectionDictionary(Dictionary<string, TeamConnectionProfile> localConnectionDictionary)
        //{
        //    connectionKey = localConnectionDictionary.
        //    ConnectionDictionary = localConnectionDictionary;
        //}


        /// <summary>
        /// Create a key/value pair of the database keys and the connection string.
        /// </summary>
        /// <param name="connectionDictionary"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetLocalConnectionDictionary(Dictionary<string, TeamConnection> connectionDictionary)
        {
            Dictionary<string, string> returnDictionary = new Dictionary<string, string>();

            foreach (var connection in connectionDictionary)
            {
                returnDictionary.Add(connection.Value.ConnectionInternalId, connection.Value.CreateSqlServerConnectionString(false));
            }

            return returnDictionary;
        }
    }


}
