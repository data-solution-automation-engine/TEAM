using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace TEAM
{
    public class TeamDatabaseConnection
    {
        public string databaseName { get; set; }
        public string schemaName { get; set; }
        public string serverName { get; set; }
        public ServerAuthenticationTypes authenticationType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string namedUserName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string namedUserPassword { get; set; }
        public bool IntegratedSecuritySelectionEvaluation()
        {
            bool returnValue = true;
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
            bool returnValue = true;
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

    public enum ServerAuthenticationTypes
    {
        NamedUser,
        SSPI,
        Undefined
    }

    public class TeamConnectionProfile
    {
        public string connectionInternalId { get; set; }
        public string databaseConnectionName { get; set; }
        public string databaseConnectionKey { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string databaseConnectionNotes { get; set; }

        public TeamDatabaseConnection databaseServer { get; set; }

        public string CreateConnectionString(bool mask)
        {
            // Initialise the return variable
            var outputConnectionString = "";

            if (databaseServer != null)
            {
                var localDatabaseConnection = databaseServer;

                var localServerName = localDatabaseConnection.serverName ?? "<>";
                var localDatabaseName = localDatabaseConnection.databaseName ?? "<>";
                var localNamedUserName = localDatabaseConnection.namedUserName ?? "<>";
                var localNamedUserPassword = localDatabaseConnection.namedUserPassword ?? "<>";

                var connectionString = new StringBuilder();

                connectionString.Append("Server=" + localServerName + ";");
                connectionString.Append("Initial Catalog=" + localDatabaseName + ";");

                if (databaseServer.authenticationType == ServerAuthenticationTypes.SSPI)
                {
                    connectionString.Append("Integrated Security=SSPI;");
                }
                else if (databaseServer.authenticationType == ServerAuthenticationTypes.NamedUser)
                {
                    connectionString.Append("user id=" + localNamedUserName + ";");
                    connectionString.Append("password=" + localNamedUserPassword + ";");
                }

                if (localNamedUserPassword != null)
                {
                    if (localNamedUserPassword.Length > 0 && mask == true)
                    {
                        outputConnectionString = connectionString.ToString()
                            .Replace(localNamedUserPassword, "*****");
                    }
                    else
                    {
                        outputConnectionString = connectionString.ToString();
                    }
                }
            }
            else
            {
                outputConnectionString = "Server=<undefined>;Initial Catalog=<undefined>;user id=<>; password=<>";
            }

            return outputConnectionString;
        }
    }

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
        public static List<LocalTeamConnection> GetConnections(Dictionary<string, TeamConnectionProfile> localConnectionDictionary)
        {
            var possibleConnections = new List<LocalTeamConnection>();
            foreach (var connection in localConnectionDictionary)
            {
                possibleConnections.Add(new LocalTeamConnection(connection.Value.databaseConnectionKey, connection.Value.connectionInternalId));

            }

            return possibleConnections;
        }

        /// <summary>
        /// List of unique connection keys derived from the TEAM connections dictionary.
        /// </summary>
        /// <returns></returns>
        public static List<string> TeamConnectionKeyList(Dictionary<string, TeamConnectionProfile> connectionDictionary)
        {
            List<string> returnList = new List<string>();

            foreach (var connection in connectionDictionary)
            {
                if (!returnList.Contains(connection.Value.connectionInternalId))
                {
                    returnList.Add(connection.Value.connectionInternalId);
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
        public static Event LoadConnectionFile(string connectionFileName, Dictionary<string, TeamConnectionProfile> connectionDictionary)
        {
            Event localEvent = new Event();

            // Load the connections
            try
            {
                // Create a new empty file if it doesn't exist.
                if (!File.Exists(connectionFileName))
                {
                    File.Create(connectionFileName).Close();

                    // Generate the sample connection dictionary and commit to memory.
                    connectionDictionary = CreateDummyConnectionDictionary();

                    // There was no key in the file for this connection, so it's new.
                    var list = new List<TeamConnectionProfile>();

                    foreach (var connection in connectionDictionary)
                    {
                        list.Add(connection.Value);
                    }

                    string output = JsonConvert.SerializeObject(list.ToArray(), Formatting.Indented);
                    File.WriteAllText(connectionFileName, output);
                }
                else
                {
                    // Clear the in-memory dictionary and load the file.
                    connectionDictionary.Clear();
                    TeamConnectionProfile[] connectionJson = JsonConvert.DeserializeObject<TeamConnectionProfile[]>(File.ReadAllText(connectionFileName));

                    foreach (var connection in connectionJson)
                    {
                       connectionDictionary.Add(connection.connectionInternalId, connection);
                    }
                }

                localEvent = Event.CreateNewEvent(EventTypes.Information, $"The connections file {connectionFileName} was loaded successfully.");
            }
            catch
            {
                localEvent = Event.CreateNewEvent(EventTypes.Error, $"An issue was encountered loading the connections file {connectionFileName}.");
            }

            return localEvent;
        }

        /// <summary>
        /// Create the sample / start dictionary of connections and commit to memory (global parameter connection dictionary).
        /// </summary>
        internal static Dictionary<string, TeamConnectionProfile> CreateDummyConnectionDictionary()
        {
            var localDictionary = new Dictionary<string, TeamConnectionProfile>();

            // Metadata
            var newTeamConnectionProfileMetadata = new TeamConnectionProfile
            {
                connectionInternalId = "Metadata",
                databaseConnectionKey = "Metadata",
                databaseConnectionName = "Metadata Repository",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionMetadata = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "900_Metadata",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfileMetadata.databaseServer = newTeamDatabaseConnectionMetadata;

            // Source
            var newTeamConnectionProfileSource = new TeamConnectionProfile
            {
                connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Source" }, "%$@"),
                databaseConnectionKey = "Source",
                databaseConnectionName = "Source System",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionSource = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "000_Source",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfileSource.databaseServer = newTeamDatabaseConnectionSource;

            // Staging
            var newTeamConnectionProfileStaging = new TeamConnectionProfile
            {
                connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Staging" }, "%$@"),
                databaseConnectionKey = "Staging",
                databaseConnectionName = "Staging / Landing Area",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionStaging = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "100_Staging_Area",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfileStaging.databaseServer = newTeamDatabaseConnectionStaging;

            // PSA
            var newTeamConnectionProfilePsa = new TeamConnectionProfile
            {
                connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "PersistentStagingArea" }, "%$@"),
                databaseConnectionKey = "PSA",
                databaseConnectionName = "Persistent Staging Area",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionPsa = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "150_Persistent_Staging_Area",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfilePsa.databaseServer = newTeamDatabaseConnectionPsa;

            // Integration
            var newTeamConnectionProfileIntegration = new TeamConnectionProfile
            {
                connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Integration" }, "%$@"),
                databaseConnectionKey = "Integration",
                databaseConnectionName = "Integration Layer",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionIntegration = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "200_Integration_Layer",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfileIntegration.databaseServer = newTeamDatabaseConnectionIntegration;

            // Presentation
            var newTeamConnectionProfilePresentation = new TeamConnectionProfile
            {
                connectionInternalId = Utility.CreateMd5(new[] { Utility.GetRandomString(100), "Presentation" }, "%$@"),
                databaseConnectionKey = "Presentation",
                databaseConnectionName = "Presentation Layer",
                databaseConnectionNotes = ""
            };

            var newTeamDatabaseConnectionPresentation = new TeamDatabaseConnection
            {
                authenticationType = ServerAuthenticationTypes.SSPI,
                databaseName = "300_Presentation_Layer",
                schemaName = "dbo",
                namedUserName = "sa",
                namedUserPassword = "",
                serverName = "localhost"
            };

            newTeamConnectionProfilePresentation.databaseServer = newTeamDatabaseConnectionPresentation;

            // Compile the dictionary
            localDictionary.Add(newTeamConnectionProfileMetadata.connectionInternalId, newTeamConnectionProfileMetadata);
            localDictionary.Add(newTeamConnectionProfileSource.connectionInternalId, newTeamConnectionProfileSource);
            localDictionary.Add(newTeamConnectionProfileStaging.connectionInternalId, newTeamConnectionProfileStaging);
            localDictionary.Add(newTeamConnectionProfilePsa.connectionInternalId, newTeamConnectionProfilePsa);
            localDictionary.Add(newTeamConnectionProfileIntegration.databaseConnectionNotes, newTeamConnectionProfileIntegration);
            localDictionary.Add(newTeamConnectionProfilePresentation.connectionInternalId, newTeamConnectionProfilePresentation);

            // Commit to memory.
            //FormBase.ConfigurationSettings.connectionDictionary = localDictionary;
            return localDictionary;
        }
    }

    public class LocalConnectionDictionary
    {
        public Dictionary<string, TeamConnectionProfile> ConnectionDictionary { get; set; }

        public string connectionKey;
        public string connectionString;

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
        public static Dictionary<string, string> GetLocalConnectionDictionary(Dictionary<string, TeamConnectionProfile> connectionDictionary)
        {
            Dictionary<string, string> returnDictionary = new Dictionary<string, string>();

            foreach (var connection in connectionDictionary)
            {
                returnDictionary.Add(connection.Value.connectionInternalId, connection.Value.CreateConnectionString(false));
            }

            return returnDictionary;
        }
    }


}
