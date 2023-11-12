using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace TEAM_Library
{
    /// <summary>
    /// Possible ways to authenticate (SQL Server).
    /// </summary>
    public enum ServerAuthenticationTypes
    {
        NamedUser,
        SSPI,
        MFA,
        SSO
    }

    public enum TechnologyConnectionType
    {
        SqlServer,
        Snowflake
    }

    /// <summary>
    /// Connection types
    /// </summary>
    public enum CatalogConnectionTypes
    {
        Catalog,
        Custom
    }
    
    public class TeamConnection
    {
        public string ConnectionInternalId { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectionKey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

        public TechnologyConnectionType TechnologyConnectionType { get; set; }

        public CatalogConnectionTypes CatalogConnectionType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ConnectionNotes { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TeamDatabaseConnection DatabaseServer { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ConnectionCustomQuery { get; set; }
        
        /// <summary>
        /// Return the full TeamConnection object for a given (TeamConnection) connection Id string.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static TeamConnection GetTeamConnectionByConnectionInternalId(string connectionId, TeamConfiguration teamConfiguration, EventLog eventLog)
        {
            if (!teamConfiguration.ConnectionDictionary.TryGetValue(connectionId, out var teamConnection))
            {
                // The key isn't in the dictionary.
                eventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The connection could not be matched for Connection Id '{connectionId}'."));
            }

            return teamConnection;
        }

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

                outputConnectionString += $"Server={localDatabaseConnection.ServerName}";

                if (!string.IsNullOrEmpty(localDatabaseConnection.PortNumber))
                {
                    outputConnectionString += ("," + localDatabaseConnection.PortNumber);
                }

                if (DatabaseServer.AuthenticationType == ServerAuthenticationTypes.SSPI)
                {
                    outputConnectionString += ";TrustServerCertificate=true";
                    outputConnectionString += ";Initial Catalog=" + localDatabaseConnection.DatabaseName;
                    outputConnectionString += ";Integrated Security=SSPI";
                }
                else if (DatabaseServer.AuthenticationType == ServerAuthenticationTypes.NamedUser)
                {
                    outputConnectionString += ";TrustServerCertificate=true";
                    outputConnectionString += ";Initial Catalog=" + localDatabaseConnection.DatabaseName;
                    outputConnectionString += ";user id=" + localDatabaseConnection.NamedUserName;
                    outputConnectionString += ";password=" + localDatabaseConnection.NamedUserPassword;
                }
                else if (DatabaseServer.AuthenticationType == ServerAuthenticationTypes.MFA)
                {
                    outputConnectionString += ";Authentication=Active Directory Interactive;";
                    outputConnectionString += ";user id=" + localDatabaseConnection.MultiFactorAuthenticationUser;
                    outputConnectionString += ";Database=" + localDatabaseConnection.DatabaseName;
                }

                if (localDatabaseConnection.NamedUserPassword != null && localDatabaseConnection.NamedUserPassword.Length > 0 && mask)
                {
                    outputConnectionString = outputConnectionString.Replace(localDatabaseConnection.NamedUserPassword, "*****");
                }
            }
            else
            {
                outputConnectionString = "Server=<undefined>,<undefined>;Initial Catalog=<undefined>;user id=<>; password=<>";
            }

            // Add hard-coded timeout value.
            outputConnectionString += ";Command Timeout=800";

            return outputConnectionString;
        }

        public string CreateSnowflakeSSOConnectionString(bool mask)
        {
            // Initialise the return variable
            var outputConnectionString = "";

            if (DatabaseServer != null)
            {
                outputConnectionString = $"ACCOUNT={DatabaseServer.Account};" +
                                         $"USER={DatabaseServer.MultiFactorAuthenticationUser};" +
                                         $"DB={DatabaseServer.DatabaseName};" +
                                          "AUTHENTICATOR=externalbrowser;" +
                                         $"ROLE={DatabaseServer.Role};" +
                                         $"SCHEMA={DatabaseServer.SchemaName}";
            }
            else
            {
                outputConnectionString = "ACCOUNT=<>;" +
                                         "USER=<>;" +
                                         "DB=<>;" +
                                         "AUTHENTICATOR=externalbrowser;" +
                                         "ROLE=<>;" +
                                         "SCHEMA=<>";
            }

            return outputConnectionString;
        }

        public static TeamConnection GetTeamConnectionByConnectionKey(string connectionKey, TeamConfiguration teamConfiguration, EventLog eventLog)
        {
            TeamConnection returnTeamConnection = new TeamConnection();

            foreach (var teamConnection in teamConfiguration.ConnectionDictionary)
            {
                if (teamConnection.Value.ConnectionKey == connectionKey)
                {
                    returnTeamConnection = teamConnection.Value;
                }
            }

            if (returnTeamConnection.ConnectionInternalId.IsNullOrEmpty())
            {
                // The key isn't in the dictionary.
                eventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The connection could not be matched for Connection Key '{connectionKey}'."));
            }

            return returnTeamConnection;
        }
    }

    /// <summary>
    /// Specification of a database connection within TEAM.
    /// </summary>
    public class TeamDatabaseConnection
    {
        public string DatabaseName { get; set; }

        public string Role { get; set; } //Snowflake only

        public string Warehouse { get; set; } //Snowflake only
        public string SchemaName { get; set; }
        public string ServerName { get; set; }
        public string PortNumber { get; set; }

        public string Account { get; set; }
        public ServerAuthenticationTypes AuthenticationType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NamedUserName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string NamedUserPassword { get; set; }

        public string MultiFactorAuthenticationUser { get; set; }

        public bool IsSSPI()
        {
            var returnValue = AuthenticationType == ServerAuthenticationTypes.SSPI;

            return returnValue;
        }
        public bool IsNamedUser()
        {
            var returnValue = AuthenticationType == ServerAuthenticationTypes.NamedUser;

            return returnValue;
        }

        public bool IsMfa()
        {
            var returnValue = AuthenticationType == ServerAuthenticationTypes.MFA;

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
        public static Dictionary<string, TeamConnection> LoadConnectionFile(string connectionFileName, EventLog teamEventLog)
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

                teamEventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The connections file {connectionFileName} was loaded successfully."));
            }
            catch (Exception exception)
            {
                teamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered loading the connections file {connectionFileName}. The reported error is {exception.Message}."));
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
                CatalogConnectionType = CatalogConnectionTypes.Catalog,
                ConnectionNotes = "Default metadata repository connection."
            };

            var newTeamDatabaseConnectionMetadata = new TeamDatabaseConnection
            {
                AuthenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "900_Metadata",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfileMetadata.DatabaseServer = newTeamDatabaseConnectionMetadata;
            localDictionary.Add(newTeamConnectionProfileMetadata.ConnectionInternalId, newTeamConnectionProfileMetadata);

            // Source
            var newTeamConnectionProfileSource = new TeamConnection
            {
                ConnectionInternalId =  "SourceConnectionInternalId",
                ConnectionKey = "Source",
                ConnectionName = "Source System",
                CatalogConnectionType = CatalogConnectionTypes.Catalog,
                ConnectionNotes = "Sample source system connection."
            };

            var newTeamDatabaseConnectionSource = new TeamDatabaseConnection
            {
                AuthenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "000_Source",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfileSource.DatabaseServer = newTeamDatabaseConnectionSource;
            localDictionary.Add(newTeamConnectionProfileSource.ConnectionInternalId, newTeamConnectionProfileSource);

            // Staging
            var newTeamConnectionProfileStaging = new TeamConnection
            {
                ConnectionInternalId =  "StagingConnectionInternalId",
                ConnectionKey = "Staging",
                ConnectionName = "Staging / Landing Area",
                CatalogConnectionType = CatalogConnectionTypes.Catalog,
                ConnectionNotes = ""
            };

            var newTeamDatabaseConnectionStaging = new TeamDatabaseConnection
            {
                AuthenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "100_Staging_Area",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfileStaging.DatabaseServer = newTeamDatabaseConnectionStaging;
            localDictionary.Add(newTeamConnectionProfileStaging.ConnectionInternalId, newTeamConnectionProfileStaging);

            // PSA
            var newTeamConnectionProfilePsa = new TeamConnection
            {
                ConnectionInternalId =  "PsaConnectionInternalId",
                ConnectionKey = "PSA",
                ConnectionName = "Persistent Staging Area",
                CatalogConnectionType = CatalogConnectionTypes.Catalog,
                ConnectionNotes = ""
            };

            var newTeamDatabaseConnectionPsa = new TeamDatabaseConnection
            {
                AuthenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "150_Persistent_Staging_Area",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfilePsa.DatabaseServer = newTeamDatabaseConnectionPsa;
            localDictionary.Add(newTeamConnectionProfilePsa.ConnectionInternalId, newTeamConnectionProfilePsa);

            // Integration
            var newTeamConnectionProfileIntegration = new TeamConnection
            {
                ConnectionInternalId =  "IntegrationConnectionInternalId",
                ConnectionKey = "Integration",
                ConnectionName = "Integration Layer",
                CatalogConnectionType = CatalogConnectionTypes.Catalog,
                ConnectionNotes = ""
            };

            var newTeamDatabaseConnectionIntegration = new TeamDatabaseConnection
            {
                AuthenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "200_Integration_Layer",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfileIntegration.DatabaseServer = newTeamDatabaseConnectionIntegration;
            localDictionary.Add(newTeamConnectionProfileIntegration.ConnectionInternalId, newTeamConnectionProfileIntegration);
            
            // Integration Derived
            var newTeamConnectionProfileIntegrationDerived = new TeamConnection
            {
                ConnectionInternalId = "IntegrationConnectionInternalDerivedId",
                ConnectionKey = "Derived",
                ConnectionName = "Integration Derived",
                CatalogConnectionType = CatalogConnectionTypes.Catalog,
                ConnectionNotes = ""
            };

            var newTeamDatabaseConnectionIntegrationDerived = new TeamDatabaseConnection
            {
                AuthenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "200_Integration_Layer",
                SchemaName = "bdv",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfileIntegrationDerived.DatabaseServer = newTeamDatabaseConnectionIntegrationDerived;
            localDictionary.Add(newTeamConnectionProfileIntegrationDerived.ConnectionInternalId, newTeamConnectionProfileIntegrationDerived);
            
            // Presentation
            var newTeamConnectionProfilePresentation = new TeamConnection
            {
                ConnectionInternalId = "PresentationConnectionInternalId",
                ConnectionKey = "Presentation",
                ConnectionName = "Presentation Layer",
                CatalogConnectionType = CatalogConnectionTypes.Catalog,
                ConnectionNotes = ""
            };

            var newTeamDatabaseConnectionPresentation = new TeamDatabaseConnection
            {
                AuthenticationType = ServerAuthenticationTypes.SSPI,
                DatabaseName = "300_Presentation_Layer",
                SchemaName = "dbo",
                NamedUserName = "sa",
                NamedUserPassword = "",
                ServerName = "localhost"
            };

            newTeamConnectionProfilePresentation.DatabaseServer = newTeamDatabaseConnectionPresentation;
            localDictionary.Add(newTeamConnectionProfilePresentation.ConnectionInternalId, newTeamConnectionProfilePresentation);

            return localDictionary;
        }
    }

    public class LocalConnectionDictionary
    {
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
