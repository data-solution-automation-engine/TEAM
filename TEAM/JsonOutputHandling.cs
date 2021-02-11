using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataWarehouseAutomation;
using TEAM_Library;
using Extension = DataWarehouseAutomation.Extension;

namespace TEAM
{
    /// <summary>
    /// Manages the output in Json conform the schema for Data Warehouse Automation.
    /// </summary>
    internal static class JsonOutputHandling
    {
        /// <summary>
        /// Create Data Object in TEAM.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="teamConnection"></param>
        /// <param name="jsonExportSetting"></param>
        /// <returns></returns>
        public static DataObject CreateDataObject(string dataObjectName, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            DataObject localDataObject = new DataObject();

            localDataObject.name = dataObjectName;

            // Data Object Connection
            localDataObject = SetDataObjectConnection(localDataObject, teamConnection, jsonExportSetting);
            
            // Source and target connection information as extensions
            localDataObject = SetDataObjectDatabaseExtension(localDataObject, teamConnection, jsonExportSetting);
            localDataObject = SetDataObjectSchemaExtension(localDataObject, teamConnection, jsonExportSetting);

            // Add classifications
            localDataObject = SetDataObjectTypeClassification(localDataObject, jsonExportSetting);
            
            return localDataObject;
        }

        /// <summary>
        /// Add a Data Object Connection based on the TeamConnection details.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="teamConnection"></param>
        /// <param name="jsonExportSetting"></param>
        /// <returns></returns>
        public static DataObject SetDataObjectConnection(DataObject dataObject, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            if (jsonExportSetting.GenerateDataObjectConnection == "True")
            {
                // Add dataObjectConnection, including connection string (to Data Object).
                var localDataConnection = new DataConnection();
                localDataConnection.dataConnectionString = teamConnection.ConnectionKey;

                dataObject.dataObjectConnection = localDataConnection;
            }

            return dataObject;
        }

        /// <summary>
        /// Updates an input DataObject with a Database and Schema extension (two key/value pairs) based on its connection properties (TeamConnection object).
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="teamConnection"></param>
        /// <param name="jsonExportSetting"></param>
        /// <returns></returns>
        public static DataObject SetDataObjectDatabaseExtension(DataObject dataObject, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            if (jsonExportSetting.GenerateDatabaseAsExtension == "True" && dataObject.dataObjectConnection != null)
            {
                List<Extension> extensions = new List<Extension>();
                
                var extension = new Extension
                {
                    key = "database",
                    value = teamConnection.DatabaseServer.DatabaseName,
                    description = "database name"
                };

                extensions.Add(extension);

                if (dataObject.dataObjectConnection.extensions is null)
                {
                    dataObject.dataObjectConnection.extensions = extensions;
                }
                else
                {
                    dataObject.dataObjectConnection.extensions.AddRange(extensions);
                }
            }

            return dataObject;
        }

        /// <summary>
        /// Updates an input DataObject with a Database and Schema extension (two key/value pairs) based on its connection properties (TeamConnection object).
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="teamConnection"></param>
        /// <param name="jsonExportSetting"></param>
        /// <returns></returns>
        public static DataObject SetDataObjectSchemaExtension(DataObject dataObject, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            if (jsonExportSetting.GenerateSchemaAsExtension == "True" && dataObject.dataObjectConnection != null)
            {
                List<Extension> extensions = new List<Extension>();

                var extension = new Extension
                {
                    key = "schema",
                    value = teamConnection.DatabaseServer.SchemaName,
                    description = "schema name"
                };

                extensions.Add(extension);

                if (dataObject.dataObjectConnection.extensions is null)
                {
                    dataObject.dataObjectConnection.extensions = extensions;
                }
                else
                {
                    dataObject.dataObjectConnection.extensions.AddRange(extensions);
                }
            }

            return dataObject;
        }
        
        public static List<DataObject> SetLineageRelatedDataObjectList(DataTable dataObjectMappingDataTable, string targetDataObjectName, JsonExportSetting jsonExportSetting)
        {
            List<DataObject> dataObjectList = new List<DataObject>();

            if (jsonExportSetting.AddUpstreamDataObjectsAsRelatedDataObject == "True")
            {
                // Find the corresponding row in the Data Object Mapping grid
                DataRow[] DataObjectMappings = dataObjectMappingDataTable.Select("[" + TableMappingMetadataColumns.SourceTable + "] = '" + targetDataObjectName + "'");

                foreach (DataRow DataObjectMapping in DataObjectMappings)
                {
                    var localDataObjectName = DataObjectMapping[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                    var localDataObjectConnectionInternalId = DataObjectMapping[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();

                    TeamConnection localConnection = FormBase.GetTeamConnectionByConnectionId(localDataObjectConnectionInternalId);

                    // Set the name and further settings.
                    dataObjectList.Add(CreateDataObject(localDataObjectName, localConnection, jsonExportSetting));
                }
            }

            return dataObjectList;
        }

        public static DataObject SetDataObjectTypeClassification(DataObject dataObject, JsonExportSetting jsonExportSetting)
        {
            if (jsonExportSetting.GenerateTypeAsClassification == "True")
            {
                List<Classification> localClassifications = new List<Classification>();
                Classification localClassification = new Classification();
                
                var tableType = MetadataHandling.GetDataObjectType(dataObject.name, "", FormBase.TeamConfiguration);
                localClassification.classification = tableType.ToString();

                localClassifications.Add(localClassification);

                if (dataObject.dataObjectClassification is null)
                {
                    dataObject.dataObjectClassification = localClassifications;
                }
                else
                {
                    dataObject.dataObjectClassification.AddRange(localClassifications);
                }

            }

            return dataObject;
        }

        /// <summary>
        /// Create the special-type metadata Data Object.
        /// </summary>
        /// <param name="metaDataConnection"></param>
        /// <param name="jsonExportSetting"></param>
        /// <returns></returns>
        public static DataObject CreateMetadataDataObject(TeamConnection metaDataConnection, JsonExportSetting jsonExportSetting)
        {
            DataObject localDataObject = new DataObject();

            if (jsonExportSetting.AddMetadataAsRelatedDataObject == "True")
            {
                localDataObject = CreateDataObject("Metadata", metaDataConnection, jsonExportSetting);

                // Override classification
                if (jsonExportSetting.GenerateTypeAsClassification == "True")
                {
                    localDataObject.dataObjectClassification[0].classification = "Metadata";
                }
            }


            return localDataObject;
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
