using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace TEAM_Library
{
    public class MetadataValidations
    {
        public int ValidationIssues { get; set; } = 0;
        public static bool ValidationRunning { get; set; } = false;
    }

    public static class TeamValidation
    {
        public static List<string> ValidateDuplicateDataItemMappings(List<DataRow> rowList, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            // Informing the user.
            resultList.Add("\r\n--> Commencing the validation to check if any duplicate data item mappings are present.\r\n");

            var duplicateRows = rowList.GroupBy(r =>
                    new {
                        sourceDataObjectName = r.ItemArray[(int)DataItemMappingGridColumns.SourceDataObject].ToString(),
                        sourceDataItemName = r.ItemArray[(int)DataItemMappingGridColumns.SourceDataItem].ToString(),
                        TargetDataObjectName = r.ItemArray[(int)DataItemMappingGridColumns.TargetDataObject].ToString(),
                        TargetDataItemName = r.ItemArray[(int)DataItemMappingGridColumns.TargetDataItem].ToString()
                    })
                .Where(g => g.Count() > 1);

            var distinctDeduplicatedRows = duplicateRows.Distinct().ToList();

            // Evaluate the results.
            int localValidationIssues = 0;

            foreach (var result in distinctDeduplicatedRows)
            {
                resultList.Add($"     The data item mapping from '{result.Key.sourceDataObjectName}' to '{result.Key.TargetDataObjectName}' with the data item mapping '{result.Key.sourceDataItemName}' to '{result.Key.TargetDataItemName}' is duplicate.\r\n");

                localValidationIssues++;
                metadataValidations.ValidationIssues++;
            }

            if (localValidationIssues == 0)
            {
                resultList.Add("     There were no full row duplicates found in the data item mapping.\r\n");
            }

            return resultList;
        }


        public static List<string> ValidateDuplicateDataObjectMappings(List<DataGridViewRow> rowList, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            // Informing the user.
            resultList.Add("\r\n--> Commencing the validation to check if any duplicate data object mappings are present.\r\n");

            var duplicateRows = rowList.GroupBy(r =>
                    new {
                        sourceDataObjectName = r.Cells[(int)DataObjectMappingGridColumns.SourceDataObjectName].Value.ToString(),
                        TargetDataObjectName = r.Cells[(int)DataObjectMappingGridColumns.TargetDataObjectName].Value.ToString(),
                        BusinessKeyDefinition = r.Cells[(int)DataObjectMappingGridColumns.BusinessKeyDefinition].Value.ToString(),
                        FilterCriterion = r.Cells[(int)DataObjectMappingGridColumns.FilterCriterion].Value.ToString(),
                    })
                .Where(g => g.Count() > 1);

            var distinctDeduplicatedRows = duplicateRows.Distinct().ToList();

            // Evaluate the results.
            int localValidationIssues = 0;

            foreach (var result in distinctDeduplicatedRows)
            {
                resultList.Add($"     The data object mapping from {result.Key.sourceDataObjectName} to {result.Key.TargetDataObjectName} with {result.Key.BusinessKeyDefinition} is duplicate.\r\n");

                localValidationIssues++;
                metadataValidations.ValidationIssues++;
            }

            if (localValidationIssues == 0)
            {
                resultList.Add("     There were no full row duplicates found in the data object mapping.\r\n");
            }

            return resultList;
        }

        /// <summary>
        /// This method runs a check against the Column Mappings DataGrid to assert if model metadata is available for the attributes. The column needs to exist somewhere, either in the physical model or in the model metadata in order for activation to run successfully.
        /// </summary>
        public static List<string> ValidateSchemaConfiguration(List<DataRow> dataRows, TeamConfiguration teamConfiguration, EventLog eventLog, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            // Informing the user.
            resultList.Add("\r\n--> Commencing the validation to check if connection settings align with schemas entered in the Data Object mapping grid.\r\n");

            int resultCounter = 0;

            foreach (DataRow row in dataRows)
            {
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True") // If row is enabled
                {
                    string localSourceConnectionInternalId = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    string localTargetConnectionInternalId = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();

                    TeamConnection sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(localSourceConnectionInternalId, teamConfiguration, eventLog);
                    TeamConnection targetConnection = TeamConnection.GetTeamConnectionByConnectionId(localTargetConnectionInternalId, teamConfiguration, eventLog);

                    // The values in the data grid, fully qualified. This means the default schema is added if necessary.
                    var sourceDataObject = MetadataHandling.GetFullyQualifiedDataObjectName(row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString(), sourceConnection).FirstOrDefault();
                    var targetDataObject = MetadataHandling.GetFullyQualifiedDataObjectName(row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString(), targetConnection).FirstOrDefault();

                    // The values as defined in the associated connections
                    var sourceSchemaNameForConnection = TeamConfiguration.GetTeamConnectionByInternalId(localSourceConnectionInternalId, teamConfiguration.ConnectionDictionary).DatabaseServer.SchemaName.Replace("[", "").Replace("]", "");
                    var targetSchemaNameForConnection = TeamConfiguration.GetTeamConnectionByInternalId(localTargetConnectionInternalId, teamConfiguration.ConnectionDictionary).DatabaseServer.SchemaName.Replace("[", "").Replace("]", "");


                    if (sourceDataObject.Key.Replace("[", "").Replace("]", "") != sourceSchemaNameForConnection)
                    {
                        resultList.Add($"--> Inconsistency detected for '{sourceDataObject.Key}.{sourceDataObject.Value}' between the schema definition in the table grid '{sourceDataObject.Key}' and its assigned connection '{sourceConnection.ConnectionName}' which has been configured as '{sourceSchemaNameForConnection}'.\r\n");
                        resultCounter++;
                        metadataValidations.ValidationIssues++;
                    }

                    if (targetDataObject.Key.Replace("[", "").Replace("]", "") != targetSchemaNameForConnection)
                    {
                        resultList.Add($"--> Inconsistency for '{sourceDataObject.Key}.{sourceDataObject.Value}' detected between the schema definition in the table grid {targetDataObject.Key} and its assigned connection ''{targetConnection.ConnectionName}'' which has been configured as '{targetSchemaNameForConnection}'.\r\n");
                        resultCounter++;
                        metadataValidations.ValidationIssues++;
                    }
                }
            }

            if (resultCounter == 0)
            {
                resultList.Add("     There were no validation issues related to schema configuration.\r\n");
            }

            return resultList;
        }

        /// <summary>
        /// Hard-coded fields in Staging Layer data objects are not supported. Instead, an attribute mapping should be created.
        /// </summary>
        public static List<string> ValidateHardcodedFields(List<DataRow> dataRows, TeamConfiguration teamConfiguration, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            resultList.Add("\r\n--> Commencing the validation to see if any hard-coded fields are not correctly set in enabled mappings.\r\n");

            int localValidationIssues = 0;

            foreach (DataRow row in dataRows)
            {
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                // If enabled and is a Staging Layer object
                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True" &&
                    MetadataHandling.GetDataObjectType((string)row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()], "", teamConfiguration).In(MetadataHandling.DataObjectTypes.StagingArea, MetadataHandling.DataObjectTypes.PersistentStagingArea))
                {
                    if (row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].ToString().Contains("'"))
                    {
                        localValidationIssues++;
                        metadataValidations.ValidationIssues++;
                        resultList.Add($"     Data Object {(string)row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()]} should not contain hard-coded values in the Business Key definition. This can not be supported in the Staging Layer (Staging Area and Persistent Staging Area)");
                    }
                }
            }

            if (localValidationIssues == 0)
            {
                resultList.Add("     There were no validation issues related to the definition of hard-coded Business Key components.\r\n");
            }

            return resultList;
        }

        /// <summary>
        /// For the input data objects, check if the data objects as used in the data item mapping grid are also available in the data object mappings.
        /// </summary>
        /// <param name="filteredDataItemMappingRows"></param>
        /// <param name="dataObjectMappingRows"></param>
        /// <param name="metadataValidations"></param>
        /// <returns></returns>
        public static List<string> ValidateAttributeDataObjectsForTableMappings(List<DataRow> filteredDataItemMappingRows, List<DataRow> dataObjectMappingRows, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            resultList.Add("\r\n--> Commencing the validation to see if all data item (attribute) mappings exist as data object (table) mapping also (if enabled in the grid).\r\n");

            // Create a list of all sources and targets for the Data Object mappings
            List<Tuple<string, string>> sourceDataObjectListTableMapping = new List<Tuple<string, string>>();
            List<Tuple<string, string>> targetDataObjectListTableMapping = new List<Tuple<string, string>>();

            int localValidationIssues = 0;

            foreach (DataRow row in dataObjectMappingRows)
            {
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                var sourceDataObjectTuple = new Tuple<string, string>((string)row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()], row[DataObjectMappingGridColumns.Enabled.ToString()].ToString());
                var targetDataObjectTuple = new Tuple<string, string>((string)row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()], row[DataObjectMappingGridColumns.Enabled.ToString()].ToString());

                if (!sourceDataObjectListTableMapping.Contains(sourceDataObjectTuple))
                {
                    sourceDataObjectListTableMapping.Add(sourceDataObjectTuple);
                }

                if (!targetDataObjectListTableMapping.Contains(targetDataObjectTuple))
                {
                    targetDataObjectListTableMapping.Add(targetDataObjectTuple);
                }
            }

            foreach (DataRow row in filteredDataItemMappingRows)
            {
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                var localSource = (string)row[DataItemMappingGridColumns.SourceDataObject.ToString()];
                var localTarget = (string)row[DataItemMappingGridColumns.TargetDataObject.ToString()];

                // If the value exists, but is disabled just a warning is sufficient.
                // If the value does not exist for an enabled mapping or at all, then it's an error.

                if (sourceDataObjectListTableMapping.Contains(new Tuple<string, string>(localSource, "False")))
                {
                    //_alertValidation.SetTextLogging($"     Data Object {localSource} in the attribute mappings exists in the table mappings for an disabled mapping. This can be disregarded.\r\n");
                }
                else if (sourceDataObjectListTableMapping.Contains(new Tuple<string, string>(localSource, "True")))
                {
                    // No problem, it's found.
                }
                else
                {
                    resultList.Add($"     Data Object {localSource} in the attribute mappings (source) does not seem to exist in the table mappings for an enabled mapping. Please check if this name is mapped at table level in the grid also.\r\n");
                    metadataValidations.ValidationIssues++;
                    localValidationIssues++;
                }

                if (targetDataObjectListTableMapping.Contains(new Tuple<string, string>(localTarget, "False")))
                {
                    //_alertValidation.SetTextLogging($"     Data Object {localTarget} in the attribute mappings exists in the table mappings for an disabled mapping. This can be disregarded.\r\n");
                }
                else if (targetDataObjectListTableMapping.Contains(new Tuple<string, string>(localTarget, "True")))
                {
                    // No problem, it's found/
                }
                else
                {
                    resultList.Add($"     Data Object {localTarget}, as used as a target object in the data item (column) mappings has encountered a validation issue. This object does not seem to exist in the data object mappings for an enabled mapping. Please check if this name is mapped at data object level in the grid.\r\n");
                    metadataValidations.ValidationIssues++;
                    localValidationIssues++;
                }
            }

            if (localValidationIssues == 0)
            {
                resultList.Add("     There were no validation issues related to the existence of Data Objects related to defined Data Item Mappings.\r\n");
            }

            return resultList;
        }

        public static List<string> ValidateLinkCompletion(List<DataRow> rowList, TeamConfiguration teamConfiguration, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            // Informing the user.
            resultList.Add("\r\n--> Commencing the validation to determine if Link concept is correctly defined.\r\n");

            var localConnectionDictionary = LocalConnectionDictionary.GetLocalConnectionDictionary(teamConfiguration.ConnectionDictionary);

            var objectList = new List<Tuple<string, string, string, string>>(); // Source, Target, Business Key, Target Connection

            foreach (DataRow row in rowList)
            {
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                // Only process enabled mappings.
                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() != "True") continue;

                // Only select the lines that relate to a Link target.
                if (row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString().StartsWith(teamConfiguration.LinkTablePrefixValue))
                {
                    // Derive the business key.
                    var businessKey = row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].ToString().Replace("''''", "'");

                    // Derive the connection
                    localConnectionDictionary.TryGetValue(row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString(), out var targetConnectionValue);

                    var newValidationObject = new Tuple<string, string, string, string>
                    (
                        row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString(),
                        row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString(),
                        businessKey,
                        targetConnectionValue
                    );

                    if (!objectList.Contains(newValidationObject))
                    {
                        objectList.Add(newValidationObject);
                    }
                }

            }

            // Execute the validation check using the list of unique objects
            var resultDictionary = new Dictionary<string, bool>();

            foreach (var linkObject in objectList)
            {
                // The validation check returns a Dictionary
                var validatedObject = ValidateLinkBusinessKeyForCompletion(linkObject);

                // Looping through the dictionary to evaluate results.
                foreach (var pair in validatedObject)
                {
                    if (pair.Value == false)
                    {
                        if (!resultDictionary.ContainsKey(pair.Key)) // Prevent incorrect links to be added multiple times
                        {
                            resultDictionary.Add(pair.Key, pair.Value); // Add objects that did not pass the test
                        }
                    }
                }
            }

            // Return the results back to the user
            if (resultDictionary.Count > 0)
            {
                foreach (var sourceObjectResult in resultDictionary)
                {
                    resultList.Add("     " + sourceObjectResult.Key + " is tested with this outcome: " + sourceObjectResult.Value + ". This means there is an issue with the Link definition, and in particular the Business Key. Are two Hubs assigned?\r\n");
                    metadataValidations.ValidationIssues++;
                }
                resultList.Add("\r\n");
            }
            else
            {
                resultList.Add("     There were no validation issues related to the definition of Link tables.\r\n");
            }

            return resultList;
        }

        internal static Dictionary<string, bool> ValidateLinkBusinessKeyForCompletion(Tuple<string, string, string, string> validationObject)
        {
            // Incoming validationObject Tuple is defined as Source, Target, Business Key, Target Connection.

            // Preparing result set to store validation results.
            Dictionary<string, bool> result = new Dictionary<string, bool>();

            List<string> hubBusinessKeysForLink = validationObject.Item3.Split(',').ToList();

            if (hubBusinessKeysForLink.Count <= 1)
            {
                // This is not correct, there should be at least 2 Hub keys
                result.Add(validationObject.Item2, false);
            }
            else
            {
                result.Add(validationObject.Item2, true);
            }

            return result;
        }

        /// <summary>
        /// This method will check if the order of the keys in the Link is consistent with the physical table structures.
        /// </summary>
        public static List<string> ValidateLinkKeyOrder(List<DataRow> dataRows, DataTable dataTableDataObjects, DataTable dataTablePhysicalModel, List<DataGridViewRow> dataGridViewRowsDataObjects, TeamConfiguration teamConfiguration, EventLog eventLog, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            #region Retrieving the Links

            // Informing the user.
            resultList.Add("\r\n--> Commencing the validation to ensure the order of Business Keys in the Link metadata corresponds with the physical model.\r\n");

            var localConnectionDictionary = LocalConnectionDictionary.GetLocalConnectionDictionary(teamConfiguration.ConnectionDictionary);

            var objectList = new List<Tuple<string, string, string, string>>();

            foreach (DataRow row in dataRows)
            {
                // Skip deleted rows
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True")
                {
                    // Only select the lines that relate to a Link target.
                    if (row[DataObjectMappingGridColumns.TargetDataObject.ToString()].ToString().StartsWith(teamConfiguration.LinkTablePrefixValue))
                    {
                        // Derive the business key.
                        var businessKey = row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].ToString().Replace("''''", "'");

                        // Derive the connection
                        localConnectionDictionary.TryGetValue(row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString(), out var connectionValue);

                        var newValidationObject = new Tuple<string, string, string, string>
                        (
                            row[DataObjectMappingGridColumns.SourceDataObject.ToString()].ToString(),
                            row[DataObjectMappingGridColumns.TargetDataObject.ToString()].ToString(),
                            businessKey,
                            connectionValue
                        );

                        if (!objectList.Contains(newValidationObject))
                        {
                            objectList.Add(newValidationObject);
                        }
                    }
                }
            }

            // Execute the validation check using the list of unique objects
            var resultDictionary = new Dictionary<string, bool>();

            foreach (var sourceObject in objectList)
            {
                // The validation check returns a Dictionary
                var sourceObjectValidated = ValidateLinkKeyOrder(sourceObject, dataTableDataObjects, dataTablePhysicalModel, dataGridViewRowsDataObjects, teamConfiguration, eventLog);

                // Looping through the dictionary
                foreach (var pair in sourceObjectValidated)
                {
                    if (pair.Value == false)
                    {
                        if (!resultDictionary.ContainsKey(pair.Key)) // Prevent incorrect links to be added multiple times
                        {
                            resultDictionary.Add(pair.Key, pair.Value); // Add objects that did not pass the test
                        }
                    }
                }
            }

            #endregion

            // Return the results back to the user
            if (resultDictionary.Count > 0)
            {
                foreach (var sourceObjectResult in resultDictionary)
                {
                    resultList.Add("     " + sourceObjectResult.Key + " is tested with this outcome: " + sourceObjectResult.Value + "\r\n");

                    if (sourceObjectResult.Value == false)
                    {
                        metadataValidations.ValidationIssues++;
                    }
                }
                resultList.Add("\r\n");
            }
            else
            {
                resultList.Add("     There were no validation issues related to order of business keys in the Link tables.\r\n");
            }

            return resultList;
        }

        /// <summary>
        /// Check the ordinal position of Link Keys against their business key definitions.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="dataObjectMappingDataTable"></param>
        /// <param name="physicalModelDataTable"></param>
        /// <returns></returns>
        internal static Dictionary<string, bool> ValidateLinkKeyOrder(Tuple<string, string, string, string> validationObject, DataTable dataObjectMappingDataTable, DataTable physicalModelDataTable, List<DataGridViewRow> dataGridViewRowsDataObjects, TeamConfiguration teamConfiguration, EventLog eventLog)
        {
            // First, the Hubs need to be identified using the Business Key information. This, for the Link, is the combination of Business keys separated by a comma.
            // Every business key needs to be iterated over to query the individual Hub information
            List<string> hubBusinessKeys = validationObject.Item3.Split(',').ToList();

            // Now iterate over each Hub, as identified by the business key.
            // Maintain the ordinal position of the business key
            var hubKeyOrder = new Dictionary<int, string>();

            int businessKeyOrder = 0;
            foreach (string hubBusinessKey in hubBusinessKeys)
            {
                // Determine the order in the business key array
                businessKeyOrder++;

                // Query the Hub information
                DataRow[] selectionRows = dataObjectMappingDataTable.Select(DataObjectMappingGridColumns.SourceDataObject + " = '" + validationObject.Item1 + "' AND " +
                                                                DataObjectMappingGridColumns.BusinessKeyDefinition + " = '" + hubBusinessKey.Replace("'", "''").Trim() + "' AND " +
                                                                DataObjectMappingGridColumns.TargetDataObject + " NOT LIKE '" + teamConfiguration.SatTablePrefixValue + "_%'");

                try
                {
                    // Derive the Hub surrogate key name, as this can be compared against the Link
                    string hubTableConnectionId = selectionRows[0][DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                    var hubTableConnection = TeamConnection.GetTeamConnectionByConnectionId(hubTableConnectionId, teamConfiguration, eventLog);

                    string hubSurrogateKeyName = JsonOutputHandling.DeriveSurrogateKey(validationObject.Item2, validationObject.Item1, validationObject.Item3, hubTableConnection, teamConfiguration, dataGridViewRowsDataObjects);

                    // Add to the dictionary that contains the keys in order.
                    hubKeyOrder.Add(businessKeyOrder, hubSurrogateKeyName);
                }
                catch
                {
                    //
                }
            }

            // Derive the Hub surrogate key name, as this can be compared against the Link
            var linkKeyOrder = new Dictionary<int, string>();


            int linkHubSurrogateKeyPosition = 1;

            var workingTable = new DataTable();

            try
            {
                // Select only the business keys in a link table. 
                // Excluding all non-business key attributes
                workingTable = physicalModelDataTable
                    .Select($"{PhysicalModelMappingMetadataColumns.tableName} LIKE '%{teamConfiguration.LinkTablePrefixValue}%' " +
                            $"AND {PhysicalModelMappingMetadataColumns.tableName} = '{validationObject.Item2}' " +
                            $"AND {PhysicalModelMappingMetadataColumns.ordinalPosition} > 4", $"{PhysicalModelMappingMetadataColumns.ordinalPosition} ASC").CopyToDataTable();
            }
            catch (Exception ex)
            {
                eventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error occurred during validation of the metadata. The errors is {ex}."));
            }

            if (workingTable.Rows.Count > 0)
            {
                foreach (DataRow row in workingTable.Rows)
                {
                    var linkHubSurrogateKeyName = row[PhysicalModelMappingMetadataColumns.columnName.ToString()].ToString();

                    if (linkHubSurrogateKeyName.Contains(teamConfiguration.KeyIdentifier)) // Exclude degenerate attributes from the order
                    {
                        linkKeyOrder.Add(linkHubSurrogateKeyPosition, linkHubSurrogateKeyName);
                        linkHubSurrogateKeyPosition++;
                    }
                }
            }

            // Check for duplicates, which indicate a Same-As Link or Hierarchical Link
            var duplicateValues = hubKeyOrder.Where(i => hubKeyOrder.Any(t => t.Key != i.Key && t.Value == i.Value)).ToDictionary(i => i.Key, i => i.Value);


            // Run the comparison, test for equality.
            // Only if there are no duplicates, as this indicates the SAL / HLINK which is not currently supported
            bool equal = false;
            if (duplicateValues.Count == 0)
            {
                if (hubKeyOrder.Count == linkKeyOrder.Count) // Require equal count.
                {
                    equal = true;
                    foreach (var pair in hubKeyOrder)
                    {
                        string value;
                        if (linkKeyOrder.TryGetValue(pair.Key, out value))
                        {
                            // Require value be equal.
                            if (value != pair.Value)
                            {
                                equal = false;
                                break;
                            }
                        }
                        else
                        {
                            // Require key be present.
                            equal = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                equal = true;
            }

            // return the result of the test;
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            result.Add(validationObject.Item2, equal);
            return result;
        }

        /// <summary>
        /// Checks if all the supporting mappings are available (e.g. a Context table also needs a Core Business Concept present).
        /// </summary>
        public static List<string> ValidateLogicalGroup(List<DataRow> dataRows, DataTable dataTableDataObjectMappings, TeamConfiguration teamConfiguration, EventLog eventLog, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            #region Retrieving the Integration Layer tables

            // Informing the user.
            resultList.Add("\r\n--> Commencing the validation to check if the functional dependencies (logical group / unit of work) are present.\r\n");

            // Creating a list of tables which are dependent on other tables being present
            var objectList = new List<Tuple<string, string, string, string>>(); // Source Name, Target Name, Business Key, FilterCriterion

            foreach (DataRow row in dataRows)
            {
                // Skip deleted rows
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True")
                {
                    var targetDataObjectName = row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString();
                    var targetConnectionInternalId = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                    var targetConnection = TeamConnection.GetTeamConnectionByConnectionId(targetConnectionInternalId, teamConfiguration, eventLog);
                    var targetFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObjectName, targetConnection).FirstOrDefault();
                    var targetTableType = MetadataHandling.GetDataObjectType(targetDataObjectName, "", teamConfiguration);
                    var targetFilterCriterion = row[DataObjectMappingGridColumns.FilterCriterion.ToString()].ToString();

                    var sourceDataObjectName = row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString();
                    var sourceConnectionInternalId = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    var sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(sourceConnectionInternalId, teamConfiguration, eventLog);
                    var sourceFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(sourceDataObjectName, sourceConnection).FirstOrDefault();

                    if (targetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationship || targetTableType == MetadataHandling.DataObjectTypes.Context || targetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext)
                    {
                        var businessKey = row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].ToString().Replace("''''", "'");

                        if (!objectList.Contains(new Tuple<string, string, string, string>
                            (
                            sourceFullyQualifiedName.Key + '.' + sourceFullyQualifiedName.Value, targetFullyQualifiedName.Key + '.' + targetFullyQualifiedName.Value, businessKey, targetFilterCriterion)
                            )
                        )
                        {
                            objectList.Add(new Tuple<string, string, string, string>
                                (
                                    sourceFullyQualifiedName.Key + '.' + sourceFullyQualifiedName.Value, targetFullyQualifiedName.Key + '.' + targetFullyQualifiedName.Value, businessKey, targetFilterCriterion)
                            );
                        }
                    }
                }
            }

            // Execute the validation check using the list of unique objects
            var resultDictionary = new Dictionary<string, bool>();

            foreach (var validationObject in objectList)
            {
                // The validation check returns a Dictionary
                var sourceObjectValidated = ValidateLogicalGroup(validationObject, dataTableDataObjectMappings, teamConfiguration, eventLog);

                // Looping through the dictionary
                foreach (var pair in sourceObjectValidated)
                {
                    if (pair.Value == false)
                    {
                        if (!resultDictionary.ContainsKey(pair.Key)) // Prevent incorrect links to be added multiple times
                        {
                            resultDictionary.Add(pair.Key, pair.Value); // Add objects that did not pass the test
                        }
                    }
                }
            }

            #endregion

            // Return the results back to the user
            if (resultDictionary.Count > 0)
            {
                foreach (var sourceObjectResult in resultDictionary)
                {
                    resultList.Add("     " + sourceObjectResult.Key + " is tested with this outcome: " + sourceObjectResult.Value + "." +
                                                    "\r\n     This means there is a Link or Satellite without it's supporting Hub(s) defined." +
                                                    "\r\n     If a source loads a Link or Satellite, this source should also load a Hub that relates to the Link or Satellite.\r\n");
                    metadataValidations.ValidationIssues++;
                }

                resultList.Add("\r\n");
            }
            else
            {
                resultList.Add("     There were no validation issues related to the logical grouping of objects.\r\n");
            }

            return resultList;
        }

        /// <summary>
        /// Validate the relationship between Data Object Mappings, i.e. dependencies between objects which should exist because they are related.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="dataObjectMappingDataTable"></param>
        /// <returns></returns>
        internal static Dictionary<string, bool> ValidateLogicalGroup(Tuple<string, string, string, string> validationObject, DataTable dataObjectMappingDataTable, TeamConfiguration teamConfiguration, EventLog eventLog)
        {
            // The incoming validationObject is defined as Source Name, Target Name and Business Key.

            // First, the Business Key need to be checked. This is to determine how many dependents are expected.
            // For instance, if a Link has a three-part Business Key then three Hubs will be expected
            List<string> hubBusinessKeys = validationObject.Item3.Split(',').ToList();
            int businessKeyCount = hubBusinessKeys.Count;

            // We need to manipulate the query to account for multiplicity in the model i.e. many Satellites linking to a single Hub.
            // The only interest is whether the Hub is there.
            string tableInclusionFilterCriterion;
            var tableClassification = "";

            var inputTargetTableType = MetadataHandling.GetDataObjectType(validationObject.Item2, "", teamConfiguration);

            if (inputTargetTableType == MetadataHandling.DataObjectTypes.Context) // If the table is a Satellite, only the Hub is required
            {
                tableInclusionFilterCriterion = teamConfiguration.HubTablePrefixValue;
                tableClassification = teamConfiguration.SatTablePrefixValue;
            }
            else if (inputTargetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationship) // If the table is a Link, we're only interested in the Hubs
            {
                tableInclusionFilterCriterion = teamConfiguration.HubTablePrefixValue;
                tableClassification = "LNK";
            }
            else if (inputTargetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext) // If the table is a Link-Satellite, only the Link is required
            {
                tableInclusionFilterCriterion = teamConfiguration.LinkTablePrefixValue;
                tableClassification = "LSAT";
            }
            else
            {
                tableInclusionFilterCriterion = "";
            }

            // Unfortunately, there is a separate process for Links and Satellites
            // Iterate through the various keys (mainly for the purpose of evaluating Links)
            int numberOfDependents = 0;
            if (tableClassification == teamConfiguration.SatTablePrefixValue || tableClassification == "LNK")
            {
                foreach (string businessKeyComponent in hubBusinessKeys)
                {
                    foreach (DataRow dataObjectMappingRow in dataObjectMappingDataTable.Rows)
                    {
                        // Skip deleted rows
                        if (dataObjectMappingRow.RowState == DataRowState.Deleted)
                            continue;

                        var TargetDataObject = dataObjectMappingRow[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString();
                        var targetConnectionInternalId = dataObjectMappingRow[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                        var targetConnection = TeamConnection.GetTeamConnectionByConnectionId(targetConnectionInternalId, teamConfiguration, eventLog);
                        var targetFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(TargetDataObject, targetConnection).FirstOrDefault();
                        //var targetTableType = MetadataHandling.GetDataObjectType(TargetDataObject, "", FormBase.TeamConfiguration);
                        var filterCriterion = dataObjectMappingRow[DataObjectMappingGridColumns.FilterCriterion.ToString()].ToString();

                        var sourceDataObjectName = dataObjectMappingRow[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString();
                        var sourceConnectionInternalId = dataObjectMappingRow[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                        var sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(sourceConnectionInternalId, teamConfiguration, eventLog);
                        var sourceFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(sourceDataObjectName, sourceConnection).FirstOrDefault();
                        //var sourceTableType = MetadataHandling.GetDataObjectType(sourceDataObjectName, "", FormBase.TeamConfiguration);

                        // Count the number of dependents.
                        if (
                             dataObjectMappingRow[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True" && // Only active generated objects
                             sourceFullyQualifiedName.Key + '.' + sourceFullyQualifiedName.Value == validationObject.Item1 &&
                             (string)dataObjectMappingRow[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()] == businessKeyComponent.Trim() &&
                             targetFullyQualifiedName.Key + '.' + targetFullyQualifiedName.Value != validationObject.Item2 && // Exclude itself
                             filterCriterion == validationObject.Item4 && // Adding filtercriterion for uniquification of join (see https://github.com/RoelantVos/TEAM/issues/87);
                             targetFullyQualifiedName.Value.StartsWith(tableInclusionFilterCriterion)
                           )
                        {
                            numberOfDependents++;
                        }
                    }
                }
            }
            else // In the case of an LSAT, only join on the Link using the full business key
            {
                // Query the dependent information
                foreach (DataRow row in dataObjectMappingDataTable.Rows)
                {
                    try
                    {
                        var targetDataObjectName = row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString();
                        var targetConnectionInternalId = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                        var targetConnection = TeamConnection.GetTeamConnectionByConnectionId(targetConnectionInternalId, teamConfiguration, eventLog);
                        var targetFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObjectName, targetConnection).FirstOrDefault();
                        //var targetTableType = MetadataHandling.GetDataObjectType(TargetDataObject, "", FormBase.TeamConfiguration);

                        var sourceDataObjectName = row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString();
                        var sourceConnectionInternalId = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                        var sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(sourceConnectionInternalId, teamConfiguration, eventLog);
                        var sourceFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(sourceDataObjectName, sourceConnection).FirstOrDefault();
                        //var sourceTableType = MetadataHandling.GetDataObjectType(sourceDataObjectName, "", FormBase.TeamConfiguration);

                        if (
                            row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True" && // Only active generated objects
                            sourceFullyQualifiedName.Key + '.' + sourceFullyQualifiedName.Value == validationObject.Item1 &&
                            (string)row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()] == validationObject.Item3.Trim() &&
                            targetFullyQualifiedName.Key + '.' + targetFullyQualifiedName.Value != validationObject.Item2 && // Exclude itself
                            targetFullyQualifiedName.Value.StartsWith(tableInclusionFilterCriterion)
                        )
                        {
                            numberOfDependents++;
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore for now.
                    }
                }
            }

            // Run the comparison
            // Test for equality.
            bool equal;
            if ((tableClassification == teamConfiguration.SatTablePrefixValue || tableClassification == "LNK") && businessKeyCount == numberOfDependents) // For Sats and Links we can count the keys and rows
            {
                equal = true;
            }
            else if (tableClassification == "LSAT" && numberOfDependents == 1)
            {
                equal = true;
            }
            else
            {
                equal = false;
            }

            // return the result of the test;
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            result.Add(validationObject.Item2, equal);
            return result;
        }

        /// <summary>
        /// This method runs a check against the Attribute Mappings DataGrid to assert if model metadata is available for the attributes. The attribute needs to exist somewhere, either in the physical model or in the model metadata in order for activation to run successfully.
        /// </summary>
        public static List<string> ValidateDataItemExistence(List<DataRow> filteredDataItemMappingRows, DataTable dataObjectMappingDataTable, DataTable physicalModelDataTable, TeamConfiguration teamConfiguration, EventLog eventLog, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            // Informing the user.
            resultList.Add("\r\n--> Commencing the validation to determine if the data items (columns) in the metadata exists in the model.\r\n");

            var resultDictionary = new Dictionary<string, string>();

            foreach (DataRow row in filteredDataItemMappingRows)
            {
                // Look for the corresponding Data Object Mapping row.
                var dataObjectRow = GetDataObjectMappingFromDataItemMapping(dataObjectMappingDataTable, row[DataItemMappingGridColumns.SourceDataObject.ToString()].ToString(), row[DataItemMappingGridColumns.TargetDataObject.ToString()].ToString(), teamConfiguration, eventLog);

                if (dataObjectRow.Item1 == "True") //If the corresponding Data Object is enabled
                {
                    var validationObjectSource = row[DataItemMappingGridColumns.SourceDataObject.ToString()].ToString();
                    TeamConnection sourceConnection = dataObjectRow.Item3;
                    var validationAttributeSource = row[DataItemMappingGridColumns.SourceDataItem.ToString()].ToString();

                    var validationObjectTarget = row[DataItemMappingGridColumns.TargetDataObject.ToString()].ToString();
                    TeamConnection targetConnection = dataObjectRow.Item5;
                    var validationAttributeTarget = row[DataItemMappingGridColumns.TargetDataItem.ToString()].ToString();

                    var sourceDataObjectType = MetadataHandling.GetDataObjectType(validationObjectSource, "", teamConfiguration).ToString();

                    // No need to evaluate the operational system (real sources), or if the source is a data query (logic).
                    if (sourceDataObjectType != MetadataHandling.DataObjectTypes.Source.ToString() && !validationAttributeSource.IsDataQuery())
                    {
                        var objectValidated = ValidateAttributeExistence(validationObjectSource, validationAttributeSource, sourceConnection, physicalModelDataTable);

                        // Add negative results to dictionary
                        if (objectValidated == "False" && !resultDictionary.ContainsKey(validationAttributeSource))
                        {
                            resultDictionary.Add(validationAttributeSource, validationObjectSource); // Add objects that did not pass the test
                        }

                        objectValidated = ValidateAttributeExistence(validationObjectTarget, validationAttributeTarget, targetConnection, physicalModelDataTable);

                        // Add negative results to dictionary
                        if (objectValidated == "False" && !resultDictionary.ContainsKey(validationAttributeTarget))
                        {
                            resultDictionary.Add(validationAttributeTarget, validationObjectTarget); // Add objects that did not pass the test
                        }
                    }
                }
            }

            // Return the results back to the user
            if (resultDictionary.Count > 0)
            {
                foreach (var objectValidationResult in resultDictionary)
                {
                    resultList.Add($"     {objectValidationResult.Key} belonging to {objectValidationResult.Value} does not exist in the physical model snapshot.\r\n");
                    metadataValidations.ValidationIssues++;
                }
                resultList.Add("\r\n");
            }
            else
            {
                resultList.Add($"     There were no validation issues related to the existence of the data item(s).\r\n");
            }

            return resultList;
        }

        /// <summary>
        /// Returns the source, and target connection for a given input source and target mapping.
        /// Item 1 is the enabled flag, item 2 is the source, item 3 the source connection, item 4 the target and Item 5 is the target connection.
        /// </summary>
        /// <param name="dataObjectMappingDataTable"></param>
        /// <param name="sourceTable"></param>
        /// <param name="targetTable"></param>
        /// <returns></returns>
        private static Tuple<string, string, TeamConnection, string, TeamConnection> GetDataObjectMappingFromDataItemMapping(DataTable dataObjectMappingDataTable, string sourceTable, string targetTable, TeamConfiguration teamConfiguration, EventLog eventLog)
        {
            // Default return value
            Tuple<string, string, TeamConnection, string, TeamConnection> returnTuple = new Tuple<string, string, TeamConnection, string, TeamConnection>
                (
                    "False",
                    sourceTable,
                    null,
                    targetTable,
                    null
                );

            // Find the corresponding row in the Data Object Mapping grid
            DataRow[] DataObjectMappings = dataObjectMappingDataTable.Select("[" + DataObjectMappingGridColumns.SourceDataObjectName + "] = '" + sourceTable + "' AND" + "[" + DataObjectMappingGridColumns.TargetDataObjectName + "] = '" + targetTable + "'");

            if (DataObjectMappings.Length == 0)
            {
                // There is no matching row found in the Data Object Mapping grid. Validation should pick this up!
                eventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"While processing the Data Item mappings, no matching Data Object mapping was found."));

            }
            else if (DataObjectMappings.Length > 1)
            {
                // There are too many entries! There should be only a single mapping from source to target
                eventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"While processing the Data Item mappings, to many (more than 1) matching Data Object mapping were found."));
            }
            else
            {
                var connectionInternalIdSource = DataObjectMappings[0][DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                var connectionInternalIdTarget = DataObjectMappings[0][DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                TeamConnection sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(connectionInternalIdSource, teamConfiguration, eventLog);
                TeamConnection targetConnection = TeamConnection.GetTeamConnectionByConnectionId(connectionInternalIdTarget, teamConfiguration, eventLog);

                // Set the right values
                returnTuple = new Tuple<string, string, TeamConnection, string, TeamConnection>
                (
                    DataObjectMappings[0][DataObjectMappingGridColumns.Enabled.ToString()].ToString(),
                    sourceTable,
                    sourceConnection,
                    targetTable,
                    targetConnection
                );

            }

            return returnTuple;
        }


        /// <summary>
        /// Check if an attribute exists in the metadata.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="validationAttribute"></param>
        /// <param name="teamConnection"></param>
        /// <param name="physicalModelDataTable"></param>
        /// <returns></returns>
        internal static string ValidateAttributeExistence(string validationObject, string validationAttribute, TeamConnection teamConnection, DataTable physicalModelDataTable)
        {
            string returnExistenceEvaluation = "False";

            if (validationAttribute != "NULL")
            {
                var objectDetails = MetadataHandling.GetFullyQualifiedDataObjectName(validationObject, teamConnection).FirstOrDefault();

                string filterCriterion = PhysicalModelMappingMetadataColumns.tableName + " = '" + objectDetails.Value + "' AND " + PhysicalModelMappingMetadataColumns.schemaName + "='" + objectDetails.Key + "' AND " + PhysicalModelMappingMetadataColumns.columnName + " = '" + validationAttribute + "'";

                DataRow[] foundRows = physicalModelDataTable.Select(filterCriterion);

                if (foundRows.Length > 0)
                {
                    returnExistenceEvaluation = "True";
                }
            }
            else
            {
                returnExistenceEvaluation = "True";
            }

            // return the result of the test;
            return returnExistenceEvaluation;
        }

        /// <summary>
        ///   A validation check to make sure the Business Key is available in the source model.
        /// </summary>
        public static List<string> ValidateBusinessKeyObject(List<DataRow> dataRows, TeamConfiguration teamConfiguration, EventLog eventLog, DataTable physicalModelDataTable, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            var resultDictionary = new Dictionary<Tuple<string, string>, bool>();

            // Informing the user.
            resultList.Add("\r\n--> Commencing the validation to determine if the Business Key metadata attributes exist in the physical model.\r\n");

            foreach (DataRow row in dataRows)
            {
                // Skip deleted rows
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True") // If row is enabled
                {
                    Dictionary<Tuple<string, string>, bool> objectValidated = new Dictionary<Tuple<string, string>, bool>();

                    // Source table and business key definitions.
                    string validationObject = row[DataObjectMappingGridColumns.SourceDataObject.ToString()].ToString();
                    string validationConnectionId = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    TeamConnection validationConnection = TeamConnection.GetTeamConnectionByConnectionId(validationConnectionId, teamConfiguration, eventLog);
                    string businessKeyDefinition = row[DataObjectMappingGridColumns.BusinessKeyDefinition.ToString()].ToString();

                    // Exclude a lookup to the source
                    if (MetadataHandling.GetDataObjectType(validationObject, "", teamConfiguration).ToString() != MetadataHandling.DataObjectTypes.Source.ToString())
                    {
                        objectValidated = ValidateSourceBusinessKeyExistenceVirtual(validationObject, businessKeyDefinition, validationConnection, physicalModelDataTable);
                    }

                    // Add negative results to dictionary
                    foreach (var objectValidationTuple in objectValidated)
                    {
                        if (objectValidationTuple.Value == false && !resultDictionary.ContainsKey(objectValidationTuple.Key))
                        {
                            resultDictionary.Add(objectValidationTuple.Key, false); // Add objects that did not pass the test
                        }
                    }
                }
            }

            // Return the results back to the user
            if (resultDictionary.Count > 0)
            {
                foreach (var sourceObjectResult in resultDictionary)
                {
                    resultList.Add("     Table " + sourceObjectResult.Key.Item1 + " does not contain Business Key attribute " + sourceObjectResult.Key.Item2 + ".\r\n");
                    metadataValidations.ValidationIssues++;
                }
            }
            else
            {
                resultList.Add("     There were no validation issues related to the existence of the business keys in the Source tables.\r\n");
            }

            return resultList;
        }

        /// <summary>
        /// Validate the Business Key definition against the snapshot of the physical model (the physical model data grid), taking the source object and business key definition as input parameters, together with a connection string to validate against.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="teamConnection"></param>
        /// <param name="inputDataTable"></param>
        /// <returns></returns>
        /// 
        internal static Dictionary<Tuple<string, string>, bool> ValidateSourceBusinessKeyExistenceVirtual(string validationObject, string businessKeyDefinition, TeamConnection teamConnection, DataTable inputDataTable)
        {
            // First, the Business Keys for each table need to be identified information. This can be the combination of Business keys separated by a comma.
            // Every business key needs to be iterated over to validate if the attribute exists in that table.
            List<string> businessKeys = businessKeyDefinition.Split(',').ToList();

            Dictionary<Tuple<string, string>, bool> result = new Dictionary<Tuple<string, string>, bool>();

            foreach (string businessKey in businessKeys)
            {
                var trimBusinessKey = businessKey.Trim();

                // Handle concatenate and composite
                List<string> subKeys = new List<string>();

                if (trimBusinessKey.StartsWith("CONCATENATE"))
                {
                    var localBusinessKey = trimBusinessKey.Replace("CONCATENATE(", "").Replace(")", "");

                    subKeys = localBusinessKey.Split(';').ToList();
                }
                else if (trimBusinessKey.StartsWith("COMPOSITE"))
                {
                    var localBusinessKey = trimBusinessKey.Replace("COMPOSITE(", "").Replace(")", "");

                    subKeys = localBusinessKey.Split(';').ToList();
                }
                else
                {
                    subKeys.Add(trimBusinessKey);
                }

                foreach (string businessKeyPart in subKeys)
                {
                    // Handle hard-coded business key values
                    if (businessKeyPart.StartsWith("'") && businessKeyPart.EndsWith("'"))
                    {
                        // Do nothing
                    }
                    else
                    {
                        var objectDetails = MetadataHandling.GetFullyQualifiedDataObjectName(validationObject, teamConnection).FirstOrDefault();

                        bool returnExistenceEvaluation = false;

                        DataRow[] foundAuthors = inputDataTable.Select($"" + PhysicalModelMappingMetadataColumns.tableName + " = '" + objectDetails.Value + "' AND " + PhysicalModelMappingMetadataColumns.schemaName + " = '" + objectDetails.Key + "' AND " + PhysicalModelMappingMetadataColumns.columnName + " = '" + businessKeyPart.Trim() + "'");
                        if (foundAuthors.Length != 0)
                        {
                            returnExistenceEvaluation = true;
                        }

                        result.Add(Tuple.Create(validationObject, businessKeyPart.Trim()), returnExistenceEvaluation);
                    }
                }
            }

            // Return the result of the test;
            return result;
        }

        /// <summary>
        /// This method runs a check against the DataGrid to assert if model metadata is available for the object. The object needs to exist somewhere, either in the physical model or in the model metadata in order for activation to run succesfully.
        /// </summary>
        public static List<string> ValidateObjectExistence(List<DataRow> dataObjectMappingDataRows, DataTable physicalModelDataTable, TeamConfiguration teamConfiguration, EventLog eventLog, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            // Informing the user.
            resultList.Add("\r\n--> Commencing the validation to determine if the defined Data Objects exists in the physical model snapshot.\r\n");

            var resultDictionary = new Dictionary<string, string>();

            // Iterating over the grid
            foreach (DataRow row in dataObjectMappingDataRows)
            {
                // Skip deleted rows
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row[(int)DataObjectMappingGridColumns.Enabled] != DBNull.Value && (string)row[(int)DataObjectMappingGridColumns.Enabled] == "True")
                {
                    // Sources
                    var validationObjectSource = row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString();
                    var validationObjectSourceConnectionId = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    var sourceConnection = TeamConnection.GetTeamConnectionByConnectionId(validationObjectSourceConnectionId, teamConfiguration, eventLog);
                    KeyValuePair<string, string> fullyQualifiedValidationObjectSource = MetadataHandling.GetFullyQualifiedDataObjectName(validationObjectSource, sourceConnection).FirstOrDefault();

                    // Targets
                    var validationObjectTarget = row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString();
                    var validationObjectTargetConnectionId = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                    var targetConnection = TeamConnection.GetTeamConnectionByConnectionId(validationObjectTargetConnectionId, teamConfiguration, eventLog);
                    KeyValuePair<string, string> fullyQualifiedValidationObjectTarget = MetadataHandling.GetFullyQualifiedDataObjectName(validationObjectTarget, targetConnection).FirstOrDefault();

                    // No need to evaluate the operational system (real sources))
                    if (MetadataHandling.GetDataObjectType(validationObjectSource, "", teamConfiguration).ToString() != MetadataHandling.DataObjectTypes.Source.ToString())
                    {
                        var objectValidated = ValidateObjectExistence(validationObjectSource, sourceConnection, physicalModelDataTable);

                        if (objectValidated == "False" && !resultDictionary.ContainsKey(fullyQualifiedValidationObjectSource.Key + '.' + fullyQualifiedValidationObjectSource.Value))
                        {
                            resultDictionary.Add(fullyQualifiedValidationObjectSource.Key + '.' + fullyQualifiedValidationObjectSource.Value, objectValidated); // Add objects that did not pass the test
                        }

                        objectValidated = ValidateObjectExistence(validationObjectTarget, targetConnection, physicalModelDataTable);

                        if (objectValidated == "False" && !resultDictionary.ContainsKey(fullyQualifiedValidationObjectTarget.Key + '.' + fullyQualifiedValidationObjectTarget.Value))
                        {
                            resultDictionary.Add(fullyQualifiedValidationObjectTarget.Key + '.' + fullyQualifiedValidationObjectTarget.Value, objectValidated); // Add objects that did not pass the test
                        }
                    }
                }
            }

            // Return the results back to the user
            if (resultDictionary.Count > 0)
            {
                foreach (var objectValidationResult in resultDictionary)
                {
                    resultList.Add($"     {objectValidationResult.Key} is tested with outcome {objectValidationResult.Value}. This may be because the schema is defined differently in the connection, or because it simply does not exist.\r\n");
                    metadataValidations.ValidationIssues++;
                }
            }
            else
            {
                resultList.Add($"     There were no validation issues related to the (physical) existence of the defined Data Object in the model.\r\n");
            }

            return resultList;
        }

        /// <summary>
        /// Check if an object / table exists in the metadata.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="teamConnection"></param>
        /// <param name="inputDataTable"></param>
        /// <returns></returns>
        internal static string ValidateObjectExistence(string validationObject, TeamConnection teamConnection, DataTable inputDataTable)
        {
            string returnExistenceEvaluation = "False";

            var objectDetails = MetadataHandling.GetFullyQualifiedDataObjectName(validationObject, teamConnection).FirstOrDefault();

            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.databaseName].ColumnName = PhysicalModelMappingMetadataColumns.databaseName.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.schemaName].ColumnName = PhysicalModelMappingMetadataColumns.schemaName.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.tableName].ColumnName = PhysicalModelMappingMetadataColumns.tableName.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.columnName].ColumnName = PhysicalModelMappingMetadataColumns.columnName.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.dataType].ColumnName = PhysicalModelMappingMetadataColumns.dataType.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.characterLength].ColumnName = PhysicalModelMappingMetadataColumns.characterLength.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.numericPrecision].ColumnName = PhysicalModelMappingMetadataColumns.numericPrecision.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.numericScale].ColumnName = PhysicalModelMappingMetadataColumns.numericScale.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.ordinalPosition].ColumnName = PhysicalModelMappingMetadataColumns.ordinalPosition.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.primaryKeyIndicator].ColumnName = PhysicalModelMappingMetadataColumns.primaryKeyIndicator.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.multiActiveIndicator].ColumnName = PhysicalModelMappingMetadataColumns.multiActiveIndicator.ToString();

            DataRow[] foundRows = inputDataTable.Select("" + PhysicalModelMappingMetadataColumns.tableName + " = '" + objectDetails.Value + "' AND " + PhysicalModelMappingMetadataColumns.schemaName + " = '" + objectDetails.Key + "'");

            if (foundRows.Length > 0)
            {
                returnExistenceEvaluation = "True";
            }

            // return the result of the test;
            return returnExistenceEvaluation;
        }

        public static List<string> ValidateBasicDataVaultAttributeExistence(List<DataRow> rowList, TeamConfiguration teamConfiguration, EventLog eventLog, ref MetadataValidations metadataValidations)
        {
            List<string> resultList = new List<string>();

            // Informing the user.
            resultList.Add("\r\n--> Commencing the validation to check if basic Data Vault attributes are present.\r\n");

            List<Tuple<string, string, bool>> masterResultList = new List<Tuple<string, string, bool>>();

            foreach (DataRow row in rowList)
            {
                // Skip deleted rows.
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (row[DataObjectMappingGridColumns.Enabled.ToString()].ToString() == "True")
                {
                    var localDataObjectSourceName = row[DataObjectMappingGridColumns.SourceDataObjectName.ToString()].ToString();
                    var localDataObjectSourceConnectionId = row[DataObjectMappingGridColumns.SourceConnection.ToString()].ToString();
                    var localDataObjectSourceConnection = TeamConnection.GetTeamConnectionByConnectionId(localDataObjectSourceConnectionId, teamConfiguration, eventLog);
                    var localDataObjectSourceTableType = MetadataHandling.GetDataObjectType(localDataObjectSourceName, "", teamConfiguration);

                    var localDataObjectTargetName = row[DataObjectMappingGridColumns.TargetDataObjectName.ToString()].ToString();
                    var localDataObjectTargetConnectionId = row[DataObjectMappingGridColumns.TargetConnection.ToString()].ToString();
                    var localDataObjectTargetConnection = TeamConnection.GetTeamConnectionByConnectionId(localDataObjectTargetConnectionId, teamConfiguration, eventLog);
                    var localDataObjectTargetTableType = MetadataHandling.GetDataObjectType(localDataObjectTargetName, "", teamConfiguration);

                    // Source
                    if (localDataObjectSourceTableType == MetadataHandling.DataObjectTypes.CoreBusinessConcept ||
                        localDataObjectSourceTableType == MetadataHandling.DataObjectTypes.Context ||
                        localDataObjectSourceTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationship ||
                        localDataObjectSourceTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext ||
                        localDataObjectSourceTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
                    {
                        var result = BasicDataVaultValidation(localDataObjectSourceName, localDataObjectSourceConnection, teamConfiguration, localDataObjectSourceTableType);
                        masterResultList.AddRange(result);
                    }

                    // Target
                    if (localDataObjectTargetTableType == MetadataHandling.DataObjectTypes.CoreBusinessConcept ||
                        localDataObjectTargetTableType == MetadataHandling.DataObjectTypes.Context ||
                        localDataObjectTargetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationship ||
                        localDataObjectTargetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContext ||
                        localDataObjectTargetTableType == MetadataHandling.DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey)
                    {
                        var result = BasicDataVaultValidation(localDataObjectTargetName, localDataObjectTargetConnection, teamConfiguration, localDataObjectTargetTableType);
                        masterResultList.AddRange(result);
                    }
                }
            }

            // Evaluate the results
            int localValidationIssues = 0;

            // Deduplicate
            List<Tuple<string, string, bool>> deduplicatedResultList = masterResultList.Distinct().ToList();

            foreach (var result in deduplicatedResultList)
            {
                if (result.Item3 == false)
                {
                    resultList.Add($"     Warning - {result.Item1} was evaluated as a Data Vault object but the expected attribute {result.Item2} was not found in the table.\r\n");
                    metadataValidations.ValidationIssues++;
                    localValidationIssues++;
                }
            }

            if (localValidationIssues == 0)
            {
                resultList.Add("     There were no validation issues related Data Vault attribute existence.\r\n");
            }

            return resultList;
        }

        internal static List<Tuple<string, string, bool>> BasicDataVaultValidation(string dataObjectName, TeamConnection teamConnection, TeamConfiguration teamConfiguration, MetadataHandling.DataObjectTypes tableType)
        {
            // Initialise the return type
            List<Tuple<string, string, bool>> returnList = new List<Tuple<string, string, bool>>();

            // Define the list to validate, this is different for each validation type.
            List<string> validationAttributeList = new List<string>();

            switch (tableType)
            {
                case MetadataHandling.DataObjectTypes.CoreBusinessConcept:
                    validationAttributeList.Add(teamConfiguration.LoadDateTimeAttribute);
                    break;

                case MetadataHandling.DataObjectTypes.Context:

                    if (teamConfiguration.EnableAlternativeSatelliteLoadDateTimeAttribute == "True")
                    {
                        validationAttributeList.Add(teamConfiguration.AlternativeSatelliteLoadDateTimeAttribute);
                    }
                    else
                    {
                        validationAttributeList.Add(teamConfiguration.LoadDateTimeAttribute);
                    }

                    validationAttributeList.Add(teamConfiguration.RecordChecksumAttribute);
                    break;

                case MetadataHandling.DataObjectTypes.NaturalBusinessRelationship:
                    validationAttributeList.Add(teamConfiguration.LoadDateTimeAttribute);
                    break;
            }

            // return the result of the test;
            return returnList;
        }

    }
}
