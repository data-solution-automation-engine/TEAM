using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using TEAM_Library;
using static TEAM.FormBase;

namespace TEAM
{
    internal class MetadataValidation 
    {
        internal static List<Tuple<string,string, bool>> BasicDataVaultValidation(string dataObjectName, TeamConnection teamConnection, MetadataHandling.TableTypes tableType)
        {
            // Initialise the return type
            List<Tuple<string, string, bool>> returnList = new List<Tuple<string, string, bool>>();
            
            // Define the list to validate, this is different for each validation type.
            List<string> validationAttributeList = new List<string>();

            switch (tableType)
            {
                case MetadataHandling.TableTypes.CoreBusinessConcept:
                    validationAttributeList.Add(FormBase.TeamConfiguration.LoadDateTimeAttribute);
                    break;
                
                case MetadataHandling.TableTypes.Context:

                    if (FormBase.TeamConfiguration.EnableAlternativeSatelliteLoadDateTimeAttribute == "True")
                    {
                        validationAttributeList.Add(FormBase.TeamConfiguration.AlternativeSatelliteLoadDateTimeAttribute);
                    }
                    else
                    {
                        validationAttributeList.Add(FormBase.TeamConfiguration.LoadDateTimeAttribute);
                    }

                    validationAttributeList.Add(FormBase.TeamConfiguration.RecordChecksumAttribute);
                    break;
                
                case MetadataHandling.TableTypes.NaturalBusinessRelationship:
                    validationAttributeList.Add(FormBase.TeamConfiguration.LoadDateTimeAttribute);
                    break;
            }

            // Now check if the attribute exists in the table
            foreach (string validationAttribute in validationAttributeList)
            {
                var fullyQualifiedValidationObject = MetadataHandling.GetFullyQualifiedDataObjectName(dataObjectName, teamConnection).FirstOrDefault();
                var localTable = fullyQualifiedValidationObject.Value.Replace("[", "").Replace("]", "");
                var localSchema = fullyQualifiedValidationObject.Key.Replace("[", "").Replace("]", "");

                if (GlobalParameters.EnvironmentMode == EnvironmentModes.PhysicalMode)
                {
                    var conn = new SqlConnection
                        {ConnectionString = teamConnection.CreateSqlServerConnectionString(false)};
                    conn.Open();

                    // Execute the check
                    var cmd = new SqlCommand(
                        "SELECT CASE WHEN EXISTS ((SELECT * FROM INFORMATION_SCHEMA.COLUMNS " +
                        "WHERE " +
                        "[TABLE_NAME] = '" + localTable + "' AND " +
                        "[TABLE_SCHEMA] = '" + localSchema + "' AND " +
                        "[COLUMN_NAME] = '" + validationAttribute + "')) THEN 1 ELSE 0 END", conn);

                    var exists = (int) cmd.ExecuteScalar() == 1;

                    returnList.Add(new Tuple<string, string, bool>(localSchema + '.'+localTable, validationAttribute, exists));

                    conn.Close();
                }
            }

            // return the result of the test;
            return returnList;
        }
        
        
        /// <summary>
        /// This method ensures that a table object exists in the physical model against the catalog.
        /// </summary>
        /// <param name="fullyQualifiedValidationObject"></param>
        /// <param name="teamConnection"></param>
        /// <returns></returns>
        internal static string ValidateObjectExistencePhysical (KeyValuePair<string, string> fullyQualifiedValidationObject, TeamConnection teamConnection)
        { 
            var conn = new SqlConnection {ConnectionString = teamConnection.CreateSqlServerConnectionString(false)};
            conn.Open();

            var localTable = fullyQualifiedValidationObject.Value.Replace("[", "").Replace("]", "");
            var localSchema = fullyQualifiedValidationObject.Key.Replace("[", "").Replace("]", "");

            // Execute the check
            var cmd = new SqlCommand(
                "SELECT CASE WHEN EXISTS ((SELECT * " +
                "FROM sys.objects a " +
                "JOIN sys.schemas b on a.schema_id = b.schema_id " +
                "WHERE a.[name] = '" + localTable + "' and b.[name]= '"+ localSchema + "')) THEN 1 ELSE 0 END", conn);

            
            var exists = (int) cmd.ExecuteScalar() == 1;
            string returnExistenceEvaluation = exists.ToString();

            conn.Close();

            // return the result of the test;
            return returnExistenceEvaluation;
        }

