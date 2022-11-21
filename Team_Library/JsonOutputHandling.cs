﻿using System;
using System.Collections.Generic;
using System.Data;
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
            DataObject dataObject = new DataObject {name = dataObjectName};

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
            if (!jsonExportSetting.IsAddDataObjectDataItems())
            {
                dataObject.dataItems = null;
            }
            else if (jsonExportSetting.IsAddDataObjectDataItems())
            {
                var fullyQualifiedName = GetFullyQualifiedDataObjectName(dataObject.name, teamConnection).FirstOrDefault();

                List<dynamic> dataItems = new List<dynamic>();

                foreach (DataGridViewRow physicalModelGridViewRow in dataGridViewRowsPhysicalModel)
                {
                    if (!physicalModelGridViewRow.IsNewRow)
                    {
                        var schemaName = physicalModelGridViewRow.Cells[(int)PhysicalModelMappingMetadataColumns.schemaName].Value.ToString();
                        var tableName = physicalModelGridViewRow.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString();

                        if (fullyQualifiedName.Key == schemaName && fullyQualifiedName.Value == tableName)
                        {
                            DataItem dataItem = new DataItem { name = physicalModelGridViewRow.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString() };

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
                    var dataType = physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.dataType].Value.ToString();

                    dataItem.dataType = dataType;

                    switch (dataType)
                    {
                        case "varchar":
                        case "nvarchar":
                        case "binary":
                            dataItem.characterLength = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.characterLength].Value.ToString());
                            break;
                        case "numeric":
                            dataItem.numericPrecision = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericPrecision].Value.ToString());
                            dataItem.numericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericScale].Value.ToString());
                            break;
                        case "int":
                            // No length etc.
                            break;
                        case "datetime":
                        case "datetime2":
                        case "date":
                            dataItem.numericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericScale].Value.ToString());
                            break;
                    }

                    dataItem.ordinalPosition = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.ordinalPosition].Value.ToString());
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
                DataGridViewRow physicalModelRow = dataGridViewRowsPhysicalModel
                    .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.schemaName].Value.ToString().Equals(fullyQualifiedName.Key))
                    .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString().Equals(fullyQualifiedName.Value))
                    .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString().Equals(dataItem.name))
                    .First();

                if (physicalModelRow != null)
                {
                    var dataType = physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.dataType].Value.ToString();

                    dataItem.dataType = dataType;

                    switch (dataType)
                    {
                        case "varchar":
                        case "nvarchar":
                        case "binary":
                            dataItem.characterLength = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.characterLength].Value.ToString());
                            break;
                        case "numeric":
                            dataItem.numericPrecision = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericPrecision].Value.ToString());
                            dataItem.numericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericScale].Value.ToString());
                            break;
                        case "int":
                            // No length etc.
                            break;
                        case "datetime":
                        case "datetime2":
                        case "date":
                            dataItem.numericScale = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.numericScale].Value.ToString());
                            break;
                    }

                    dataItem.ordinalPosition = int.Parse(physicalModelRow.Cells[(int)PhysicalModelMappingMetadataColumns.ordinalPosition].Value.ToString());
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
        public static List<Classification> SetMappingClassifications(string dataObjectName, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration, string drivingKeyValue)
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
        public static List<DataObject> SetRelatedDataObjects(string dataObjectName, DataGridView dataObjectMappingGrid, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration, EventLog eventLog, List<DataGridViewRow> dataGridViewRowsPhysicalModel)
        {
            List<DataObject> relatedDataObjects = new List<DataObject>();

            #region Add metadata connection as related data object

            // Add the metadata connection as related data object (assuming this is set in the json export settings).
            if (jsonExportSetting.IsAddMetadataAsRelatedDataObject())
            {
                var metaDataObject = GetMetadataDataObject(teamConfiguration, jsonExportSetting, dataGridViewRowsPhysicalModel);

                if (metaDataObject.name != null)
                {
                    relatedDataObjects.Add(metaDataObject);
                }
            }

            #endregion

            #region Add related data object(s)

            if (jsonExportSetting.IsAddRelatedDataObjectsAsRelatedDataObject())
            {
                relatedDataObjects.AddRange(GetLineageRelatedDataObjectList(dataObjectName, dataObjectMappingGrid, jsonExportSetting, teamConfiguration,eventLog, dataGridViewRowsPhysicalModel));
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
        public static List<DataObject> GetLineageRelatedDataObjectList(string targetDataObject, DataGridView dataObjectDataGrid, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration, EventLog eventLog, List<DataGridViewRow> dataGridViewRowsPhysicalModel)
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

                    TeamConnection localConnection = TeamConnection.GetTeamConnectionByConnectionId(localDataObjectConnectionInternalId, teamConfiguration, eventLog);

                    // Set the name and further settings.
                    dataObjectList.Add(CreateDataObject(localDataObjectName, localConnection, jsonExportSetting, teamConfiguration, dataGridViewRowsPhysicalModel));
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
            // Store the extensions that may be there, if any.
            var tempExtensions = new List<Extension>();

            if (dataObject.dataObjectConnection != null && dataObject.dataObjectConnection.extensions != null)
            {
                tempExtensions = dataObject.dataObjectConnection.extensions;
            }

            var dataObjectConnection = new DataConnection
            {
                dataConnectionString = teamConnection.ConnectionKey
            };

            // Re-add extensions, if available.
            if (tempExtensions != null)
            {
                dataObjectConnection.extensions = tempExtensions;
            }

            dataObject.dataObjectConnection = dataObjectConnection;

            return dataObject;
        }

        public static DataQuery SetDataQueryConnection(DataQuery dataQuery, TeamConnection teamConnection, JsonExportSetting jsonExportSetting)
        {
            var dataObjectConnection = new DataConnection { dataConnectionString = teamConnection.ConnectionKey };

            dataQuery.dataQueryConnection = dataObjectConnection;

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
            else if (jsonExportSetting.IsAddDatabaseAsExtension() && dataObject.dataObjectConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();
                List<Extension> returnExtensions = new List<Extension>();

                // Copy any existing classifications already in place, if any.
                if (dataObject.dataObjectConnection.extensions != null)
                {
                    localExtensions = dataObject.dataObjectConnection.extensions;
                }

                // Check if this particular classification already exists before adding.
                // Preserve the others.
                foreach (var extension in localExtensions)
                {
                    if (extension.key != "database")
                    {
                        returnExtensions.Add(extension);
                    }
                }

                var localExtension = new Extension
                {
                    key = "database",
                    value = teamConnection.DatabaseServer.DatabaseName,
                    description = "database name"
                };

                returnExtensions.Add(localExtension);

                dataObject.dataObjectConnection.extensions = returnExtensions;
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
            else if (jsonExportSetting.IsAddDatabaseAsExtension() && dataQuery.dataQueryConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();
                List<Extension> returnExtensions = new List<Extension>();

                // Copy any existing classifications already in place, if any.
                if (dataQuery.dataQueryConnection.extensions != null)
                {
                    localExtensions = dataQuery.dataQueryConnection.extensions;
                }

                // Check if this particular classification already exists before adding.
                foreach (var extension in localExtensions)
                {
                    if (extension.key != "database")
                    {
                        returnExtensions.Add(extension);
                    }
                }

                var localExtension = new Extension
                {
                    key = "database",
                    value = teamConnection.DatabaseServer.DatabaseName,
                    description = "database name"
                };

                returnExtensions.Add(localExtension);

                dataQuery.dataQueryConnection.extensions = returnExtensions;
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
            // Remove an existing extension, if indeed existing.
            // If no extensions exists, do nothing. Otherwise check if one needs removal.
            if (!jsonExportSetting.IsAddSchemaAsExtension())
            {
                if (dataObject.dataObjectConnection?.extensions != null)
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
            else if (jsonExportSetting.IsAddSchemaAsExtension() && dataObject.dataObjectConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();
                List<Extension> returnExtensions = new List<Extension>();

                // Copy any existing extensions that are already in place, if any.
                if (dataObject.dataObjectConnection.extensions != null)
                {
                    localExtensions = dataObject.dataObjectConnection.extensions;
                }

                // Check if this particular classification already exists before adding.
                // Preserve the others.
                foreach (var extension in localExtensions)
                {
                    if (extension.key != "schema")
                    {
                        returnExtensions.Add(extension);
                    }
                }

                // Re-create the schema extension.
                var localExtension = new Extension
                {
                    key = "schema",
                    value = teamConnection.DatabaseServer.SchemaName,
                    description = "schema name"
                };

                returnExtensions.Add(localExtension);

                // Apply all the extensions back to the connection object.
                dataObject.dataObjectConnection.extensions = returnExtensions;
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
            else if (jsonExportSetting.IsAddSchemaAsExtension() && dataQuery.dataQueryConnection != null)
            {
                List<Extension> localExtensions = new List<Extension>();
                List<Extension> returnExtensions = new List<Extension>();

                // Copy any existing classifications already in place, if any.
                if (dataQuery.dataQueryConnection.extensions != null)
                {
                    localExtensions = dataQuery.dataQueryConnection.extensions;
                }

                // Check if this particular classification already exists before adding.
                // Preserve the others.
                foreach (var extension in localExtensions)
                {
                    if (extension.key != "schema")
                    {
                        returnExtensions.Add(extension);
                    }
                }

                var localExtension = new Extension
                {
                    key = "schema",
                    value = teamConnection.DatabaseServer.SchemaName,
                    description = "schema name"
                };

                returnExtensions.Add(localExtension);


                dataQuery.dataQueryConnection.extensions = returnExtensions;
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
        public static DataObject SetDataObjectTypeClassification(DataObject dataObject, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration, string classificationOverrideValue=null)
        {
            string dataObjectType;

            if (classificationOverrideValue != null)
            {
                dataObjectType = classificationOverrideValue;
            }
            else
            {
                dataObjectType = GetDataObjectType(dataObject.name, "", teamConfiguration).ToString();
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
        /// <param name="teamConfiguration"></param>
        /// <param name="drivingKeyValue"></param>
        /// <returns></returns>
        public static DataObjectMapping SetBusinessKeys(DataObjectMapping dataObjectMapping, string businessKeyDefinition, string sourceDataObjectName, TeamConnection teamConnection, TeamConfiguration teamConfiguration, string drivingKeyValue, List<DataGridViewRow> dataGridViewRowsDataObjects, List<DataGridViewRow> dataGridViewRowsPhysicalModel, EventLog eventLog)
        {
            // The list of business keys that will be saved against the data object mapping.
            List<BusinessKey> businessKeys = new List<BusinessKey>();

            List<BusinessKeyComponentList> businessKeyComponentValueList = GetBusinessKeyComponents(dataObjectMapping, businessKeyDefinition, sourceDataObjectName, teamConnection, teamConfiguration, dataGridViewRowsDataObjects, dataGridViewRowsPhysicalModel, eventLog);

            foreach (BusinessKeyComponentList businessKeyComponentList in businessKeyComponentValueList)
            {
                // Each business key component in the initial list become a mapping of data items. It will be created as a business key together with a surrogate key.
                BusinessKey businessKey = new BusinessKey();

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
                            businessKeyComponentList.targetComponentList.Add("Placeholder");
                        }

                    }
                }
                else
                {
                    eventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"The target data item for the business key could not be determined. This is for {sourceDataObjectName} with definition {businessKeyDefinition}. A default value was substituted."));
                    businessKeyComponentList.targetComponentList = businessKeyComponentList.sourceComponentList;
                }

                for (int i = 0; i < iterations; i++)
                {
                    // Exception for Presentation Layer TODO fix as reported issue to add excluded columns in configuration settings
                    // https://github.com/RoelantVos/TEAM/issues/104

                    DataItemMapping businessKeyDataItemMapping = new DataItemMapping();

                    if (dataObjectMapping.mappingClassifications[0].classification == DataObjectTypes.Presentation.ToString())
                    {
                        // Map the key to itself (workaround as above).
                        businessKeyDataItemMapping = GetBusinessKeyComponentDataItemMapping(businessKeyComponentList.sourceComponentList[i], businessKeyComponentList.sourceComponentList[i], drivingKeyValue);
                    }
                    else
                    {
                        businessKeyDataItemMapping = GetBusinessKeyComponentDataItemMapping(businessKeyComponentList.sourceComponentList[i], businessKeyComponentList.targetComponentList[i], drivingKeyValue);
                    }

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

        public class BusinessKeyComponentList
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
        public static List<BusinessKeyComponentList>  GetBusinessKeyComponents(DataObjectMapping dataObjectMapping, string businessKeyDefinition, string sourceDataObjectName, TeamConnection teamConnection, TeamConfiguration teamConfiguration, List<DataGridViewRow> dataGridViewRowsDataObjects, List<DataGridViewRow> dataGridViewRowsPhysicalModel, EventLog eventLog)
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
                var tempComponent = new BusinessKeyComponentList
                {
                    sourceComponentList = GetBusinessKeySourceComponentElements(businessKeyDefinition)
                };
                businessKeyComponents.Add(tempComponent);

                // This is the Link name.
                tempComponent.originalTargetDataObject = dataObjectMapping.targetDataObject.name;

                // Get the target column(s) for the business key, based on the target data object (the Link in this case).
                var tempTargetComponentList = GetBusinessKeyTargetComponentElements(dataObjectMapping.targetDataObject, businessKeyDefinition, sourceDataObjectName, teamConnection, teamConfiguration, dataGridViewRowsDataObjects, dataGridViewRowsPhysicalModel, eventLog);
                tempComponent.targetComponentList = tempTargetComponentList;

                tempComponent.ordinal = ordinal;

                // Link surrogate key
                var surrogateKey = DeriveSurrogateKey(dataObjectMapping.targetDataObject.name, sourceDataObjectName, businessKeyDefinition, teamConnection, teamConfiguration, dataGridViewRowsDataObjects);
                tempComponent.surrogateKey = surrogateKey;

                // Add individual key parts (the individual keys) as well.
                var tempList = businessKeyDefinition.Split(',').ToList();

                foreach (string componentElement in tempList)
                {
                    var individualTempComponent = new BusinessKeyComponentList();

                    individualTempComponent.sourceComponentList = GetBusinessKeySourceComponentElements(componentElement);

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
                        var individualTempTargetComponentList = GetBusinessKeyTargetComponentElements(dataObjectMapping.targetDataObject, componentElement, originalSourceDataObjectName, teamConnection, teamConfiguration, dataGridViewRowsDataObjects, dataGridViewRowsPhysicalModel, eventLog);
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
                var tempTargetComponentList = GetBusinessKeyTargetComponentElements(dataObjectMapping.targetDataObject, businessKeyDefinition, sourceDataObjectName, teamConnection, teamConfiguration, dataGridViewRowsDataObjects, dataGridViewRowsPhysicalModel, eventLog);
                tempComponent.targetComponentList = tempTargetComponentList;

                tempComponent.ordinal = ordinal;

                var surrogateKey = DeriveSurrogateKey(dataObjectMapping.targetDataObject.name, sourceDataObjectName, businessKeyDefinition, teamConnection, teamConfiguration, dataGridViewRowsDataObjects);
                tempComponent.surrogateKey = surrogateKey;

                businessKeyComponents.Add(tempComponent);
            }

            return businessKeyComponents;
        }

        /// <summary>
        /// Get the target business key component elements in the context of the data item mapping for the business key.
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="sourceDataObjectName"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static List<string> GetBusinessKeyTargetComponentElements(DataObject dataObject, string businessKeyDefinition, string sourceDataObjectName, TeamConnection teamConnection, TeamConfiguration teamConfiguration, List<DataGridViewRow> dataGridViewRowsDataObjects, List<DataGridViewRow> dataGridViewRowsPhysicalModel, EventLog eventLog)
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
                var dataObjectGridViewRow = dataGridViewRowsDataObjects
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
                    var dataObjectGridViewRow = dataGridViewRowsDataObjects
                        .Where(r => !r.IsNewRow)
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(businessKeyComponentElement.Trim()))
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName))
                        .Where(r => !r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString().Equals(dataObject.name))
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
                var physicalModelDataGridViewRow = dataGridViewRowsPhysicalModel
                    .Where(r => !r.IsNewRow)
                    .Where(r => r.Cells[(int)PhysicalModelMappingMetadataColumns.tableName].Value.ToString().Equals(lookupDataObject.name))
                    .ToList();

                // Sorting separately for debugging purposes. Issues were found in string to int conversion when sorting on ordinal position.
                var orderedList = physicalModelDataGridViewRow.OrderBy(row => Int32.Parse(row.Cells[(int)PhysicalModelMappingMetadataColumns.ordinalPosition].Value.ToString()));

                if (!orderedList.Any())
                {
                    // There are no matching target values. This should not happen and should be caught by the validator. 
                    // But just in case...
                    eventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"There was no matching target business key component found for {dataObject.name} with business key definition {businessKeyDefinition} in the physical model."));
                }

                foreach (var row in orderedList)
                {
                    var column = row.Cells[(int)PhysicalModelMappingMetadataColumns.columnName].Value.ToString();

                    // Add if it's not a standard element.
                    var surrogateKey = DeriveSurrogateKey(lookupDataObject.name, sourceDataObjectName, businessKeyDefinition, teamConnection, teamConfiguration, dataGridViewRowsDataObjects);

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

            #region Driving Key

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

            #endregion

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

        public static string GetParentDataObject(string targetDataObjectName, string sourceDataObjectName, string businessKeyDefinition, TeamConfiguration teamConfiguration, List<DataGridViewRow>dataGridViewRowsDataObjects)
        {
            string returnValue = "";

            var dataObjectGridViewRow = dataGridViewRowsDataObjects
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
        /// Evaluate which data object to find the surrogate key for, and then get it.
        /// </summary>
        /// <param name="targetDataObjectName"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="sourceDataObjectName"></param>
        /// <param name="dataGridViewRowsDataObjects"></param>
        /// <returns>surrogateKey</returns>
        public static string DeriveSurrogateKey(string targetDataObjectName, string sourceDataObjectName, string businessKeyDefinition, TeamConnection teamConnection, TeamConfiguration teamConfiguration, List<DataGridViewRow> dataGridViewRowsDataObjects)
        {
            // Get the type
            var dataObjectType = GetDataObjectType(targetDataObjectName, "", teamConfiguration);

            // If a data object has been evaluated to be a Satellite (or Link-Satellite), replace the data object to query with the parent Hub or Link.
            if (new [] { DataObjectTypes.Context, DataObjectTypes.NaturalBusinessRelationshipContext, DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey}.Contains(dataObjectType))
            {
                targetDataObjectName = GetParentDataObject(targetDataObjectName, sourceDataObjectName, businessKeyDefinition, teamConfiguration, dataGridViewRowsDataObjects);
            }

            var surrogateKey = GetSurrogateKey(targetDataObjectName, teamConfiguration, teamConnection);

            return surrogateKey;
        }

        /// <summary>
        /// Return the Surrogate Key for a given table using the TEAM settings (key pattern).
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="teamConfiguration"></param>
        /// <param name="teamConnection"></param>
        /// <returns></returns>
        private static string GetSurrogateKey(string dataObjectName, TeamConfiguration teamConfiguration, TeamConnection teamConnection)
        {
            string returnValue = teamConfiguration.KeyPattern;

            // Get the fully qualified name.
            KeyValuePair<string, string> fullyQualifiedName = GetFullyQualifiedDataObjectName(dataObjectName, teamConnection).FirstOrDefault();

            dataObjectName = fullyQualifiedName.Value;

            var dataObjectType = GetDataObjectType(dataObjectName, "", teamConfiguration);

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
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObjectName.Replace(teamConfiguration.StgTablePrefixValue,""));
                }
                else if (dataObjectType == DataObjectTypes.PersistentStagingArea)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObjectName.Replace(teamConfiguration.PsaTablePrefixValue, ""));
                }
                else if (dataObjectType == DataObjectTypes.CoreBusinessConcept)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObjectName.Replace(teamConfiguration.HubTablePrefixValue,""));
                }
                else if (dataObjectType == DataObjectTypes.Context)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObjectName.Replace(teamConfiguration.SatTablePrefixValue,""));
                }
                else if (dataObjectType == DataObjectTypes.NaturalBusinessRelationship)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObjectName.Replace(teamConfiguration.LinkTablePrefixValue, ""));
                }
                else if (dataObjectType == DataObjectTypes.NaturalBusinessRelationshipContext)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObjectName.Replace(teamConfiguration.LsatTablePrefixValue,""));
                }
                else if (dataObjectType == DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObjectName.Replace(teamConfiguration.LsatTablePrefixValue,""));
                }
                else
                {
                    returnValue = returnValue.Replace("{dataObject.baseName}", dataObjectName);
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
            
            var businessKeyComponentElements = GetBusinessKeySourceComponentElements(businessKeyDefinition);

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
            else if (new[] { DataObjectTypes.StagingArea, DataObjectTypes.PersistentStagingArea }.Contains(dataObjectType) && !businessKeyComponentElements.Contains(dataItemName))
            {
                returnValue = true;
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
    }
}