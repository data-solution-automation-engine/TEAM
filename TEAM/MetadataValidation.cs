using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace TEAM
{
    internal class MetadataValidation 
    {

        /// <summary>
        ///    This class ensures that a source object exists in the physical model
        /// </summary>
        internal static string ValidateObjectExistence (string validationObject, string connectionString)
        {
            var conn = new SqlConnection { ConnectionString = connectionString };
            conn.Open();

            // Execute the check
            var cmd = new SqlCommand("SELECT CASE WHEN EXISTS ((SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + validationObject + "')) THEN 1 ELSE 0 END", conn);
            var exists = (int) cmd.ExecuteScalar() == 1;

            conn.Close();

            // return the result of the test;
            return exists.ToString();
        }

        internal static Dictionary<string, bool> ValidateLogicalGroup(Tuple<string, string, string> validationObject, string connectionString, int versionId)
        {
            // First, the Business Key need to be checked. This is to determine how many dependents are expected.
            // For instance, if a Link has a three-part Business Key then three Hubs will be expected
            List<string> hubBusinessKeys = validationObject.Item3.Split(',').ToList();
            int businessKeyCount = hubBusinessKeys.Count;

            // We need to manupulate the query to account for multiplicity in the model i.e. many Satellites linking to a single Hub.
            // The only interest is whether the Hub is there...
            var tableInclusionFilterCriterion = "";
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
            else if (validationObject.Item2.StartsWith(FormBase.ConfigurationSettings.LsatPrefixValue)) // If the table is a Link-Satellite, only the Link is required
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

                    sqlStatementForDependent.AppendLine("SELECT COUNT(*) AS NR_OF_DEPENDENTS");
                    sqlStatementForDependent.AppendLine("FROM [MD_TABLE_MAPPING]");
                    sqlStatementForDependent.AppendLine("WHERE");
                    sqlStatementForDependent.AppendLine("    [GENERATE_INDICATOR] = 'Y'");
                    sqlStatementForDependent.AppendLine("AND [VERSION_ID] = " + versionId);
                    sqlStatementForDependent.AppendLine("AND [STAGING_AREA_TABLE] = '" + validationObject.Item1 + "'");
                    sqlStatementForDependent.AppendLine("AND [BUSINESS_KEY_ATTRIBUTE] = '" + businessKeyComponent.Replace("'", "''").Trim() + "'");
                    sqlStatementForDependent.AppendLine("AND [INTEGRATION_AREA_TABLE] != '" + validationObject.Item2 + "'"); // Exclude itself
                    sqlStatementForDependent.AppendLine("AND [INTEGRATION_AREA_TABLE] LIKE '" + tableInclusionFilterCriterion + "_%'");

                    var dependentsList = FormBase.GetDataTable(ref conn, sqlStatementForDependent.ToString());

                    // Derive the Hub surrogate key name, as this can be compared against the Link
                    foreach (DataRow row in dependentsList.Rows)
                    {
                        numberOfDependents = numberOfDependents + Convert.ToInt32(row["NR_OF_DEPENDENTS"]);
                    }
                }
            }
            else // In the case of an LSAT, only join on the Link using the full business key
            {
                // Query the dependent information
                var sqlStatementForDependent = new StringBuilder();

                sqlStatementForDependent.AppendLine("SELECT COUNT(*) AS NR_OF_DEPENDENTS");
                sqlStatementForDependent.AppendLine("FROM [MD_TABLE_MAPPING]");
                sqlStatementForDependent.AppendLine("WHERE");
                sqlStatementForDependent.AppendLine("    [GENERATE_INDICATOR] = 'Y'");
                sqlStatementForDependent.AppendLine("AND [VERSION_ID] = " + versionId);
                sqlStatementForDependent.AppendLine("AND [STAGING_AREA_TABLE] = '" + validationObject.Item1 + "'");
                sqlStatementForDependent.AppendLine("AND [BUSINESS_KEY_ATTRIBUTE] = '" + validationObject.Item3.Replace("'", "''").Trim() + "'");
                sqlStatementForDependent.AppendLine("AND [INTEGRATION_AREA_TABLE] != '" + validationObject.Item2 + "'"); // Exclude itself
                sqlStatementForDependent.AppendLine("AND [INTEGRATION_AREA_TABLE] LIKE '" + tableInclusionFilterCriterion + "_%'");

                var dependentsList = FormBase.GetDataTable(ref conn, sqlStatementForDependent.ToString());

                // Derive the Hub surrogate key name, as this can be compared against the Link
                foreach (DataRow row in dependentsList.Rows)
                {
                    numberOfDependents = Convert.ToInt32(row["NR_OF_DEPENDENTS"]);
                }
            }

            conn.Close();



            // Run the comparison
            // Test for equality.
            bool equal = false;
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


        internal static Dictionary<string,bool> ValidateLinkKeyOrder(Tuple<string,string,string> validationObject, string connectionString, int versionId)
        {
            // First, the Hubs need to be identified using the Business Key information. This, for the Link, is the combination of Business keys separated by a comma.
            // Every business key needs to be iterated over to query the individual Hub information
            List<string> hubBusinessKeys = validationObject.Item3.Split(',').ToList();

            // Now iterate over each Hub, as identified by the business key.
            // Maintain the ordinal position of the business key
            var conn = new SqlConnection { ConnectionString = connectionString };
            conn.Open();

            var hubKeyOrder = new Dictionary<int, string>();

            int businessKeyOrder = 0;

            foreach (string hubBusinessKey in hubBusinessKeys)
            {
                // Determine the order in the business key array
                businessKeyOrder++;

                // Query the Hub information
                var sqlStatementForHub = new StringBuilder();

                sqlStatementForHub.AppendLine("SELECT");
                sqlStatementForHub.AppendLine("   [STAGING_AREA_TABLE]");
                sqlStatementForHub.AppendLine("  ,[BUSINESS_KEY_ATTRIBUTE]");
                sqlStatementForHub.AppendLine("  ,[INTEGRATION_AREA_TABLE] AS [HUB_TABLE_NAME]");
                sqlStatementForHub.AppendLine("FROM [MD_TABLE_MAPPING]");
                sqlStatementForHub.AppendLine("WHERE");
                sqlStatementForHub.AppendLine("    [GENERATE_INDICATOR] = 'Y'");
                sqlStatementForHub.AppendLine("AND [VERSION_ID] = " + versionId);
                sqlStatementForHub.AppendLine("AND [STAGING_AREA_TABLE] = '" + validationObject.Item1 + "'");
                sqlStatementForHub.AppendLine("AND [BUSINESS_KEY_ATTRIBUTE] = '"+hubBusinessKey.Replace("'","''").Trim()+"'");
                sqlStatementForHub.AppendLine("AND [INTEGRATION_AREA_TABLE] NOT LIKE '" + FormBase.ConfigurationSettings.SatTablePrefixValue + "_%'");

                var hubList = FormBase.GetDataTable(ref conn, sqlStatementForHub.ToString());

                // Derive the Hub surrogate key name, as this can be compared against the Link
                string hubSurrogateKeyName = "";
                foreach (DataRow row in hubList.Rows)
                {
                    string hubTableName = row["HUB_TABLE_NAME"].ToString();
                    hubSurrogateKeyName = hubTableName.Replace(FormBase.ConfigurationSettings.HubTablePrefixValue+'_', "")+"_"+FormBase.ConfigurationSettings.DwhKeyIdentifier;
                }

                hubKeyOrder.Add(businessKeyOrder, hubSurrogateKeyName);
            }
            conn.Close();

            // The hubKeyOrder contains the order of the keys in the Hub, now we need to do the same for the (target) Link so we can compare.
            var connTarget = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringInt };
            connTarget.Open();

            var sqlStatementForLink = new StringBuilder();

            sqlStatementForLink.AppendLine("SELECT");
            sqlStatementForLink.AppendLine("   TABLE_NAME");
            sqlStatementForLink.AppendLine("  ,COLUMN_NAME");
            sqlStatementForLink.AppendLine("  ,ORDINAL_POSITION");
            sqlStatementForLink.AppendLine("  ,ROW_NUMBER() OVER(PARTITION BY TABLE_NAME ORDER BY ORDINAL_POSITION) AS [HUB_KEY_POSITION]");
            sqlStatementForLink.AppendLine("FROM "+ FormBase.ConfigurationSettings.IntegrationDatabaseName+".INFORMATION_SCHEMA.COLUMNS");
            sqlStatementForLink.AppendLine("    WHERE TABLE_NAME LIKE '"+ FormBase.ConfigurationSettings.LinkTablePrefixValue+"_%'");
            sqlStatementForLink.AppendLine("AND ORDINAL_POSITION > 4");
            sqlStatementForLink.AppendLine("AND TABLE_NAME = '"+validationObject.Item2+"'");

            var linkList = FormBase.GetDataTable(ref connTarget, sqlStatementForLink.ToString());
            // Derive the Hub surrogate key name, as this can be compared against the Link
            var linkKeyOrder = new Dictionary<int, string>();

            foreach (DataRow row in linkList.Rows)
            {
                var linkHubSurrogateKeyName = row["COLUMN_NAME"].ToString();
                int linkHubSurrogateKeyPosition = Convert.ToInt32(row["HUB_KEY_POSITION"]);

                linkKeyOrder.Add(linkHubSurrogateKeyPosition, linkHubSurrogateKeyName);
            }
            connTarget.Close();


            // Run the comparison
            // Test for equality.
            bool equal = false;
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

            // return the result of the test;
            Dictionary<string,bool> result = new Dictionary<string, bool>();
            result.Add(validationObject.Item2,equal);
            return result;
        }

        internal static Dictionary<Tuple<string,string>, bool> ValidateSourceBusinessKeyExistence(Tuple<string, string> validationObject, string connectionString, int versionId)
        {
            // First, the Business Keys for each table need to be identified information. This can be the combination of Business keys separated by a comma.
            // Every business key needs to be iterated over to validate if the attribute exists in that table.
            List<string> businessKeys = validationObject.Item2.Split(',').ToList();

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
                    if (businessKeyPart.StartsWith("'") && businessKeyPart.EndsWith("'"))
                    {
                        // Do nothing
                    }
                    else
                    {
                        // Query the data dictionary to valdiate existence
                        var cmd = new SqlCommand("SELECT CASE WHEN EXISTS ((SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + validationObject.Item1 + "' AND COLUMN_NAME = '" + businessKeyPart.Trim() + "')) THEN 1 ELSE 0 END", conn);
                        var exists = (int)cmd.ExecuteScalar() == 1;
                        result.Add(Tuple.Create(validationObject.Item1, businessKeyPart.Trim()), exists);
                    }
                }
            }
            conn.Close();
            // Return the result of the test;
            return result;
        }
    }
}
