using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataWarehouseAutomation;
using Microsoft.Data.SqlClient;
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
        /// <param name="teamConfiguration"></param>
        /// <param name="sourceOrTarget"></param>
        /// <returns></returns>
        public static DataObject CreateDataObject(string dataObjectName, TeamConnection teamConnection, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration, string sourceOrTarget = "Source")
        {
            // Initialise the object and set the name.
            DataObject localDataObject = new DataObject {name = dataObjectName};
            
            // Add Connection to Data Object, if allowed.
            localDataObject = SetDataObjectConnection(localDataObject, teamConnection, jsonExportSetting);
            
            // Connection information as extensions. Only allowed if a Connection is added to the Data Object.
            localDataObject = SetDataObjectDatabaseExtension(localDataObject, teamConnection, jsonExportSetting);
            localDataObject = SetDataObjectSchemaExtension(localDataObject, teamConnection, jsonExportSetting);

            // Add classifications.
            localDataObject = SetDataObjectTypeClassification(localDataObject, jsonExportSetting);

            // Only add if setting is enabled.
            if (jsonExportSetting.GenerateDataObjectDataItems == "True")
            {
                var fullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(dataObjectName, teamConnection).FirstOrDefault();

                SqlConnection metadataConnection = new SqlConnection
                {
                    ConnectionString = teamConfiguration.MetadataConnection.CreateSqlServerConnectionString(false)
                };

                var physicalModelDataTable = MetadataHandling.GetPhysicalModelDataTable(metadataConnection);

                DataRow[] dataItemRows = physicalModelDataTable.Select("[TABLE_NAME] = '" + fullyQualifiedName.Value + "' " + "AND [SCHEMA_NAME] = '" + fullyQualifiedName.Key + "' " + "AND [DATABASE_NAME] = '" + teamConnection.DatabaseServer.DatabaseName + "'");

                var sortedDataItemRows = dataItemRows.OrderBy(dr => dr["ORDINAL_POSITION"]);

                List<dynamic> dataItems = new List<dynamic>();

                foreach (var dataItemRow in sortedDataItemRows)
                {
                    DataItem dataItem = new DataItem { name = (string)dataItemRow["COLUMN_NAME"] };

                    // Only add Data Type details if the setting is enabled.
                    if (sourceOrTarget == "Source" && jsonExportSetting.GenerateDataItemDataTypes == "True" || sourceOrTarget == "Target" && jsonExportSetting.GenerateDataItemDataTypes == "True")
                    {
                        MetadataHandling.PrepareDataItemDataType(dataItem, dataItemRow);
                    }

                    dataItems.Add(dataItem);
                }

                localDataObject.dataItems = dataItems;
            }

            return localDataObject;
        }

        /// <summary>
        /// Add a Connection to a Data Object based on the TeamConnection details.
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
                var localDataConnection = new DataConnection {dataConnectionString = teamConnection.ConnectionKey};

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
            if (jsonExportSetting.GenerateDatabaseAsExtension == "True" && jsonExportSetting.GenerateDataObjectConnection == "True" && dataObject.dataObjectConnection != null)
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
            if (jsonExportSetting.GenerateSchemaAsExtension == "True" && jsonExportSetting.GenerateDataObjectConnection == "True" && dataObject.dataObjectConnection != null)
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
        
        public static List<DataObject> SetLineageRelatedDataObjectList(DataTable dataObjectMappingDataTable, string targetDataObjectName, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration)
        {
            List<DataObject> dataObjectList = new List<DataObject>();

            if (jsonExportSetting.AddUpstreamDataObjectsAsRelatedDataObject == "True")
            {
                // Find the corresponding row in the Data Object Mapping grid
                DataRow[] dataObjectMappings = dataObjectMappingDataTable.Select("[" + TableMappingMetadataColumns.SourceDataObject + "] = '" + targetDataObjectName + "'");

                foreach (DataRow dataObjectMapping in dataObjectMappings)
                {
                    var localDataObjectName = dataObjectMapping[TableMappingMetadataColumns.TargetDataObject.ToString()].ToString();
                    var localDataObjectConnectionInternalId = dataObjectMapping[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();

                    TeamConnection localConnection = FormBase.GetTeamConnectionByConnectionId(localDataObjectConnectionInternalId);

                    // Set the name and further settings.
                    dataObjectList.Add(CreateDataObject(localDataObjectName, localConnection, jsonExportSetting, teamConfiguration));
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

                if (dataObject.dataObjectClassifications is null)
                {
                    dataObject.dataObjectClassifications = localClassifications;
                }
                else
                {
                    dataObject.dataObjectClassifications.AddRange(localClassifications);
                }

            }

            return dataObject;
        }

        /// <summary>
        /// Create the special-type metadata Data Object.
        /// </summary>
        /// <param name="metaDataConnection"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static DataObject CreateMetadataDataObject(TeamConnection metaDataConnection, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration)
        {
            DataObject localDataObject = new DataObject();

            if (jsonExportSetting.AddMetadataAsRelatedDataObject == "True")
            {
                localDataObject = CreateDataObject("Metadata", metaDataConnection, jsonExportSetting, teamConfiguration);

                // Override classification
                if (jsonExportSetting.GenerateTypeAsClassification == "True")
                {
                    localDataObject.dataObjectClassifications[0].classification = "Metadata";
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
                
                targetColumn.name = indexExists ? targetBusinessKeyComponentList[counter] : "";

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

            var sourceBusinessKeyComponentList = new List<string>();

            foreach (var keyComponent in temporaryBusinessKeyComponentList)
            {
                var keyPart = keyComponent.Replace("(", "").Replace(")", "").Replace(" ", "");

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
