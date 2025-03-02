﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataWarehouseAutomation;
using Newtonsoft.Json;
using DataObject = DataWarehouseAutomation.DataObject;

namespace TEAM_Library
{
    /// <summary>
    /// Enumerator to hold the column index for the columns (headers) in the Table Metadata data grid view.
    /// </summary>
    public enum DataObjectMappingGridColumns
    {
        Enabled = 0,
        HashKey = 1,
        SourceConnection = 2,
        SourceDataObject = 3,
        TargetConnection = 4,
        TargetDataObject = 5,
        BusinessKeyDefinition = 6,
        DrivingKeyDefinition = 7,
        FilterCriterion = 8,
        DataObjectMappingExtension = 9,
        // The below are hidden, for sorting and back-end management only.
        SourceDataObjectName = 10,
        TargetDataObjectName = 11,
        PreviousTargetDataObjectName = 12,
        SurrogateKey = 13,
        // Hidden in the main table, but can be set via the JSON editor
        DataObjectMappingClassification = 14,
    }

    public class TeamDataObjectMappings
    {
        public SortableBindingList<DataObjectMappingsFileCombination> DataObjectMappingsFileCombinations { get; set; }

        public EventLog EventLog { get; set; }

        public DataTable DataTable { get; set; }

        public TeamDataObjectMappings(TeamDataObjectMappingsFileCombinations teamDataObjectMappingsFileCombinations)
        {
            DataObjectMappingsFileCombinations = teamDataObjectMappingsFileCombinations.DataObjectMappingsFileCombinations;

            EventLog = new EventLog();

            DataTable = new DataTable();

            DataTable.Columns.Add(DataObjectMappingGridColumns.Enabled.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.HashKey.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.SourceConnection.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.SourceDataObject.ToString(), typeof(DataObject));
            DataTable.Columns.Add(DataObjectMappingGridColumns.TargetConnection.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.TargetDataObject.ToString(), typeof(DataObject));
            DataTable.Columns.Add(DataObjectMappingGridColumns.BusinessKeyDefinition.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.DrivingKeyDefinition.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.FilterCriterion.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.DataObjectMappingExtension.ToString());
            // For sorting purposes only.
            DataTable.Columns.Add(DataObjectMappingGridColumns.SourceDataObjectName.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.TargetDataObjectName.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.PreviousTargetDataObjectName.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.SurrogateKey.ToString());
            // Hidden, but editable.
            DataTable.Columns.Add(DataObjectMappingGridColumns.DataObjectMappingClassification.ToString());
        }

