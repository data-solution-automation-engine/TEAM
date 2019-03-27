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
                else if (tableName.StartsWith(FormBase.ConfigurationSettings.LsatPrefixValue))
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
                else if (tableName.EndsWith(FormBase.ConfigurationSettings.LsatPrefixValue))
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
        ///   Return just the table name, in case a fully qualified name is available (including schema etc.)
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

        internal static string getFullSchemaTable(string tableName)
        {
            var fullyQualifiedSourceName = GetSchema(tableName).FirstOrDefault();

            var returnTableName = fullyQualifiedSourceName.Key + '.' + fullyQualifiedSourceName.Value;

            return returnTableName;
        }

        public static List<string> GetHubTargetBusinessKeyList(string hubSchemaName, string hubTableName, int versionId, string queryMode)
        {
            // Obtain the business key as it is known in the target Hub table. Can be multiple due to composite keys
            var conn = new SqlConnection();

            if (queryMode == "physical")
            {
                conn = new SqlConnection {ConnectionString = FormBase.ConfigurationSettings.ConnectionStringInt};
            }
            else // Virtual
            {
                conn = new SqlConnection { ConnectionString = FormBase.ConfigurationSettings.ConnectionStringOmd };
            }

            try
            {
                conn.Open();
            }
            catch (Exception exception)
            {
               // SetTextDebug("An error has occurred defining the Hub Business Key in the model due to connectivity issues (connection string " + conn.ConnectionString + "). The associated message is " + exception.Message);
            }

            var sqlStatementForHubBusinessKeys = new StringBuilder();

            var keyText = FormBase.ConfigurationSettings.DwhKeyIdentifier;
            var localkeyLength = keyText.Length;
            var localkeySubstring = localkeyLength + 1;

            // Make sure brackets are removed
            hubSchemaName = hubSchemaName.Replace("[","").Replace("]","");
            hubTableName = hubTableName.Replace("[", "").Replace("]", "");

            if (queryMode == "physical")
            {
                // Make sure the live database is hit when the checkbox is ticked
                sqlStatementForHubBusinessKeys.AppendLine("SELECT COLUMN_NAME");
                sqlStatementForHubBusinessKeys.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS");
                sqlStatementForHubBusinessKeys.AppendLine("WHERE SUBSTRING(COLUMN_NAME,LEN(COLUMN_NAME)-" + localkeyLength + "," + localkeySubstring + ")!='_" + FormBase.ConfigurationSettings.DwhKeyIdentifier + "'");
                sqlStatementForHubBusinessKeys.AppendLine("AND TABLE_SCHEMA = '" + hubSchemaName + "'");
                sqlStatementForHubBusinessKeys.AppendLine("  AND TABLE_NAME= '" + hubTableName + "'");
                sqlStatementForHubBusinessKeys.AppendLine("  AND COLUMN_NAME NOT IN ('" + FormBase.ConfigurationSettings.RecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeRecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeLoadDateTimeAttribute + "','" +
                                                          FormBase.ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute + "','" + FormBase.ConfigurationSettings.EtlProcessAttribute + "','" + FormBase.ConfigurationSettings.LoadDateTimeAttribute + "')");
            }
            else
            {
                //Ignore version is not checked, so versioning is used - meaning the business key metadata is sourced from the version history metadata.
                sqlStatementForHubBusinessKeys.AppendLine("SELECT COLUMN_NAME");
                sqlStatementForHubBusinessKeys.AppendLine("FROM MD_VERSION_ATTRIBUTE");
                sqlStatementForHubBusinessKeys.AppendLine("WHERE SUBSTRING(COLUMN_NAME,LEN(COLUMN_NAME)-" + localkeyLength + "," + localkeySubstring + ")!='_" + FormBase.ConfigurationSettings.DwhKeyIdentifier + "'");
                sqlStatementForHubBusinessKeys.AppendLine("  AND TABLE_NAME= '" + hubTableName + "'");
                sqlStatementForHubBusinessKeys.AppendLine("  AND SCHEMA_NAME= '" + hubSchemaName + "'");
                sqlStatementForHubBusinessKeys.AppendLine("  AND COLUMN_NAME NOT IN ('" + FormBase.ConfigurationSettings.RecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeRecordSourceAttribute + "','" + FormBase.ConfigurationSettings.AlternativeLoadDateTimeAttribute + "','" + FormBase.ConfigurationSettings.AlternativeSatelliteLoadDateTimeAttribute + "','" +
                                                          FormBase.ConfigurationSettings.EtlProcessAttribute + "','" + FormBase.ConfigurationSettings.LoadDateTimeAttribute + "')");
                sqlStatementForHubBusinessKeys.AppendLine("  AND VERSION_ID = " + versionId + "");
            }


            var hubKeyList = FormBase.GetDataTable(ref conn, sqlStatementForHubBusinessKeys.ToString());

            if (hubKeyList == null)
            {
                //SetTextDebug("An error has occurred defining the Hub Business Key in the model for " + hubTableName + ". The Business Key was not found when querying the underlying metadata. This can be either that the attribute is missing in the metadata or in the table (depending if versioning is used). If the 'ignore versioning' option is checked, then the metadata will be retrieved directly from the data dictionary. Otherwise the metadata needs to be available in the repository (manage model metadata).");
            }

            var businessKeyList = new List<string>();
            foreach (DataRow row in hubKeyList.Rows)
            {
                if (!businessKeyList.Contains((string)row["COLUMN_NAME"]))
                {
                    businessKeyList.Add((string)row["COLUMN_NAME"]);
                }
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
            returnValue.AppendLine("FROM " + databaseName + ".sys.columns A");
            returnValue.AppendLine("JOIN sys.types t ON A.user_type_id= t.user_type_id");
            returnValue.AppendLine("-- Primary Key");
            returnValue.AppendLine(" LEFT OUTER JOIN (");
            returnValue.AppendLine("     SELECT");
            returnValue.AppendLine("       sc.name AS TABLE_NAME,");
            returnValue.AppendLine("       C.name AS COLUMN_NAME");
            returnValue.AppendLine("     FROM " + databaseName + ".sys.index_columns A");
            returnValue.AppendLine("     JOIN " + databaseName + ".sys.indexes B");
            returnValue.AppendLine("     ON A.OBJECT_ID= B.OBJECT_ID AND A.index_id= B.index_id");
            returnValue.AppendLine("     JOIN " + databaseName + ".sys.columns C");
            returnValue.AppendLine("     ON A.column_id= C.column_id AND A.OBJECT_ID= C.OBJECT_ID");
            returnValue.AppendLine("     JOIN " + databaseName + ".sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
            returnValue.AppendLine("     WHERE is_primary_key = 1");
            returnValue.AppendLine(" ) keysub");
            returnValue.AppendLine("    ON OBJECT_NAME(A.OBJECT_ID, DB_ID('" + databaseName + "')) = keysub.[TABLE_NAME]");
            returnValue.AppendLine("   AND A.[name] = keysub.COLUMN_NAME");
            returnValue.AppendLine("    WHERE A.[OBJECT_ID] IN (" + filterObjects + ")");

            return returnValue;
        }
    }
}
