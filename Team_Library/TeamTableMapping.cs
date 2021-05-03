using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TEAM_Library
{
    public class TableMappingJson
    {
        //JSON representation of the table mapping metadata
        public bool enabledIndicator { get; set; }
        public string tableMappingHash { get; set; }
        public int versionId { get; set; }
        public string sourceTable { get; set; }
        public string sourceConnectionKey { get; set; }
        public string targetTable { get; set; }
        public string targetConnectionKey { get; set; }
        public string businessKeyDefinition { get; set; }
        public string drivingKeyDefinition { get; set; }
        public string filterCriteria { get; set; }
    }

    /// <summary>
    /// Enumerator to hold the column index for the columns (headers) in the Table Metadata data grid view.
    /// </summary>
    public enum TableMappingMetadataColumns
    {
        Enabled = 0,
        HashKey = 1,
        VersionId = 2,
        SourceTable = 3,
        SourceConnection = 4,
        TargetTable = 5,
        TargetConnection = 6,
        BusinessKeyDefinition = 7,
        DrivingKeyDefinition = 8,
        FilterCriterion = 9
    }

    /// <summary>
    /// The metadata collection object containing all dataObject to dataObject (source-to-target) mappings.
    /// </summary>
    public class TeamTableMapping
    {
        public EventLog EventLog { get; set; }

        public List<TableMappingJson> JsonList { get; set; }

        public DataTable DataTable { get; set; }

        public TeamTableMapping()
        {
            EventLog = new EventLog();
            DataTable = new DataTable();
            JsonList = new List<TableMappingJson>();
        }

        public enum BusinessKeyEvaluationMode
        {
            Full, // The full business key is evaluated.
            Partial // The business is broken into components and evaluated separately.
        }

        /// <summary>
        /// Find the related Data Objects in the same level, based on the business key evaluation.
        /// </summary>
        /// <param name="SourceDataObjectName"></param>
        /// <param name="TargetDataObjectName"></param>
        /// <param name="BusinessKey"></param>
        /// <param name="DataTable"></param>
        /// <param name="businessKeyEvaluationMode"></param>
        /// <returns></returns>
        public List<DataRow> GetPeerDataRows(string SourceDataObjectName, string SourceDataObjectSchema, string TargetDataObjectName, string TargetDataObjectSchema, string BusinessKey, string FilterCriterion, DataTable DataTable, BusinessKeyEvaluationMode businessKeyEvaluationMode)
        {
            // Prepare the return information
            List<DataRow> localDataRows = new List<DataRow>();

            // First, the Business Key need to be checked. This is to determine how many dependents are expected.
            // For instance, if a Link has a three-part Business Key then three Hubs will be expected
            List<string> businessKeyComponents = BusinessKey.Split(',').ToList();

            //var localSourceDataObjectName = SourceDataObjectSchema + "." + SourceDataObjectName;
            //var localTargetDataObjectName = TargetDataObjectSchema + "." + TargetDataObjectName;

            // NOTE: THIS NEEDS TO BE REFACTORED TO USE THE FULLY QUALIFIED NAME BUT THIS REQUIRES THE LOADPATTERNQUERY (e.g. base query) CONCEPT TO BE DEPRECATED
            var localSourceDataObjectName = SourceDataObjectName.Substring(SourceDataObjectName.IndexOf('.') + 1);
            var localTargetDataObjectName = TargetDataObjectName.Substring(TargetDataObjectName.IndexOf('.') + 1);


            // Only evaluate a business key component based on its part.
            // E.g. a single business key such as CustomerID will be looked up in the rest of the mappings.
            // E.g. a Composite business key such as for a Link (customerID, OfferID) will be evaluated on each component to find any Hubs, Sats etc.
            if (businessKeyEvaluationMode == BusinessKeyEvaluationMode.Partial)
            {
                foreach (string businessKeyComponent in businessKeyComponents)
                {
                    var relatedDataObjectRows = from localRow in DataTable.AsEnumerable()
                        where 
                              localRow.Field<bool>(TableMappingMetadataColumns.Enabled.ToString()) == true &&
                              localRow.Field<string>(TableMappingMetadataColumns.SourceTable.ToString()).Substring(localRow.Field<string>(TableMappingMetadataColumns.SourceTable.ToString()).IndexOf('.') + 1 ) == localSourceDataObjectName &&
                              localRow.Field<string>(TableMappingMetadataColumns.BusinessKeyDefinition.ToString()) == businessKeyComponent.Trim() &&
                              localRow.Field<string>(TableMappingMetadataColumns.FilterCriterion.ToString()) == FilterCriterion &&
                              localRow.Field<string>(TableMappingMetadataColumns.TargetTable.ToString()).Substring(localRow.Field<string>(TableMappingMetadataColumns.TargetTable.ToString()).IndexOf('.') + 1) != localTargetDataObjectName
                        select localRow;

                    foreach (DataRow detailRow in relatedDataObjectRows)
                    {
                        localDataRows.Add(detailRow);
                    }

                    //foreach (DataRow row in DataTable.Rows)
                    //{
                    //    if (
                    //         (bool)row[TableMappingMetadataColumns.Enabled.ToString()] == true && // Only active generated objects
                    //         (string)row[TableMappingMetadataColumns.SourceTable.ToString()] == SourceDataObjectName &&
                    //         (string)row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()] == businessKeyComponent.Trim() &&
                    //         (string)row[TableMappingMetadataColumns.FilterCriterion.ToString()] == FilterCriterion &&
                    //         (string)row[TableMappingMetadataColumns.TargetTable.ToString()] != TargetDataObjectName 
                    //        // && // Exclude itself
                    //        // row[TableMappingMetadataColumns.TargetTable.ToString()].ToString().StartsWith(tableInclusionFilterCriterion)
                    //    )
                    //    {
                    //        localDataRows.Add(row);
                    //    }
                    //}
                }
            }
            else // In the case of an LSAT, only join on the Link using the full business key
            {
                // Query the dependent information
                foreach (DataRow row in DataTable.Rows)
                {
                    if (
                         (bool)row[TableMappingMetadataColumns.Enabled.ToString()] == true && // Only active generated objects
                         (string)row[TableMappingMetadataColumns.SourceTable.ToString()] == SourceDataObjectName &&
                         (string)row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()] == BusinessKey &&
                         (string)row[TableMappingMetadataColumns.TargetTable.ToString()] != TargetDataObjectName 
                         //&& // Exclude itself
                        // row[TableMappingMetadataColumns.TargetTable.ToString()].ToString().StartsWith(tableInclusionFilterCriterion)
                       )
                    {
                        localDataRows.Add(row);
                    }
                }
            }

            // return the result
            return localDataRows;
        }

        public List<TeamConnection> GetConnectionList(Dictionary<string, TeamConnection> connectionDictionary)
        {
            // Create the group nodes (systems)
            var connectionList = new List<TeamConnection>();

            var sourceConnectionList = DataTable.AsEnumerable().Select(r => r.Field<string>(TableMappingMetadataColumns.SourceConnection.ToString())).ToList();
            var targetConnectionList = DataTable.AsEnumerable().Select(r => r.Field<string>(TableMappingMetadataColumns.TargetConnection.ToString())).ToList();

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

        /// <summary>
        /// Set the sort order for the data table.
        /// </summary>
        public void SetDataTableSorting()
        {
            DataTable.DefaultView.Sort = $"[{TableMappingMetadataColumns.TargetTable}] ASC, [{TableMappingMetadataColumns.SourceTable}] ASC, [{TableMappingMetadataColumns.BusinessKeyDefinition}] ASC";
        }

        /// <summary>
        /// Set the column names for the data table.
        /// </summary>
        public void SetDataTableColumns()
        {
            DataTable.Columns[(int)TableMappingMetadataColumns.Enabled].ColumnName = TableMappingMetadataColumns.Enabled.ToString();
            DataTable.Columns[(int)TableMappingMetadataColumns.HashKey].ColumnName = TableMappingMetadataColumns.HashKey.ToString();
            DataTable.Columns[(int)TableMappingMetadataColumns.VersionId].ColumnName = TableMappingMetadataColumns.VersionId.ToString();
            DataTable.Columns[(int)TableMappingMetadataColumns.SourceTable].ColumnName = TableMappingMetadataColumns.SourceTable.ToString();
            DataTable.Columns[(int)TableMappingMetadataColumns.SourceConnection].ColumnName = TableMappingMetadataColumns.SourceConnection.ToString();
            DataTable.Columns[(int)TableMappingMetadataColumns.TargetTable].ColumnName = TableMappingMetadataColumns.TargetTable.ToString();
            DataTable.Columns[(int)TableMappingMetadataColumns.TargetConnection].ColumnName = TableMappingMetadataColumns.TargetConnection.ToString();
            DataTable.Columns[(int)TableMappingMetadataColumns.BusinessKeyDefinition].ColumnName = TableMappingMetadataColumns.BusinessKeyDefinition.ToString();
            DataTable.Columns[(int)TableMappingMetadataColumns.DrivingKeyDefinition].ColumnName = TableMappingMetadataColumns.DrivingKeyDefinition.ToString();
            DataTable.Columns[(int)TableMappingMetadataColumns.FilterCriterion].ColumnName = TableMappingMetadataColumns.FilterCriterion.ToString();
        }

        /// <summary>
        /// Creates a TeamMappingDataTable object (Json List and DataTable) from a Table Mapping Json file.
        /// </summary>
        /// <returns></returns>
        public void GetMetadata(string fileName)
        {
            EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"Retrieving Table Mapping metadata from {fileName}."));

            // Check if the file exists
            if (!File.Exists(fileName))
            {
                EventLog.Add(Event.CreateNewEvent(EventTypes.Warning, "No Json Table Mapping file was found."));
            }
            else
            {
                EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"Reading file {fileName}"));

                // Load the file, convert it to a DataTable and bind it to the source
                List<TableMappingJson> jsonArray = JsonConvert.DeserializeObject<List<TableMappingJson>>(File.ReadAllText(fileName));

                // Commit to the object
                JsonList = jsonArray;
                var dataTable = Utility.ConvertToDataTable(jsonArray);

                //Make sure the changes are seen as committed, so that changes can be detected later on.
                dataTable.AcceptChanges();

                // Commit it to the object itself
                DataTable = dataTable;

                // Set the column names.
                SetDataTableColumns();

                // Set the sort order.
                SetDataTableSorting();
            }
        }

        /// <summary>
        /// Using the TableMapping, create a list of Core Business Concept / Natural Business Relationship links.
        /// </summary>
        /// <param name="configurationSetting"></param>
        /// <returns></returns>
        public List<Tuple<string, string>> BusinessConceptRelationshipList(TeamConfiguration configurationSetting)
        {
            DataView sourceContainerView = new DataView((DataTable)DataTable);
            DataTable distinctValues = sourceContainerView.ToTable(true, TableMappingMetadataColumns.SourceTable.ToString());
            var businessConceptRelationshipList = new List<Tuple<string, string>>();

            foreach (DataRow sourceContainerRow in distinctValues.Rows)
            {
                string sourceContainer = (string)sourceContainerRow[TableMappingMetadataColumns.SourceTable.ToString()];

                foreach (DataRow row in DataTable.Rows)
                {
                    string sourceObject = (string)row[TableMappingMetadataColumns.SourceTable.ToString()];

                    if (sourceContainer == sourceObject)
                    {
                        //var sourceObject = (string) row[TableMappingMetadataColumns.SourceTable.ToString()];
                        var targetObject = (string)row[TableMappingMetadataColumns.TargetTable.ToString()];

                        //var sourceObjectType = MetadataHandling.GetTableType(sourceObject, "", TeamConfigurationSettings);
                        var targetObjectType = MetadataHandling.GetDataObjectType(targetObject, "", configurationSetting);
                        var targetObjectBusinessKey =
                            (string)row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()];

                        if (targetObjectType == MetadataHandling.TableTypes.CoreBusinessConcept)
                        {
                            // Retrieve the related objects to a CBC.
                            var cbcResults = from localRow in DataTable.AsEnumerable()
                                             where localRow.Field<string>(TableMappingMetadataColumns.SourceTable.ToString()) ==
                                                   sourceObject && // Is in the same source cluster
                                                   localRow.Field<string>(TableMappingMetadataColumns.BusinessKeyDefinition.ToString())
                                                       .Contains(targetObjectBusinessKey) && // Contains a part of the business key
                                                   localRow.Field<string>(TableMappingMetadataColumns.TargetTable.ToString()) !=
                                                   targetObject && // Is not itself
                                                   MetadataHandling.GetDataObjectType(
                                                       localRow.Field<string>(TableMappingMetadataColumns.TargetTable.ToString()), "",
                                                       configurationSetting) ==
                                                   MetadataHandling.TableTypes.NaturalBusinessRelationship // Is a NBR.
                                             select localRow;

                            foreach (DataRow detailRow in cbcResults)
                            {
                                var targetObjectDetail = (string)detailRow[TableMappingMetadataColumns.TargetTable.ToString()];
                                businessConceptRelationshipList.Add(new Tuple<string, string>(targetObject, targetObjectDetail));
                            }
                        }
                    }
                }
            }

            return businessConceptRelationshipList;
        }

        /// <summary>
        /// Using the Table Mapping, create a list of Subject Areas and their contents.
        /// </summary>
        /// <param name="configurationSetting"></param>
        /// <returns></returns>
        public List<Tuple<string, string, string>> SubjectAreaList(TeamConfiguration configurationSetting)
        {
            var subjectAreaList = new List<Tuple<string, string, string>>();

            foreach (DataRow row in DataTable.Rows)
            {
                string sourceObject = (string)row[TableMappingMetadataColumns.SourceTable.ToString()];
                string targetObject = (string)row[TableMappingMetadataColumns.TargetTable.ToString()];

                var targetObjectType = MetadataHandling.GetDataObjectType(targetObject, "", configurationSetting);
                var targetObjectBusinessKey = (string)row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()];

                if (targetObjectType == MetadataHandling.TableTypes.CoreBusinessConcept)
                {
                    string subjectArea = targetObject.Replace(configurationSetting.HubTablePrefixValue + "_", "");

                    // Retrieve the related objects (Context Tables in this case)
                    var results = from localRow in DataTable.AsEnumerable()
                                  where localRow.Field<string>(TableMappingMetadataColumns.SourceTable.ToString()) == sourceObject && // Is in the same source cluster
                                        localRow.Field<string>(TableMappingMetadataColumns.BusinessKeyDefinition.ToString()).Contains(targetObjectBusinessKey) && // Contains a part of the business key
                                                                                                                                                                  //localRow.Field<string>(TableMappingMetadataColumns.TargetTable.ToString()) != targetObject && // Is not itself
                                        MetadataHandling.GetDataObjectType(localRow.Field<string>(TableMappingMetadataColumns.TargetTable.ToString()), "",
                                            configurationSetting) != MetadataHandling.TableTypes.NaturalBusinessRelationship &&
                                        MetadataHandling.GetDataObjectType(localRow.Field<string>(TableMappingMetadataColumns.TargetTable.ToString()), "",
                                            configurationSetting) != MetadataHandling.TableTypes.NaturalBusinessRelationshipContext
                                  select localRow;

                    foreach (DataRow detailRow in results)
                    {
                        var targetObjectDetail = (string)detailRow[TableMappingMetadataColumns.TargetTable.ToString()];

                        bool tupleAlreadyExists = subjectAreaList.Any(m => m.Item1 == subjectArea && m.Item2 == targetObject && m.Item3 == targetObjectDetail);

                        if (!tupleAlreadyExists)
                        {
                            subjectAreaList.Add(new Tuple<string, string, string>(subjectArea, targetObject, targetObjectDetail));
                        }
                    }
                }

                if (targetObjectType == MetadataHandling.TableTypes.NaturalBusinessRelationship)
                {
                    string subjectArea = targetObject.Replace(configurationSetting.LinkTablePrefixValue + "_", "");

                    // Retrieve the related objects (relationship context tables in this case)
                    var results = from localRow in DataTable.AsEnumerable()
                                  where localRow.Field<string>(TableMappingMetadataColumns.SourceTable.ToString()) == sourceObject && // Is in the same source cluster
                                        localRow.Field<string>(TableMappingMetadataColumns.BusinessKeyDefinition.ToString()).Contains(targetObjectBusinessKey) && // Contains a part of the business key
                                                                                                                                                                  //localRow.Field<string>(TableMappingMetadataColumns.TargetTable.ToString()) != targetObject && // Is not itself
                                        MetadataHandling.GetDataObjectType(localRow.Field<string>(TableMappingMetadataColumns.TargetTable.ToString()), "",
                                            configurationSetting) != MetadataHandling.TableTypes.CoreBusinessConcept // Is a relationship context table.
                                  select localRow;

                    foreach (DataRow detailRow in results)
                    {
                        var targetObjectDetail = (string)detailRow[TableMappingMetadataColumns.TargetTable.ToString()];

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
    }
}
