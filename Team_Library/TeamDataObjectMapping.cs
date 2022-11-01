using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataWarehouseAutomation;
using Newtonsoft.Json;
using DataObject = DataWarehouseAutomation.DataObject;

namespace TEAM_Library
{
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

            // For sorting purposes only.
            DataTable.Columns.Add(DataObjectMappingGridColumns.SourceDataObjectName.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.TargetDataObjectName.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.PreviousTargetDataObjectName.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.SurrogateKey.ToString());
        }

        /// <summary>
        /// Build and deploy a data table using the JSON files.
        /// </summary>
        public void SetDataTable(TeamConfiguration teamConfiguration)
        {
            foreach (var dataObjectMappingFileCombination in DataObjectMappingsFileCombinations)
            {
                foreach (var dataObjectMapping in dataObjectMappingFileCombination.DataObjectMappings.dataObjectMappings)
                {
                    #region region Target Data Object

                    var targetDataObject = dataObjectMapping.targetDataObject;

                    string targetConnectionInternalId = teamConfiguration.MetadataConnection.ConnectionInternalId;

                    if (dataObjectMapping.targetDataObject.dataObjectConnection != null)
                    {
                        targetConnectionInternalId = TeamConnection.GetTeamConnectionByConnectionKey(dataObjectMapping.targetDataObject.dataObjectConnection.dataConnectionString, teamConfiguration).ConnectionInternalId;
                    }

                    #endregion

                    string filterCriterion = dataObjectMapping.filterCriterion;
                    string drivingKeyDefinition = "";

                    #region Business Key Definition

                    string businessKeyDefinitionString = "";
                    int businessKeyCounter = 1;

                    if (dataObjectMapping.businessKeys != null)
                    {
                        foreach (var businessKey in dataObjectMapping.businessKeys)
                        {
                            // For Links, skip the first business key (because it's the full key).
                            if (businessKeyCounter == 1 && targetDataObject.name.IsDataVaultLink(teamConfiguration))
                            {
                                // Do nothing.
                            }
                            else
                            {
                                // If there is more than 1 data item mapping / business key component mapping then COMPOSITE ;
                                if (businessKey.businessKeyComponentMapping.Count > 1)
                                {
                                    businessKeyDefinitionString += "COMPOSITE(";
                                }

                                if (businessKey.businessKeyClassification != null && businessKey.businessKeyClassification[0].classification.Contains("DegenerateAttribute"))
                                    continue;

                                foreach (var dataItemMapping in businessKey.businessKeyComponentMapping)
                                {
                                    foreach (var dataItem in dataItemMapping.sourceDataItems)
                                    {
                                        // Explicitly type-cast the value as string to avoid issues using dynamic type.
                                        string dataItemName = dataItem.name;

                                        if (dataItemName.Contains("+"))
                                        {
                                            businessKeyDefinitionString += $"CONCATENATE({dataItem.name})".Replace("+", ";");
                                        }
                                        else
                                        {
                                            businessKeyDefinitionString += dataItemName;
                                        }

                                        businessKeyDefinitionString += ";";

                                        // Evaluate if a Driving Key needs to be set.
                                        if (dataItem.dataItemClassification != null)
                                        {
                                            // It must be a DataItem so it can be safely cast.
                                            DataItem tempDataItem = dataItem.ToObject<DataItem>();

                                            List<Classification> classifications = tempDataItem.dataItemClassification;

                                            if (classifications[0].classification == "DrivingKey")
                                            {
                                                drivingKeyDefinition = dataItemName;
                                            }
                                        }
                                    }
                                }

                                businessKeyDefinitionString = businessKeyDefinitionString.TrimEnd(';');

                                // If there is more than 1 data item mapping / business key component mapping then COMPOSITE ;
                                if (businessKey.businessKeyComponentMapping.Count > 1)
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
                    
                    foreach (var sourceDataObject in dataObjectMapping.sourceDataObjects)
                    {
                        // The new data object.
                        DataObject singleSourceDataObject = new DataObject();

                        // Default connection, to be updated later.
                        string sourceConnectionInternalId = teamConfiguration.MetadataConnection.ConnectionInternalId;

                        var intermediateJson = JsonConvert.SerializeObject(sourceDataObject);
                        
                        // Workaround, if the source is a data query it will be cast as data object because the data table does not support dynamic types.
                        if (JsonConvert.DeserializeObject(intermediateJson).ContainsKey("dataQueryCode"))
                        {
                            DataQuery tempDataItem = JsonConvert.DeserializeObject<DataQuery>(intermediateJson);

                            singleSourceDataObject.name = "`"+tempDataItem.dataQueryCode+"`";

                            if (tempDataItem.dataQueryConnection != null)
                            {
                                singleSourceDataObject.dataObjectConnection = tempDataItem.dataQueryConnection;

                                string sourceConnectionString = tempDataItem.dataQueryConnection.dataConnectionString;
                                sourceConnectionInternalId = TeamConnection.GetTeamConnectionByConnectionKey(sourceConnectionString, teamConfiguration).ConnectionInternalId;
                            }
                        }
                        else
                        {
                            singleSourceDataObject = JsonConvert.DeserializeObject<DataObject>(intermediateJson);

                            if (sourceDataObject.dataObjectConnection != null)
                            {
                                string sourceConnectionString = sourceDataObject.dataObjectConnection.dataConnectionString;
                                sourceConnectionInternalId = TeamConnection.GetTeamConnectionByConnectionKey(sourceConnectionString, teamConfiguration).ConnectionInternalId;
                            }
                        }

                        var newRow = DataTable.NewRow();

                        string[] hashKey =
                        {
                            singleSourceDataObject.name, targetDataObject.name, businessKeyDefinitionString, drivingKeyDefinition, filterCriterion
                        };

                        string surrogateKey = "";

                        if (dataObjectMapping.businessKeys != null)
                        {
                            surrogateKey = dataObjectMapping.businessKeys[0].surrogateKey;
                        }

                        Utility.CreateMd5(hashKey, Utility.SandingElement);

                        newRow[(int)DataObjectMappingGridColumns.Enabled] = dataObjectMapping.enabled;
                        newRow[(int)DataObjectMappingGridColumns.HashKey] = hashKey;
                        newRow[(int)DataObjectMappingGridColumns.SourceConnection] = sourceConnectionInternalId;
                        newRow[(int)DataObjectMappingGridColumns.SourceDataObject] = singleSourceDataObject;
                        newRow[(int)DataObjectMappingGridColumns.TargetConnection] = targetConnectionInternalId;
                        newRow[(int)DataObjectMappingGridColumns.TargetDataObject] = targetDataObject;
                        newRow[(int)DataObjectMappingGridColumns.BusinessKeyDefinition] = businessKeyDefinitionString;
                        newRow[(int)DataObjectMappingGridColumns.DrivingKeyDefinition] = drivingKeyDefinition;
                        newRow[(int)DataObjectMappingGridColumns.FilterCriterion] = filterCriterion;
                        newRow[(int)DataObjectMappingGridColumns.SourceDataObjectName] = singleSourceDataObject.name;
                        newRow[(int)DataObjectMappingGridColumns.TargetDataObjectName] = targetDataObject.name;
                        newRow[(int)DataObjectMappingGridColumns.PreviousTargetDataObjectName] = targetDataObject.name;
                        newRow[(int)DataObjectMappingGridColumns.SurrogateKey] = surrogateKey;

                        DataTable.Rows.Add(newRow);
                    }
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
                if (row[(int)DataObjectMappingGridColumns.TargetDataObjectName].ToString().IsDataVaultLinkSatellite(teamConfiguration))
                {
                    // For LSATs only, look up the corresponding LNK and re-use that business key.
                    string surrogateKey = (string)row[(int)DataObjectMappingGridColumns.SurrogateKey];

                    string filterCriterion = DataObjectMappingGridColumns.SourceDataObjectName + " = '" + row[(int)DataObjectMappingGridColumns.SourceDataObjectName] + "' AND " +
                                             DataObjectMappingGridColumns.SurrogateKey + " = '" + surrogateKey + "' AND " + DataObjectMappingGridColumns.TargetDataObjectName + " <> '" +
                                             row[(int)DataObjectMappingGridColumns.TargetDataObjectName] + "'";

                    // Should be only one return value.
                    var dataTableLookup = DataTable.Select(filterCriterion).FirstOrDefault();

                    // Update the LSAT business key.
                    row[(int)DataObjectMappingGridColumns.BusinessKeyDefinition] = dataTableLookup[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].ToString();
                }
            }
        }

        /// <summary>
        /// Set the header / column names for each data table column.
        /// </summary>
        public static void SetDataTableColumnNames(DataTable dataTable)
        {
            dataTable.Columns[(int)DataObjectMappingGridColumns.Enabled].ColumnName = DataObjectMappingGridColumns.Enabled.ToString();
            dataTable.Columns[(int)DataObjectMappingGridColumns.HashKey].ColumnName = DataObjectMappingGridColumns.HashKey.ToString();
            dataTable.Columns[(int)DataObjectMappingGridColumns.SourceConnection].ColumnName = DataObjectMappingGridColumns.SourceConnection.ToString();
            dataTable.Columns[(int)DataObjectMappingGridColumns.SourceDataObject].ColumnName = DataObjectMappingGridColumns.SourceDataObject.ToString();
            dataTable.Columns[(int)DataObjectMappingGridColumns.TargetConnection].ColumnName = DataObjectMappingGridColumns.TargetConnection.ToString();
            dataTable.Columns[(int)DataObjectMappingGridColumns.TargetDataObject].ColumnName = DataObjectMappingGridColumns.TargetDataObject.ToString();
            dataTable.Columns[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].ColumnName = DataObjectMappingGridColumns.BusinessKeyDefinition.ToString();
            dataTable.Columns[(int)DataObjectMappingGridColumns.DrivingKeyDefinition].ColumnName = DataObjectMappingGridColumns.DrivingKeyDefinition.ToString();
            dataTable.Columns[(int)DataObjectMappingGridColumns.SourceDataObjectName].ColumnName = DataObjectMappingGridColumns.SourceDataObjectName.ToString();
            dataTable.Columns[(int)DataObjectMappingGridColumns.TargetDataObjectName].ColumnName = DataObjectMappingGridColumns.TargetDataObjectName.ToString();
            dataTable.Columns[(int)DataObjectMappingGridColumns.PreviousTargetDataObjectName].ColumnName = DataObjectMappingGridColumns.PreviousTargetDataObjectName.ToString();
            dataTable.Columns[(int)DataObjectMappingGridColumns.SurrogateKey].ColumnName = DataObjectMappingGridColumns.SurrogateKey.ToString();
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
                var connectionProfile = TeamConfiguration.GetTeamConnectionByInternalId(connection, connectionDictionary);

                if (!connectionList.Contains(connectionProfile))
                {
                    connectionList.Add(connectionProfile);
                }
            }

            foreach (string connection in targetConnectionList)
            {
                var connectionProfile = TeamConfiguration.GetTeamConnectionByInternalId(connection, connectionDictionary);

                if (!connectionList.Contains(connectionProfile))
                {
                    connectionList.Add(connectionProfile);
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
        // The below are hidden, for sorting and back-end management only.
        SourceDataObjectName = 9,
        TargetDataObjectName = 10,
        PreviousTargetDataObjectName = 11,
        SurrogateKey = 12
    }
}
