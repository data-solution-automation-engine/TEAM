using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DataWarehouseAutomation;
using TEAM_Library;
using static TEAM.FormBase;
using static TEAM.JsonOutputHandling;
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
            DataObject dataObject = new DataObject {name = dataObjectName};

            // Set the data object connection.
            SetDataObjectConnection(dataObject, teamConnection, jsonExportSetting);

            // Connection information as extensions. Only allowed if a Connection is added to the Data Object.
            dataObject = SetDataObjectConnectionDatabaseExtension(dataObject, teamConnection, jsonExportSetting);
            dataObject = SetDataObjectConnectionSchemaExtension(dataObject, teamConnection, jsonExportSetting);

            // Add classifications, overridden for the metadata connection.
            if (dataObjectName == "Metadata")
            {
                SetDataObjectTypeClassification(dataObject, jsonExportSetting, "Metadata");
            }
            else
            {
                SetDataObjectTypeClassification(dataObject, jsonExportSetting);
            }
            
            // Set the data items.
            SetDataObjectDataItems(dataObject, teamConnection, teamConfiguration, jsonExportSetting);

            return dataObject;
        }

        /// <summary>
        /// Add Data Items to the Data Object definition.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="jsonExportSetting"></param>
        internal static DataObject SetDataObjectDataItems(DataObject dataObject, TeamConnection teamConnection, TeamConfiguration teamConfiguration, JsonExportSetting jsonExportSetting)
        {
            // Remove the list if the setting is disabled.
            if (!jsonExportSetting.IsAddDataObjectDataItems())
            {
                dataObject.dataItems = null;
            }
            else if (jsonExportSetting.IsAddDataObjectDataItems())
            {
                var fullyQualifiedName = GetFullyQualifiedDataObjectName(dataObject.name, teamConnection).FirstOrDefault();

                List<dynamic> dataItems = new List<dynamic>();

                foreach (DataGridViewRow physicalModelGridViewRow in _dataGridViewPhysicalModel.Rows)
                {
                    if (!physicalModelGridViewRow.IsNewRow)
                    {
                        var schemaName = physicalModelGridViewRow.Cells[(int)PhysicalModelMappingMetadataColumns.Schema_Name].Value.ToString();
                        var tableName = physicalModelGridViewRow.Cells[(int)PhysicalModelMappingMetadataColumns.Table_Name].Value.ToString();

                        if (fullyQualifiedName.Key == schemaName && fullyQualifiedName.Value == tableName)
                        {
                            DataItem dataItem = new DataItem { name = physicalModelGridViewRow.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString() };

                            // Apply or remove data item details.
                            SetDataItemDataType(dataItem, physicalModelGridViewRow, jsonExportSetting);

                            dataItems.Add(dataItem);
                        }
                    }
                }

                dataObject.dataItems = dataItems;
            }

            return dataObject;
        }

        /// <summary>
        /// Add the data type details to a data item.
        /// </summary>
        /// <param name="dataItem"></param>
        /// <param name="physicalModelRow"></param>
        /// <param name="jsonExportSetting"></param>
        /// <returns></returns>
        public static DataItem SetDataItemDataType(DataItem dataItem, DataGridViewRow physicalModelRow, JsonExportSetting jsonExportSetting)
        {
            if (!jsonExportSetting.IsAddDataItemDataTypes())
            {
                // Remove any data type details.
                dataItem.characterLength = null;
                dataItem.dataType = null;
                dataItem.numericPrecision = null;
                dataItem.numericScale = null;
                dataItem.ordinalPosition = null;
                dataItem.isPrimaryKey = null;
                dataItem.isHardCodedValue = null;
            }
            else if (jsonExportSetting.IsAddDataItemDataTypes())
            {
                if (physicalModelRow != null)
                {
                    var dataType = physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Data_Type].Value.ToString();

                    dataItem.dataType = dataType;

                    switch (dataType)
                    {
                        case "varchar":
                        case "nvarchar":
                        case "binary":
                            dataItem.characterLength = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Character_Length].Value.ToString());
                            break;
                        case "numeric":
                            dataItem.numericPrecision = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Numeric_Precision].Value.ToString());
                            dataItem.numericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Numeric_Scale].Value.ToString());
                            break;
                        case "int":
                            // No length etc.
                            break;
                        case "datetime":
                        case "datetime2":
                        case "date":
                            dataItem.numericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Numeric_Scale].Value.ToString());
                            break;
                    }

                    dataItem.ordinalPosition = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Ordinal_Position].Value.ToString());
                }
            }

            return dataItem;
        }

        /// <summary>
        /// Add the data type details to a data item, used at mapping level.
        /// </summary>
        /// <param name="dataItem"></param>
        /// <param name="teamConnection"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="dataObject"></param>
        /// <returns></returns>
        public static DataItem SetDataItemMappingDataType(DataItem dataItem, DataObject dataObject, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            if (!jsonExportSetting.IsAddDataItemDataTypes())
            {
                // Remove any data type details.
                dataItem.characterLength = null;
                dataItem.dataType = null;
                dataItem.numericPrecision = null;
                dataItem.numericScale = null;
                dataItem.ordinalPosition = null;
                dataItem.isPrimaryKey = null;
                dataItem.isHardCodedValue = null;
            }
            else if (jsonExportSetting.IsAddDataItemDataTypes())
            {
                var fullyQualifiedName = GetFullyQualifiedDataObjectName(dataObject.name, teamConnection).FirstOrDefault();

                // Find the matching physical model row.
                DataGridViewRow physicalModelRow = _dataGridViewPhysicalModel.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.Schema_Name].Value.ToString().Equals(fullyQualifiedName.Key))
                    .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.Table_Name].Value.ToString().Equals(fullyQualifiedName.Value))
                    .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString().Equals(dataItem.name))
                    .First();

                if (physicalModelRow != null)
                {
                    var dataType = physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Data_Type].Value.ToString();

                    dataItem.dataType = dataType;

                    switch (dataType)
                    {
                        case "varchar":
                        case "nvarchar":
                        case "binary":
                            dataItem.characterLength = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Character_Length].Value.ToString());
                            break;
                        case "numeric":
                            dataItem.numericPrecision = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Numeric_Precision].Value.ToString());
                            dataItem.numericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Numeric_Scale].Value.ToString());
                            break;
                        case "int":
                            // No length etc.
                            break;
                        case "datetime":
                        case "datetime2":
                        case "date":
                            dataItem.numericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Numeric_Scale].Value.ToString());
                            break;
                    }

                    dataItem.ordinalPosition = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Ordinal_Position].Value.ToString());
                }
            }

            return dataItem;
        }

        /// <summary>
        /// Add a classification at data object mapping level, derived from the target data object.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        internal static List<Classification> SetMappingClassifications(string dataObjectName, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration, string drivingKeyValue)
        {
            var tableType = GetDataObjectType(dataObjectName, "", teamConfiguration);

            // Override for driving key.
            if (drivingKeyValue != null && !string.IsNullOrEmpty(drivingKeyValue))
            {
                tableType = DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey;
            }

            List<Classification> dataObjectsMappingClassifications = new List<Classification>();
            var dataObjectMappingClassification = new Classification
            {
                classification = tableType.ToString()
            };

            dataObjectsMappingClassifications.Add(dataObjectMappingClassification);

            return dataObjectsMappingClassifications;
        }

        /// <summary>
        /// Assert upstream (next layer) data objects and add them to the existing data object mapping.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="dataObjectMappingGrid"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        internal static List<DataObject> SetRelatedDataObjects(string dataObjectName, DataGridView dataObjectMappingGrid, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration)
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
    
        /// <summary>
        /// Convenience method to identify the next-layer-up data objects for a given data object (string).
        /// </summary>
        /// <param name="targetDataObject"></param>
        /// <param name="dataObjectDataGrid"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static List<DataObject> GetLineageRelatedDataObjectList(string targetDataObject, DataGridView dataObjectDataGrid, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration)
        {
            List<DataObject> dataObjectList = new List<DataObject>();

            if (jsonExportSetting.AddRelatedDataObjectsAsRelatedDataObject == "True")
            {
                var dataObjectMappings = dataObjectDataGrid.Rows.Cast<DataGridViewRow>()
                    .Where(x => !x.IsNewRow)
                    .Where(x => ((DataRowView)x.DataBoundItem).Row.Field<DataObject>(DataObjectMappingGridColumns.SourceDataObject.ToString()).name == targetDataObject).ToList();


                foreach (DataGridViewRow row in dataObjectMappings)
                {
                    var localDataObjectName = row.Cells[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].Value.ToString();
                    var localDataObjectConnectionInternalId = row.Cells[DataObjectMappingGridColumns.TargetConnection.ToString()].Value.ToString();

                    TeamConnection localConnection = GetTeamConnectionByConnectionId(localDataObjectConnectionInternalId);

                    // Set the name and further settings.
                    dataObjectList.Add(CreateDataObject(localDataObjectName, localConnection, jsonExportSetting, teamConfiguration));
                }
            }

            return dataObjectList;
        }

        /// <summary>
        /// Adds the parent Data Object as a property to the Data Item. This is sometimes needed to produce fully qualified names to the Data Items in a Data Item Mapping.
        /// *Only* applies to Data Items that are part of a Data Item mapping.
        /// </summary>
        /// <param name="dataItem"></param>
        /// <param name="dataObject"></param>
        /// <param name="jsonExportSetting"></param>
        internal static DataItem SetParentDataObjectToDataItem(DataItem dataItem, DataObject dataObject, JsonExportSetting jsonExportSetting)
        {
            // If the setting is disabled, remove the data object from the data item
            if (!jsonExportSetting.IsAddParentDataObject())
            {
                dataItem.dataObject = null;
            }
            else if (jsonExportSetting.IsAddParentDataObject())
            {
                // Create a separate, smaller / limited, data object to avoid any circular dependencies when assigning the Data Object to the Data Item.
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

            return dataItem;
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
            // Remove an existing connection, if existing.
            if (!jsonExportSetting.IsAddDataObjectConnection())
            {
                if (dataObject.dataObjectConnection != null)
                {
                    dataObject.dataObjectConnection = null;
                }
            }
            // Otherwise, if the setting is enabled, add the extension if it does not yet exist already.
            else if (jsonExportSetting.IsAddDataObjectConnection())
            {
                var dataObjectConnection = new DataConnection {dataConnectionString = teamConnection.ConnectionKey};

                dataObject.dataObjectConnection = dataObjectConnection;
            }

            return dataObject;
        }

        public static DataQuery SetDataQueryConnection(DataQuery dataQuery, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            // Remove an existing connection, if existing.
            if (!jsonExportSetting.IsAddDataObjectConnection())
            {
                if (dataQuery.dataQueryConnection != null)
                {
                    dataQuery.dataQueryConnection = null;
                }
            }
            // Otherwise, if the setting is enabled, add the extension if it does not yet exist already.
            else if (jsonExportSetting.IsAddDataObjectConnection())
            {
                var dataObjectConnection = new DataConnection { dataConnectionString = teamConnection.ConnectionKey };

                dataQuery.dataQueryConnection = dataObjectConnection;
            }

            return dataQuery;
        }

        /// <summary>
        /// Updates a DataObject Connection with a Database extension (key/value pair) based on its connection properties (TeamConnection object).
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
                if (dataObject.dataObjectConnection != null && dataObject.dataObjectConnection.extensions != null)
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

        public static DataQuery SetDataQueryConnectionDatabaseExtension(DataQuery dataQuery, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            // Remove an existing classification, if indeed existing.
            // If no classifications exists, do nothing. Otherwise check if one needs removal.
            if (!jsonExportSetting.IsAddDatabaseAsExtension())
            {
                if (dataQuery.dataQueryConnection != null && dataQuery.dataQueryConnection.extensions != null)
                {
                    List<Extension> localExtensions = new List<Extension>();

                    foreach (var extension in dataQuery.dataQueryConnection.extensions)
                    {
                        if (extension.key != "database")
                        {
                            localExtensions.Add(extension);
                        }
                    }

                    // If there's any left, re-add them. Otherwise set to empty.
                    if (localExtensions.Count > 0)
                    {
                        dataQuery.dataQueryConnection.extensions = localExtensions;
                    }
                    else
                    {
                        dataQuery.dataQueryConnection.extensions = null;
                    }
                }
            }
            // Otherwise, if the setting is enabled, add the extension if it does not yet exist already.
            else if (jsonExportSetting.IsAddDatabaseAsExtension() && jsonExportSetting.IsAddDataObjectConnection() && dataQuery.dataQueryConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();

                // Copy any existing classifications already in place, if any.
                if (dataQuery.dataQueryConnection.extensions != null)
                {
                    localExtensions = dataQuery.dataQueryConnection.extensions;
                }

                // Check if this particular classification already exists before adding.
                bool extensionExists = false;
                foreach (var extension in localExtensions)
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

                dataQuery.dataQueryConnection.extensions = localExtensions;
            }

            return dataQuery;
        }


        /// <summary>
        /// Updates a DataObject Connection with a Schema extension (key/value pair) based on its connection properties (TeamConnection object).
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="teamConnection"></param>
        /// <param name="jsonExportSetting"></param>
        /// <returns></returns>
        public static DataObject SetDataObjectConnectionSchemaExtension(DataObject dataObject, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            // Remove an existing classification, if indeed existing.
            // If no classifications exists, do nothing. Otherwise check if one needs removal.
            if (!jsonExportSetting.IsAddSchemaAsExtension())
            {
                if (dataObject.dataObjectConnection != null && dataObject.dataObjectConnection.extensions != null)
                {
                    List<Extension> localExtensions = new List<Extension>();

                    foreach (var extension in dataObject.dataObjectConnection.extensions)
                    {
                        if (extension.key != "schema")
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
            else if (jsonExportSetting.IsAddSchemaAsExtension() && jsonExportSetting.IsAddDataObjectConnection() && dataObject.dataObjectConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();

                // Copy any existing classifications already in place, if any.
                if (dataObject.dataObjectConnection.extensions != null)
                {
                    localExtensions = dataObject.dataObjectConnection.extensions;
                }

                // Check if this particular classification already exists before adding.
                bool extensionExists = false;
                foreach (var extension in localExtensions)
                {
                    if (extension.key == "schema")
                    {
                        extensionExists = true;
                    }
                }

                if (extensionExists == false)
                {
                    var localExtension = new Extension
                    {
                        key = "schema",
                        value = teamConnection.DatabaseServer.SchemaName,
                        description = "schema name"
                    };

                    localExtensions.Add(localExtension);
                }

                dataObject.dataObjectConnection.extensions = localExtensions;
            }

            return dataObject;
        }

        public static DataQuery SetDataQueryConnectionSchemaExtension(DataQuery dataQuery, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            // Remove an existing classification, if indeed existing.
            // If no classifications exists, do nothing. Otherwise check if one needs removal.
            if (!jsonExportSetting.IsAddSchemaAsExtension())
            {
                if (dataQuery.dataQueryConnection != null && dataQuery.dataQueryConnection.extensions != null)
                {
                    List<Extension> localExtensions = new List<Extension>();

                    foreach (var extension in dataQuery.dataQueryConnection.extensions)
                    {
                        if (extension.key != "schema")
                        {
                            localExtensions.Add(extension);
                        }
                    }

                    // If there's any left, re-add them. Otherwise set to empty.
                    if (localExtensions.Count > 0)
                    {
                        dataQuery.dataQueryConnection.extensions = localExtensions;
                    }
                    else
                    {
                        dataQuery.dataQueryConnection.extensions = null;
                    }
                }
            }
            // Otherwise, if the setting is enabled, add the extension if it does not yet exist already.
            else if (jsonExportSetting.IsAddSchemaAsExtension() && jsonExportSetting.IsAddDataObjectConnection() && dataQuery.dataQueryConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();

                // Copy any existing classifications already in place, if any.
                if (dataQuery.dataQueryConnection.extensions != null)
                {
                    localExtensions = dataQuery.dataQueryConnection.extensions;
                }

                // Check if this particular classification already exists before adding.
                bool extensionExists = false;
                foreach (var extension in localExtensions)
                {
                    if (extension.key == "schema")
                    {
                        extensionExists = true;
                    }
                }

                if (extensionExists == false)
                {
                    var localExtension = new Extension
                    {
                        key = "schema",
                        value = teamConnection.DatabaseServer.SchemaName,
                        description = "schema name"
                    };

                    localExtensions.Add(localExtension);
                }

                dataQuery.dataQueryConnection.extensions = localExtensions;
            }

            return dataQuery;
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
            string dataObjectType;

            if (classificationOverrideValue != null)
            {
                dataObjectType = classificationOverrideValue;
            }
            else
            {
                dataObjectType = GetDataObjectType(dataObject.name, "", FormBase.TeamConfiguration).ToString();
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
            DataObject dataObject = new DataObject();

            if (jsonExportSetting.AddMetadataAsRelatedDataObject == "True")
            {
                dataObject = CreateDataObject("Metadata", teamConfiguration.MetadataConnection, jsonExportSetting, teamConfiguration);
            }

            // Manage connections.
            SetDataObjectConnection(dataObject, teamConfiguration.MetadataConnection, jsonExportSetting);

            // Manage connection extension.
            SetDataObjectConnectionDatabaseExtension(dataObject, teamConfiguration.MetadataConnection, jsonExportSetting);
            SetDataObjectConnectionSchemaExtension(dataObject, teamConfiguration.MetadataConnection, jsonExportSetting);

            return dataObject;
        }

        /// <summary>
        /// Add the Business Key segment to a Data Object Mapping.
        /// </summary>
        /// <param name="dataObjectMapping"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        internal static DataObjectMapping SetBusinessKeys(DataObjectMapping dataObjectMapping, string businessKeyDefinition, string sourceDataObjectName, TeamConnection teamConnection, TeamConfiguration teamConfiguration, string drivingKeyValue)
        {
            // The list of business keys that will be saved against the data object mapping.
            List<BusinessKey> businessKeys = new List<BusinessKey>();

            List<BusinessKeyComponentList> businessKeyComponentValueList = GetBusinessKeyComponents(dataObjectMapping, businessKeyDefinition, sourceDataObjectName, teamConnection, teamConfiguration);

            foreach (BusinessKeyComponentList businessKeyComponentList in businessKeyComponentValueList)
            {
                // Each business key component in the initial list become a mapping of data items. It will be created as a business key together with a surrogate key.
                BusinessKey businessKey = new BusinessKey();

                // Evaluate the data item mappings that belong to the business key component mapping.
                List<DataItemMapping> businessKeyComponentMapping = new List<DataItemMapping>();

                int iterations = businessKeyComponentList.sourceComponentList.Count;

                for (int i = 0; i < iterations; i++)
                {
                    var businessKeyDataItemMapping = GetBusinessKeyComponentDataItemMapping(businessKeyComponentList.sourceComponentList[i], businessKeyComponentList.targetComponentList[i], drivingKeyValue);
                    businessKeyComponentMapping.Add(businessKeyDataItemMapping);
                }

                businessKey.businessKeyComponentMapping = businessKeyComponentMapping;

                // Evaluate the surrogate key that comes with the business key component mapping.

                //var targetDataObjectSurrogateKey = GetSurrogateKey(businessKeyComponentList.originalTargetDataObject, sourceDataObjectName, businessKeyDefinition, teamConnection, teamConfiguration);
                businessKey.surrogateKey = businessKeyComponentList.surrogateKey;

                businessKeys.Add(businessKey);
            }

            dataObjectMapping.businessKeys = businessKeys;

            return dataObjectMapping;
        }

        internal class BusinessKeyComponentList
        {
            internal string originalTargetDataObject { get; set; }
            internal List<string> sourceComponentList { get; set; }
            internal List<string> targetComponentList { get; set; }
            internal int ordinal { get; set; }
            internal string surrogateKey { get; set; }
        }

        /// <summary>
        /// Evaluate the number of components for the complete business key.
        /// </summary>
        /// <param name="dataObjectMapping"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="sourceDataObjectName"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static List<BusinessKeyComponentList>  GetBusinessKeyComponents(DataObjectMapping dataObjectMapping, string businessKeyDefinition, string sourceDataObjectName, TeamConnection teamConnection, TeamConfiguration teamConfiguration)
        {
            List<BusinessKeyComponentList> businessKeyComponents = new List<BusinessKeyComponentList>();

            // If a business key definition is related to a relationship (e.g. Link), multiple business keys must be asserted.
            // For the relationship itself the complete business key is evaluated.
            // For the individual components, individual business keys must be evaluated.
            // Each of these are business key components (e.g. a 2-way Link has 3 components, a Hub as 1 component)

            int ordinal = 1;

            var mappingType = dataObjectMapping.mappingClassifications[0].classification;

            if (mappingType == DataObjectTypes.NaturalBusinessRelationship.ToString())
            {
                // Add the full list straight away (the relationships).
                var tempComponent = new BusinessKeyComponentList();
                tempComponent.sourceComponentList = GetBusinessKeySourceComponentElements(businessKeyDefinition);
                businessKeyComponents.Add(tempComponent);

                // This is the Link name.
                tempComponent.originalTargetDataObject = dataObjectMapping.targetDataObject.name;

                // Get the target column(s) for the business key, based on the target data object (the Link in this case).
                var tempTargetComponentList = GetBusinessKeyTargetComponentElements(dataObjectMapping.targetDataObject, businessKeyDefinition, sourceDataObjectName, teamConnection, teamConfiguration);
                tempComponent.targetComponentList = tempTargetComponentList;

                tempComponent.ordinal = ordinal;

                // Link surrogate key
                var surrogateKey = GetSurrogateKey(dataObjectMapping.targetDataObject.name, sourceDataObjectName, businessKeyDefinition, teamConnection, teamConfiguration);
                tempComponent.surrogateKey = surrogateKey;

                // Add individual key parts (the individual keys) as well.
                var tempList = businessKeyDefinition.Split(',').ToList();
                foreach (string componentElement in tempList)
                {
                    var individualTempComponent = new BusinessKeyComponentList();

                    var componentElementList = new List<string>();
                    componentElementList.Add(componentElement);
                    //individualTempComponent.sourceComponentList = componentElementList;
                    individualTempComponent.sourceComponentList = GetBusinessKeySourceComponentElements(componentElement);

                    // First, let's get the Hubs for the key. It's the one with the same source and business key definition.
                    // Find the matching physical model row.

                    var dataObjectGridViewRow = _dataGridViewDataObjects.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => !r.IsNewRow)
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(componentElement))
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName))
                        .FirstOrDefault();

                    if (dataObjectGridViewRow != null)
                    {
                        ordinal++;

                        var originalTargetDataObjectName = dataObjectGridViewRow.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString();
                        var originalSourceDataObjectName = dataObjectGridViewRow.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString();

                        individualTempComponent.originalTargetDataObject = originalTargetDataObjectName;
                        // Get the target column(s) for the business key, based on the target data object.
                        var individualTempTargetComponentList = GetBusinessKeyTargetComponentElements(dataObjectMapping.targetDataObject, componentElement, originalSourceDataObjectName, teamConnection, teamConfiguration);
                        individualTempComponent.targetComponentList = individualTempTargetComponentList;
                        individualTempComponent.ordinal = ordinal;

                        // Hub surrogate keys, needs to manage SAl and HAL
                        // This can ONLY be derived from the physical model. To be improved.
                        var physicalModelDataGridViewRowList = _dataGridViewPhysicalModel.Rows
                            .Cast<DataGridViewRow>()
                            .Where(r => !r.IsNewRow)
                            .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.Table_Name].Value.ToString().Equals(tempComponent.originalTargetDataObject))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString().Equals(tempComponent.surrogateKey))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString().Equals(teamConfiguration.LoadDateTimeAttribute))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString().Equals(teamConfiguration.EtlProcessAttribute))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString().Equals(teamConfiguration.RecordSourceAttribute))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString().Equals(tempComponent.surrogateKey))
                            .ToList();

                        // The physical key order must now be established.
                        int counter = 2;
                        foreach (var physicalModelRow in physicalModelDataGridViewRowList)
                        {
                            if (ordinal == counter)
                            {
                                individualTempComponent.surrogateKey = physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString();

                            }
                            counter++;
                        }

                        //var individualSurrogateKey = GetSurrogateKey(dataObjectMapping.targetDataObject.name, sourceDataObjectName, businessKeyDefinition, teamConnection, teamConfiguration);
                        //tempComponent.surrogateKey = individualSurrogateKey;
                    }
                    else
                    {
                        // Exception - should be picked up by validation
                        individualTempComponent.originalTargetDataObject = "Unknown";
                    }

                    businessKeyComponents.Add(individualTempComponent);
                }
            }
            else
            {
                // Not a relationship, add the list straight away.
                var tempComponent = new BusinessKeyComponentList();

                var tempSourceComponentList = GetBusinessKeySourceComponentElements(businessKeyDefinition);
                tempComponent.sourceComponentList = tempSourceComponentList;

                // The associated target data object is just the original one.
                tempComponent.originalTargetDataObject = dataObjectMapping.targetDataObject.name;

                // Get the target column(s) for the business key, based on the target data object.
                var tempTargetComponentList = GetBusinessKeyTargetComponentElements(dataObjectMapping.targetDataObject, businessKeyDefinition, sourceDataObjectName, teamConnection, teamConfiguration);
                tempComponent.targetComponentList = tempTargetComponentList;

                tempComponent.ordinal = ordinal;

                var surrogateKey = GetSurrogateKey(dataObjectMapping.targetDataObject.name, sourceDataObjectName, businessKeyDefinition, teamConnection, teamConfiguration);
                tempComponent.surrogateKey = surrogateKey;

                businessKeyComponents.Add(tempComponent);
            }

            return businessKeyComponents;
        }

        public static List<string> GetBusinessKeyTargetComponentElements(DataObject dataObject, string businessKeyDefinition, string sourceDataObjectName, TeamConnection teamConnection, TeamConfiguration teamConfiguration)
        {
            List<string> targetBusinessKeyComponents = new List<string>();
            
            // ReSharper disable once RedundantAssignment
            List<DataObject> lookupDataObjects = new List<DataObject>();

            // Evaluate the data object to lookup the target business key component elements for.
            // For a Context or Relationship Context entity the 'parent' needs to be found.

            var dataObjectType = GetDataObjectType(dataObject.name, "", teamConfiguration);
            
            if (new[] { DataObjectTypes.Context }.Contains(dataObjectType))
            {
                // Find the parent. This is the data object with the same key definition, but is not a context type entity.
                var dataObjectGridViewRow = _dataGridViewDataObjects.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(businessKeyDefinition))
                    .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName))
                    .Where(r => !r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(dataObject.name))
                    .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != DataObjectTypes.Context)
                    .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != DataObjectTypes.NaturalBusinessRelationshipContext)
                    .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey);

                // Add the Hub to the list. Should be only one but the list is created to facilitate Links.
                foreach (var row in dataObjectGridViewRow)
                {
                    lookupDataObjects.Add((DataObject)row.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value);
                }
            }
            else if (new[] { DataObjectTypes.NaturalBusinessRelationship, DataObjectTypes.NaturalBusinessRelationshipContext, DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey }.Contains(dataObjectType))
            {
                // Find the Hubs. This is the data object with the same *partial* key definition, but is not a context type entity.
                var businessKeyComponentElements = businessKeyDefinition.Split(',').ToList();

                foreach (string businessKeyComponentElement in businessKeyComponentElements)
                {
                    var dataObjectGridViewRow = _dataGridViewDataObjects.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => !r.IsNewRow)
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(businessKeyComponentElement))
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName))
                        .Where(r => !r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(dataObject.name))
                        .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) == DataObjectTypes.CoreBusinessConcept)
                        .FirstOrDefault();

                    lookupDataObjects.Add((DataObject)dataObjectGridViewRow.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value);
                }
            }
            else // Staging, PSA, Hub, other.
            {
                lookupDataObjects.Add(dataObject);
            }

            foreach (var lookupDataObject in lookupDataObjects)
            {
                var physicalModelDataGridViewRow = _dataGridViewPhysicalModel.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.Table_Name].Value.ToString().Equals(lookupDataObject.name))
                    .ToList();

                // Sorting separately for debugging purposes. Issues were found in string to int conversion when sorting on ordinal position.
                var orderedList = physicalModelDataGridViewRow.OrderBy(row => Int32.Parse(row.Cells[(int)PhysicalModelMappingMetadataColumns.Ordinal_Position].Value.ToString()));

                foreach (var row in orderedList)
                {
                    var column = row.Cells[(int)PhysicalModelMappingMetadataColumns.Column_Name].Value.ToString();

                    // Add if it's not a standard element.
                    var surrogateKey = GetSurrogateKey(lookupDataObject.name, sourceDataObjectName, businessKeyDefinition, teamConnection, teamConfiguration);

                    if (!column.IsExcludedBusinessKeyDataItem(dataObjectType, surrogateKey, businessKeyDefinition, teamConnection, teamConfiguration))
                    {
                        targetBusinessKeyComponents.Add(column);
                    }
                }
            }

            return targetBusinessKeyComponents;
        }

        public static DataItemMapping GetBusinessKeyComponentDataItemMapping(string sourceBusinessKeyDefinition, string targetBusinessKeyDefinition, string drivingKeyValue = "")
        {
            DataItemMapping dataItemMapping = new DataItemMapping();

            // Is set to true if there are quotes in the key part.

            DataItemMapping keyComponent = new DataItemMapping();

            List<dynamic> sourceColumns = new List<dynamic>();

            DataItem sourceColumn = new DataItem();
            DataItem targetColumn = new DataItem();

            sourceColumn.name = sourceBusinessKeyDefinition;
            sourceColumn.isHardCodedValue = sourceBusinessKeyDefinition.StartsWith("'") && sourceBusinessKeyDefinition.EndsWith("'");

            // Driving Key
            if (sourceBusinessKeyDefinition == drivingKeyValue)
            {
                List<Classification> classificationList = new List<Classification>();
                Classification classification = new Classification();
                classification.classification = "DrivingKey";
                classification.notes = "The attribute that triggers (drives) the closing of a relationship.";
                classificationList.Add(classification);
                sourceColumn.dataItemClassification = classificationList;
            }

            sourceColumns.Add(sourceColumn);

            keyComponent.sourceDataItems = sourceColumns;

            targetColumn.name = targetBusinessKeyDefinition;

            keyComponent.targetDataItem = targetColumn;

            dataItemMapping.sourceDataItems = sourceColumns;
            dataItemMapping.targetDataItem = targetColumn;

            return dataItemMapping;
        }

        /// <summary>
        /// Returns the list of elements for a given input Business Key string value. Breaking up the original string that represents the business key definition in elements.
        /// </summary>
        /// <param name="businessKeyDefinition"></param>
        /// <returns></returns>
        private static List<string> GetBusinessKeySourceComponentElements(string businessKeyDefinition)
        {
            var temporaryBusinessKeyComponentList = businessKeyDefinition.Split(',').ToList();

            var businessKeyComponentList = new List<string>();

            foreach (var keyComponent in temporaryBusinessKeyComponentList)
            {
                var keyPart = keyComponent.Replace("(", "").Replace(")", "").Replace(" ", "");

                if (keyPart.StartsWith("COMPOSITE"))
                {
                    keyPart = keyPart.Replace("COMPOSITE", "");

                    var temporaryKeyPartList = keyPart.Split(';').ToList();
                    foreach (var item in temporaryKeyPartList)
                    {
                        businessKeyComponentList.Add(item);
                    }
                }
                else if (keyPart.StartsWith("CONCATENATE"))
                {
                    keyPart = keyPart.Replace("CONCATENATE", "");
                    keyPart = keyPart.Replace(";", "+");

                    businessKeyComponentList.Add(keyPart);
                }
                else
                {
                    businessKeyComponentList.Add(keyPart);
                }
            }

            businessKeyComponentList = businessKeyComponentList.Select(t => t.Trim()).ToList();
            return businessKeyComponentList;
        }

        public static string GetParentDataObject(string targetDataObjectName, string sourceDataObjectName, string businessKeyDefinition, TeamConfiguration teamConfiguration)
        {
            string returnValue = "";

            var dataObjectGridViewRow = _dataGridViewDataObjects.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(businessKeyDefinition))
                .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName))
                .Where(r => !r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(targetDataObjectName))
                .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != MetadataHandling.DataObjectTypes.Context)
                .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext)
                .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
                .FirstOrDefault();

            if (dataObjectGridViewRow != null)
            {
                returnValue = dataObjectGridViewRow.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString();
            }

            return returnValue;
        }

        /// <summary>
        /// Return the Surrogate Key for a given table using the TEAM settings (i.e. prefix/suffix settings etc.).
        /// </summary>
        /// <param name="targetDataObjectName"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns>surrogateKey</returns>
        public static string GetSurrogateKey(string targetDataObjectName, string sourceDataObjectName, string businessKeyDefinition, TeamConnection teamConnection, TeamConfiguration teamConfiguration)
        {
            // Get the type
            var dataObjectType = GetDataObjectType(targetDataObjectName, "", teamConfiguration);

            // If a Sat or Lsat, replace with parent Hub or Link.
            if (new [] { DataObjectTypes.Context, DataObjectTypes.NaturalBusinessRelationshipContext, DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey}.Contains(dataObjectType))
            {
                targetDataObjectName = GetParentDataObject(targetDataObjectName, sourceDataObjectName, businessKeyDefinition, teamConfiguration);
            }

            // Get the fully qualified name
            KeyValuePair<string, string> fullyQualifiedName = GetFullyQualifiedDataObjectName(targetDataObjectName, teamConnection).FirstOrDefault();

            // Initialise the return value
            string surrogateKey = "";
            string newDataObjectName = fullyQualifiedName.Value;
            string keyLocation = teamConfiguration.DwhKeyIdentifier;

            string[] prefixSuffixArray = {
                teamConfiguration.HubTablePrefixValue,
                teamConfiguration.SatTablePrefixValue,
                teamConfiguration.LinkTablePrefixValue,
                teamConfiguration.LsatTablePrefixValue
            };

            if (newDataObjectName != "Not applicable")
            {
                // Removing the table pre- or suffixes from the table name based on the TEAM configuration settings.
                if (teamConfiguration.TableNamingLocation == "Prefix")
                {
                    foreach (string prefixValue in prefixSuffixArray)
                    {
                        if (newDataObjectName.StartsWith(prefixValue))
                        {
                            newDataObjectName = newDataObjectName.Replace(prefixValue, "");
                        }
                    }
                }
                else
                {
                    foreach (string suffixValue in prefixSuffixArray)
                    {
                        if (newDataObjectName.EndsWith(suffixValue))
                        {
                            newDataObjectName = newDataObjectName.Replace(suffixValue, "");
                        }
                    }
                }

                // Define the surrogate key using the table name and key prefix/suffix settings.
                if (teamConfiguration.KeyNamingLocation == "Prefix")
                {
                    surrogateKey = keyLocation + newDataObjectName;
                }
                else
                {
                    surrogateKey = newDataObjectName + keyLocation;
                }
            }

            return surrogateKey;
        }

        public static bool IsExcludedBusinessKeyDataItem(this string dataItemName, DataObjectTypes dataObjectType, string surrogateKey, string businessKeyDefinition, TeamConnection teamConnection, TeamConfiguration teamConfiguration)
        {
            bool returnValue = false;
            
            var businessKeyComponentElements = GetBusinessKeySourceComponentElements(businessKeyDefinition);

            if (dataItemName == surrogateKey)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.LoadDateTimeAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.AlternativeLoadDateTimeAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.RecordSourceAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.AlternativeRecordSourceAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.RowIdAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.EtlProcessAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.RecordChecksumAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.ChangeDataCaptureAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.LogicalDeleteAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.EventDateTimeAttribute)
            {
                returnValue = true;
            }
            else if (new[] { DataObjectTypes.StagingArea, DataObjectTypes.PersistentStagingArea }.Contains(dataObjectType) && !businessKeyComponentElements.Contains(dataItemName))
            {
                returnValue = true;
            }

            return returnValue;
        }

        public static bool IsIncludedDataItem(this string dataItemName, DataObjectTypes dataObjectType, string surrogateKey, TeamConnection teamConnection, TeamConfiguration teamConfiguration)
        {
            bool returnValue = true;

            if (dataItemName == teamConfiguration.LoadDateTimeAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.AlternativeLoadDateTimeAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.RecordSourceAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.AlternativeRecordSourceAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.RowIdAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.EtlProcessAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.RecordChecksumAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.ChangeDataCaptureAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.LogicalDeleteAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.EventDateTimeAttribute)
            {
                returnValue = false;
            }
            else if (!new[] { DataObjectTypes.StagingArea, DataObjectTypes.PersistentStagingArea }.Contains(dataObjectType) && dataItemName == surrogateKey) 
            {
                returnValue = false;
            }

            return returnValue;
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
    }
}