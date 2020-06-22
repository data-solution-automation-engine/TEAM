using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TEAM
{
    internal class ClassMetadataValidation 
    {

        /// <summary>
        ///    This method ensures that a table object exists in the physical model against the catalog
        /// </summary>
        internal static string ValidateObjectExistencePhysical (string validationObject, string connectionString)
        {
            string returnExistenceEvaluation = "False";

            var conn = new SqlConnection {ConnectionString = connectionString};
            conn.Open();

            var objectName = ClassMetadataHandling.GetNonQualifiedTableName(validationObject);
            var schemaName = ClassMetadataHandling.GetSchema(validationObject);

            // Execute the check
            var cmd = new SqlCommand(
                "SELECT CASE WHEN EXISTS ((SELECT * " +
                "FROM sys.objects a " +
                "JOIN sys.schemas b on a.schema_id = b.schema_id " +
                "WHERE a.[name] = '" + objectName + "' and b.[name]= '"+ schemaName.FirstOrDefault(x => x.Value.Contains(objectName)).Key + "')) THEN 1 ELSE 0 END", conn);

            
            var exists = (int) cmd.ExecuteScalar() == 1;
            returnExistenceEvaluation = exists.ToString();

            conn.Close();

            // return the result of the test;
            return returnExistenceEvaluation;
        }

        /// <summary>
        ///    This method ensures that an attribute object exists in the physical model against the catalog
        /// </summary>
        internal static string ValidateAttributeExistencePhysical(string validationObject, string validationAttribute, string connectionString)
        {
            string returnExistenceEvaluation = "False";

            // Temporary fix to allow 'transformations', in this case hard-coded NULL values to be loaded.
            if (validationAttribute != "NULL")
            {

                var objectName = ClassMetadataHandling.GetNonQualifiedTableName(validationObject).Replace("[", "")
                    .Replace("]", "");
                var schemaName = ClassMetadataHandling.GetSchema(validationObject);

                var conn = new SqlConnection {ConnectionString = connectionString};
                conn.Open();

                // Execute the check
                var cmd = new SqlCommand(
                    "SELECT CASE WHEN EXISTS ((SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE [TABLE_NAME] = '" +
                    objectName + "' AND [TABLE_SCHEMA] = '" +
                    schemaName.FirstOrDefault(x => x.Value.Contains(objectName)).Key.Replace("[", "").Replace("]", "") +
                    "' AND [COLUMN_NAME] = '" + validationAttribute + "')) THEN 1 ELSE 0 END", conn);

                var exists = (int) cmd.ExecuteScalar() == 1;
                returnExistenceEvaluation = exists.ToString();

                conn.Close();
            }
            else
            {
                returnExistenceEvaluation = "True";
            }

            // return the result of the test;
            return returnExistenceEvaluation;
        }

        // Check if an object / table exists in the metadata
        internal static string ValidateObjectExistenceVirtual (string validationObject, DataTable inputDataTable)
        {
            string returnExistenceEvaluation = "False";

            var objectDetails = ClassMetadataHandling.GetSchema(validationObject).FirstOrDefault();

            //DataColumn[] columns = inputDataTable.Columns.Cast<DataColumn>().ToArray();

            DataRow[] foundRows = inputDataTable.Select("TABLE_NAME = '"+ objectDetails.Value+ "' AND SCHEMA_NAME='"+ objectDetails.Key+"'");

            //bool existenceCheck = inputDataTable.AsEnumerable().Any(row => columns.Any(col => row[col].ToString() == objectName));

            if (foundRows.Length > 0)
            {
                returnExistenceEvaluation = "True";
            }

            // return the result of the test;
            return returnExistenceEvaluation;
        }

        // Check if an attribute exists in the metadata
        internal static string ValidateAttributeExistenceVirtual(string validationObject, string validationAttribute, DataTable inputDataTable)
        {
            string returnExistenceEvaluation = "False";

            if (validationAttribute != "NULL")
            {

                //DataColumn[] columns = inputDataTable.Columns.Cast<DataColumn>().ToArray();

                //bool existenceCheckTables = inputDataTable.AsEnumerable()
                //    .Any(row => columns.Any(col => row[col].ToString() == validationObject));
                //bool existenceCheckAttributes = inputDataTable.AsEnumerable()
                //    .Any(row => columns.Any(col => row[col].ToString() == validationAttribute));

                //if (existenceCheckTables == true && existenceCheckAttributes == true)
                //{
                //    returnExistenceEvaluation = "True";
                //}
                var objectDetails = ClassMetadataHandling.GetSchema(validationObject).FirstOrDefault();

                DataRow[] foundRows = inputDataTable.Select("TABLE_NAME = '" + objectDetails.Value + "' AND SCHEMA_NAME='" + objectDetails.Key + "' AND COLUMN_NAME = '"+validationAttribute+"'");

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

        internal static Dictionary<string, bool> ValidateLogicalGroup(Tuple<string, string, string> validationObject, string connectionString, int versionId, DataTable inputDataTable)
        {
            // First, the Business Key need to be checked. This is to determine how many dependents are expected.
            // For instance, if a Link has a three-part Business Key then three Hubs will be expected
            List<string> hubBusinessKeys = validationObject.Item3.Split(',').ToList();
            int businessKeyCount = hubBusinessKeys.Count;

            // We need to manupulate the query to account for multiplicity in the model i.e. many Satellites linking to a single Hub.
            // The only interest is whether the Hub is there...
            string tableInclusionFilterCriterion;
            var tableClassification = "";
            if (validationObject.Item2.StartsWith(FormBase.ConfigurationSettings.SatTablePrefixValue)) // If the table is a Satellite, only the Hub is required
            {
                tableInclusionFilterCriterion = FormBase.ConfigurationSettings.HubTablePrefixValue;
                tableClassification = "SAT";
            }
            else if (validationObject.Item2.StartsWith(FormBase.ConfigurationSettings.LinkTablePrefixValue)) // If the table is a Link, we're only interested in the Hubs
            {
                tableInclusionFilterCriterion = FormBase.ConfigurationSettings.HubTablePrefixValue;
                tableClassification = "LNK";
            }
            else if (validationObject.Item2.StartsWith(FormBase.ConfigurationSettings.LsatTablePrefixValue)) // If the table is a Link-Satellite, only the Link is required
            {
                tableInclusionFilterCriterion = FormBase.ConfigurationSettings.LinkTablePrefixValue;
                tableClassification = "LSAT";
            }
            else
            {
                tableInclusionFilterCriterion = "";
            }


            var conn = new SqlConnection { ConnectionString = connectionString };
            conn.Open();


            // Unfortunately, there is a separate process for Links and Sats
            // Iterate through the various keys (mainly for the purpose of evaluating Links)
            int numberOfDependents = 0;
            if (tableClassification == "SAT" || tableClassification == "LNK")
            {
                foreach (string businessKeyComponent in hubBusinessKeys)
                {
                    // Query the dependent information
                    var sqlStatementForDependent = new StringBuilder();

                    foreach (DataRow row in inputDataTable.Rows)
                    {
                        if (
                             (bool)row[TableMappingMetadataColumns.Enabled.ToString()] == true && // Only active generated objects
                             (string)row[TableMappingMetadataColumns.SourceTable.ToString()] == validationObject.Item1 &&
                             (string)row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()] == businessKeyComponent.Trim() &&
                             (string)row[TableMappingMetadataColumns.TargetTable.ToString()] != validationObject.Item2 && // Exclude itself
                             row[TableMappingMetadataColumns.TargetTable.ToString()].ToString().StartsWith(tableInclusionFilterCriterion) 
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
                foreach (DataRow row in inputDataTable.Rows)
                {
                    if (
                         (bool)row[TableMappingMetadataColumns.Enabled.ToString()] == true && // Only active generated objects
                         (string)row[TableMappingMetadataColumns.SourceTable.ToString()] == validationObject.Item1 &&
                         (string)row[TableMappingMetadataColumns.BusinessKeyDefinition.ToString()] == validationObject.Item3.Trim() &&
                         (string)row[TableMappingMetadataColumns.TargetTable.ToString()] != validationObject.Item2 && // Exclude itself
                         row[TableMappingMetadataColumns.TargetTable.ToString()].ToString().StartsWith(tableInclusionFilterCriterion)
                       )
                    {
                        numberOfDependents++;
                    }
                }
            }

            conn.Close();


            // Run the comparison
            // Test for equality.
            bool equal;
            if ((tableClassification == "SAT" || tableClassification == "LNK") && businessKeyCount == numberOfDependents) // For Sats and Links we can count the keys and rows
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

        internal static Dictionary<string,bool> ValidateLinkKeyOrder(Tuple<string,string,string, string> validationObject, DataTable inputDataTable, DataTable physicalModelDataTable, string evaluationMode)
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
                DataRow[] selectionRows = inputDataTable.Select(TableMappingMetadataColumns.SourceTable+" = '" + validationObject.Item1+ "' AND "+TableMappingMetadataColumns.BusinessKeyDefinition+" = '"+ hubBusinessKey.Replace("'", "''").Trim()+ "' AND "+TableMappingMetadataColumns.TargetTable+" NOT LIKE '" + FormBase.ConfigurationSettings.SatTablePrefixValue + "_%'");

                // Derive the Hub surrogate key name, as this can be compared against the Link
                string hubTableName = selectionRows[0][TableMappingMetadataColumns.TargetTable.ToString()].ToString();
                string hubSurrogateKeyName = hubTableName.Replace(FormBase.ConfigurationSettings.HubTablePrefixValue + '_', "") + "_" + FormBase.ConfigurationSettings.DwhKeyIdentifier;
                
                // Add to the dictionary that contains the keys in order.
                hubKeyOrder.Add(businessKeyOrder, hubSurrogateKeyName);
            }

            // Derive the Hub surrogate key name, as this can be compared against the Link
            var linkKeyOrder = new Dictionary<int, string>();

            if (evaluationMode == "physical")
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
                sqlStatementForLink.AppendLine("    WHERE OBJECT_NAME([object_id]) LIKE '" +FormBase.ConfigurationSettings.LinkTablePrefixValue + "_%'");
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

                    if (linkHubSurrogateKeyName.Contains(FormBase.ConfigurationSettings.DwhKeyIdentifier)
                    ) // Exclude degenerate attributes from the order
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
                    workingTable = physicalModelDataTable
                        .Select(
                            "TABLE_NAME LIKE '" + FormBase.ConfigurationSettings.LinkTablePrefixValue +
                            "_%' AND TABLE_NAME = '" + validationObject.Item2 + "' AND ORDINAL_POSITION > 4",
                            "ORDINAL_POSITION ASC").CopyToDataTable();
                }
                catch
                {
                    //
                }

                if (workingTable.Rows.Count > 0)
                {
                    foreach (DataRow row in workingTable.Rows)
                    {
                        var linkHubSurrogateKeyName = row["COLUMN_NAME"].ToString();

                        if (linkHubSurrogateKeyName.Contains(FormBase.ConfigurationSettings.DwhKeyIdentifier)
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
        /// Validate the Business Key definition against the physical model, taking the source object and business key definition as input parameters, together with a connectionstring to validate against.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        internal static Dictionary<Tuple<string,string>, bool> ValidateSourceBusinessKeyExistencePhysical(Tuple<string, string> validationObject, string connectionString)
        {
            // First, the Business Keys for each table need to be identified information. This can be the combination of Business keys separated by a comma.
            // Every business key needs to be iterated over to validate if the attribute exists in that table.
            List<string> businessKeys = validationObject.Item2.Split(',').ToList();


            // Get the table the component belongs to if available
            var objectName = ClassMetadataHandling.GetNonQualifiedTableName(validationObject.Item1);
            var schemaName = ClassMetadataHandling.GetSchema(validationObject.Item1);

            // Now iterate over each table, as identified by the business key.
            var conn = new SqlConnection { ConnectionString = connectionString };
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
                                                 "WHERE OBJECT_NAME(a.[object_id]) = '" + objectName + "' AND c.[name] = '" + schemaName.FirstOrDefault(x => x.Value.Contains(objectName)).Key.Replace("[", "").Replace("]", "") + "' AND a.[name] = '" + businessKeyPart.Trim() + "'" +
                                                 ")" +
                                                 ") THEN 1 ELSE 0 END", conn);
                        
                        var exists = (int)cmd.ExecuteScalar() == 1;
                        result.Add(Tuple.Create(validationObject.Item1, businessKeyPart.Trim()), exists);
                    }
                }
            }
            conn.Close();
            // Return the result of the test;
            return result;
        }

        internal static Dictionary<Tuple<string, string>, bool> ValidateSourceBusinessKeyExistenceVirtual(Tuple<string, string> validationObject, DataTable inputDataTable)
        {
            // First, the Business Keys for each table need to be identified information. This can be the combination of Business keys separated by a comma.
            // Every business key needs to be iterated over to validate if the attribute exists in that table.
            List<string> businessKeys = validationObject.Item2.Split(',').ToList();

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
                        var objectDetails = ClassMetadataHandling.GetSchema(validationObject.Item1).FirstOrDefault();

                        bool returnExistenceEvaluation = false;

                        DataRow[] foundAuthors = inputDataTable.Select("TABLE_NAME = '" + objectDetails.Value + "' AND SCHEMA_NAME = '"+objectDetails.Key+"' AND COLUMN_NAME = '"+ businessKeyPart.Trim() + "'");
                        if (foundAuthors.Length != 0)
                        {
                            returnExistenceEvaluation = true;
                        }

                        result.Add(Tuple.Create(validationObject.Item1, businessKeyPart.Trim()), returnExistenceEvaluation);
                    }
                }
            }

            // Return the result of the test;
            return result;
        }


    }


}
