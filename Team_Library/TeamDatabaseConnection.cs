using System;
using System.Collections.Generic;
using System.Text;

namespace TEAM
{
    public class TeamDatabaseConnection
    {
        public string databaseName { get; set; }

        public string schemaName { get; set; }
        public string serverName { get; set; }
        public ServerAuthenticationTypes authenticationType { get; set; }
        public string namedUserName { get; set; }
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
        SSPI
    }

    public class TeamConnectionProfile
    {
        public string databaseConnectionName { get; set; }
        public string databaseConnectionKey { get; set; }

        public TeamDatabaseConnection databaseServer { get; set; }

        public string CreateConnectionString(bool mask, bool SSPI, bool namedUser)
        {
            // Initialise the return variable
            var outputConnectionString = "";

            var localDatabaseConnection = databaseServer;

            var connectionString = new StringBuilder();

            connectionString.Append("Server=" + localDatabaseConnection.serverName + ";");
            connectionString.Append("Initial Catalog=" + localDatabaseConnection.databaseName + ";");
            
            if (SSPI)
            {
                connectionString.Append("Integrated Security=SSPI;");
            }
            else if (namedUser)
            {
                connectionString.Append("user id=" + localDatabaseConnection.namedUserName + ";");
                connectionString.Append("password=" + localDatabaseConnection.namedUserPassword + ";");
            }

            if (localDatabaseConnection.namedUserPassword.Length > 0 && mask == true)
            {
                outputConnectionString = connectionString.ToString().Replace(localDatabaseConnection.namedUserPassword, "*****");
            }
            else
            {
                outputConnectionString = connectionString.ToString();
            }

            return outputConnectionString;
        }
    }

    public class TeamDatabaseConnectionList
    {
        public List<TeamDatabaseConnection> teamDatabaseConnectionList { get; set; }
    }

}