        /// <summary>
        /// Build and deploy a data table using the JSON files.
        /// </summary>
        public void SetDataTable(TeamConfiguration teamConfiguration)
        {
            foreach (var dataObjectMappingFileCombination in DataObjectMappingsFileCombinations)
            {
                foreach (var dataObjectMapping in dataObjectMappingFileCombination.DataObjectMappings.DataObjectMappings)
                {
                    #region Data Object Mapping level extensions

                    string dataObjectMappingExtension = "";

                    if (dataObjectMapping.Extensions != null)
                    {
                        //dataObjectMappingExtension = JsonConvert.SerializeObject(dataObjectMapping.Extensions, Formatting.Indented);
                        dataObjectMappingExtension = JsonConvert.SerializeObject(dataObjectMapping.Extensions);
                    }

                    #endregion

                    #region Data Object Mapping level classification

                    string dataObjectMappingClassification = "";

                    if (dataObjectMapping.MappingClassifications != null)
                    {
                        dataObjectMappingClassification = JsonConvert.SerializeObject(dataObjectMapping.MappingClassifications, Formatting.Indented);
                    }

                    #endregion

                    #region region Target Data Object

                    var targetDataObject = dataObjectMapping.TargetDataObject;

                    string targetConnectionInternalId = teamConfiguration.MetadataConnection.ConnectionInternalId;

                    if (dataObjectMapping.TargetDataObject.DataObjectConnection != null)
                    {
                        targetConnectionInternalId = TeamConnection.GetTeamConnectionByConnectionKey(dataObjectMapping.TargetDataObject.DataObjectConnection.DataConnectionString, teamConfiguration, EventLog).ConnectionInternalId;
                    }

                    #endregion

                    string filterCriterion = dataObjectMapping.FilterCriterion;
                    string drivingKeyDefinition = "";

                    #region Business Key Definition

                    string businessKeyDefinitionString = "";
                    int businessKeyCounter = 1;

                    if (dataObjectMapping.BusinessKeys != null)
                    {
                        foreach (var businessKey in dataObjectMapping.BusinessKeys)
                        {
                            // For Links, skip the first business key (because it's the full key).
                            if (businessKeyCounter == 1 && targetDataObject.Name.IsDataVaultLink(teamConfiguration))
                            {
                                // Do nothing.
                            }
                            else
                            {
                                // If there is more than 1 data item mapping / business key component mapping then COMPOSITE ;
                                if (businessKey.BusinessKeyComponentMapping.Count > 1)
                                {
                                    businessKeyDefinitionString += "COMPOSITE(";
                                }

                                if (businessKey.BusinessKeyClassification != null && businessKey.BusinessKeyClassification[0].Classification.Contains("DegenerateAttribute"))
                                    continue;

                                foreach (var dataItemMapping in businessKey.BusinessKeyComponentMapping)
                                {
                                    foreach (var dataItem in dataItemMapping.SourceDataItems)
                                    {
                                        try
                                        {
                                            // The new data item.
                                            DataItem tempDataItem = new DataItem();

                                            var intermediateJson = JsonConvert.SerializeObject(dataItem);

                                            // Workaround, if the source is a data query it will be cast as data item because the c# data table does not support dynamic types as column definitions.
                                            if (JsonConvert.DeserializeObject(intermediateJson).ContainsKey("dataQueryCode"))
                                            {
                                                DataQuery dataQuery = JsonConvert.DeserializeObject<DataQuery>(intermediateJson);
                                                tempDataItem.Name = "`" + dataQuery.DataQueryCode + "`";

                                                businessKeyDefinitionString += tempDataItem.Name;
                                                businessKeyDefinitionString += ";";
                                            }
                                            else
                                            {
                                                // It must be a DataItem so it can be safely cast.
                                                tempDataItem = dataItem.ToObject<DataItem>();

                                                // Explicitly type-cast the value as string to avoid issues using dynamic type.
                                                string dataItemName = tempDataItem.Name;

                                                if (dataItemName.Contains("+"))
                                                {
                                                    businessKeyDefinitionString += $"CONCATENATE({tempDataItem.Name})".Replace("+", ";");
                                                }
                                                else
                                                {
                                                    businessKeyDefinitionString += dataItemName;
                                                }

                                                businessKeyDefinitionString += ";";

                                                // Evaluate if a Driving Key needs to be set.
                                                if (tempDataItem.DataItemClassification != null)
                                                {
                                                    List<DataClassification> classifications = tempDataItem.DataItemClassification;

                                                    if (classifications[0].Classification == "DrivingKey")
                                                    {
                                                        drivingKeyDefinition = dataItemName;
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception exception)
                                        {
                                            EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error occurred interpreting a data item: "+exception.Message));
                                        }
                                    }
                                }

                                businessKeyDefinitionString = businessKeyDefinitionString.TrimEnd(';');

                                // If there is more than 1 data item mapping / business key component mapping then COMPOSITE.
                                if (businessKey.BusinessKeyComponentMapping.Count > 1)
                                {
                                    businessKeyDefinitionString += ")";
                                }

                                businessKeyDefinitionString += ",";
                            }

                            businessKeyCounter++;
                        }

                        businessKeyDefinitionString = businessKeyDefinitionString.TrimEnd(',');
                    }

                    #endregion

                    #region Source Data Objects
                    foreach (var sourceDataObject in dataObjectMapping.SourceDataObjects)
                    {
                        // The new data object.
                        DataObject singleSourceDataObject = new DataObject();

                        // Default connection, to be updated later.
                        string sourceConnectionInternalId = teamConfiguration.MetadataConnection.ConnectionInternalId;

                        var intermediateJson = JsonConvert.SerializeObject(sourceDataObject);
                        
                        // Workaround, if the source is a data query it will be cast as data object because the data table does not support dynamic types.
                        if (JsonConvert.DeserializeObject(intermediateJson).ContainsKey("dataQueryCode"))
                        {
                            DataQuery tempDataObject = JsonConvert.DeserializeObject<DataQuery>(intermediateJson);

                            singleSourceDataObject.Name = "`"+tempDataObject.DataQueryCode+"`";

                            if (tempDataObject.DataQueryConnection != null)
                            {
                                singleSourceDataObject.DataObjectConnection = tempDataObject.DataQueryConnection;

                                string sourceConnectionString = tempDataObject.DataQueryConnection.DataConnectionString;
                                sourceConnectionInternalId = TeamConnection.GetTeamConnectionByConnectionKey(sourceConnectionString, teamConfiguration, EventLog).ConnectionInternalId;
                            }
                        }
                        else
                        {
                            singleSourceDataObject = JsonConvert.DeserializeObject<DataObject>(intermediateJson);

                            if (sourceDataObject.dataObjectConnection != null)
                            {
                                string sourceConnectionString = sourceDataObject.dataObjectConnection.dataConnectionString;
                                sourceConnectionInternalId = TeamConnection.GetTeamConnectionByConnectionKey(sourceConnectionString, teamConfiguration, EventLog).ConnectionInternalId;
                            }
                        }

                        var newRow = DataTable.NewRow();

                        string[] hashKey =
                        {
                            singleSourceDataObject.Name, targetDataObject.Name, businessKeyDefinitionString, drivingKeyDefinition, filterCriterion
                        };

                        string surrogateKey = "";

                        if (dataObjectMapping.BusinessKeys != null)
                        {
                            surrogateKey = dataObjectMapping.BusinessKeys[0].SurrogateKey;
                        }

                        Utility.CreateMd5(hashKey, Utility.SandingElement);

                        newRow[(int)DataObjectMappingGridColumns.Enabled] = dataObjectMapping.Enabled;
                        newRow[(int)DataObjectMappingGridColumns.HashKey] = hashKey;
                        newRow[(int)DataObjectMappingGridColumns.SourceConnection] = sourceConnectionInternalId;
                        newRow[(int)DataObjectMappingGridColumns.SourceDataObject] = singleSourceDataObject;
                        newRow[(int)DataObjectMappingGridColumns.TargetConnection] = targetConnectionInternalId;
                        newRow[(int)DataObjectMappingGridColumns.TargetDataObject] = targetDataObject;
                        newRow[(int)DataObjectMappingGridColumns.BusinessKeyDefinition] = businessKeyDefinitionString;
                        newRow[(int)DataObjectMappingGridColumns.DrivingKeyDefinition] = drivingKeyDefinition;
                        newRow[(int)DataObjectMappingGridColumns.FilterCriterion] = filterCriterion;
                        // Hidden column
                        newRow[(int)DataObjectMappingGridColumns.DataObjectMappingExtension] = dataObjectMappingExtension;
                        // Sorting only.
                        newRow[(int)DataObjectMappingGridColumns.SourceDataObjectName] = singleSourceDataObject.Name;
                        newRow[(int)DataObjectMappingGridColumns.TargetDataObjectName] = targetDataObject.Name;
                        newRow[(int)DataObjectMappingGridColumns.PreviousTargetDataObjectName] = targetDataObject.Name;
                        newRow[(int)DataObjectMappingGridColumns.SurrogateKey] = surrogateKey;
                        // Hidden column
                        newRow[(int)DataObjectMappingGridColumns.DataObjectMappingClassification] = dataObjectMappingClassification;
                        DataTable.Rows.Add(newRow);
                    }
                    #endregion
                }
            }

            SetDataTableColumnNames(DataTable);

            SynchroniseLinkSatelliteBusinessKeyDefinition(teamConfiguration);
        }

        /// <summary>
        /// Update the DataTable to ensure the LSAT business key definition matches the LNK one.
        /// </summary>
        /// <param name="teamConfiguration"></param>
        private void SynchroniseLinkSatelliteBusinessKeyDefinition(TeamConfiguration teamConfiguration)
        {
            // Workaround to match LSAT keys to LNK keys, because there is no way to reverse-engineer the original TEAM definition based on the JSON structure.
            // This is because LNK has the original business keys, as well as the LNK key (which can be ignored) to reconstruct the string value.
            // But, the LSAT only has the LNK key.

            foreach (DataRow row in DataTable.Rows)
            {
                // Only if it concerns an LSAT...
                if (row[(int)DataObjectMappingGridColumns.TargetDataObjectName].ToString().IsDataVaultLinkSatellite(teamConfiguration))
                {
                    // For LSATs only, look up the corresponding LNK and re-use that business key.
                    string surrogateKey = (string)row[(int)DataObjectMappingGridColumns.SurrogateKey];
                    string filterCriterion = (string)row[(int)DataObjectMappingGridColumns.FilterCriterion];

                    string selectionQuery = DataObjectMappingGridColumns.SourceDataObjectName + " = '" + row[(int)DataObjectMappingGridColumns.SourceDataObjectName] + "' AND " +
                                             DataObjectMappingGridColumns.SurrogateKey + " = '" + surrogateKey + "' AND " +
                                             DataObjectMappingGridColumns.TargetDataObjectName + " <> '" + row[(int)DataObjectMappingGridColumns.TargetDataObjectName] + "' AND " +
                                             DataObjectMappingGridColumns.FilterCriterion + " = '" + filterCriterion + "'";

                    // Should be only one return value.
                    var dataTableLookup = DataTable.Select(selectionQuery).FirstOrDefault();

                    // Update the LSAT business key, if available
                    if (dataTableLookup != null && dataTableLookup[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].ToString() != null)
                    {
                        row[(int)DataObjectMappingGridColumns.BusinessKeyDefinition] = dataTableLookup[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].ToString();
                    }
                    else
                    {
                        row[(int)DataObjectMappingGridColumns.BusinessKeyDefinition] = "Placeholder";
                    }
                }
            }
        }

        /// <summary>
        /// Set the header / column names for each data table column.
        /// </summary>
        public static void SetDataTableColumnNames(DataTable dataTable)
        {
            try
            {
                dataTable.Columns[(int)DataObjectMappingGridColumns.Enabled].ColumnName = DataObjectMappingGridColumns.Enabled.ToString();
                dataTable.Columns[(int)DataObjectMappingGridColumns.HashKey].ColumnName = DataObjectMappingGridColumns.HashKey.ToString();
                dataTable.Columns[(int)DataObjectMappingGridColumns.SourceConnection].ColumnName = DataObjectMappingGridColumns.SourceConnection.ToString();
                dataTable.Columns[(int)DataObjectMappingGridColumns.SourceDataObject].ColumnName = DataObjectMappingGridColumns.SourceDataObject.ToString();
                dataTable.Columns[(int)DataObjectMappingGridColumns.TargetConnection].ColumnName = DataObjectMappingGridColumns.TargetConnection.ToString();
                dataTable.Columns[(int)DataObjectMappingGridColumns.TargetDataObject].ColumnName = DataObjectMappingGridColumns.TargetDataObject.ToString();
                dataTable.Columns[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].ColumnName = DataObjectMappingGridColumns.BusinessKeyDefinition.ToString();
                dataTable.Columns[(int)DataObjectMappingGridColumns.DrivingKeyDefinition].ColumnName = DataObjectMappingGridColumns.DrivingKeyDefinition.ToString();
                dataTable.Columns[(int)DataObjectMappingGridColumns.FilterCriterion].ColumnName = DataObjectMappingGridColumns.FilterCriterion.ToString();
                dataTable.Columns[(int)DataObjectMappingGridColumns.DataObjectMappingExtension].ColumnName = DataObjectMappingGridColumns.DataObjectMappingExtension.ToString();

                // The below are hidden, for sorting and back-end management only.
                if (!dataTable.Columns.Contains(DataObjectMappingGridColumns.SourceDataObjectName.ToString()))
                {
                    dataTable.Columns[(int)DataObjectMappingGridColumns.SourceDataObjectName].ColumnName = DataObjectMappingGridColumns.SourceDataObjectName.ToString();
                }

                if (!dataTable.Columns.Contains(DataObjectMappingGridColumns.TargetDataObjectName.ToString()))
                {
                    dataTable.Columns[(int)DataObjectMappingGridColumns.TargetDataObjectName].ColumnName = DataObjectMappingGridColumns.TargetDataObjectName.ToString();
                }

                if (!dataTable.Columns.Contains(DataObjectMappingGridColumns.PreviousTargetDataObjectName.ToString()))
                {
                    dataTable.Columns[(int)DataObjectMappingGridColumns.PreviousTargetDataObjectName].ColumnName = DataObjectMappingGridColumns.PreviousTargetDataObjectName.ToString();
                }

                if (!dataTable.Columns.Contains(DataObjectMappingGridColumns.SurrogateKey.ToString()))
                {
                    dataTable.Columns[(int)DataObjectMappingGridColumns.SurrogateKey].ColumnName = DataObjectMappingGridColumns.SurrogateKey.ToString();
                }

                // Hidden in the main table, but can be set via the JSON editor
                if (!dataTable.Columns.Contains(DataObjectMappingGridColumns.DataObjectMappingClassification.ToString()))
                {
                    dataTable.Columns[(int)DataObjectMappingGridColumns.DataObjectMappingClassification].ColumnName = DataObjectMappingGridColumns.DataObjectMappingClassification.ToString();
                }
            }
            catch (Exception exception)
            {
                // TODO: return event log issue

            }
        }

        /// <summary>
        /// Using the Data Object Mappings, create a list of Subject Areas and their contents.
        /// </summary>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public List<Tuple<string, string, string>> SubjectAreaList(TeamConfiguration teamConfiguration)
        {
            var subjectAreaList = new List<Tuple<string, string, string>>();

            foreach (DataRow row in DataTable.Rows)
            {
                string sourceObject = (string)row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()];
                string targetObject = (string)row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()];

                var targetObjectType = MetadataHandling.GetDataObjectType(targetObject, "", teamConfiguration);
                var targetObjectBusinessKey = (string)row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()];

                if (targetObjectType == MetadataHandling.DataObjectTypes.CoreBusinessConcept)
                {
                    string subjectArea = targetObject.Replace(teamConfiguration.HubTablePrefixValue + "_", "");

                    // Retrieve the related objects (Context Tables in this case)
                    var results = from localRow in DataTable.AsEnumerable()
                        where localRow.Field<string>(DataObjectMappingGridColumns.SourceDataObjectName.ToString()) == sourceObject && // Is in the same source cluster
                              localRow.Field<string>(DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()).Contains(targetObjectBusinessKey) && // Contains a part of the business key
                              MetadataHandling.GetDataObjectType(localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObjectName.ToString()), "", teamConfiguration) != MetadataHandling.DataObjectTypes.NaturalBusinessRelationship &&
                              MetadataHandling.GetDataObjectType(localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObjectName.ToString()), "", teamConfiguration) != MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext
                        select localRow;

                    foreach (DataRow detailRow in results)
                    {
                        var targetObjectDetail = (string)detailRow[DataObjectMappingGridColumns.TargetDataObjectName.ToString()];

                        bool tupleAlreadyExists = subjectAreaList.Any(m => m.Item1 == subjectArea && m.Item2 == targetObject && m.Item3 == targetObjectDetail);

                        if (!tupleAlreadyExists)
                        {
                            subjectAreaList.Add(new Tuple<string, string, string>(subjectArea, targetObject, targetObjectDetail));
                        }
                    }
                }

