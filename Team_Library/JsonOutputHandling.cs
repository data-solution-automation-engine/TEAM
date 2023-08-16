using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DataWarehouseAutomation;
using static TEAM_Library.MetadataHandling;
using DataObject = DataWarehouseAutomation.DataObject;
using Extension = DataWarehouseAutomation.Extension;

namespace TEAM_Library
{
    /// <summary>
    /// Manages the output in Json conform the schema for Data Warehouse Automation.
    /// </summary>
    public static class JsonOutputHandling
    {
        /// <summary>
        /// Create Data Object in TEAM.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="teamConnection"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="dataGridViewRowsPhysicalModel"></param>
        /// <param name="sourceOrTarget"></param>
        /// <returns></returns>
        public static DataObject CreateDataObject(string dataObjectName, TeamConnection teamConnection, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration, List<DataGridViewRow> dataGridViewRowsPhysicalModel, string sourceOrTarget = "Source")
        {
            // Initialise the object and set the name.
            DataObject dataObject = new DataObject {Name = dataObjectName};

            // Set the data object connection.
            SetDataObjectConnection(dataObject, teamConnection, jsonExportSetting);

            // Connection information as extensions. Only allowed if a Connection is added to the Data Object.
            dataObject = SetDataObjectConnectionDatabaseExtension(dataObject, teamConnection, jsonExportSetting);
            dataObject = SetDataObjectConnectionSchemaExtension(dataObject, teamConnection, jsonExportSetting);

            // Add classifications, overridden for the metadata connection.
            if (dataObjectName == "Metadata")
            {
                SetDataObjectTypeClassification(dataObject, jsonExportSetting, teamConfiguration, "Metadata");
            }
            else
            {
                SetDataObjectTypeClassification(dataObject, jsonExportSetting, teamConfiguration);
            }
            
            // Set the data items.
            SetDataObjectDataItems(dataObject, teamConnection, teamConfiguration, jsonExportSetting, dataGridViewRowsPhysicalModel);

            return dataObject;
        }