        /// <summary>
        /// This method ensures that an attribute object exists in the physical model against the catalog.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="validationAttribute"></param>
        /// <param name="teamConnection"></param>
        /// <returns></returns>
        internal static string ValidateAttributeExistencePhysical(string validationObject, string validationAttribute, TeamConnection teamConnection)
        {
            var returnExistenceEvaluation = "False";
            
            // Temporary fix to allow 'transformations', in this case hard-coded NULL values to be loaded.
            if (validationAttribute != "NULL")
            {
                var fullyQualifiedValidationObject = MetadataHandling.GetFullyQualifiedDataObjectName(validationObject, teamConnection).FirstOrDefault();
                var localTable = fullyQualifiedValidationObject.Value.Replace("[", "").Replace("]", "");
                var localSchema = fullyQualifiedValidationObject.Key.Replace("[", "").Replace("]", "");

                var conn = new SqlConnection {ConnectionString = teamConnection.CreateSqlServerConnectionString(false)};
                conn.Open();

                // Execute the check
                var cmd = new SqlCommand(
                    "SELECT CASE WHEN EXISTS ((SELECT * FROM INFORMATION_SCHEMA.COLUMNS " +
                    "WHERE " +
                    "[TABLE_NAME] = '" + localTable + "' AND " +
                    "[TABLE_SCHEMA] = '" + localSchema + "' AND " +
                    "[COLUMN_NAME] = '" + validationAttribute + "')) THEN 1 ELSE 0 END", conn);

                var exists = (int) cmd.ExecuteScalar() == 1;
                returnExistenceEvaluation = exists.ToString();

                conn.Close();
            }
            else
            {
                // Set True if NULL
                returnExistenceEvaluation = "True";
            }

            // return the result of the test;
            return returnExistenceEvaluation;
        }

