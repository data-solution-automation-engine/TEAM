using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DataWarehouseAutomation;
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
        internal static List<Classification> SetMappingClassifications(string dataObjectName, JsonExportSetting jsonExportSetting, TeamConfiguration teamConfiguration)
        {
            var tableType = GetDataObjectType(dataObjectName, "", teamConfiguration);

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
                if (dataObject.dataObjectConnection.extensions != null)
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
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        internal static DataObjectMapping SetBusinessKeys(DataObjectMapping dataObjectMapping, string businessKeyDefinition, TeamConnection teamConnection, TeamConfiguration teamConfiguration)
        {
            List<BusinessKey> businessKeys = new List<BusinessKey>();

            List<BusinessKeyComponentList> businessKeyComponentValueList = GetBusinessKeyComponents(dataObjectMapping, businessKeyDefinition);

            foreach (var businessKeyComponentList in businessKeyComponentValueList)
            {
                // Each business key component in the initial list become a mapping of data items. It will be created as a business key together with a surrogate key.
                BusinessKey businessKey = new BusinessKey();

                // Evaluate the data item mappings that belong to the business key component mapping.
                List<DataItemMapping> businessKeyComponentMapping = new List<DataItemMapping>();
                foreach (var individualComponentList in businessKeyComponentList.componentList)
                {
                    var businessKeyDataItemMapping = GetBusinessKeyComponentDataItemMapping(individualComponentList, individualComponentList);
                    businessKeyComponentMapping.Add(businessKeyDataItemMapping);
                }

                businessKey.businessKeyComponentMapping = businessKeyComponentMapping;

                // Evaluate the surrogate key that comes with the business key component mapping.
                var targetDataObjectSurrogateKey = GetSurrogateKey(businessKeyComponentList.originalDataObject, teamConnection, teamConfiguration);
                businessKey.surrogateKey = targetDataObjectSurrogateKey;

                businessKeys.Add(businessKey);
            }

            dataObjectMapping.businessKeys = businessKeys;

            return dataObjectMapping;
        }

        internal class BusinessKeyComponentList
        {
            internal string originalDataObject { get; set; }
            internal List<string> componentList { get; set; }
        }

        /// <summary>
        /// Evaluate the number of components for the complete business key.
        /// </summary>
        /// <param name="dataObjectMapping"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <returns></returns>
        public static List<BusinessKeyComponentList> GetBusinessKeyComponents(DataObjectMapping dataObjectMapping, string businessKeyDefinition)
        {
            List<BusinessKeyComponentList> businessKeyComponents = new List<BusinessKeyComponentList>();

            // If a business key definition is related to a relationship (e.g. Link), multiple business keys must be asserted.
            // For the relationship itself the complete business key is evaluated.
            // For the individual components, individual business keys must be evaluated.
            // Each of these are business key components (e.g. a 2-way Link has 3 components, a Hub as 1 component)

            var mappingType = dataObjectMapping.mappingClassifications[0].classification;

            if (mappingType == DataObjectTypes.NaturalBusinessRelationship.ToString())
            {
                // Add the full list straight away (the relationships).
                var tempComponent = new BusinessKeyComponentList();
                var tempList = GetBusinessKeyComponentElements(businessKeyDefinition);
                tempComponent.componentList = tempList;
                businessKeyComponents.Add(tempComponent);

                // This is the Link name.
                tempComponent.originalDataObject = dataObjectMapping.targetDataObject.name;

                // Add individual key parts (the individual keys) as well.
                foreach (string componentElement in tempList)
                {
                    var individualTempComponent = new BusinessKeyComponentList();

                    var componentElementList = new List<string>();
                    componentElementList.Add(componentElement);
                    individualTempComponent.componentList = componentElementList;

                    // First, let's get the Hubs for the key. It's the one with the same source and business key definition.
                    // Find the matching physical model row.
                    var singleSourceDataObject = (DataObject)dataObjectMapping.sourceDataObjects[0];
                    string sourceDataObjectName = singleSourceDataObject.name;

                    var dataObjectGridViewRow = _dataGridViewDataObjects.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => !r.IsNewRow)
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString().Equals(componentElement))
                        .Where(r => r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString().Equals(sourceDataObjectName))
                        .FirstOrDefault();

                    if (dataObjectGridViewRow != null)
                    {
                        individualTempComponent.originalDataObject = dataObjectGridViewRow.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString();
                    }
                    else
                    {
                        // Exception - should be picked up by validation
                        individualTempComponent.originalDataObject = "Unknown";
                    }

                    businessKeyComponents.Add(individualTempComponent);
                }
            }
            else
            {
                // Not a relationship, add the list straight away.
                var tempComponent = new BusinessKeyComponentList();

                var tempList = GetBusinessKeyComponentElements(businessKeyDefinition);
                tempComponent.componentList = tempList;

                // The associated target data object is just the original one.
                tempComponent.originalDataObject = dataObjectMapping.targetDataObject.name;

                businessKeyComponents.Add(tempComponent);
            }

            return businessKeyComponents;
        }

        public static DataItemMapping GetBusinessKeyComponentDataItemMapping(string sourceBusinessKeyDefinition, string targetBusinessKeyDefinition)
        {
            DataItemMapping dataItemMapping = new DataItemMapping();

            // Is set to true if there are quotes in the key part.

            DataItemMapping keyComponent = new DataItemMapping();

            List<dynamic> sourceColumns = new List<dynamic>();

            DataItem sourceColumn = new DataItem();
            DataItem targetColumn = new DataItem();

            sourceColumn.name = sourceBusinessKeyDefinition;
            sourceColumn.isHardCodedValue = sourceBusinessKeyDefinition.StartsWith("'") && sourceBusinessKeyDefinition.EndsWith("'");

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
        private static List<string> GetBusinessKeyComponentElements(string businessKeyDefinition)
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