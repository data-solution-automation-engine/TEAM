using System.Collections.Generic;
using System.Linq;
using DataWarehouseAutomation;
using TEAM_Library;
using Extension = DataWarehouseAutomation.Extension;

namespace TEAM
{
    /// <summary>
    /// Manages the output in Json conform the schema for Data Warehouse Automation.
    /// </summary>
    class JsonOutputHandling
    {
        /// <summary>
        /// Updates an input DataObject with a Database and Schema extension (two key/value pairs) based on its connection properties (TeamConnection object).
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="teamConnection"></param>
        /// <returns></returns>
        public static DataObject AddDataObjectExtensionsForDatabase(DataObject dataObject, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            // Add dataObjectConnection, including connection string (to Data Object)
            var localDataConnection = new DataConnection();
            localDataConnection.dataConnectionString = teamConnection.ConnectionKey;

            List<Extension> extensions = new List<Extension>();

            // Database Extension
            if (jsonExportSetting.GenerateDatabaseAsExtension == "True")
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
            if (jsonExportSetting.GenerateSchemaAsExtension == "True")
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

        /// <summary>
        /// Returns a list of Data Item Mappings for a source- and target Business Key definition.
        /// The Business Keys will be split into components and mapped in order.
        /// </summary>
        /// <param name="sourceBusinessKeyDefinition"></param>
        /// <param name="targetBusinessKeyDefinition"></param>
        /// <returns></returns>
        public static List<DataItemMapping> GetBusinessKeyComponentDataItemMappings(string sourceBusinessKeyDefinition, string targetBusinessKeyDefinition)
        {
            // Set the return type
            List<DataItemMapping> returnList = new List<DataItemMapping>();

            // Evaluate key components for source and target key definitions
            var sourceBusinessKeyComponentList = GetBusinessKeyComponentList(sourceBusinessKeyDefinition);
            var targetBusinessKeyComponentList = GetBusinessKeyComponentList(targetBusinessKeyDefinition);

            int counter = 0;

            // Only the source can be a hard-coded value, as it is mapped to a target.
            foreach (string keyPart in sourceBusinessKeyComponentList)
            {
                // Is set to true if there are quotes in the key part.

                DataItemMapping keyComponent = new DataItemMapping();

                List<dynamic> sourceColumns = new List<dynamic>();

                DataItem sourceColumn = new DataItem();
                DataItem targetColumn = new DataItem();

                sourceColumn.name = keyPart;
                sourceColumn.isHardCodedValue = keyPart.StartsWith("'") && keyPart.EndsWith("'");

                sourceColumns.Add(sourceColumn);

                keyComponent.sourceDataItems = sourceColumns;

                var indexExists = targetBusinessKeyComponentList.ElementAtOrDefault(counter) != null;
                if (indexExists)
                {
                    targetColumn.name = targetBusinessKeyComponentList[counter];
                }
                else
                {
                    targetColumn.name = "";
                }

                keyComponent.targetDataItem = targetColumn;

                returnList.Add(keyComponent);
                counter++;
            }

            return returnList;
        }

        /// <summary>
        /// Returns the list of components for a given input Business Key string value.
        /// </summary>
        /// <param name="sourceBusinessKeyDefinition"></param>
        /// <returns></returns>
        private static List<string> GetBusinessKeyComponentList(string sourceBusinessKeyDefinition)
        {
            var temporaryBusinessKeyComponentList = sourceBusinessKeyDefinition.Split(',').ToList();

            List<string> sourceBusinessKeyComponentList = new List<string>();

            foreach (var keyComponent in temporaryBusinessKeyComponentList)
            {
                var keyPart = keyComponent.TrimStart().TrimEnd();
                keyPart = keyComponent.Replace("(", "").Replace(")", "").Replace(" ", "");

                if (keyPart.StartsWith("COMPOSITE"))
                {
                    keyPart = keyPart.Replace("COMPOSITE", "");

                    var temporaryKeyPartList = keyPart.Split(';').ToList();
                    foreach (var item in temporaryKeyPartList)
                    {
                        sourceBusinessKeyComponentList.Add(item);
                    }
                }
                else if (keyPart.StartsWith("CONCATENATE"))
                {
                    keyPart = keyPart.Replace("CONCATENATE", "");
                    keyPart = keyPart.Replace(";", "+");

                    sourceBusinessKeyComponentList.Add(keyPart);
                }
                else
                {
                    sourceBusinessKeyComponentList.Add(keyPart);
                }
            }

            sourceBusinessKeyComponentList = sourceBusinessKeyComponentList.Select(t => t.Trim()).ToList();
            return sourceBusinessKeyComponentList;
        }
    }
}
