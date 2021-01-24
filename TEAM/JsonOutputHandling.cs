using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataWarehouseAutomation;

namespace TEAM
{
    /// <summary>
    /// Manages the output in Json conform the schema for Data Warehouse Automation.
    /// </summary>
    class JsonOutputHandling
    {
        public static DataObject AddDataObjectExtensionsForDatabase(DataObject dataObject, TeamConnection teamConnection)
        {
            // Add dataObjectConnection, including connection string (to Data Object)
            var localDataConnection = new DataConnection();
            localDataConnection.dataConnectionString = teamConnection.ConnectionKey;

            List<Extension> extensions = new List<Extension>();

            // Database Extension
            if (FormBase.JsonExportSettings.GenerateDatabaseAsExtension == "True")
            {
                var extension = new Extension
                {
                    key = "database",
                    value = teamConnection.DatabaseServer.DatabaseName,
                    description = "database name"
                };

                extensions.Add(extension);
            }

            // Schema Extension
            if (FormBase.JsonExportSettings.GenerateSchemaAsExtension == "True")
            {
                var extension = new Extension
                {
                    key = "schema",
                    value = teamConnection.DatabaseServer.SchemaName,
                    description = "schema name"
                };

                extensions.Add(extension);
            }

            // Add the (list of) extensions to the data connection.
            if (extensions.Count > 0)
            {
                localDataConnection.extensions = extensions;
            }
            else
            {
                localDataConnection.extensions = null;
            }

            // Add the data connection to the data object
            dataObject.dataObjectConnection = localDataConnection;

            return dataObject;
        }
    }
}
