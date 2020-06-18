using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        public bool integratedSecuritySelectionEvaluation()
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
        public bool namedUserSecuritySelectionEvaluation()
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

    public class LocalConnection
    {
        public string ConnectionKey { get; }

        public LocalConnection(string connectionKey)
        {
            ConnectionKey = connectionKey;
        }

        /// <summary>
        /// Used to populate combo boxes in a key/value setting.
        /// </summary>
        /// <param name="localConnectionDictionary"></param>
        /// <returns></returns>
        public static List<LocalConnection> GetConnections(Dictionary<string, TeamConnectionProfile> localConnectionDictionary)
        {
            var possibleConnections = new List<LocalConnection>();
            foreach (var connection in localConnectionDictionary)
            {
                possibleConnections.Add(new LocalConnection(connection.Value.databaseConnectionKey));

            }

            return possibleConnections;
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
                returnDictionary.Add(connection.Value.databaseConnectionKey, connection.Value.CreateConnectionString(false));
            }

            return returnDictionary;
        }
    }
}
