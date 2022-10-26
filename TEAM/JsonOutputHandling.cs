using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DataWarehouseAutomation;
using Microsoft.Data.SqlClient;
using TEAM_Library;
using static TEAM.FormBase;
using static TEAM_Library.MetadataHandling;
using DataObject = DataWarehouseAutomation.DataObject;
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
            localDataObject = SetDataObjectConnectionDatabaseExtension(localDataObject, teamConnection, jsonExportSetting);
            localDataObject = SetDataObjectConnectionSchemaExtension(localDataObject, teamConnection, jsonExportSetting);

            // Add classifications.
            if (dataObjectName == "Metadata")
            {
                localDataObject = SetDataObjectTypeClassification(localDataObject, jsonExportSetting, "Metadata");
            }
            else
            {
                localDataObject = SetDataObjectTypeClassification(localDataObject, jsonExportSetting);
            }

            // Only add if setting is enabled.
            if (jsonExportSetting.AddDataObjectDataItems == "True")
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
                    if (sourceOrTarget == "Source" && jsonExportSetting.AddDataItemDataTypes == "True" || sourceOrTarget == "Target" && jsonExportSetting.AddDataItemDataTypes == "True")
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
        /// Add a classification at data object mapping level, derived from the target data object.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        internal static List<Classification> AddMappingClassifications(string dataObjectName, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration)
        {
            var tableType = MetadataHandling.GetDataObjectType(dataObjectName, "", teamConfiguration);

            List<Classification> dataObjectsMappingClassifications = new List<Classification>();
            var dataObjectMappingClassification = new Classification
            {
                classification = tableType.ToString()
            };

            dataObjectsMappingClassifications.Add(dataObjectMappingClassification);

            return dataObjectsMappingClassifications;
        }

        internal static List<DataObject> AddRelatedDataObjects(string dataObjectName, DataGridView dataObjectMappingGrid, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration)
        {
            List<DataObject> relatedDataObjects = new List<DataObject>();

            #region Add metadata connection as related data object

            // Add the metadata connection as related data object (assuming this is set in the json export settings).
            if (jsonExportSetting.IsAddMetadataAsRelatedDataObject())
            {
                var metaDataObject = GetMetadataDataObject(teamConfiguration, jsonExportSetting);

                if (metaDataObject.name != null)
                {
                    relatedDataObjects.Add(metaDataObject);
                }
            }
            #endregion

            #region Add related data object(s)
            if (jsonExportSetting.IsAddRelatedDataObjectsAsRelatedDataObject())
            {
                relatedDataObjects.AddRange(GetLineageRelatedDataObjectList(dataObjectName, dataObjectMappingGrid, jsonExportSetting, teamConfiguration));
            }
            #endregion

            return relatedDataObjects;
        }

        public static void GetFullSourceDataItemPresentation(DataItem dataItem, string dataObjectName, TeamConnection teamConnection, DataGridView physicalModelGridView, DataGridViewRow row, JsonExportSetting jsonExportSetting, string sourceOrTarget = "Regular")
        {
            if (jsonExportSetting.IsAddDataItemDataTypes())
            {
                var tableSchema = MetadataHandling.GetFullyQualifiedDataObjectName(dataObjectName, teamConnection);

                var columnNameFilter = "";
                if (sourceOrTarget == "Source")
                {
                    columnNameFilter = MetadataHandling.QuoteStringValuesForAttributes(row.Cells[DataItemMappingGridColumns.SourceDataObject.ToString()].Value.ToString());
                }
                else if (sourceOrTarget == "Target")
                {
                    columnNameFilter = MetadataHandling.QuoteStringValuesForAttributes(row.Cells[DataItemMappingGridColumns.TargetDataObject.ToString()].Value.ToString());
                }

                //foreach (DataGridViewRow row in physicalModelGridView.Rows)
                //{
                //    if (row.Cells[PhysicalModelMappingMetadataColumns.TableName.ToString()].Value.ToString() == tableSchema.Values.FirstOrDefault() &&
                //        row.Cells[PhysicalModelMappingMetadataColumns.SchemaName.ToString()].Value.ToString() == tableSchema.Keys.FirstOrDefault() &&
                //        row.Cells[PhysicalModelMappingMetadataColumns.ColumnName.ToString()].Value.ToString() == columnNameFilter &&
                //        row.Cells[PhysicalModelMappingMetadataColumns.DatabaseName.ToString()].Value.ToString() == teamConnection.DatabaseServer.DatabaseName)
                //    {
                //        rowList.Add(row);
                //    }
                //}

                //dataItemMappingRow.Cells[DataItemMappingMetadataColumns.SourceDataObject.ToString()].Value.ToString();

                foreach (DataGridViewRow physicalModelRow in physicalModelGridView.Rows)
                {
                    if (!physicalModelRow.IsNewRow)
                    {
                        var tableName = physicalModelRow.Cells[PhysicalModelMappingMetadataColumns.Table_Name.ToString()].Value.ToString();
                        var schemaName = physicalModelRow.Cells[PhysicalModelMappingMetadataColumns.Schema_Name.ToString()].Value.ToString();
                        var databaseName = physicalModelRow.Cells[PhysicalModelMappingMetadataColumns.Database_Name.ToString()].Value.ToString();
                        var columnName = physicalModelRow.Cells[PhysicalModelMappingMetadataColumns.Column_Name.ToString()].Value.ToString();

                        if (tableName == tableSchema.Values.FirstOrDefault() &&
                            schemaName == tableSchema.Keys.FirstOrDefault() &&
                            databaseName == teamConnection.DatabaseServer.DatabaseName &&
                            columnName == columnNameFilter)
                        {
                            var dataType = physicalModelRow.Cells[PhysicalModelMappingMetadataColumns.Data_Type.ToString()].Value.ToString();
                            dataItem.dataType = dataType;

                            switch (dataType)
                            {
                                case "varchar":
                                case "nvarchar":
                                case "binary":
                                    dataItem.characterLength = (int)physicalModelRow.Cells[PhysicalModelMappingMetadataColumns.Character_Length.ToString()].Value;
                                    break;
                                case "numeric":
                                    dataItem.numericPrecision = (int)physicalModelRow.Cells[PhysicalModelMappingMetadataColumns.Numeric_Precision.ToString()].Value;
                                    dataItem.numericScale = (int)physicalModelRow.Cells[PhysicalModelMappingMetadataColumns.Numeric_Scale.ToString()].Value;
                                    break;
                                case "int":
                                    // No length etc.
                                    break;
                                case "datetime":
                                case "datetime2":
                                case "date":
                                    dataItem.numericScale = (int)physicalModelRow.Cells[PhysicalModelMappingMetadataColumns.Numeric_Scale.ToString()].Value;
                                    break;
                            }

                            dataItem.ordinalPosition = (int)physicalModelRow.Cells[PhysicalModelMappingMetadataColumns.Ordinal_Position.ToString()].Value;
                        }
                    }

                    //DataGridViewRow test = physicalModelGridView.Rows
                    //    .Cast<DataGridViewRow>()
                    //    .Where(x => !x.IsNewRow)
                    //    .Where(x => ((DataRowView)x.DataBoundItem).Row.Field<string>(PhysicalModelMappingMetadataColumns.TableName.ToString()) == tableSchema.Values.FirstOrDefault())
                    //    .Where(x => ((DataRowView)x.DataBoundItem).Row.Field<string>(PhysicalModelMappingMetadataColumns.SchemaName.ToString()) == tableSchema.Keys.FirstOrDefault())
                    //    .Where(x => ((DataRowView)x.DataBoundItem).Row.Field<string>(PhysicalModelMappingMetadataColumns.ColumnName.ToString()) == columnNameFilter)
                    //    .FirstOrDefault(x => ((DataRowView)x.DataBoundItem).Row.Field<string>(PhysicalModelMappingMetadataColumns.DatabaseName.ToString()) == teamConnection.DatabaseServer.DatabaseName);


                    //DataRow physicalModelRow = physicalModelDataTable.Select($"[TABLE_NAME] = '" + tableSchema.Values.FirstOrDefault() + "' " +
                    //                                                         "AND [SCHEMA_NAME] = '" + tableSchema.Keys.FirstOrDefault() + "' " +
                    //                                                         "AND [COLUMN_NAME] = '" + columnNameFilter + "' " +
                    //                                                         "AND [DATABASE_NAME] = '" + teamConnection.DatabaseServer.DatabaseName + "'" +
                    //                                                         "").FirstOrDefault();

                }
            }
        }
       
        /// <summary>
        /// Extension method to infer the target path for a given string value (should be a target data object name).
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetMetadataFilePath(this string fileName)
        {
            return GlobalParameters.MetadataPath + fileName + ".json";
        }

        public static List<DataObject> GetLineageRelatedDataObjectList(string targetDataObject, DataGridView dataObjectDataGrid, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration)
        {
            List<DataObject> dataObjectList = new List<DataObject>();

            if (jsonExportSetting.AddRelatedDataObjectsAsRelatedDataObject == "True")
            {
                // Find the corresponding row in the Data Object Mapping grid
                //DataGridViewRow[] dataObjectMappings = dataObjectDataGrid.Select("[" + DataObjectMappingGridColumns.SourceDataObject + "] = '" + TargetDataObject + "'");

                var dataObjectMappings = dataObjectDataGrid.Rows.Cast<DataGridViewRow>()
                    .Where(x => !x.IsNewRow)
                    .Where(x => ((DataRowView)x.DataBoundItem).Row.Field<DataObject>(DataObjectMappingGridColumns.SourceDataObject.ToString()).name == targetDataObject).ToList();


                foreach (DataGridViewRow row in dataObjectMappings)
                {
                    var localDataObjectName = row.Cells[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].Value.ToString();
                    var localDataObjectConnectionInternalId = row.Cells[DataObjectMappingGridColumns.TargetConnection.ToString()].Value.ToString();

                    TeamConnection localConnection = FormBase.GetTeamConnectionByConnectionId(localDataObjectConnectionInternalId);

                    // Set the name and further settings.
                    dataObjectList.Add(CreateDataObject(localDataObjectName, localConnection, jsonExportSetting, teamConfiguration));
                }
            }

            return dataObjectList;
        }

        public static List<DataObject> GetLineageRelatedDataObjectList(DataTable dataObjectMappingDataTable, string TargetDataObject, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration)
        {
            List<DataObject> dataObjectList = new List<DataObject>();

            if (jsonExportSetting.AddRelatedDataObjectsAsRelatedDataObject == "True")
            {
                // Find the corresponding row in the Data Object Mapping grid
                DataRow[] dataObjectMappings = dataObjectMappingDataTable.Select("[" + DataObjectMappingGridColumns.SourceDataObject + "] = '" + TargetDataObject + "'");

                foreach (DataRow dataObjectMapping in dataObjectMappings)
                {
                    var localDataObjectName = dataObjectMapping[DataObjectMappingGridColumns.TargetDataObject.ToString()].ToString();
                    var localDataObjectConnectionInternalId = dataObjectMapping[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();

                    TeamConnection localConnection = FormBase.GetTeamConnectionByConnectionId(localDataObjectConnectionInternalId);

                    // Set the name and further settings.
                    dataObjectList.Add(CreateDataObject(localDataObjectName, localConnection, jsonExportSetting, teamConfiguration));
                }
            }

            return dataObjectList;
        }


        /// <summary>
        /// Adds the parent Data Object as a property to the Data Item. This is sometimes needed to produce fully qualified names to the Data Items in a Data Item Mapping.
        /// Only applies to Data Items that are part of a Data Item mapping.
        /// </summary>
        /// <param name="dataItem"></param>
        /// <param name="dataObject"></param>
        /// <param name="jsonExportSetting"></param>
        internal static void SetParentDataObjectToDataItem(DataItem dataItem, DataObject dataObject, JsonExportSetting jsonExportSetting)
        {
            if (jsonExportSetting.IsAddParentDataObject())
            {
                // Create a separate smaller data object to avoid any circular dependencies when assigning the Data Object to the Data Item.
                var localDataObject = new DataObject
                {
                    name = dataObject.name
                };

                if (dataObject.dataObjectClassifications != null && dataObject.dataObjectClassifications.Count > 0)
                {
                    localDataObject.dataObjectClassifications = dataObject.dataObjectClassifications;
                }

                if (dataObject.dataObjectConnection != null && !string.IsNullOrEmpty(dataObject.dataObjectConnection.dataConnectionString))
                {
                    localDataObject.dataObjectConnection = dataObject.dataObjectConnection;
                }

                // Add the Data Object to the Data Item.
                dataItem.dataObject = localDataObject;
            }
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
            if (jsonExportSetting.IsAddDataObjectConnection())
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
        public static DataObject SetDataObjectConnectionDatabaseExtension(DataObject dataObject, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            // Remove an existing classification, if indeed existing.
            // If no classifications exists, do nothing. Otherwise check if one needs removal.
            if (!jsonExportSetting.IsAddDatabaseAsExtension())
            {
                if (dataObject.dataObjectConnection.extensions != null)
                {
                    List<Extension> localExtensions = new List<Extension>();

                    foreach (var extension in dataObject.dataObjectConnection.extensions)
                    {
                        if (extension.key != "database")
                        {
                            localExtensions.Add(extension);
                        }
                    }

                    // If there's any left, re-add them. Otherwise set to empty.
                    if (localExtensions.Count > 0)
                    {
                        dataObject.dataObjectConnection.extensions = localExtensions;
                    }
                    else
                    {
                        dataObject.dataObjectConnection.extensions = null;
                    }
                }
            }
            // Otherwise, if the setting is enabled, add the extension if it does not yet exist already.
            else if (jsonExportSetting.IsAddDatabaseAsExtension() && jsonExportSetting.IsAddDataObjectConnection() && dataObject.dataObjectConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();
                
                // Copy any existing classifications already in place, if any.
                if (dataObject.dataObjectConnection.extensions != null)
                {
                    localExtensions = dataObject.dataObjectConnection.extensions;
                }

                // Check if this particular classification already exists before adding.
                bool extensionExists = false;
                foreach (var extension  in localExtensions)
                {
                    if (extension.key == "database")
                    {
                        extensionExists = true;
                    }
                }

                if (extensionExists == false)
                {
                    var localExtension = new Extension
                    {
                        key = "database",
                        value = teamConnection.DatabaseServer.DatabaseName,
                        description = "database name"
                    };

                    localExtensions.Add(localExtension);
                }

                dataObject.dataObjectConnection.extensions = localExtensions;
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
        public static DataObject SetDataObjectConnectionSchemaExtension(DataObject dataObject, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            if (jsonExportSetting.AddSchemaAsExtension == "True" && jsonExportSetting.AddDataObjectConnection == "True" && dataObject.dataObjectConnection != null)
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

        /// <summary>
        /// Updates an input DataObject with a classification based on its type, evaluated by its name against defined conventions.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="classificationOverrideValue"></param>
        /// <returns></returns>
        public static DataObject SetDataObjectTypeClassification(DataObject dataObject, JsonExportSetting jsonExportSetting, string classificationOverrideValue=null)
        {
            var dataObjectType = "";

            if (classificationOverrideValue != null)
            {
                dataObjectType = classificationOverrideValue;
            }
            else
            {
                dataObjectType = MetadataHandling.GetDataObjectType(dataObject.name, "", FormBase.TeamConfiguration).ToString();
            }

            if (!jsonExportSetting.IsAddTypeAsClassification())
            {
                // Remove an existing classification, if indeed existing.
                // If no classifications exists, do nothing. Otherwise check if one needs removal.
                if (dataObject.dataObjectClassifications != null)
                {
                    List<Classification> localClassifications = new List<Classification>();

                    foreach (var classification in dataObject.dataObjectClassifications)
                    {
                        if (classification.classification != dataObjectType)
                        {
                            localClassifications.Add(classification);
                        }
                    }

                    // If there's any left, re-add them. Otherwise set to empty.
                    if (localClassifications.Count > 0)
                    {
                        dataObject.dataObjectClassifications = localClassifications;
                    }
                    else
                    {
                        dataObject.dataObjectClassifications = null;
                    }
                }
            }
            else
            {
                List<Classification> localClassifications = new List<Classification>();

                // Copy any existing classifications already in place, if any.
                if (dataObject.dataObjectClassifications != null)
                {
                    localClassifications = dataObject.dataObjectClassifications;
                }

                Classification localClassification = new Classification();

                localClassification.classification = dataObjectType;

                // Check if this particular classification already exists before adding.
                bool classificationExists = false;
                foreach (var classification in localClassifications)
                {
                    if (classification.classification == dataObjectType)
                    {
                        classificationExists = true;
                    }
                }

                if (classificationExists == false)
                {
                    localClassifications.Add(localClassification);
                }

                dataObject.dataObjectClassifications = localClassifications;
            }

            return dataObject;
        }

        /// <summary>
        /// Creates the special-type metadata Data Object;
        /// </summary>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static DataObject GetMetadataDataObject(TeamConfiguration teamConfiguration, JsonExportSetting jsonExportSetting)
        {
            DataObject localDataObject = new DataObject();

            if (jsonExportSetting.AddMetadataAsRelatedDataObject == "True")
            {
                localDataObject = CreateDataObject("Metadata", teamConfiguration.MetadataConnection, jsonExportSetting, teamConfiguration);
            }

            return localDataObject;
        }
        
        /// <summary>
        /// Returns a list of Data Item Mappings for a Business Key definition.
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