        /// <summary>
        /// Add Data Items to the Data Object definition.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="dataGridViewRowsPhysicalModel"></param>
        public static DataObject SetDataObjectDataItems(DataObject dataObject, TeamConnection teamConnection, TeamConfiguration teamConfiguration, JsonExportSetting jsonExportSetting, List<DataGridViewRow> dataGridViewRowsPhysicalModel)
        {
            // Remove the list if the setting is disabled.
            if (!jsonExportSetting.IsAddDataItemsToDataObject())
            {
                dataObject.DataItems = null;
            }
            else if (jsonExportSetting.IsAddDataItemsToDataObject())
            {
                var fullyQualifiedName = GetFullyQualifiedDataObjectName(dataObject, teamConnection).FirstOrDefault();

                List<dynamic> dataItems = new List<dynamic>();

                foreach (DataGridViewRow physicalModelGridViewRow in dataGridViewRowsPhysicalModel)
                {
                    if (!physicalModelGridViewRow.IsNewRow)
                    {
                        var schemaName = physicalModelGridViewRow.Cells[(int)PhysicalModelMappingMetadataColumns.schemaName].Value.ToString();
                        var tableName = physicalModelGridViewRow.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString();

                        if (fullyQualifiedName.Key == schemaName && fullyQualifiedName.Value == tableName)
                        {
                            DataItem dataItem = new DataItem { Name = physicalModelGridViewRow.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString() };

                            // Apply or remove data item details.
                            SetDataItemDataType(dataItem, physicalModelGridViewRow, jsonExportSetting);

                            dataItems.Add(dataItem);
                        }
                    }
                }

                dataObject.DataItems = dataItems;
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
            if (!jsonExportSetting.IsAddDataTypeToDataItem())
            {
                // Remove any data type details.
                dataItem.CharacterLength = null;
                dataItem.DataType = null;
                dataItem.NumericPrecision = null;
                dataItem.NumericScale = null;
                dataItem.OrdinalPosition = null;
                dataItem.IsPrimaryKey = null;
                dataItem.IsHardCodedValue = null;
            }
            else if (jsonExportSetting.IsAddDataTypeToDataItem())
            {
                if (physicalModelRow != null)
                {
                    var dataType = physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.dataType].Value.ToString();

                    dataItem.DataType = dataType;

                    switch (dataType)
                    {
                        case "varchar":
                        case "nvarchar":
                        case "binary":
                            dataItem.CharacterLength = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.characterLength].Value.ToString());
                            break;
                        case "numeric":
                            dataItem.NumericPrecision = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericPrecision].Value.ToString());
                            dataItem.NumericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericScale].Value.ToString());
                            break;
                        case "int":
                            // No length etc.
                            break;
                        case "datetime":
                        case "datetime2":
                        case "date":
                            dataItem.NumericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericScale].Value.ToString());
                            break;
                    }

                    dataItem.OrdinalPosition = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.ordinalPosition].Value.ToString());
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
        /// <param name="dataGridViewRowsPhysicalModel"></param>
        /// <returns></returns>
        public static DataItem SetDataItemMappingDataType(DataItem dataItem, DataObject dataObject, TeamConnection teamConnection, JsonExportSetting jsonExportSetting, List<DataGridViewRow> dataGridViewRowsPhysicalModel)
        {
            if (!jsonExportSetting.IsAddDataTypeToDataItem())
            {
                // Remove any data type details.
                dataItem.CharacterLength = null;
                dataItem.DataType = null;
                dataItem.NumericPrecision = null;
                dataItem.NumericScale = null;
                dataItem.OrdinalPosition = null;
                dataItem.IsPrimaryKey = null;
                dataItem.IsHardCodedValue = null;
            }
            else if (jsonExportSetting.IsAddDataTypeToDataItem())
            {
                // Only continue if the physical model is there.
                if (dataGridViewRowsPhysicalModel.Any())
                {
                    var fullyQualifiedName = GetFullyQualifiedDataObjectName(dataObject, teamConnection).FirstOrDefault();

                    // Find the matching physical model row.
                    DataGridViewRow physicalModelRow = dataGridViewRowsPhysicalModel
                        .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.schemaName].Value.ToString().Equals(fullyQualifiedName.Key))
                        .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString().Equals(fullyQualifiedName.Value))
                        .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(dataItem.Name))
                        .FirstOrDefault();

                    if (physicalModelRow != null)
                    {
                        var dataType = physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.dataType].Value.ToString();

                        dataItem.DataType = dataType;

                        switch (dataType)
                        {
                            case "varchar":
                            case "nvarchar":
                            case "binary":
                                dataItem.CharacterLength = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.characterLength].Value.ToString());
                                break;
                            case "numeric":
                                dataItem.NumericPrecision = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericPrecision].Value.ToString());
                                dataItem.NumericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericScale].Value.ToString());
                                break;
                            case "int":
                                // No length etc.
                                break;
                            case "datetime":
                            case "datetime2":
                            case "date":
                                dataItem.NumericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericScale].Value.ToString());
                                break;
                        }

                        dataItem.OrdinalPosition = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.ordinalPosition].Value.ToString());
                    }
                }
            }

            return dataItem;
        }

        /// <summary>
        /// Add a classification at data object mapping level.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="classifications"></param>
        /// <returns></returns>
        public static List<DataClassification> MappingClassification(string dataObjectName, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration,TeamConnection connection, string drivingKeyValue, List<DataClassification> classifications)
        {
            List<DataClassification> dataObjectsMappingClassifications = new List<DataClassification>();

            // Keep existing classifications.
            if (classifications != null)
            {
                dataObjectsMappingClassifications = classifications;
            }
            // Create new classifications.
            else
            {
                var dataObjectType = GetDataObjectType(dataObjectName, "", teamConfiguration);

                // Override for driving key.
                if (drivingKeyValue != null && !string.IsNullOrEmpty(drivingKeyValue))
                {
                    dataObjectType = DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey;
                }

                var stringDataObjectType = "Unknown";

                if (dataObjectType == DataObjectTypes.Source || dataObjectType == DataObjectTypes.Unknown)
                {
                    stringDataObjectType = connection.ConnectionKey;
                }
                else
                {
                    stringDataObjectType = dataObjectType.ToString();
                }

                // Work around for unknowns - replace with connection information.
                var dataObjectMappingClassification = new DataClassification
                {
                    Classification = stringDataObjectType
                };

                dataObjectsMappingClassifications.Add(dataObjectMappingClassification);
            }

            return dataObjectsMappingClassifications;
        }

        /// <summary>
        /// Create a data object for the metadata connection as a related data object.
        /// </summary>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="dataGridViewRowsPhysicalModel"></param>
        /// <returns></returns>
        public static DataObject SetMetadataAsRelatedDataObject(JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration, List<DataGridViewRow> dataGridViewRowsPhysicalModel)
        {
            DataObject returnDataObject = new DataObject();

            // Add the metadata connection as related data object (assuming this is set in the json export settings).
            if (jsonExportSetting.IsAddMetadataAsRelatedDataObject())
            {
                var metaDataObject = GetMetadataDataObject(teamConfiguration, jsonExportSetting, dataGridViewRowsPhysicalModel);

                if (metaDataObject.Name != null && metaDataObject.Name != "NewDataObject")
                {
                    returnDataObject = metaDataObject;
                }
            }

            return returnDataObject;
        }

        /// <summary>
        /// Get the 'parent' data object for a given data object, i.e. the object that is referenced to in the data model.
        /// </summary>
        /// <param name="targetDataObjectName"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="dataObjectDataGridViewRows"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="sourceDataObjectName"></param>
        /// <returns></returns>
        public static List<DataObject> GetParentRelatedDataObjectList(string targetDataObjectName, string sourceDataObjectName, string businessKeyDefinition, List<DataGridViewRow> dataObjectDataGridViewRows, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration)
        {
            List<DataObject> relatedDataObjectList = new List<DataObject>();

            if (jsonExportSetting.AddParentDataObjectAsRelatedDataObject == "True")
            {
                // Find the parent data object.
                var parentDataObjects = GetParentDataObjects(targetDataObjectName, sourceDataObjectName, businessKeyDefinition, teamConfiguration, dataObjectDataGridViewRows);

                // Create the parent data object.
                if (parentDataObjects != null)
                {
                    // Set the name and further settings.
                    relatedDataObjectList.AddRange(parentDataObjects);
                }
            }

            return relatedDataObjectList;
        }

        /// <summary>
        /// Convenience method to identify the next-layer-up data objects for a given data object (string).
        /// </summary>
        /// <param name="targetDataObjectName"></param>
        /// <param name="dataObjectDataGrid"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static List<DataObject> SetNextUpRelatedDataObjectList(string targetDataObjectName, DataGridView dataObjectDataGrid, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration, EventLog eventLog, List<DataGridViewRow> dataGridViewRowsPhysicalModel)
        {
            List<DataObject> dataObjectList = new List<DataObject>();

            if (jsonExportSetting.IsAddNextUpDataObjectsAsRelatedDataObject())
            {
                var dataObjectMappings = dataObjectDataGrid.Rows.Cast<DataGridViewRow>()
                    .Where(x => !x.IsNewRow)
                    .Where(x => ((DataRowView)x.DataBoundItem).Row.Field<DataObject>(DataObjectMappingGridColumns.SourceDataObject.ToString()).Name == targetDataObjectName).ToList();

                foreach (DataGridViewRow row in dataObjectMappings)
                {
                    var localDataObjectName = row.Cells[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].Value.ToString();
                    var localDataObjectConnectionInternalId = row.Cells[DataObjectMappingGridColumns.TargetConnection.ToString()].Value.ToString();

                    TeamConnection localConnection = TeamConnection.GetTeamConnectionByConnectionInternalId(localDataObjectConnectionInternalId, teamConfiguration, eventLog);

                    // Set the name and further settings.

                    var localRelatedDataObject = CreateDataObject(localDataObjectName, localConnection, jsonExportSetting, teamConfiguration, dataGridViewRowsPhysicalModel);

                    if (localRelatedDataObject.Name != "NewDataObject")
                    {
                        dataObjectList.Add(localRelatedDataObject);
                    }
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
        public static DataItem SetParentDataObjectToDataItem(DataItem dataItem, DataObject dataObject, JsonExportSetting jsonExportSetting)
        {
            // If the setting is disabled, remove the data object from the data item
            if (!jsonExportSetting.IsAddParentDataObjectToDataItem())
            {
                dataItem.DataObject = null;
            }
            else if (jsonExportSetting.IsAddParentDataObjectToDataItem())
            {
                // Create a separate, smaller / limited, data object to avoid any circular dependencies when assigning the Data Object to the Data Item.
                var localDataObject = new DataObject
                {
                    Name = dataObject.Name
                };

                if (dataObject.DataObjectClassifications != null && dataObject.DataObjectClassifications.Count > 0)
                {
                    localDataObject.DataObjectClassifications = dataObject.DataObjectClassifications;
                }

                if (dataObject.DataObjectConnection != null && !string.IsNullOrEmpty(dataObject.DataObjectConnection.DataConnectionString))
                {
                    localDataObject.DataObjectConnection = dataObject.DataObjectConnection;
                }

                // Add the Data Object to the Data Item.
                dataItem.DataObject = localDataObject;
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
            // Store the extensions that may be there, if any.
            var tempExtensions = new List<Extension>();

            if (dataObject.DataObjectConnection != null && dataObject.DataObjectConnection.Extensions != null)
            {
                tempExtensions = dataObject.DataObjectConnection.Extensions;
            }

            var dataObjectConnection = new DataConnection
            {
                DataConnectionString = teamConnection.ConnectionKey
            };

            // Re-add extensions, if available.
            if (tempExtensions != null)
            {
                dataObjectConnection.Extensions = tempExtensions;
            }

            dataObject.DataObjectConnection = dataObjectConnection;

            return dataObject;
        }

        public static DataQuery SetDataQueryConnection(DataQuery dataQuery, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            var dataObjectConnection = new DataConnection { DataConnectionString = teamConnection.ConnectionKey };

            dataQuery.DataQueryConnection = dataObjectConnection;

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
            if (!jsonExportSetting.IsAddDatabaseAsExtensionToConnection())
            {
                if (dataObject.DataObjectConnection != null && dataObject.DataObjectConnection.Extensions != null)
                {
                    List<Extension> localExtensions = new List<Extension>();

                    foreach (var extension in dataObject.DataObjectConnection.Extensions)
                    {
                        if (extension.Key != "database")
                        {
                            localExtensions.Add(extension);
                        }
                    }

                    // If there's any left, re-add them. Otherwise set to empty.
                    if (localExtensions.Count > 0)
                    {
                        dataObject.DataObjectConnection.Extensions = localExtensions;
                    }
                    else
                    {
                        dataObject.DataObjectConnection.Extensions = null;
                    }
                }
            }
            // Otherwise, if the setting is enabled, add the extension if it does not yet exist already.
            else if (jsonExportSetting.IsAddDatabaseAsExtensionToConnection() && dataObject.DataObjectConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();
                List<Extension> returnExtensions = new List<Extension>();

                // Copy any existing classifications already in place, if any.
                if (dataObject.DataObjectConnection.Extensions != null)
                {
                    localExtensions = dataObject.DataObjectConnection.Extensions;
                }

                // Check if this particular classification already exists before adding.
                // Preserve the others.
                foreach (var extension in localExtensions)
                {
                    if (extension.Key != "database")
                    {
                        returnExtensions.Add(extension);
                    }
                }

                var localExtension = new Extension
                {
                    Key = "database",
                    Value = teamConnection.DatabaseServer.DatabaseName,
                    Description = "database name"
                };

                returnExtensions.Add(localExtension);

                dataObject.DataObjectConnection.Extensions = returnExtensions;
            }

            return dataObject;
        }

        public static DataQuery SetDataQueryConnectionDatabaseExtension(DataQuery dataQuery, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            // Remove an existing classification, if indeed existing.
            // If no classifications exists, do nothing. Otherwise check if one needs removal.
            if (!jsonExportSetting.IsAddDatabaseAsExtensionToConnection())
            {
                if (dataQuery.DataQueryConnection != null && dataQuery.DataQueryConnection.Extensions != null)
                {
                    List<Extension> localExtensions = new List<Extension>();

                    foreach (var extension in dataQuery.DataQueryConnection.Extensions)
                    {
                        if (extension.Key != "database")
                        {
                            localExtensions.Add(extension);
                        }
                    }

                    // If there's any left, re-add them. Otherwise set to empty.
                    if (localExtensions.Count > 0)
                    {
                        dataQuery.DataQueryConnection.Extensions = localExtensions;
                    }
                    else
                    {
                        dataQuery.DataQueryConnection.Extensions = null;
                    }
                }
            }
            // Otherwise, if the setting is enabled, add the extension if it does not yet exist already.
            else if (jsonExportSetting.IsAddDatabaseAsExtensionToConnection() && dataQuery.DataQueryConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();
                List<Extension> returnExtensions = new List<Extension>();

                // Copy any existing classifications already in place, if any.
                if (dataQuery.DataQueryConnection.Extensions != null)
                {
                    localExtensions = dataQuery.DataQueryConnection.Extensions;
                }

                // Check if this particular classification already exists before adding.
                foreach (var extension in localExtensions)
                {
                    if (extension.Key != "database")
                    {
                        returnExtensions.Add(extension);
                    }
                }

                var localExtension = new Extension
                {
                    Key = "database",
                    Value = teamConnection.DatabaseServer.DatabaseName,
                    Description = "database name"
                };

                returnExtensions.Add(localExtension);

                dataQuery.DataQueryConnection.Extensions = returnExtensions;
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
            // If no extensions exists, do nothing. Otherwise check if one needs removal.

            // Setting disabled. Remove an existing schema extension, if it exists.
            if (!jsonExportSetting.IsAddSchemaAsExtensionToConnection())
            {
                if (dataObject.DataObjectConnection?.Extensions != null)
                {
                    List<Extension> localExtensions = new List<Extension>();

                    foreach (var extension in dataObject.DataObjectConnection.Extensions)
                    {
                        if (extension.Key != "schema")
                        {
                            localExtensions.Add(extension);
                        }
                    }

                    // If there's any left, re-add them. Otherwise set to empty.
                    if (localExtensions.Count > 0)
                    {
                        dataObject.DataObjectConnection.Extensions = localExtensions;
                    }
                    else
                    {
                        dataObject.DataObjectConnection.Extensions = null;
                    }
                }
            }
            // Otherwise, if the setting is enabled, add the extension if it does not yet exist already.
            else if (jsonExportSetting.IsAddSchemaAsExtensionToConnection() && dataObject.DataObjectConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();
                List<Extension> returnExtensions = new List<Extension>();

                // Copy any existing extensions that are already in place, if any.
                if (dataObject.DataObjectConnection.Extensions != null)
                {
                    localExtensions = dataObject.DataObjectConnection.Extensions;
                }

                // Preserve any other extensions.
                foreach (var extension in localExtensions)
                {
                    if (extension.Key != "schema")
                    {
                        returnExtensions.Add(extension);
                    }
                }

                // Re-create the schema extension.
                var localExtension = new Extension
                {
                    Key = "schema",
                    Description = "schema name"
                };

                var schemaExtension = dataObject.DataObjectConnection?.Extensions?.Where(x => x.Key.Equals("schema")).FirstOrDefault();

                if (schemaExtension != null && schemaExtension.Value != teamConnection.DatabaseServer.SchemaName)
                {

                    localExtension.Value = schemaExtension.Value;
                }
                else
                {
                    localExtension.Value = teamConnection.DatabaseServer.SchemaName;
                }

                returnExtensions.Add(localExtension);

                // Apply all the extensions back to the connection object.
                dataObject.DataObjectConnection.Extensions = returnExtensions;

                // Addition for backwards compatibility, also adding the schema an extension at dataObject level.
                if (dataObject.Extensions == null)
                {
                    List<Extension> extensions = new List<Extension>();
                    extensions.Add(localExtension);
                    dataObject.Extensions = extensions;
                }
                else
                {
                    // Temporary extension list
                    List<Extension> extensions = new List<Extension>();

                    // Check if the schema extension already exists, replace if so.
                    foreach (var extension in dataObject.Extensions)
                    {
                        if (extension.Key != "schema")
                        {
                            extensions.Add(extension);
                        }
                        else
                        {
                            extensions.Add(localExtension);
                        }
                    }

                    dataObject.Extensions = extensions;
                }
            }

            return dataObject;
        }

        public static DataQuery SetDataQueryConnectionSchemaExtension(DataQuery dataQuery, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            // Remove an existing classification, if indeed existing.
            // If no classifications exists, do nothing. Otherwise check if one needs removal.
            if (!jsonExportSetting.IsAddSchemaAsExtensionToConnection())
            {
                if (dataQuery.DataQueryConnection != null && dataQuery.DataQueryConnection.Extensions != null)
                {
                    List<Extension> localExtensions = new List<Extension>();

                    foreach (var extension in dataQuery.DataQueryConnection.Extensions)
                    {
                        if (extension.Key != "schema")
                        {
                            localExtensions.Add(extension);
                        }
                    }

                    // If there's any left, re-add them. Otherwise set to empty.
                    if (localExtensions.Count > 0)
                    {
                        dataQuery.DataQueryConnection.Extensions = localExtensions;
                    }
                    else
                    {
                        dataQuery.DataQueryConnection.Extensions = null;
                    }
                }
            }
            // Otherwise, if the setting is enabled, add the extension if it does not yet exist already.
            else if (jsonExportSetting.IsAddSchemaAsExtensionToConnection() && dataQuery.DataQueryConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();
                List<Extension> returnExtensions = new List<Extension>();

                // Copy any existing classifications already in place, if any.
                if (dataQuery.DataQueryConnection.Extensions != null)
                {
                    localExtensions = dataQuery.DataQueryConnection.Extensions;
                }

                // Check if this particular classification already exists before adding.
                // Preserve the others.
                foreach (var extension in localExtensions)
                {
                    if (extension.Key != "schema")
                    {
                        returnExtensions.Add(extension);
                    }
                }

                var localExtension = new Extension
                {
                    Key = "schema",
                    Value = teamConnection.DatabaseServer.SchemaName,
                    Description = "schema name"
                };

                returnExtensions.Add(localExtension);


                dataQuery.DataQueryConnection.Extensions = returnExtensions;
            }

            return dataQuery;
        }
        
        /// <summary>
        /// Updates an input DataObject with a classification based on its type, evaluated by its name against defined conventions.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="classificationOverrideValue"></param>
        /// <returns></returns>
        public static DataObject SetDataObjectTypeClassification(DataObject dataObject, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration, string classificationOverrideValue=null)
        {
            string dataObjectType;

            if (classificationOverrideValue != null)
            {
                dataObjectType = classificationOverrideValue;
            }
            else
            {
                dataObjectType = GetDataObjectType(dataObject.Name, "", teamConfiguration).ToString();
            }

            if (!jsonExportSetting.IsAddTypeAsClassificationToDataObject())
            {
                // Remove an existing classification, if indeed existing.
                // If no classifications exists, do nothing. Otherwise check if one needs removal.
                if (dataObject.DataObjectClassifications != null)
                {
                    List<DataClassification> localClassifications = new List<DataClassification>();

                    foreach (var classification in dataObject.DataObjectClassifications)
                    {
                        if (classification.Classification != dataObjectType)
                        {
                            localClassifications.Add(classification);
                        }
                    }

                    // If there's any left, re-add them. Otherwise set to empty.
                    if (localClassifications.Count > 0)
                    {
                        dataObject.DataObjectClassifications = localClassifications;
                    }
                    else
                    {
                        dataObject.DataObjectClassifications = null;
                    }
                }
            }
            else
            {
                List<DataClassification> localClassifications = new List<DataClassification>();

                // Copy any existing classifications already in place, if any.
                if (dataObject.DataObjectClassifications != null)
                {
                    localClassifications = dataObject.DataObjectClassifications;
                }

                DataClassification localClassification = new DataClassification();

                localClassification.Classification = dataObjectType;

                // Check if this particular classification already exists before adding.
                bool classificationExists = false;
                foreach (var classification in localClassifications)
                {
                    if (classification.Classification == dataObjectType)
                    {
                        classificationExists = true;
                    }
                }

                if (classificationExists == false)
                {
                    localClassifications.Add(localClassification);
                }

                dataObject.DataObjectClassifications = localClassifications;
            }

            return dataObject;
        }

        /// <summary>
        /// Creates the special-type metadata Data Object;
        /// </summary>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static DataObject GetMetadataDataObject(TeamConfiguration teamConfiguration, JsonExportSetting jsonExportSetting, List<DataGridViewRow> dataGridViewRowsPhysicalModel)
        {
            DataObject dataObject = new DataObject();

            if (jsonExportSetting.AddMetadataAsRelatedDataObject == "True")
            {
                dataObject = CreateDataObject("Metadata", teamConfiguration.MetadataConnection, jsonExportSetting, teamConfiguration, dataGridViewRowsPhysicalModel);
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
        /// <param name="sourceDataObjectName"></param>
        /// <param name="teamConnection"></param>
        /// <param name="jsonExportSetting"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="drivingKeyValue"></param>
        /// <param name="dataGridViewRowsDataObjects"></param>
        /// <param name="dataGridViewRowsPhysicalModel"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static DataObjectMapping SetBusinessKeys(DataObjectMapping dataObjectMapping, string businessKeyDefinition, string sourceDataObjectName, string drivingKeyValue, TeamConnection teamConnection, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration, List<DataGridViewRow> dataGridViewRowsDataObjects, List<DataGridViewRow> dataGridViewRowsPhysicalModel, EventLog eventLog)
        {
            // The list of business keys that will be saved against the data object mapping.
            List<BusinessKeyDefinition> businessKeys = new List<BusinessKeyDefinition>();

            List<BusinessKeyComponentList> businessKeyComponentValueList = GetBusinessKeyComponents(dataObjectMapping, businessKeyDefinition, sourceDataObjectName, drivingKeyValue, teamConnection, teamConfiguration, dataGridViewRowsDataObjects, dataGridViewRowsPhysicalModel, eventLog);

            foreach (BusinessKeyComponentList businessKeyComponentList in businessKeyComponentValueList)
            {
                // Each business key component in the initial list become a mapping of data items. It will be created as a business key together with a surrogate key.
                BusinessKeyDefinition businessKey = new BusinessKeyDefinition();

                // Evaluate the data item mappings that belong to the business key component mapping.
                List<DataItemMapping> businessKeyComponentMapping = new List<DataItemMapping>();

                int iterations = businessKeyComponentList.sourceComponentList.Count;

                // Exception handling. Source and Target component lists must match.
                if (businessKeyComponentList.targetComponentList != null)
                {
                    if (businessKeyComponentList.sourceComponentList.Count > businessKeyComponentList.targetComponentList.Count)
                    {
                        eventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The source business key has more components than the target business key. This is for {sourceDataObjectName} with definition {businessKeyDefinition}. A default value was substituted."));

                        int diff = businessKeyComponentList.sourceComponentList.Count - businessKeyComponentList.targetComponentList.Count;

                        for (int i = 0; i < diff; i++)
                        {
                            var localTargetBusinessKeyComponent = new BusinessKeyComponentElement();
                            localTargetBusinessKeyComponent.businessKeyComponentElement = "Placeholder";
                            localTargetBusinessKeyComponent.businessKeyComponentElementSurrogateKey = "Placeholder";

                            businessKeyComponentList.targetComponentList.Add(localTargetBusinessKeyComponent);
                        }
                    }
                    else
                    {
                        eventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"More source business key components ({businessKeyComponentList.sourceComponentList.Count()}) have been found than target business key components ({businessKeyComponentList.targetComponentList.Count()}). Please check the business key mapping for '{sourceDataObjectName}' and definition '{businessKeyDefinition}' if this worked as expected."));
                    }
                }
                else
                {
                    eventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The target data item for the business key could not be determined. This is for {sourceDataObjectName} with definition {businessKeyDefinition}. A default value was substituted."));
                    businessKeyComponentList.targetComponentList = businessKeyComponentList.sourceComponentList;
                }

                // Iterating over the source business key components to match them to the target business key components in order.
                for (int i = 0; i < iterations; i++)
                {
                    // Exception for Presentation Layer TODO fix as reported issue to add excluded columns in configuration settings
                    // https://github.com/RoelantVos/TEAM/issues/104

                    DataItemMapping businessKeyDataItemMapping = new DataItemMapping();

                    if (new[] { DataObjectTypes.Presentation.ToString(), DataObjectTypes.StagingArea.ToString(), DataObjectTypes.PersistentStagingArea.ToString() }.Contains(dataObjectMapping.MappingClassifications[0].Classification))
                    {
                        // Map the key to itself (workaround as above).
                        businessKeyDataItemMapping = GetBusinessKeyComponentDataItemMapping(businessKeyComponentList.sourceComponentList[i].businessKeyComponentElement, businessKeyComponentList.sourceComponentList[i].businessKeyComponentElement, drivingKeyValue);
                    }
                    else
                    {
                        businessKeyDataItemMapping = GetBusinessKeyComponentDataItemMapping(businessKeyComponentList.sourceComponentList[i].businessKeyComponentElement, businessKeyComponentList.targetComponentList[i].businessKeyComponentElement, drivingKeyValue);
                    }

                    businessKeyComponentMapping.Add(businessKeyDataItemMapping);
                }

                businessKey.BusinessKeyComponentMapping = businessKeyComponentMapping;

                // Evaluate the surrogate key that comes with the business key component mapping.
                businessKey.SurrogateKey = businessKeyComponentList.surrogateKey;

                // If the mapping is for a driving key AND the extension setting is enabled, add the surrogate key as extension.
                if (jsonExportSetting.IsAddDrivingKeyAsBusinessKeyExtension() && dataObjectMapping.MappingClassifications[0].Classification == DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey.ToString())
                {
                    var businessKeyExtensions = new List<Extension>();

                    // The Driving Key is set on the source, but we need to add the SK here. We can do this by position.
                    var test = businessKeyComponentList.sourceComponentList.Select(x => x.isDrivingKey == true);

                    for (int i = 0; i < businessKeyComponentList.sourceComponentList.Count; i++)
                    {
                        if (businessKeyComponentList.sourceComponentList[i].isDrivingKey)
                        {
                            var drivingKeySurrogateKeyExtension = new Extension();
                            drivingKeySurrogateKeyExtension.Key = "DrivingKey";
                            drivingKeySurrogateKeyExtension.Value = businessKeyComponentList.targetComponentList[i].businessKeyComponentElementSurrogateKey;
                            drivingKeySurrogateKeyExtension.Description = "DrivingKey";

                            businessKeyExtensions.Add(drivingKeySurrogateKeyExtension);
                        }
                    }

                    businessKey.Extensions = businessKeyExtensions;
                }

                businessKeys.Add(businessKey);
            }

            dataObjectMapping.BusinessKeys = businessKeys;

            return dataObjectMapping;
        }

        public class BusinessKeyComponentList
        {
            internal string originalTargetDataObject { get; set; }
            internal List<BusinessKeyComponentElement> sourceComponentList { get; set; }
            internal List<BusinessKeyComponentElement> targetComponentList { get; set; }
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
        /// <param name="dataGridViewRowsDataObjects"></param>
        /// <param name="dataGridViewRowsPhysicalModel"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static List<BusinessKeyComponentList>  GetBusinessKeyComponents(DataObjectMapping dataObjectMapping, string businessKeyDefinition, string sourceDataObjectName, string drivingKeyValue, TeamConnection teamConnection, TeamConfiguration teamConfiguration, List<DataGridViewRow> dataGridViewRowsDataObjects, List<DataGridViewRow> dataGridViewRowsPhysicalModel, EventLog eventLog)
        {
            List<BusinessKeyComponentList> businessKeyComponents = new List<BusinessKeyComponentList>();

            // If a business key definition is related to a relationship (e.g. Link), multiple business keys must be asserted.
            // For the relationship itself the complete business key is evaluated.
            // For the individual components, individual business keys must be evaluated.
            // Each of these are business key components (e.g. a 2-way Link has 3 components, a Hub as 1 component)

            int ordinal = 1;

            var mappingType = dataObjectMapping.MappingClassifications[0].Classification;

            if (mappingType == DataObjectTypes.NaturalBusinessRelationship.ToString())
            {
                // Add the full list straight away (the relationships).
                var tempComponent = new BusinessKeyComponentList
                {
                    sourceComponentList = GetBusinessKeySourceComponentElements(businessKeyDefinition, drivingKeyValue)
                };
                businessKeyComponents.Add(tempComponent);

                // This is the Link name.
                tempComponent.originalTargetDataObject = dataObjectMapping.TargetDataObject.Name;

                // Get the target column(s) for the business key, based on the target data object (the Link in this case).
                var tempTargetComponentList = GetBusinessKeyTargetComponentElements(dataObjectMapping.TargetDataObject, businessKeyDefinition, sourceDataObjectName, drivingKeyValue, teamConnection, teamConfiguration, dataGridViewRowsDataObjects, dataGridViewRowsPhysicalModel, eventLog);
                tempComponent.targetComponentList = tempTargetComponentList;

                tempComponent.ordinal = ordinal;

                // Link surrogate key
                var surrogateKey = DeriveSurrogateKey(dataObjectMapping.TargetDataObject, sourceDataObjectName, businessKeyDefinition, teamConnection, teamConfiguration, dataGridViewRowsDataObjects, eventLog);
                tempComponent.surrogateKey = surrogateKey;

                // Add individual key parts (the individual keys) as well.
                var tempList = businessKeyDefinition.Split(',').ToList();

                foreach (string componentElement in tempList)
                {
                    var individualTempComponent = new BusinessKeyComponentList();

                    individualTempComponent.sourceComponentList = GetBusinessKeySourceComponentElements(componentElement, drivingKeyValue);

                    // First, let's get the Hubs for the key. It's the one with the same source and business key definition.
                    // Find the matching physical model row.

                    var dataObjectGridViewRow = dataGridViewRowsDataObjects
                        .Where(r => !r.IsNewRow)
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(componentElement.Trim()))
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName))
                        .FirstOrDefault();

                    if (dataObjectGridViewRow != null)
                    {
                        ordinal++;

                        var originalTargetDataObjectName = dataObjectGridViewRow.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString();
                        var originalSourceDataObjectName = dataObjectGridViewRow.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString();

                        individualTempComponent.originalTargetDataObject = originalTargetDataObjectName;

                        // Get the target column(s) for the business key, based on the target data object.
                        var individualTempTargetComponentList = GetBusinessKeyTargetComponentElements(dataObjectMapping.TargetDataObject, componentElement, originalSourceDataObjectName, drivingKeyValue, teamConnection, teamConfiguration, dataGridViewRowsDataObjects, dataGridViewRowsPhysicalModel, eventLog);
                        individualTempComponent.targetComponentList = individualTempTargetComponentList;
                        individualTempComponent.ordinal = ordinal;

                        // Hub surrogate keys, needs to manage SAl and HAL
                        // This can ONLY be derived from the physical model to cater for same-as scenarios. To be improved.
                        var physicalModelDataGridViewRowList = dataGridViewRowsPhysicalModel
                            .Where(r => !r.IsNewRow)
                            .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString().Equals(tempComponent.originalTargetDataObject))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(tempComponent.surrogateKey))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(teamConfiguration.LoadDateTimeAttribute))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(teamConfiguration.EtlProcessAttribute))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(teamConfiguration.RecordSourceAttribute))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(teamConfiguration.AlternativeSatelliteLoadDateTimeAttribute))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(teamConfiguration.AlternativeLoadDateTimeAttribute))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(teamConfiguration.AlternativeRecordSourceAttribute))
                            .Where(r => !r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(tempComponent.surrogateKey))
                            .ToList();

                        // The physical key order must now be established.
                        int counter = 2;
                        foreach (var physicalModelRow in physicalModelDataGridViewRowList)
                        {
                            if (ordinal == counter)
                            {
                                individualTempComponent.surrogateKey = physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString();
                            }
                            counter++;
                        }
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

                var tempSourceComponentList = GetBusinessKeySourceComponentElements(businessKeyDefinition, drivingKeyValue);
                tempComponent.sourceComponentList = tempSourceComponentList;

                // The associated target data object is just the original one.
                tempComponent.originalTargetDataObject = dataObjectMapping.TargetDataObject.Name;

                // Get the target column(s) for the business key, based on the target data object.
                var tempTargetComponentList = GetBusinessKeyTargetComponentElements(dataObjectMapping.TargetDataObject, businessKeyDefinition, sourceDataObjectName, drivingKeyValue, teamConnection, teamConfiguration, dataGridViewRowsDataObjects, dataGridViewRowsPhysicalModel, eventLog);
                tempComponent.targetComponentList = tempTargetComponentList;

                tempComponent.ordinal = ordinal;

                var surrogateKey = DeriveSurrogateKey(dataObjectMapping.TargetDataObject, sourceDataObjectName, businessKeyDefinition, teamConnection, teamConfiguration, dataGridViewRowsDataObjects, eventLog);
                tempComponent.surrogateKey = surrogateKey;

                businessKeyComponents.Add(tempComponent);
            }

            return businessKeyComponents;
        }

        public class BusinessKeyComponentElement
        {
            public string businessKeyComponentElement { get; set; }
            public string businessKeyComponentElementSurrogateKey { get; set;}
            public bool isDrivingKey { get; set; } = false;
        }


        /// <summary>
        /// Get the target business key component elements in the context of the data item mapping for the business key.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="sourceDataObjectName"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="dataGridViewRowsDataObjects"></param>
        /// <param name="dataGridViewRowsPhysicalModel"></param>
        /// <param name="eventLog"></param>
        /// <returns></returns>
        public static List<BusinessKeyComponentElement> GetBusinessKeyTargetComponentElements(DataObject dataObject, string businessKeyDefinition, string sourceDataObjectName, string drivingKeyValue, TeamConnection teamConnection, TeamConfiguration teamConfiguration, List<DataGridViewRow> dataGridViewRowsDataObjects, List<DataGridViewRow> dataGridViewRowsPhysicalModel, EventLog eventLog)
        {
            List<BusinessKeyComponentElement> targetBusinessKeyComponents = new List<BusinessKeyComponentElement>();
            
            // ReSharper disable once RedundantAssignment
            List<DataObject> lookupDataObjects = new List<DataObject>();

            // Evaluate the data object to lookup the target business key component elements for.
            // For a Context or Relationship Context entity the 'parent' needs to be found.

            var dataObjectType = GetDataObjectType(dataObject.Name, "", teamConfiguration);
            
            if (new[] { DataObjectTypes.Context }.Contains(dataObjectType))
            {
                // Find the parent. This is the data object with the same key definition, but is not a context type entity.
                var dataObjectGridViewRow = dataGridViewRowsDataObjects
                    .Where(r => !r.IsNewRow)
                    .Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(businessKeyDefinition))
                    .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName))
                    .Where(r => !r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(dataObject.Name))
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
                    var dataObjectGridViewRow = dataGridViewRowsDataObjects
                        .Where(r => !r.IsNewRow)
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(businessKeyComponentElement.Trim()))
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName))
                        .Where(r => !r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(dataObject.Name))
                        .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) == DataObjectTypes.CoreBusinessConcept)
                        .FirstOrDefault();

                    if (dataObjectGridViewRow != null)
                    {
                        lookupDataObjects.Add((DataObject)dataObjectGridViewRow.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value);
                    }
                }
            }
            else // Staging, PSA, Hub, other.
            {
                lookupDataObjects.Add(dataObject);
            }

            foreach (var lookupDataObject in lookupDataObjects)
            {
                var physicalModelDataGridViewRow = new List<DataGridViewRow>();

                // Regular known types.
                if (new[] { DataObjectTypes.Context, DataObjectTypes.CoreBusinessConcept, DataObjectTypes.Derived, DataObjectTypes.NaturalBusinessRelationship, DataObjectTypes.NaturalBusinessRelationshipContext, DataObjectTypes.NaturalBusinessRelationshipContext, DataObjectTypes.PersistentStagingArea, DataObjectTypes.Presentation, DataObjectTypes.StagingArea }.Contains(dataObjectType))
                {
                    // Get everything.
                    if (!physicalModelDataGridViewRow.Any())
                    {
                        physicalModelDataGridViewRow = dataGridViewRowsPhysicalModel
                            .Where(r => !r.IsNewRow)
                            .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString().Equals(lookupDataObject.Name))
                            .ToList();
                    }
                }
                else // 'Source', 'Helper', 'Unknown'
                {
                    // Attempt 1 - search for PK values (defined entity types such as Hub, PSA, Link, etc.).
                    physicalModelDataGridViewRow = dataGridViewRowsPhysicalModel
                        .Where(r => !r.IsNewRow)
                        .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString().Equals(lookupDataObject.Name))
                        .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.primaryKeyIndicator].Value.ToString().Equals("Y"))
                        .ToList();

                    // Attempt 2 - search for name similarities using the business key components.
                    if (!physicalModelDataGridViewRow.Any())
                    {
                        var businessKeyComponentElements = GetBusinessKeySourceComponentElements(businessKeyDefinition, drivingKeyValue);

                        foreach (var businessKeyComponentElement in businessKeyComponentElements)
                        {
                            var physicalModelDataGridViewRowIndividualComponent = dataGridViewRowsPhysicalModel
                                .Where(r => !r.IsNewRow)
                                .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString().Equals(lookupDataObject.Name))
                                .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(businessKeyComponentElement.businessKeyComponentElement))
                                .ToList();

                            physicalModelDataGridViewRow.AddRange(physicalModelDataGridViewRowIndividualComponent);

                        }
                    }

                    // Attempt 3 - get everything.
                    if (!physicalModelDataGridViewRow.Any())
                    {
                        physicalModelDataGridViewRow = dataGridViewRowsPhysicalModel
                            .Where(r => !r.IsNewRow)
                            .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString().Equals(lookupDataObject.Name))
                            .ToList();
                    }
                }


                // Sorting separately in ordinal position for debugging purposes. Issues were found in string to int conversion when sorting on ordinal position.
                var orderedList = physicalModelDataGridViewRow.OrderBy(row => Int32.Parse(row.Cells[(int)PhysicalModelMappingMetadataColumns.ordinalPosition].Value.ToString()));

                if (!orderedList.Any())
                {
                    // There are no matching target values. This should not happen and should be caught by the validator. 
                    // But just in case...
                    eventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"There was no matching target business key component found for {dataObject.Name} with business key definition {businessKeyDefinition} in the physical model."));
                }

                foreach (var row in orderedList)
                {
                    var column = row.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString();

                    // Add if it's not a standard element.
                    var surrogateKey = DeriveSurrogateKey(lookupDataObject, sourceDataObjectName, businessKeyDefinition, teamConnection, teamConfiguration, dataGridViewRowsDataObjects, eventLog);

                    var isExcluded = column.IsExcludedBusinessKeyDataItem(dataObjectType, surrogateKey, businessKeyDefinition, teamConnection, teamConfiguration);

                    if (!isExcluded) // If not excluded.
                    {
                        // Get the corresponding Surrogate Key for the (target) component element.
                        var localSurrogateKey = GetSurrogateKey(lookupDataObject, teamConfiguration, teamConnection);

                        var localBusinessKeyTargetComponentElement = new BusinessKeyComponentElement();
                        localBusinessKeyTargetComponentElement.businessKeyComponentElement = column;
                        localBusinessKeyTargetComponentElement.businessKeyComponentElementSurrogateKey = localSurrogateKey;

                        targetBusinessKeyComponents.Add(localBusinessKeyTargetComponentElement);
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

            sourceColumn.Name = sourceBusinessKeyDefinition;
            sourceColumn.IsHardCodedValue = sourceBusinessKeyDefinition.StartsWith("'") && sourceBusinessKeyDefinition.EndsWith("'");

            #region Driving Key

            // Driving Key
            if (sourceBusinessKeyDefinition == drivingKeyValue)
            {
                List<DataClassification> classificationList = new List<DataClassification>();
                DataClassification classification = new DataClassification();
                classification.Classification = "DrivingKey";
                classification.Notes = "The attribute that triggers (drives) the closing of a relationship.";
                classificationList.Add(classification);
                sourceColumn.DataItemClassification = classificationList;
            }

            #endregion

            sourceColumns.Add(sourceColumn);

            keyComponent.SourceDataItems = sourceColumns;
            targetColumn.Name = targetBusinessKeyDefinition;
            keyComponent.TargetDataItem = targetColumn;

            dataItemMapping.SourceDataItems = sourceColumns;
            dataItemMapping.TargetDataItem = targetColumn;

            return dataItemMapping;
        }

        /// <summary>
        /// Returns the list of elements for a given input Business Key string value. Breaking up the original string that represents the business key definition in elements.
        /// </summary>
        /// <param name="businessKeyDefinition"></param>
        /// <returns></returns>
        private static List<BusinessKeyComponentElement> GetBusinessKeySourceComponentElements(string businessKeyDefinition, string drivingKeyValue)
        {
            var businessKeyComponentList = new List<BusinessKeyComponentElement>();

            var temporaryBusinessKeyComponentList = businessKeyDefinition.Split(',').ToList();

            foreach (var keyComponent in temporaryBusinessKeyComponentList)
            {
                var keyPart = keyComponent.Replace("(", "").Replace(")", "").Replace(" ", "");

                if (keyPart.StartsWith("COMPOSITE"))
                {
                    keyPart = keyPart.Replace("COMPOSITE", "");

                    var temporaryKeyPartList = keyPart.Split(';').ToList();
                    foreach (var item in temporaryKeyPartList)
                    {
                        var localBusinessComponentElement = new BusinessKeyComponentElement();
                        localBusinessComponentElement.businessKeyComponentElement = item.Trim();
                        localBusinessComponentElement.businessKeyComponentElementSurrogateKey = "Not applicable";

                        businessKeyComponentList.Add(localBusinessComponentElement);
                    }
                }
                else if (keyPart.StartsWith("CONCATENATE"))
                {
                    keyPart = keyPart.Replace("CONCATENATE", "");
                    keyPart = keyPart.Replace(";", "+");

                    var localBusinessComponentElement = new BusinessKeyComponentElement();
                    localBusinessComponentElement.businessKeyComponentElement = keyPart.Trim();
                    localBusinessComponentElement.businessKeyComponentElementSurrogateKey = "Not applicable";

                    businessKeyComponentList.Add(localBusinessComponentElement);
                }
                else
                {
                    var localBusinessComponentElement = new BusinessKeyComponentElement();
                    localBusinessComponentElement.businessKeyComponentElement = keyPart.Trim();
                    localBusinessComponentElement.businessKeyComponentElementSurrogateKey = "Not applicable";

                    businessKeyComponentList.Add(localBusinessComponentElement);
                }
            }

            // Check for Driving Keys.
            if (!string.IsNullOrEmpty(drivingKeyValue))
            {
                foreach (var localComponent in businessKeyComponentList)
                {
                    if (drivingKeyValue == localComponent.businessKeyComponentElement)
                    {
                        localComponent.isDrivingKey = true;
                    }
                }
            }

            //businessKeyComponentList = businessKeyComponentList.Select(t => t.businessKeyComponentElement.Trim().ToList);
            return businessKeyComponentList;
        }

        /// <summary>
        /// Find out what the parent (referenced to object) is, for a given input data object.
        /// </summary>
        /// <param name="targetDataObjectName"></param>
        /// <param name="sourceDataObjectName"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="dataGridViewRowsDataObjects"></param>
        /// <returns></returns>
        public static List<DataObject> GetParentDataObjects(string targetDataObjectName, string sourceDataObjectName, string businessKeyDefinition, TeamConfiguration teamConfiguration, List<DataGridViewRow>dataGridViewRowsDataObjects)
        {
            var returnValue = new List<DataObject>();

            // Finding the parent for a Satellite or Link Satellite, because this the business key definition is the same for the source- and target.
            var dataObjectGridViewRow = dataGridViewRowsDataObjects
                .Where(r => !r.IsNewRow)
                .Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(businessKeyDefinition))
                .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName))
                .Where(r => !r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(targetDataObjectName))
                .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != DataObjectTypes.Context)
                .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != DataObjectTypes.NaturalBusinessRelationshipContext)
                .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
                .FirstOrDefault();

            if (dataObjectGridViewRow != null)
            {
                returnValue.Add((DataObject)dataObjectGridViewRow.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value);
            }
            else
            {
                // If the value is null for the initial lookup, it can be tried with a key part. This supports finding the parent Hub for a Link because it matches on a part of the business key.
                //var businessKeyComponentElements = GetBusinessKeySourceComponentElements(businessKeyDefinition, "");

                var businessKeyComponentElements = businessKeyDefinition.Split(',').ToList();

                foreach (var businessKeyComponent in businessKeyComponentElements)
                {
                    var dataObjectGridViewRowKeyComponent = dataGridViewRowsDataObjects
                        .Where(r => !r.IsNewRow)
                        //.Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(businessKeyComponent.businessKeyComponentElement)) // Match on business key definition
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(businessKeyComponent)) // Match on business key definition
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName)) // Using same source
                        .Where(r => !r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(targetDataObjectName)) // Not the target
                        .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != DataObjectTypes.Context)
                        .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != DataObjectTypes.NaturalBusinessRelationshipContext)
                        .Where(r => GetDataObjectType(r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(), "", teamConfiguration) != DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
                        .FirstOrDefault();

                    if (dataObjectGridViewRowKeyComponent != null)
                    {
                        returnValue.Add((DataObject)dataObjectGridViewRowKeyComponent.Cells[(int)DataObjectMappingGridColumns.TargetDataObject].Value);
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Evaluate which data object to find the surrogate key for, and then get it.
        /// </summary>
        /// <param name="targetDataObject"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="sourceDataObjectName"></param>
        /// <param name="dataGridViewRowsDataObjects"></param>
        /// <returns>surrogateKey</returns>
        public static string DeriveSurrogateKey(DataObject targetDataObject, string sourceDataObjectName, string businessKeyDefinition, TeamConnection teamConnection, TeamConfiguration teamConfiguration, List<DataGridViewRow> dataGridViewRowsDataObjects, EventLog eventLog)
        {
            // Get the type.
            var dataObjectType = GetDataObjectType(targetDataObject.Name, "", teamConfiguration);
            var surrogateKey = "";

            // If a data object has been evaluated to be a Satellite (or Link-Satellite), replace the data object to query with the parent Hub or Link.
            if (new [] { DataObjectTypes.Context, DataObjectTypes.NaturalBusinessRelationshipContext, DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey}.Contains(dataObjectType))
            {
                try
                {
                    var parentDataObject = GetParentDataObjects(targetDataObject.Name, sourceDataObjectName, businessKeyDefinition, teamConfiguration, dataGridViewRowsDataObjects).FirstOrDefault();

                    if (parentDataObject != null)
                    {
                        surrogateKey = GetSurrogateKey(parentDataObject, teamConfiguration, teamConnection);
                    }
                }
                catch (Exception exception)
                {
                    eventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The parent data object, and therefore the surrogate key could not be identified for '{targetDataObject.Name}'. The reported error is {exception.Message}."));
                }
            }
            else
            {
                // If it's no context table, just get the SK based on the target object itself.
                surrogateKey = GetSurrogateKey(targetDataObject, teamConfiguration, teamConnection);
            }

            return surrogateKey;
        }

        /// <summary>
        /// Return the Surrogate Key for a given table using the TEAM settings (key pattern).
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="teamConnection"></param>
        /// <returns></returns>
        private static string GetSurrogateKey(DataObject dataObject, TeamConfiguration teamConfiguration, TeamConnection teamConnection)
        {
            string returnValue = teamConfiguration.KeyPattern;

            // Get the fully qualified name.
            KeyValuePair<string, string> fullyQualifiedName = GetFullyQualifiedDataObjectName(dataObject, teamConnection).FirstOrDefault();

            //dataObject.Name = fullyQualifiedName.Value;

            var dataObjectType = GetDataObjectType(dataObject.Name, "", teamConfiguration);

            if (returnValue.Contains("{dataObjectType}"))
            {
                if (dataObjectType == DataObjectTypes.StagingArea)
                {
                    returnValue = returnValue.Replace("{dataObjectType}", teamConfiguration.StgTablePrefixValue);
                }
                else if (dataObjectType == DataObjectTypes.PersistentStagingArea)
                {
                    returnValue = returnValue.Replace("{dataObjectType}", teamConfiguration.PsaTablePrefixValue);
                }
                else if (dataObjectType == DataObjectTypes.CoreBusinessConcept)
                {
                    returnValue = returnValue.Replace("{dataObjectType}", teamConfiguration.HubTablePrefixValue);
                }
                else if (dataObjectType == DataObjectTypes.Context)
                {
                    returnValue = returnValue.Replace("{dataObjectType}", teamConfiguration.SatTablePrefixValue);
                }
                else if (dataObjectType == DataObjectTypes.NaturalBusinessRelationship)
                {
                    returnValue = returnValue.Replace("{dataObjectType}", teamConfiguration.LinkTablePrefixValue);
                }
                else if (dataObjectType == DataObjectTypes.NaturalBusinessRelationshipContext)
                {
                    returnValue = returnValue.Replace("{dataObjectType}", teamConfiguration.LsatTablePrefixValue);
                }
                else if (dataObjectType == DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
                {
                    returnValue = returnValue.Replace("{dataObjectType}", teamConfiguration.LsatTablePrefixValue);
                }
                else
                {
                    returnValue = returnValue.Replace("{dataObjectType}", "");
                }
            }

            if (returnValue.Contains("{dataObject.baseName}"))
            {
                if (dataObjectType == DataObjectTypes.StagingArea)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObject.Name.Replace(teamConfiguration.StgTablePrefixValue,""));
                }
                else if (dataObjectType == DataObjectTypes.PersistentStagingArea)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObject.Name.Replace(teamConfiguration.PsaTablePrefixValue, ""));
                }
                else if (dataObjectType == DataObjectTypes.CoreBusinessConcept)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObject.Name.Replace(teamConfiguration.HubTablePrefixValue,""));
                }
                else if (dataObjectType == DataObjectTypes.Context)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObject.Name.Replace(teamConfiguration.SatTablePrefixValue,""));
                }
                else if (dataObjectType == DataObjectTypes.NaturalBusinessRelationship)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObject.Name.Replace(teamConfiguration.LinkTablePrefixValue, ""));
                }
                else if (dataObjectType == DataObjectTypes.NaturalBusinessRelationshipContext)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObject.Name.Replace(teamConfiguration.LsatTablePrefixValue,""));
                }
                else if (dataObjectType == DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObject.Name.Replace(teamConfiguration.LsatTablePrefixValue,""));
                }
                else
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObject.Name);
                }
            }

            if (returnValue.Contains("{keyIdentifier}"))
            {

                returnValue = returnValue.Replace("{keyIdentifier}", teamConfiguration.KeyIdentifier);
            }

            return returnValue;
        }

        /// <summary>
        /// Evaluation of a column whether it should be excluded when used in a business key definition.
        /// </summary>
        /// <param name="dataItemName"></param>
        /// <param name="dataObjectType"></param>
        /// <param name="surrogateKey"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static bool IsExcludedBusinessKeyDataItem(this string dataItemName, DataObjectTypes dataObjectType, string surrogateKey, string businessKeyDefinition, TeamConnection teamConnection, TeamConfiguration teamConfiguration)
        {
            bool returnValue = false;
            
            var businessKeyComponentElements = GetBusinessKeySourceComponentElements(businessKeyDefinition, "");

            if (dataItemName == surrogateKey)
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
            else if (dataItemName == teamConfiguration.EtlProcessUpdateAttribute)
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
            else if (dataItemName == teamConfiguration.CurrentRowAttribute)
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
            else if (dataItemName == teamConfiguration.LoadDateTimeAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.ExpiryDateTimeAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.RecordSourceAttribute)
            {
                returnValue = true;
            }
            // Alternative columns.
            else if (dataItemName == teamConfiguration.AlternativeRecordSourceAttribute)
            { returnValue = true;
            }
            else if (dataItemName == teamConfiguration.AlternativeLoadDateTimeAttribute)
            {
                returnValue = true;
            }
            else if (dataItemName == teamConfiguration.AlternativeSatelliteLoadDateTimeAttribute)
            {
                returnValue = true;
            }
            // Other.
            else if (new[] { DataObjectTypes.StagingArea, DataObjectTypes.PersistentStagingArea }.Contains(dataObjectType))
            {
                foreach (var element in businessKeyComponentElements)
                {
                    if (element.businessKeyComponentElement == dataItemName)
                    {
                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Evaluates if a column / data item should be included as part of a data item mapping.
        /// </summary>
        /// <param name="dataItemName"></param>
        /// <param name="dataObjectType"></param>
        /// <param name="surrogateKey"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static bool IsIncludedDataItem(this string dataItemName, DataObjectTypes dataObjectType, string surrogateKey, TeamConnection teamConnection, TeamConfiguration teamConfiguration)
        {
            bool returnValue = true;

            var exceptionColumns = teamConfiguration.GetExceptionColumns();

            if (dataItemName == teamConfiguration.LoadDateTimeAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.AlternativeRecordSourceAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.AlternativeLoadDateTimeAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.AlternativeSatelliteLoadDateTimeAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.RecordSourceAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.ExpiryDateTimeAttribute)
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
            else if (dataItemName == teamConfiguration.EtlProcessUpdateAttribute)
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
            else if (exceptionColumns.Contains(dataItemName))
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.EventDateTimeAttribute)
            {
                returnValue = false;
            }
            else if (dataItemName == teamConfiguration.CurrentRowAttribute)
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
        /// Add a Multi Active Key classification to a (source) data item.
        /// </summary>
        /// <param name="targetDataItem"></param>
        /// <param name="targetDataObjectName"></param>
        /// <param name="physicalModelGridView"></param>
        public static void AddMultiActiveKeyClassificationToDataItem(DataItem targetDataItem, string targetDataObjectName, DataGridView physicalModelGridView, EventLog eventLog)
        {
            try
            {
                // If the source data item is part of the key, a MAK classification can be added.
                // This is done using a lookup against the physical model.

                var physicalModelGridViewRow = physicalModelGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(targetDataItem.Name))
                    .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString().Equals(targetDataObjectName))
                    .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.primaryKeyIndicator].Value.ToString().Equals("Y"))
                    .FirstOrDefault();

                if (physicalModelGridViewRow != null)
                {
                    List<DataClassification> classificationList = new List<DataClassification>();
                    DataClassification classification = new DataClassification();
                    classification.Classification = "MultiActiveKey";
                    classification.Notes = "The attribute that supports granularity shift in describing context.";
                    classificationList.Add(classification);
                    targetDataItem.DataItemClassification = classificationList;
                }
            }
            catch (Exception ex)
            {
                eventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"An error was encountered: '{ex.Message}'."));
            }
        }
    }
}