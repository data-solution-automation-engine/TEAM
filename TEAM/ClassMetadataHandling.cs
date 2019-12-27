using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEAM
{
    internal class ClassMetadataHandling
    {
        /// <summary>
        /// Return the Surrogate Key for a given table using the TEAM settings (i.e. prefix/suffix settings etc.).
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>surrogateKey</returns>
        internal static string GetSurrogateKey(string tableName)
        {
            // Initialise the return value
            string surrogateKey = "";

            string keyLocation = FormBase.ConfigurationSettings.DwhKeyIdentifier;

            string[] prefixSuffixAray = {
                FormBase.ConfigurationSettings.HubTablePrefixValue,
                FormBase.ConfigurationSettings.SatTablePrefixValue,
                FormBase.ConfigurationSettings.LinkTablePrefixValue,
                FormBase.ConfigurationSettings.LsatTablePrefixValue
            };

            if (tableName != "Not applicable")
            {
                // Removing the table pre- or suffixes from the table name based on the TEAM configuration settings.
                if (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix")
                {
                    foreach (string prefixValue in prefixSuffixAray)
                    {
                        string prefixValueWithUnderscore = prefixValue + '_';
                        if (tableName.StartsWith(prefixValueWithUnderscore))
                        {
                            tableName = tableName.Replace(prefixValueWithUnderscore, "");
                        }
                    }
                }
                else
                {
                    foreach (string suffixValue in prefixSuffixAray)
                    {
                        string suffixValueWithUnderscore = '_'+suffixValue;
                        if (tableName.EndsWith(suffixValueWithUnderscore))
                        {
                            tableName = tableName.Replace(suffixValueWithUnderscore, "");
                        }
                    }
                }


                // Define the surrogate key using the table name and key prefix/suffix settings.
                if (FormBase.ConfigurationSettings.KeyNamingLocation == "Prefix")
                {
                    surrogateKey = keyLocation + '_' + tableName;
                }
                else
                {
                    surrogateKey = tableName + '_' + keyLocation;
                }
            }


            return surrogateKey;
        }


        internal static string GetTableType(string tableName)
        {
            string localType ="";

            // Remove schema, if there
            tableName = tableName.Substring(tableName.IndexOf('.') + 1);

            if (FormBase.ConfigurationSettings.TableNamingLocation == "Prefix") // I.e. HUB_CUSTOMER
            {
                if (tableName.StartsWith(FormBase.ConfigurationSettings.SatTablePrefixValue))
                {
                    localType = "Satellite";
                }
                else if (tableName.StartsWith(FormBase.ConfigurationSettings.HubTablePrefixValue))
                {
                    localType = "Hub";
                }
                else if (tableName.StartsWith(FormBase.ConfigurationSettings.LinkTablePrefixValue))
                {
                    localType = "Link";
                }
                else if (tableName.StartsWith(FormBase.ConfigurationSettings.LsatTablePrefixValue))
                {
                    localType = "Link-Satellite";
                }
                else if (tableName.StartsWith(FormBase.ConfigurationSettings.StgTablePrefixValue))
                {
                    localType = "Staging Area";
                }
                else if (tableName.StartsWith(FormBase.ConfigurationSettings.PsaTablePrefixValue))
                {
                    localType = "Persistent Staging Area";
                }
                else if (tableName.StartsWith("DIM_") && tableName.StartsWith("FACT_"))
                {
                    localType = "Presentation";
                }
                else 
                {
                    localType = "Derived";
                }
            }
            else if (FormBase.ConfigurationSettings.TableNamingLocation == "Suffix") // I.e. CUSTOMER_HUB
            {
                if (tableName.EndsWith(FormBase.ConfigurationSettings.SatTablePrefixValue))
                {
                    localType = "Satellite";
                }
                else if (tableName.EndsWith(FormBase.ConfigurationSettings.HubTablePrefixValue))
                {
                    localType = "Hub";
                }
                else if (tableName.EndsWith(FormBase.ConfigurationSettings.LinkTablePrefixValue))
                {
                    localType = "Link";
                }
                else if (tableName.EndsWith(FormBase.ConfigurationSettings.LsatTablePrefixValue))
                {
                    localType = "Link-Satellite";
                }
                else if (tableName.EndsWith(FormBase.ConfigurationSettings.StgTablePrefixValue))
                {
                    localType = "Staging Area";
                }
                else if (tableName.EndsWith(FormBase.ConfigurationSettings.PsaTablePrefixValue))
                {
                    localType = "Persistent Staging Area";
                }
                else if (tableName.EndsWith("DIM_") && tableName.EndsWith("FACT_"))
                {
                    localType = "Presentation";
                }
                else
                {
                    localType = "Derived";
                }
            }
            else
            {
                localType = "The table type cannot be defined because of an unknown prefix/suffix: "+ FormBase.ConfigurationSettings.TableNamingLocation;
            }


            return localType;
        }

        internal static string GetDatabaseForArea(string tableType)
        {
            string localDatabase = "";

            if (new string[] {"Hub", "Satellite", "Link", "Link-Satellite", "Derived"}.Contains(tableType))
            {
                localDatabase = FormBase.ConfigurationSettings.IntegrationDatabaseName;
            }
            else if (tableType == "Staging Area")
            {
                localDatabase = FormBase.ConfigurationSettings.StagingDatabaseName;
            }
            else if (tableType == "Persistent Staging Area")
            {
                localDatabase = FormBase.ConfigurationSettings.PsaDatabaseName;
            }
            else if (tableType == "Presentation")
            {
                localDatabase = FormBase.ConfigurationSettings.PresentationDatabaseName;
            }
            else // Return error
            {
                localDatabase = "Unknown - error - the database could not be derived from the object " + tableType;
            }

            return localDatabase;
        }

        internal static string GetLoadVector(string sourceMapping, string targetMapping)
        {
            // The following scenarios are supported:
            // -> If the source is not a DV table, but the target is a DV table then it's a base ('Raw') Data Vault ETL (load vector). - 'Raw'.
            // -> If the source is a DV table, and the target is a DV table then it's a Derived ('Business') Data Vault ETL - 'Interpreted'.
            // -> If the source is a DV table, but target is not a DV table then it's a Presentation Layer ETL. - 'Presentation'.

            // This is used to evaluate the correct connection for the generated ETL processes.

            string evaluatedSource = GetTableType(sourceMapping);
            string evaluatedTarget = GetTableType(targetMapping);

            string localArea = "";

            if (!new[] {"Hub", "Satellite", "Link", "Link-Satellite"}.Contains(evaluatedSource) && new[] { "Hub", "Satellite", "Link", "Link-Satellite"}.Contains(evaluatedTarget))
            {
                localArea = "Raw";
            }
            else if (new[] { "Hub", "Satellite", "Link", "Link-Satellite"}.Contains(evaluatedSource) && new[] { "Hub", "Satellite", "Link", "Link-Satellite" }.Contains(evaluatedTarget))
            {
                localArea = "Interpreted";
            }
            else if (new[] { "Hub", "Satellite", "Link", "Link-Satellite" }.Contains(evaluatedSource) && !new[] { "Hub", "Satellite", "Link", "Link-Satellite" }.Contains(evaluatedTarget))
            {
                localArea = FormBase.ConfigurationSettings.PsaDatabaseName;
            }
            else // Return error
            {
                localArea = "Unknown - error - the load vector could not be derived from the objects '" + evaluatedSource+"' and '"+evaluatedTarget+"'";
            }

            return localArea;
        }

        /// <summary>
        ///   Return just the table name, in case a fully qualified name is available (including schema etc.).
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal static string nonQualifiedTableName(string tableName)
        {
            string returnTableName = "";

            if (tableName.Contains('.')) // Split the string, keep the table name (remove the schema prefix)
            {
                var splitName = tableName.Split('.').ToList();
                returnTableName = splitName[1];

            }
            else // Return the default (e.g. just the table name)
            {
                returnTableName = tableName;
            }

            return returnTableName;
        }


        /// <summary>
        ///  Separates the schema from the table name, and returns both as individual values.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal static Dictionary<string, string> GetSchema(string tableName)
        {
            Dictionary<string, string> fullyQualifiedTableName = new Dictionary<string, string>();
            string schemaName = "";
            string returnTableName = "";

            if (tableName.Contains('.')) // Split the string
            {
                var splitName = tableName.Split('.').ToList();

                fullyQualifiedTableName.Add(splitName[0], splitName[1]);

            }
            else // Return the default (e.g. [dbo])
            {
                schemaName = FormBase.GlobalParameters.DefaultSchema;
                returnTableName = tableName;
            }

            fullyQualifiedTableName.Add(schemaName, returnTableName);

            return fullyQualifiedTableName;
        }

        /// <summary>
        /// Retrieve the fully qualified name (including schema) for a given input table name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        internal static string getFullSchemaTable(string tableName)
        {
            var fullyQualifiedSourceName = GetSchema(tableName).FirstOrDefault();

            var returnTableName = fullyQualifiedSourceName.Key + '.' + fullyQualifiedSourceName.Value;

            return returnTableName;
        }


        /// <summary>
        /// Returns a list of Business Key attributes as they are defined in the target Hub table.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="versionId"></param>
        /// <param name="queryMode"></param>
        /// <returns></returns>
        public static List<string> GetHubTargetBusinessKeyList(string schemaName, string tableName, int versionId, string queryMode)
        {
            // Obtain the business key as it is known in the target Hub table. Can be multiple due to composite keys.

            var conn = queryMode == "physical" ? new SqlConnection {ConnectionString = FormBase.ConfigurationSettings.ConnectionStringInt} : new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };

            try
            {
                conn.Open();
            }
            catch (Exception)
            {
               // SetTextDebug("An error has occurred defining the Hub Business Key in the model due to connectivity issues (connection string " + conn.ConnectionString + "). The associated message is " + exception.Message);
            }

            var sqlStatementForBusinessKeys = new StringBuilder();

            var keyText = FormBase.ConfigurationSettings.DwhKeyIdentifier;
            var localkeyLength = keyText.Length;
            var localkeySubstring = localkeyLength + 1;

            // Make sure brackets are removed
            schemaName = schemaName.Replace("[","").Replace("]","");
            tableName = tableName.Replace("[", "").Replace("]", "");

            if (queryMode == "physical")
            {
                // Make sure the live database is hit when the checkbox is ticked
                sqlStatementForBusinessKeys.AppendLine("SELECT COLUMN_NAME");
                sqlStatementForBusinessKeys.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS");
                sqlStatementForBusinessKeys.AppendLine("WHERE SUBSTRING(COLUMN_NAME,LEN(COLUMN_NAME)-" + localkeyLength + "," + localkeySubstring + ")!='_" + FormBase.ConfigurationSettings.DwhKeyIdentifier + "'");
                sqlStatementForBusinessKeys.AppendLine("AND TABLE_SCHEMA = '" + schemaName + "'");
                sqlStatementForBusinessKeys.AppendLine("  AND TABLE_NAME= '" + tableName + "'");
                sqlStatementForBusinessKeys.AppendLine("  AND COLUMN_NAME NOT IN ('" + FormBase.ConfigurationSettings.RecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeRecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeLoadDateTimeAttribute + "','" +
                                                          FormBase.ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute + "','" + FormBase.ConfigurationSettings.EtlProcessAttribute + "','" + FormBase.ConfigurationSettings.LoadDateTimeAttribute + "')");
            }
            else
            {
                //Ignore version is not checked, so versioning is used - meaning the business key metadata is sourced from the version history metadata.
                sqlStatementForBusinessKeys.AppendLine("SELECT COLUMN_NAME");
                sqlStatementForBusinessKeys.AppendLine("FROM TMP_MD_VERSION_ATTRIBUTE");
                sqlStatementForBusinessKeys.AppendLine("WHERE SUBSTRING(COLUMN_NAME,LEN(COLUMN_NAME)-" + localkeyLength + "," + localkeySubstring + ")!='_" + FormBase.ConfigurationSettings.DwhKeyIdentifier + "'");
                sqlStatementForBusinessKeys.AppendLine("  AND TABLE_NAME= '" + tableName + "'");
                sqlStatementForBusinessKeys.AppendLine("  AND SCHEMA_NAME= '" + schemaName + "'");
                sqlStatementForBusinessKeys.AppendLine("  AND COLUMN_NAME NOT IN ('" + FormBase.ConfigurationSettings.RecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeRecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeLoadDateTimeAttribute + "','" + FormBase.ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute + "','" +
                                                          FormBase.ConfigurationSettings.EtlProcessAttribute + "','" + FormBase.ConfigurationSettings.LoadDateTimeAttribute + "')");
                sqlStatementForBusinessKeys.AppendLine("  AND VERSION_ID = " + versionId + "");
            }


            var keyList = FormBase.GetDataTable(ref conn, sqlStatementForBusinessKeys.ToString());

            if (keyList == null)
            {
                //SetTextDebug("An error has occurred defining the Hub Business Key in the model for " + hubTableName + ". The Business Key was not found when querying the underlying metadata. This can be either that the attribute is missing in the metadata or in the table (depending if versioning is used). If the 'ignore versioning' option is checked, then the metadata will be retrieved directly from the data dictionary. Otherwise the metadata needs to be available in the repository (manage model metadata).");
            }

            var businessKeyList = new List<string>();
            foreach (DataRow row in keyList.Rows)
            {
                if (!businessKeyList.Contains((string)row["COLUMN_NAME"]))
                {
                    businessKeyList.Add((string)row["COLUMN_NAME"]);
                }
            }

            return businessKeyList;
        }


        /// <summary>
        /// Returns a list of Business Key attributes as they are defined in the target Link table.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="versionId"></param>
        /// <param name="queryMode"></param>
        /// <returns></returns>
        public static List<string> GetLinkTargetBusinessKeyList(string schemaName, string tableName, int versionId)
        {
            // Obtain the business key as it is known in the target Hub table. Can be multiple due to composite keys.

            var conn = new SqlConnection();
            conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };

            try
            {
                conn.Open();
            }
            catch (Exception)
            {
            }

            // Make sure brackets are removed
            tableName = tableName.Replace("[", "").Replace("]", "");

            var sqlStatementForBusinessKeys = new StringBuilder();

            sqlStatementForBusinessKeys.AppendLine("SELECT");
            sqlStatementForBusinessKeys.AppendLine("  xref.[HUB_NAME]");
            sqlStatementForBusinessKeys.AppendLine(" ,xref.[LINK_NAME]");
            sqlStatementForBusinessKeys.AppendLine(" ,hub.[BUSINESS_KEY]");
            sqlStatementForBusinessKeys.AppendLine("FROM[dbo].[MD_HUB_LINK_XREF] xref");
            sqlStatementForBusinessKeys.AppendLine("JOIN[dbo].[MD_HUB] hub ON xref.HUB_NAME = hub.HUB_NAME");
            sqlStatementForBusinessKeys.AppendLine("WHERE [LINK_NAME] = '"+tableName+"'");
            sqlStatementForBusinessKeys.AppendLine("ORDER BY [HUB_ORDER]");

            var keyList = FormBase.GetDataTable(ref conn, sqlStatementForBusinessKeys.ToString());

            if (keyList == null)
            {
                //SetTextDebug("An error has occurred defining the Hub Business Key in the model for " + hubTableName + ". The Business Key was not found when querying the underlying metadata. This can be either that the attribute is missing in the metadata or in the table (depending if versioning is used). If the 'ignore versioning' option is checked, then the metadata will be retrieved directly from the data dictionary. Otherwise the metadata needs to be available in the repository (manage model metadata).");
            }

            var businessKeyList = new List<string>();
            foreach (DataRow row in keyList.Rows)
            {
                //if (!businessKeyList.Contains((string)row["BUSINESS_KEY"]))
               // {
                    businessKeyList.Add((string)row["BUSINESS_KEY"]);
               // }
            }

            return businessKeyList;
        }
    }

    internal class AttributeSelection
    {
        internal StringBuilder CreatePhysicalModelSet(string databaseName, string filterObjects)
        {
            var returnValue = new StringBuilder();

            returnValue.AppendLine("SELECT");
            returnValue.AppendLine("  DB_NAME(DB_ID('" + databaseName + "')) AS[DATABASE_NAME],");
            returnValue.AppendLine("  OBJECT_SCHEMA_NAME(OBJECT_ID, DB_ID('" + databaseName + "')) AS[SCHEMA_NAME],");
            returnValue.AppendLine("  OBJECT_NAME(A.OBJECT_ID, DB_ID('" + databaseName + "')) AS TABLE_NAME,");
            returnValue.AppendLine("  A.OBJECT_ID,");
            returnValue.AppendLine("  A.[name] AS COLUMN_NAME,");
            returnValue.AppendLine("  t.[name] AS[DATA_TYPE], ");
            returnValue.AppendLine("  CAST(COALESCE(");
            returnValue.AppendLine("    CASE WHEN UPPER(t.[name]) = 'NVARCHAR' THEN A.[max_length]/2 ");
            returnValue.AppendLine("    ELSE A.[max_length]");
            returnValue.AppendLine("    END");
            returnValue.AppendLine("   ,0) AS VARCHAR(100)) AS[CHARACTER_MAXIMUM_LENGTH],");
            returnValue.AppendLine("  CAST(COALESCE(A.[precision],0) AS VARCHAR(100)) AS[NUMERIC_PRECISION], ");
            returnValue.AppendLine("  CAST(A.[column_id] AS VARCHAR(100)) AS[ORDINAL_POSITION],");
            returnValue.AppendLine("  CASE");
            returnValue.AppendLine("    WHEN keysub.COLUMN_NAME IS NULL");
            returnValue.AppendLine("    THEN 'N' ");
            returnValue.AppendLine("    ELSE 'Y' ");
            returnValue.AppendLine("  END AS PRIMARY_KEY_INDICATOR");
            returnValue.AppendLine("FROM [" + databaseName + "].sys.columns A");
            returnValue.AppendLine("JOIN sys.types t ON A.user_type_id= t.user_type_id");
            returnValue.AppendLine("-- Primary Key");
            returnValue.AppendLine(" LEFT OUTER JOIN (");
            returnValue.AppendLine("     SELECT");
            returnValue.AppendLine("       sc.name AS TABLE_NAME,");
            returnValue.AppendLine("       C.name AS COLUMN_NAME");
            returnValue.AppendLine("     FROM [" + databaseName + "].sys.index_columns A");
            returnValue.AppendLine("     JOIN [" + databaseName + "].sys.indexes B");
            returnValue.AppendLine("     ON A.OBJECT_ID= B.OBJECT_ID AND A.index_id= B.index_id");
            returnValue.AppendLine("     JOIN [" + databaseName + "].sys.columns C");
            returnValue.AppendLine("     ON A.column_id= C.column_id AND A.OBJECT_ID= C.OBJECT_ID");
            returnValue.AppendLine("     JOIN [" + databaseName + "].sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
            returnValue.AppendLine("     WHERE is_primary_key = 1");
            returnValue.AppendLine(" ) keysub");
            returnValue.AppendLine("    ON OBJECT_NAME(A.OBJECT_ID, DB_ID('" + databaseName + "')) = keysub.[TABLE_NAME]");
            returnValue.AppendLine("   AND A.[name] = keysub.COLUMN_NAME");
            returnValue.AppendLine("    WHERE A.[OBJECT_ID] IN (" + filterObjects + ")");

            return returnValue;
        }
    }
}
