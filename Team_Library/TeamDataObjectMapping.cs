using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DataWarehouseAutomation;
using Newtonsoft.Json;
using DataObject = DataWarehouseAutomation.DataObject;

namespace TEAM_Library
{
    public class DataObjectMappingsFileCombination
    {
        public string FileName { get; set; }
        public DataObjectMappings DataObjectMappings { get; set; }
    }

    public class TeamDataObjectMappings
    {
        public SortableBindingList<DataObjectMappingsFileCombination> DataObjectMappingsFileCombinations { get; set; }

        public EventLog EventLog { get; set; }

        public DataTable DataTable { get; set; }

        public string MetadataPath { get; set; }

        public TeamDataObjectMappings(string inputPath)
        {
            InitializeMainProperties();
            MetadataPath = inputPath;
        }

        // Reusable initialisation of the main properties.
        internal void InitializeMainProperties()
        {
            EventLog = new EventLog();
            DataTable = new DataTable();
            DataObjectMappingsFileCombinations = new SortableBindingList<DataObjectMappingsFileCombination>();

            DataTable.Columns.Add(DataObjectMappingGridColumns.Enabled.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.HashKey.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.SourceConnection.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.SourceDataObject.ToString(), typeof(DataWarehouseAutomation.DataObject));
            DataTable.Columns.Add(DataObjectMappingGridColumns.TargetConnection.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.TargetDataObject.ToString(), typeof(DataWarehouseAutomation.DataObject));
            DataTable.Columns.Add(DataObjectMappingGridColumns.BusinessKeyDefinition.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.DrivingKeyDefinition.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.FilterCriterion.ToString());
            // For sorting purposes only.
            DataTable.Columns.Add(DataObjectMappingGridColumns.SourceDataObjectName.ToString());
            DataTable.Columns.Add(DataObjectMappingGridColumns.TargetDataObjectName.ToString());
        }

        /// <summary>
        /// Load compatible JSON files into memory.
        /// </summary>
        public void GetMetadata()
        {
            if (Directory.Exists(MetadataPath))
            {
                string[] jsonFiles = Directory.GetFiles(MetadataPath, "*.json");

                // Hard-coded exclusions.
                string[] excludedFiles =
                {
                    "Development_TEAM_Table_Mapping_v0.json", "Development_TEAM_Attribute_Mapping_v0.json"
                };

                if (jsonFiles.Length > 0)
                {
                    foreach (string fileName in jsonFiles)
                    {
                        if (!Array.Exists(excludedFiles, x => x == Path.GetFileName(fileName)))
                        {
                            try
                            {
                                // Validate the file contents against the schema definition.
                                if (File.Exists(Application.StartupPath + @"\Schema\" + GlobalParameters.JsonSchemaForDataWarehouseAutomationFileName))
                                {
                                    var result = JsonHandling.ValidateJsonFileAgainstSchema(Application.StartupPath + @"\Schema\" + GlobalParameters.JsonSchemaForDataWarehouseAutomationFileName, fileName);

                                    foreach (var error in result.Errors)
                                    {
                                       EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error was encountered validating the contents {fileName}.{error.Message}. This occurs at line {error.LineNumber}."));
                                    }
                                }
                                else
                                {
                                    EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error occurred while validating the file against the Data Warehouse Automation schema. Does the schema file exist?"));
                                }

                                // Add the deserialised file to the list of mappings.

                                var jsonInput = File.ReadAllText(fileName);
                                var dataObjectMappings = JsonConvert.DeserializeObject<DataObjectMappings>(jsonInput);

                                var localDataObjectMappingsFileCombination = new DataObjectMappingsFileCombination
                                {
                                    FileName = fileName,
                                    DataObjectMappings = dataObjectMappings
                                };

                                DataObjectMappingsFileCombinations.Add(localDataObjectMappingsFileCombination);
                                EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"The file {fileName} was successfully loaded."));
                            }
                            catch
                            {
                                EventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The file {fileName} could not be loaded."));
                            }
                        }
                    }
                }
                else
                {
                    EventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"No files were found in directory {MetadataPath}."));

                }

            }
            else
            {
                EventLog.Add(Event.CreateNewEvent(EventTypes.Warning, $"There were issues accessing the directory {MetadataPath}. It does not seem to exist."));
            }
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
                    // Target Data Object details
                    var targetDataObject = dataObjectMapping.targetDataObject;
                    string targetConnectionInternalId = TeamConnection.GetTeamConnectionByConnectionKey(dataObjectMapping.targetDataObject.dataObjectConnection.dataConnectionString, teamConfiguration).ConnectionInternalId;
                    string filterCriterion = dataObjectMapping.filterCriterion;
                    string drivingKeyDefinition = "";