                if (targetObjectType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationship)
                {
                    string subjectArea = targetObject.Replace(teamConfiguration.LinkTablePrefixValue + "_", "");

                    // Retrieve the related objects (relationship context tables in this case)
                    var results = from localRow in DataTable.AsEnumerable()
                                  where localRow.Field<string>(DataObjectMappingGridColumns.SourceDataObjectName.ToString()) == sourceObject && // Is in the same source cluster
                                        localRow.Field<string>(DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()).Contains(targetObjectBusinessKey) && // Contains a part of the business key
                                        MetadataHandling.GetDataObjectType(localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObjectName.ToString()), "", teamConfiguration) != MetadataHandling.DataObjectTypes.CoreBusinessConcept // Is a relationship context table.
                                  select localRow;

                    foreach (DataRow detailRow in results)
                    {
                        var targetObjectDetail = (string)detailRow[DataObjectMappingGridColumns.TargetDataObjectName.ToString()];

                        bool tupleAlreadyExists = subjectAreaList.Any(m => m.Item1 == subjectArea && m.Item2 == targetObject && m.Item3 == targetObjectDetail);

                        if (!tupleAlreadyExists)
                        {
                            subjectAreaList.Add(new Tuple<string, string, string>(subjectArea, targetObject, targetObjectDetail));
                        }
                    }
                }
            }

            return subjectAreaList;
        }

        public List<TeamConnection> GetConnectionList(Dictionary<string, TeamConnection> connectionDictionary)
        {
            // Create the group nodes (systems)
            var connectionList = new List<TeamConnection>();

            var sourceConnectionList = DataTable.AsEnumerable().Select(r => r.Field<string>(DataObjectMappingGridColumns.SourceConnection.ToString())).ToList();
            var targetConnectionList = DataTable.AsEnumerable().Select(r => r.Field<string>(DataObjectMappingGridColumns.TargetConnection.ToString())).ToList();

            foreach (string connection in sourceConnectionList)
            {
                if (connection != null)
                {
                    var connectionProfile = TeamConfiguration.GetTeamConnectionByInternalId(connection, connectionDictionary);

                    if (!connectionList.Contains(connectionProfile))
                    {
                        connectionList.Add(connectionProfile);
                    }
                }
            }

            foreach (string connection in targetConnectionList)
            {
                if (connection != null)
                {
                    var connectionProfile = TeamConfiguration.GetTeamConnectionByInternalId(connection, connectionDictionary);

                    if (!connectionList.Contains(connectionProfile))
                    {
                        connectionList.Add(connectionProfile);
                    }
                }
            }

            return connectionList;
        }

        public List<Tuple<string, string>> BusinessConceptRelationshipList(TeamConfiguration configurationSetting)
        {
            DataView sourceContainerView = new DataView((DataTable)DataTable);
            DataTable distinctValues = sourceContainerView.ToTable(true, DataObjectMappingGridColumns.SourceDataObjectName.ToString());
            var businessConceptRelationshipList = new List<Tuple<string, string>>();

            foreach (DataRow sourceContainerRow in distinctValues.Rows)
            {
                string sourceContainer = (string)sourceContainerRow[DataObjectMappingGridColumns.SourceDataObjectName.ToString()];

                foreach (DataRow row in DataTable.Rows)
                {
                    string sourceObject = (string)row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()];

                    if (sourceContainer == sourceObject)
                    {
                        //var sourceObject = (string) row[TableMappingMetadataColumns.SourceTable.ToString()];
                        var targetObject = (string)row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()];

                        var targetObjectType = MetadataHandling.GetDataObjectType(targetObject, "", configurationSetting);
                        var targetObjectBusinessKey = (string)row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()];

                        if (targetObjectType == MetadataHandling.DataObjectTypes.CoreBusinessConcept)
                        {
                            // Retrieve the related objects to a CBC.
                            var cbcResults = from localRow in DataTable.AsEnumerable()
                                             where localRow.Field<string>(DataObjectMappingGridColumns.SourceDataObjectName.ToString()) == sourceObject && // Is in the same source cluster
                                                   localRow.Field<string>(DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()).Contains(targetObjectBusinessKey) && // Contains a part of the business key
                                                   localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObjectName.ToString()) != targetObject && // Is not itself
                                                   MetadataHandling.GetDataObjectType(localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObjectName.ToString()), "", configurationSetting) == MetadataHandling.DataObjectTypes.NaturalBusinessRelationship // Is a NBR.
                                             select localRow;

                            foreach (DataRow detailRow in cbcResults)
                            {
                                var targetObjectDetail = (string)detailRow[DataObjectMappingGridColumns.TargetDataObjectName.ToString()];
                                businessConceptRelationshipList.Add(new Tuple<string, string>(targetObject, targetObjectDetail));
                            }
                        }
                    }
                }
            }

            return businessConceptRelationshipList;
        }
    }

    public class TableMappingJson
    {
        //JSON representation of the table mapping metadata
        public bool enabledIndicator { get; set; }
        public string tableMappingHash { get; set; }
        public string sourceTable { get; set; }
        public string sourceConnectionKey { get; set; }
        public string targetTable { get; set; }
        public string targetConnectionKey { get; set; }
        public string businessKeyDefinition { get; set; }
        public string drivingKeyDefinition { get; set; }
        public string filterCriteria { get; set; }

        public List<TableMappingJson> JsonList { get; set; }
    }
}