        /// <summary>
        /// Check if an object / table exists in the metadata.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="teamConnection"></param>
        /// <param name="inputDataTable"></param>
        /// <returns></returns>
        internal static string ValidateObjectExistenceVirtual (string validationObject, TeamConnection teamConnection, DataTable inputDataTable)
        {
            string returnExistenceEvaluation = "False";

            var objectDetails = MetadataHandling.GetFullyQualifiedDataObjectName(validationObject, teamConnection).FirstOrDefault();

            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.HashKey].ColumnName = PhysicalModelMappingMetadataColumns.HashKey.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.VersionId].ColumnName = PhysicalModelMappingMetadataColumns.VersionId.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.DatabaseName].ColumnName = PhysicalModelMappingMetadataColumns.DatabaseName.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.SchemaName].ColumnName = PhysicalModelMappingMetadataColumns.SchemaName.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.TableName].ColumnName = PhysicalModelMappingMetadataColumns.TableName.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.ColumnName].ColumnName = PhysicalModelMappingMetadataColumns.ColumnName.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.DataType].ColumnName = PhysicalModelMappingMetadataColumns.DataType.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.CharacterLength].ColumnName = PhysicalModelMappingMetadataColumns.CharacterLength.ToString(); 
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.NumericPrecision].ColumnName = PhysicalModelMappingMetadataColumns.NumericPrecision.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.NumericScale].ColumnName = PhysicalModelMappingMetadataColumns.NumericScale.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.OrdinalPosition].ColumnName = PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator].ColumnName = PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator.ToString();
            inputDataTable.Columns[(int)PhysicalModelMappingMetadataColumns.MultiActiveIndicator].ColumnName = PhysicalModelMappingMetadataColumns.MultiActiveIndicator.ToString();

            DataRow[] foundRows = inputDataTable.Select("" + PhysicalModelMappingMetadataColumns.TableName + " = '" + objectDetails.Value+ "' AND " + PhysicalModelMappingMetadataColumns.SchemaName + " = '" + objectDetails.Key+"'");

            //bool existenceCheck = inputDataTable.AsEnumerable().Any(row => columns.Any(col => row[col].ToString() == objectName));

            if (foundRows.Length > 0)
            {
                returnExistenceEvaluation = "True";
            }

            // return the result of the test;
            return returnExistenceEvaluation;
        }

        /// <summary>
        /// Check if an attribute exists in the metadata.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="validationAttribute"></param>
        /// <param name="teamConnection"></param>
        /// <param name="inputDataTable"></param>
        /// <returns></returns>
        internal static string ValidateAttributeExistenceVirtual(string validationObject, string validationAttribute, TeamConnection teamConnection, DataTable inputDataTable)
        {
            string returnExistenceEvaluation = "False";

            if (validationAttribute != "NULL")
            {
                var objectDetails = MetadataHandling.GetFullyQualifiedDataObjectName(validationObject, teamConnection).FirstOrDefault();

                DataRow[] foundRows = inputDataTable.Select("" + PhysicalModelMappingMetadataColumns.TableName + " = '" + objectDetails.Value + "' AND " + PhysicalModelMappingMetadataColumns.SchemaName + "='" + objectDetails.Key + "' AND " + PhysicalModelMappingMetadataColumns.ColumnName + " = '" + validationAttribute+"'");

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
        /// Validate the relationship between Data Object Mappings, i.e. dependencies between objects which should exist because they are related.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="inputDataTable"></param>
        /// <returns></returns>
        internal static Dictionary<string, bool> ValidateLogicalGroup(Tuple<string, string, string, string> validationObject, DataTable inputDataTable)
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

            var inputTargetTableType = MetadataHandling.GetDataObjectType(validationObject.Item2, "", FormBase.TeamConfiguration);
            


            if (inputTargetTableType == MetadataHandling.TableTypes.Context) // If the table is a Satellite, only the Hub is required
            {
                tableInclusionFilterCriterion = FormBase.TeamConfiguration.HubTablePrefixValue;
                tableClassification = FormBase.TeamConfiguration.SatTablePrefixValue;
            }
            else if (inputTargetTableType == MetadataHandling.TableTypes.NaturalBusinessRelationship) // If the table is a Link, we're only interested in the Hubs
            {
                tableInclusionFilterCriterion = FormBase.TeamConfiguration.HubTablePrefixValue;
                tableClassification = "LNK";
            }
            else if (inputTargetTableType == MetadataHandling.TableTypes.NaturalBusinessRelationshipContext) // If the table is a Link-Satellite, only the Link is required
            {
                tableInclusionFilterCriterion = FormBase.TeamConfiguration.LinkTablePrefixValue;
                tableClassification = "LSAT";
            }
            else
            {
                tableInclusionFilterCriterion = "";
            }

            // Unfortunately, there is a separate process for Links and Satellites
            // Iterate through the various keys (mainly for the purpose of evaluating Links)
            int numberOfDependents = 0;
            if (tableClassification == FormBase.TeamConfiguration.SatTablePrefixValue || tableClassification == "LNK")
            {
                foreach (string businessKeyComponent in hubBusinessKeys)
                {
                    foreach (DataRow dataObjectRow in inputDataTable.Rows)
                    {
                        var targetDataObjectName = dataObjectRow[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                        var targetConnectionInternalId = dataObjectRow[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                        var targetConnection = GetTeamConnectionByConnectionId(targetConnectionInternalId);
                        var targetFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObjectName, targetConnection).FirstOrDefault();
                        var targetTableType = MetadataHandling.GetDataObjectType(targetDataObjectName, "", FormBase.TeamConfiguration);
                        var filterCriterion = dataObjectRow[TableMappingMetadataColumns.FilterCriterion.ToString()].ToString();

                        var sourceDataObjectName = dataObjectRow[TableMappingMetadataColumns.SourceTable.ToString()].ToString();
                        var sourceConnectionInternalId = dataObjectRow[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                        var sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionInternalId);
                        var sourceFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(sourceDataObjectName, sourceConnection).FirstOrDefault();
                        var sourceTableType = MetadataHandling.GetDataObjectType(sourceDataObjectName, "", FormBase.TeamConfiguration);

                        // Count the number of dependents.
                        if (
                             (bool)dataObjectRow[TableMappingMetadataColumns.Enabled.ToString()] && // Only active generated objects
                             sourceFullyQualifiedName.Key+'.'+ sourceFullyQualifiedName.Value == validationObject.Item1 &&
                             (string)dataObjectRow[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()] == businessKeyComponent.Trim() &&
                             targetFullyQualifiedName.Key+'.'+targetFullyQualifiedName.Value != validationObject.Item2 && // Exclude itself
                             filterCriterion == validationObject.Item4 && // Adding filtercriterion for uniquification of join (see https://github.com/RoelantVos/TEAM/issues/87);
                             targetFullyQualifiedName.Value.StartsWith(tableInclusionFilterCriterion) 
                           )
                        {
                            var bla = dataObjectRow;
                            numberOfDependents++;
                        }
                    }
                }
            }
            else // In the case of an LSAT, only join on the Link using the full business key
            {
                // Query the dependent information
                foreach (DataRow row in inputDataTable.Rows)
                {
                    var targetDataObjectName = row[TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                    var targetConnectionInternalId = row[TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                    var targetConnection = GetTeamConnectionByConnectionId(targetConnectionInternalId);
                    var targetFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(targetDataObjectName, targetConnection).FirstOrDefault();
                    var targetTableType = MetadataHandling.GetDataObjectType(targetDataObjectName, "", FormBase.TeamConfiguration);

                    var sourceDataObjectName = row[TableMappingMetadataColumns.SourceTable.ToString()].ToString();
                    var sourceConnectionInternalId = row[TableMappingMetadataColumns.SourceConnection.ToString()].ToString();
                    var sourceConnection = GetTeamConnectionByConnectionId(sourceConnectionInternalId);
                    var sourceFullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(sourceDataObjectName, sourceConnection).FirstOrDefault();
                    var sourceTableType = MetadataHandling.GetDataObjectType(sourceDataObjectName, "", FormBase.TeamConfiguration);

                    if (
                         (bool)row[TableMappingMetadataColumns.Enabled.ToString()] == true && // Only active generated objects
                         sourceFullyQualifiedName.Key + '.' + sourceFullyQualifiedName.Value == validationObject.Item1 &&
                         (string)row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()] == validationObject.Item3.Trim() &&
                         targetFullyQualifiedName.Key + '.' + targetFullyQualifiedName.Value != validationObject.Item2 && // Exclude itself
                         targetFullyQualifiedName.Value.StartsWith(tableInclusionFilterCriterion)
                       )
                    {
                        numberOfDependents++;
                    }
                }
            }

            // Run the comparison
            // Test for equality.
            bool equal;
            if ((tableClassification == FormBase.TeamConfiguration.SatTablePrefixValue || tableClassification == "LNK") && businessKeyCount == numberOfDependents) // For Sats and Links we can count the keys and rows
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
        /// Check the ordinal position of Link Keys against their business key definitions.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="inputDataTable"></param>
        /// <param name="physicalModelDataTable"></param>
        /// <param name="evaluationMode"></param>
        /// <returns></returns>
        internal static Dictionary<string,bool> ValidateLinkKeyOrder(Tuple<string,string,string, string> validationObject, DataTable inputDataTable, DataTable physicalModelDataTable, EnvironmentModes evaluationMode)
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
                DataRow[] selectionRows = inputDataTable.Select(TableMappingMetadataColumns.SourceTable+" = '" + validationObject.Item1+ "' AND "+TableMappingMetadataColumns.BusinessKeyDefinition+" = '"+ hubBusinessKey.Replace("'", "''").Trim()+ "' AND "+TableMappingMetadataColumns.TargetTable+" NOT LIKE '" + FormBase.TeamConfiguration.SatTablePrefixValue + "_%'");

                try
                {
                    // Derive the Hub surrogate key name, as this can be compared against the Link
                    string hubTableName = selectionRows[0][TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                    string hubTableConnectionId = selectionRows[0][TableMappingMetadataColumns.TargetConnection.ToString()].ToString();
                    var hubTableConnection = GetTeamConnectionByConnectionId(hubTableConnectionId);
                    
                    string hubSurrogateKeyName = MetadataHandling.GetSurrogateKey(hubTableName, hubTableConnection, FormBase.TeamConfiguration);

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

            if (evaluationMode == EnvironmentModes.PhysicalMode)
            {
                var connTarget = new SqlConnection { ConnectionString = validationObject.Item4 };
                var connDatabase = connTarget.Database;

                var sqlStatementForLink = new StringBuilder();
                sqlStatementForLink.AppendLine("SELECT");
                sqlStatementForLink.AppendLine("   OBJECT_NAME([object_id]) AS [TABLE_NAME]");
                sqlStatementForLink.AppendLine("  ,[name] AS [COLUMN_NAME]");
                sqlStatementForLink.AppendLine("  ,[column_id] AS [ORDINAL_POSITION]");
                sqlStatementForLink.AppendLine("  ,ROW_NUMBER() OVER(PARTITION BY object_id ORDER BY column_id) AS [HUB_KEY_POSITION]");
                sqlStatementForLink.AppendLine("FROM [" + connDatabase + "].sys.columns");
                sqlStatementForLink.AppendLine("    WHERE OBJECT_NAME([object_id]) LIKE '" +FormBase.TeamConfiguration.LinkTablePrefixValue + "_%'");
                sqlStatementForLink.AppendLine("AND OBJECTPROPERTY([object_id], 'IsTable') = 1");
                sqlStatementForLink.AppendLine("AND column_id > 4");
                sqlStatementForLink.AppendLine("AND OBJECT_NAME([object_id]) = '" + validationObject.Item2 + "'");

                // The hubKeyOrder contains the order of the keys in the Hub, now we need to do the same for the (target) Link so we can compare.
                connTarget.Open();
                var linkList = Utility.GetDataTable(ref connTarget, sqlStatementForLink.ToString());
                connTarget.Close();

                foreach (DataRow row in linkList.Rows)
                {
                    var linkHubSurrogateKeyName = row["COLUMN_NAME"].ToString();
                    int linkHubSurrogateKeyPosition = Convert.ToInt32(row["HUB_KEY_POSITION"]);

                    if (linkHubSurrogateKeyName.Contains(FormBase.TeamConfiguration.DwhKeyIdentifier)) // Exclude degenerate attributes from the order
                    {
                        linkKeyOrder.Add(linkHubSurrogateKeyPosition, linkHubSurrogateKeyName);
                    }
                }
            }
            else // virtual
            {
                int linkHubSurrogateKeyPosition = 1;

                var workingTable = new DataTable();

                try
                {
                    // Select only the business keys in a link table. 
                    // Excluding all non-business key attributes
                    workingTable = physicalModelDataTable
                        .Select($"{PhysicalModelMappingMetadataColumns.TableName} LIKE '%{FormBase.TeamConfiguration.LinkTablePrefixValue}%' " +
                                $"AND {PhysicalModelMappingMetadataColumns.TableName} = '{validationObject.Item2}' " +
                                $"AND {PhysicalModelMappingMetadataColumns.OrdinalPosition} > 4", $"{PhysicalModelMappingMetadataColumns.OrdinalPosition} ASC").CopyToDataTable();
                }
                catch (Exception ex)
                {
                    GlobalParameters.TeamEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"An error occurred during validation of the metadata. The errors is {ex}."));
                }

                if (workingTable.Rows.Count > 0)
                {
                    foreach (DataRow row in workingTable.Rows)
                    {
                        var linkHubSurrogateKeyName = row[PhysicalModelMappingMetadataColumns.ColumnName.ToString()].ToString();

                        if (linkHubSurrogateKeyName.Contains(FormBase.TeamConfiguration.DwhKeyIdentifier)
                        ) // Exclude degenerate attributes from the order
                        {
                            linkKeyOrder.Add(linkHubSurrogateKeyPosition, linkHubSurrogateKeyName);
                            linkHubSurrogateKeyPosition++;
                        }
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
            Dictionary<string,bool> result = new Dictionary<string, bool>();
            result.Add(validationObject.Item2,equal);
            return result;
        }

        /// <summary>
        /// Validate the Business Key definition against the physical model, taking the source object and business key definition as input parameters, together with a connection string to validate against.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        internal static Dictionary<Tuple<string,string>, bool> ValidateSourceBusinessKeyExistencePhysical(string validationObject, string businessKeyDefinition, TeamConnection teamConnection)
        {
            // First, the Business Keys for each table need to be identified information. This can be the combination of Business keys separated by a comma.
            // Every business key needs to be iterated over to validate if the attribute exists in that table.
            List<string> businessKeys = businessKeyDefinition.Split(',').ToList();


            // Get the table the component belongs to if available
            var fullyQualifiedValidationObject = MetadataHandling.GetFullyQualifiedDataObjectName(validationObject, teamConnection).FirstOrDefault();
            var localTable = fullyQualifiedValidationObject.Value.Replace("[", "").Replace("]", "");
            var localSchema = fullyQualifiedValidationObject.Key.Replace("[", "").Replace("]", "");

            // Now iterate over each table, as identified by the business key.
            var conn = new SqlConnection { ConnectionString = teamConnection.CreateSqlServerConnectionString(false) };
            conn.Open();

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
                } else
                {
                    subKeys.Add(trimBusinessKey);
                }

                foreach (string businessKeyPart in subKeys)
                {
                    // Handle hard-coded business key values
                    if (businessKeyPart.Trim().StartsWith("'") && businessKeyPart.Trim().EndsWith("'"))
                    {
                        // Do nothing
                    }
                    else
                    {
                        // Query the data dictionary to validate existence
                        var cmd = new SqlCommand("SELECT CASE WHEN EXISTS (" +
                                                 "(" +
                                                 "SELECT * FROM sys.columns a "+
                                                 "JOIN sys.objects b ON a.object_id = b.object_id " +
                                                 "JOIN sys.schemas c on b.schema_id = c.schema_id " +
                                                 "WHERE OBJECT_NAME(a.[object_id]) = '" + localTable + "' AND c.[name] = '" + localSchema + "' AND a.[name] = '" + businessKeyPart.Trim() + "'" +
                                                 ")" +
                                                 ") THEN 1 ELSE 0 END", conn);
                        
                        var exists = (int)cmd.ExecuteScalar() == 1;
                        result.Add(Tuple.Create(validationObject, businessKeyPart.Trim()), exists);
                    }
                }
            }
            conn.Close();
            // Return the result of the test;
            return result;
        }

        /// <summary>
        /// Validate the Business Key definition against the snapshot of the physical model (the physical model data grid), taking the source object and business key definition as input parameters, together with a connection string to validate against.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="businessKeyDefinition"></param>
        /// <param name="teamConnection"></param>
        /// <param name="inputDataTable"></param>
        /// <returns></returns>
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

                        DataRow[] foundAuthors = inputDataTable.Select($"" + PhysicalModelMappingMetadataColumns.TableName + " = '" + objectDetails.Value + "' AND "+ PhysicalModelMappingMetadataColumns.SchemaName + " = '"+objectDetails.Key+ "' AND " + PhysicalModelMappingMetadataColumns.ColumnName + " = '" + businessKeyPart.Trim() + "'");
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
    }


}