                    #region Business Key Definition
                    string businessKeyDefinitionString = "";

                    int businessKeyCounter = 1;

                    foreach (var businessKey in dataObjectMapping.businessKeys)
                    {
                        // For Links, skip the first business key (because it's the full key).
                        // For LSAT and SAT skip altogether because it will be derived.
                        if ((businessKeyCounter == 1 && targetDataObject.name.IsDataVaultLink(teamConfiguration))
                            || targetDataObject.name.IsDataVaultSatellite(teamConfiguration)
                            || targetDataObject.name.IsDataVaultLinkSatellite(teamConfiguration))
                        {
                            // Do nothing
                        }
                        else
                        {
                            // If there is more than 1 data item mapping / business key component mapping then COMPOSITE ;
                            if (businessKey.businessKeyComponentMapping.Count > 1)
                            {
                                businessKeyDefinitionString += "COMPOSITE(";
                            }

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
                                }
                            }

                            businessKeyDefinitionString = businessKeyDefinitionString.TrimEnd(';');

                            // If there is more than 1 data item mapping / business key component mapping then COMPOSITE ;
                            if (businessKey.businessKeyComponentMapping.Count > 1)
                                // && targetDataObject.dataObjectClassifications[0].classification != "NaturalBusinessRelationship" && targetDataObject.dataObjectClassifications[0].classification != "NaturalBusinessRelationshipContext")
                            {
                                businessKeyDefinitionString += ")";
                            }

                            businessKeyDefinitionString += ",";
                        }

                        businessKeyCounter++;
                    }
                    businessKeyDefinitionString = businessKeyDefinitionString.TrimEnd(',');
                    #endregion

                    foreach (var sourceDataObject in dataObjectMapping.sourceDataObjects)
                    {
                        var intermediateJson = JsonConvert.SerializeObject(sourceDataObject);
                        DataObject singleSourceDataObject = JsonConvert.DeserializeObject<DataObject>(intermediateJson);
                        // Source Data Object details.

                        string sourceConnectionString = sourceDataObject.dataObjectConnection.dataConnectionString;
                        var sourceConnectionInternalId = TeamConnection.GetTeamConnectionByConnectionKey(sourceConnectionString, teamConfiguration).ConnectionInternalId;

                        var newRow = DataTable.NewRow();

                        string[] hashKey =
                        {
                                        singleSourceDataObject.name, targetDataObject.name, businessKeyDefinitionString,
                                        drivingKeyDefinition, filterCriterion
                        };
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

                        DataTable.Rows.Add(newRow);
                    }
                }
            }

            SetDataTableColumnNames();
        }

        /// <summary>
        /// Set the header / column names for each data table column.
        /// </summary>
        public void SetDataTableColumnNames()
        {
            DataTable.Columns[(int)DataObjectMappingGridColumns.Enabled].ColumnName = DataObjectMappingGridColumns.Enabled.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.HashKey].ColumnName = DataObjectMappingGridColumns.HashKey.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.SourceConnection].ColumnName = DataObjectMappingGridColumns.SourceConnection.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.SourceDataObject].ColumnName = DataObjectMappingGridColumns.SourceDataObject.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.TargetConnection].ColumnName = DataObjectMappingGridColumns.TargetConnection.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.TargetDataObject].ColumnName = DataObjectMappingGridColumns.TargetDataObject.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].ColumnName = DataObjectMappingGridColumns.BusinessKeyDefinition.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.DrivingKeyDefinition].ColumnName = DataObjectMappingGridColumns.DrivingKeyDefinition.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.SourceDataObjectName].ColumnName = DataObjectMappingGridColumns.SourceDataObjectName.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.TargetDataObjectName].ColumnName = DataObjectMappingGridColumns.TargetDataObjectName.ToString();
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
        // The below are hidden, for sorting only.
        SourceDataObjectName = 9,
        TargetDataObjectName = 10,
    }

    /// <summary>
    /// The metadata collection object containing all dataObject to dataObject (source-to-target) mappings.
    /// </summary>
    public class TeamDataObjectMapping
    {
        public EventLog EventLog { get; set; }

        public List<TableMappingJson> JsonList { get; set; }

        public DataTable DataTable { get; set; }

        public TeamDataObjectMapping()
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
        /// <param name="TargetDataObject"></param>
        /// <param name="BusinessKey"></param>
        /// <param name="DataTable"></param>
        /// <param name="businessKeyEvaluationMode"></param>
        /// <returns></returns>
        public List<DataRow> GetPeerDataRows(string SourceDataObjectName, string SourceDataObjectSchema, string TargetDataObject, string TargetDataObjectSchema, string BusinessKey, string FilterCriterion, DataTable DataTable, BusinessKeyEvaluationMode businessKeyEvaluationMode)
        {
            // Prepare the return information
            List<DataRow> localDataRows = new List<DataRow>();

            // First, the Business Key need to be checked. This is to determine how many dependents are expected.
            // For instance, if a Link has a three-part Business Key then three Hubs will be expected
            List<string> businessKeyComponents = BusinessKey.Split(',').ToList();

            //var localSourceDataObjectName = SourceDataObjectSchema + "." + SourceDataObjectName;
            //var localTargetDataObject = TargetDataObjectSchema + "." + TargetDataObject;

            // NOTE: THIS NEEDS TO BE REFACTORED TO USE THE FULLY QUALIFIED NAME BUT THIS REQUIRES THE LOADPATTERNQUERY (e.g. base query) CONCEPT TO BE DEPRECATED
            var localSourceDataObjectName = SourceDataObjectName.Substring(SourceDataObjectName.IndexOf('.') + 1);
            var localTargetDataObject = TargetDataObject.Substring(TargetDataObject.IndexOf('.') + 1);


            // Only evaluate a business key component based on its part.
            // E.g. a single business key such as CustomerID will be looked up in the rest of the mappings.
            // E.g. a Composite business key such as for a Link (customerID, OfferID) will be evaluated on each component to find any Hubs, Sats etc.
            if (businessKeyEvaluationMode == BusinessKeyEvaluationMode.Partial)
            {
                foreach (string businessKeyComponent in businessKeyComponents)
                {
                    var relatedDataObjectRows = from localRow in DataTable.AsEnumerable()
                        where 
                              localRow.Field<bool>(DataObjectMappingGridColumns.Enabled.ToString()) == true &&
                              localRow.Field<string>(DataObjectMappingGridColumns.SourceDataObject.ToString()).Substring(localRow.Field<string>(DataObjectMappingGridColumns.SourceDataObject.ToString()).IndexOf('.') + 1 ) == localSourceDataObjectName &&
                              localRow.Field<string>(DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()) == businessKeyComponent.Trim() &&
                              localRow.Field<string>(DataObjectMappingGridColumns.FilterCriterion.ToString()) == FilterCriterion &&
                              localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObject.ToString()).Substring(localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObject.ToString()).IndexOf('.') + 1) != localTargetDataObject
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
                    //         (string)row[TableMappingMetadataColumns.TargetTable.ToString()] != TargetDataObject 
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
                         (bool)row[DataObjectMappingGridColumns.Enabled.ToString()] == true && // Only active generated objects
                         (string)row[DataObjectMappingGridColumns.SourceDataObject.ToString()] == SourceDataObjectName &&
                         (string)row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()] == BusinessKey &&
                         (string)row[DataObjectMappingGridColumns.TargetDataObject.ToString()] != TargetDataObject 
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

        public void SetDataTableColumns()
        {
            DataTable.Columns[(int)DataObjectMappingGridColumns.Enabled].ColumnName = DataObjectMappingGridColumns.Enabled.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.HashKey].ColumnName = DataObjectMappingGridColumns.HashKey.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.SourceDataObject].ColumnName = DataObjectMappingGridColumns.SourceDataObject.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.SourceConnection].ColumnName = DataObjectMappingGridColumns.SourceConnection.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.TargetDataObject].ColumnName = DataObjectMappingGridColumns.TargetDataObject.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.TargetConnection].ColumnName = DataObjectMappingGridColumns.TargetConnection.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].ColumnName = DataObjectMappingGridColumns.BusinessKeyDefinition.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.DrivingKeyDefinition].ColumnName = DataObjectMappingGridColumns.DrivingKeyDefinition.ToString();
            DataTable.Columns[(int)DataObjectMappingGridColumns.FilterCriterion].ColumnName = DataObjectMappingGridColumns.FilterCriterion.ToString();
        }
        public void SetDataTableSorting()
        {
            DataTable.DefaultView.Sort = $"[{DataObjectMappingGridColumns.TargetDataObject}] ASC, [{DataObjectMappingGridColumns.SourceDataObject}] ASC, [{DataObjectMappingGridColumns.BusinessKeyDefinition}] ASC";
        }

        /// <summary>
        /// Using the TableMapping, create a list of Core Business Concept / Natural Business Relationship links.
        /// </summary>
        /// <param name="configurationSetting"></param>
        /// <returns></returns>
        public List<Tuple<string, string>> BusinessConceptRelationshipList(TeamConfiguration configurationSetting)
        {
            DataView sourceContainerView = new DataView((DataTable)DataTable);
            DataTable distinctValues = sourceContainerView.ToTable(true, DataObjectMappingGridColumns.SourceDataObject.ToString());
            var businessConceptRelationshipList = new List<Tuple<string, string>>();

            foreach (DataRow sourceContainerRow in distinctValues.Rows)
            {
                string sourceContainer = (string)sourceContainerRow[DataObjectMappingGridColumns.SourceDataObject.ToString()];

                foreach (DataRow row in DataTable.Rows)
                {
                    string sourceObject = (string)row[DataObjectMappingGridColumns.SourceDataObject.ToString()];

                    if (sourceContainer == sourceObject)
                    {
                        //var sourceObject = (string) row[TableMappingMetadataColumns.SourceTable.ToString()];
                        var targetObject = (string)row[DataObjectMappingGridColumns.TargetDataObject.ToString()];

                        //var sourceObjectType = MetadataHandling.GetTableType(sourceObject, "", TeamConfigurationSettings);
                        var targetObjectType = MetadataHandling.GetDataObjectType(targetObject, "", configurationSetting);
                        var targetObjectBusinessKey =
                            (string)row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()];

                        if (targetObjectType == MetadataHandling.DataObjectTypes.CoreBusinessConcept)
                        {
                            // Retrieve the related objects to a CBC.
                            var cbcResults = from localRow in DataTable.AsEnumerable()
                                             where localRow.Field<string>(DataObjectMappingGridColumns.SourceDataObject.ToString()) ==
                                                   sourceObject && // Is in the same source cluster
                                                   localRow.Field<string>(DataObjectMappingGridColumns.BusinessKeyDefinition.ToString())
                                                       .Contains(targetObjectBusinessKey) && // Contains a part of the business key
                                                   localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObject.ToString()) !=
                                                   targetObject && // Is not itself
                                                   MetadataHandling.GetDataObjectType(
                                                       localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObject.ToString()), "",
                                                       configurationSetting) ==
                                                   MetadataHandling.DataObjectTypes.NaturalBusinessRelationship // Is a NBR.
                                             select localRow;

                            foreach (DataRow detailRow in cbcResults)
                            {
                                var targetObjectDetail = (string)detailRow[DataObjectMappingGridColumns.TargetDataObject.ToString()];
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
                string sourceObject = (string)row[DataObjectMappingGridColumns.SourceDataObject.ToString()];
                string targetObject = (string)row[DataObjectMappingGridColumns.TargetDataObject.ToString()];

                var targetObjectType = MetadataHandling.GetDataObjectType(targetObject, "", configurationSetting);
                var targetObjectBusinessKey = (string)row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()];

                if (targetObjectType == MetadataHandling.DataObjectTypes.CoreBusinessConcept)
                {
                    string subjectArea = targetObject.Replace(configurationSetting.HubTablePrefixValue + "_", "");

                    // Retrieve the related objects (Context Tables in this case)
                    var results = from localRow in DataTable.AsEnumerable()
                                  where localRow.Field<string>(DataObjectMappingGridColumns.SourceDataObject.ToString()) == sourceObject && // Is in the same source cluster
                                        localRow.Field<string>(DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()).Contains(targetObjectBusinessKey) && // Contains a part of the business key
                                                                                                                                                                  //localRow.Field<string>(TableMappingMetadataColumns.TargetTable.ToString()) != targetObject && // Is not itself
                                        MetadataHandling.GetDataObjectType(localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObject.ToString()), "",
                                            configurationSetting) != MetadataHandling.DataObjectTypes.NaturalBusinessRelationship &&
                                        MetadataHandling.GetDataObjectType(localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObject.ToString()), "",
                                            configurationSetting) != MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext
                                  select localRow;

                    foreach (DataRow detailRow in results)
                    {
                        var targetObjectDetail = (string)detailRow[DataObjectMappingGridColumns.TargetDataObject.ToString()];

                        bool tupleAlreadyExists = subjectAreaList.Any(m => m.Item1 == subjectArea && m.Item2 == targetObject && m.Item3 == targetObjectDetail);

                        if (!tupleAlreadyExists)
                        {
                            subjectAreaList.Add(new Tuple<string, string, string>(subjectArea, targetObject, targetObjectDetail));
                        }
                    }
                }

                if (targetObjectType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationship)
                {
                    string subjectArea = targetObject.Replace(configurationSetting.LinkTablePrefixValue + "_", "");

                    // Retrieve the related objects (relationship context tables in this case)
                    var results = from localRow in DataTable.AsEnumerable()
                                  where localRow.Field<string>(DataObjectMappingGridColumns.SourceDataObject.ToString()) == sourceObject && // Is in the same source cluster
                                        localRow.Field<string>(DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()).Contains(targetObjectBusinessKey) && // Contains a part of the business key
                                                                                                                                                                  //localRow.Field<string>(TableMappingMetadataColumns.TargetTable.ToString()) != targetObject && // Is not itself
                                        MetadataHandling.GetDataObjectType(localRow.Field<string>(DataObjectMappingGridColumns.TargetDataObject.ToString()), "",
                                            configurationSetting) != MetadataHandling.DataObjectTypes.CoreBusinessConcept // Is a relationship context table.
                                  select localRow;

                    foreach (DataRow detailRow in results)
                    {
                        var targetObjectDetail = (string)detailRow[DataObjectMappingGridColumns.TargetDataObject.ToString()];

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
