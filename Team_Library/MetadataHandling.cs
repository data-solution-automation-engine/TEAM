using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;

namespace TEAM_Library
{
    public class MetadataHandling
    {
        /// <summary>
        /// Definition of the allowed table types. These are used everywhere to derive approach based on conventions.
        /// </summary>
        public enum TableTypes
        {
            Context,
            CoreBusinessConcept,
            NaturalBusinessRelationship,
            NaturalBusinessRelationshipContext,
            NaturalBusinessRelationshipContextDrivingKey,
            StagingArea,
            PersistentStagingArea,
            Derived,
            Presentation,
            Source,
            Unknown
        }

        /// <summary>
        /// Return the Surrogate Key for a given table using the TEAM settings (i.e. prefix/suffix settings etc.).
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>surrogateKey</returns>
        public static string GetSurrogateKey(string tableName, TeamConnection teamConnection, TeamConfiguration configuration)
        {
            // Get the fully qualified name
            KeyValuePair<string,string> fullyQualifiedName = GetFullyQualifiedDataObjectName(tableName, teamConnection).FirstOrDefault();
            
            // Initialise the return value
            string surrogateKey = "";
            string newTableName = fullyQualifiedName.Value;



            string keyLocation = configuration.DwhKeyIdentifier;

            string[] prefixSuffixArray = {
                configuration.HubTablePrefixValue,
                configuration.SatTablePrefixValue,
                configuration.LinkTablePrefixValue,
                configuration.LsatTablePrefixValue
            };

            if (newTableName != "Not applicable")
            {
                // Removing the table pre- or suffixes from the table name based on the TEAM configuration settings.
                if (configuration.TableNamingLocation == "Prefix")
                {
                    foreach (string prefixValue in prefixSuffixArray)
                    {
                        string prefixValueWithUnderscore = prefixValue + '_';
                        if (newTableName.StartsWith(prefixValueWithUnderscore))
                        {
                            newTableName = newTableName.Replace(prefixValueWithUnderscore, "");
                        }
                    }
                }
                else
                {
                    foreach (string suffixValue in prefixSuffixArray)
                    {
                        string suffixValueWithUnderscore = '_'+suffixValue;
                        if (newTableName.EndsWith(suffixValueWithUnderscore))
                        {
                            newTableName = newTableName.Replace(suffixValueWithUnderscore, "");
                        }
                    }
                }


                // Define the surrogate key using the table name and key prefix/suffix settings.
                if (configuration.KeyNamingLocation == "Prefix")
                {
                    surrogateKey = keyLocation + '_' + newTableName;
                }
                else
                {
                    surrogateKey = newTableName + '_' + keyLocation;
                }
            }
            return surrogateKey;
        }
        
        /// <summary>
        /// This method returns the type of table (classification) as an enumerator based on the name and active conventions.
        /// Requires fully qualified name, or at least ignores schemas in the name.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="additionalInformation"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static TableTypes GetDataObjectType(string dataObjectName, string additionalInformation, TeamConfiguration configuration)
        {
            TableTypes localType;

            var presentationLayerLabelArray = Utility.SplitLabelIntoArray(configuration.PresentationLayerLabels);
            var transformationLabelArray = Utility.SplitLabelIntoArray(configuration.TransformationLabels);

            // Remove schema, if one is set
            dataObjectName = GetNonQualifiedTableName(dataObjectName);

            switch (configuration.TableNamingLocation)
            {
                // I.e. HUB_CUSTOMER
                case "Prefix" when dataObjectName.StartsWith(configuration.SatTablePrefixValue):
                    localType = TableTypes.Context;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.HubTablePrefixValue):
                    localType = TableTypes.CoreBusinessConcept;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.LinkTablePrefixValue):
                    localType = TableTypes.NaturalBusinessRelationship;
                    break;
                case "Prefix" when (dataObjectName.StartsWith(configuration.LsatTablePrefixValue) && additionalInformation==""):
                    localType = TableTypes.NaturalBusinessRelationshipContext;
                    break;
                case "Prefix" when (dataObjectName.StartsWith(configuration.LsatTablePrefixValue) && additionalInformation != ""):
                    localType = TableTypes.NaturalBusinessRelationshipContextDrivingKey;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.StgTablePrefixValue):
                    localType = TableTypes.StagingArea;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.PsaTablePrefixValue):
                    localType = TableTypes.PersistentStagingArea;
                    break;
                // Presentation Layer
                case "Prefix" when presentationLayerLabelArray.Any(s => dataObjectName.StartsWith(s)):
                    localType = TableTypes.Presentation;
                    break;
                // Derived or transformation
                case "Prefix" when transformationLabelArray.Any(s => dataObjectName.StartsWith(s)):
                    localType = TableTypes.Derived;
                    break;
                // Source
                case "Prefix":
                    localType = TableTypes.Source;
                    break;
                // Suffix
                // I.e. CUSTOMER_HUB
                case "Suffix" when dataObjectName.EndsWith(configuration.SatTablePrefixValue):
                    localType = TableTypes.Context;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.HubTablePrefixValue):
                    localType = TableTypes.CoreBusinessConcept;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.LinkTablePrefixValue):
                    localType = TableTypes.NaturalBusinessRelationship;
                    break;
                case "Suffix" when (dataObjectName.EndsWith(configuration.LsatTablePrefixValue) && additionalInformation == ""):
                    localType = TableTypes.NaturalBusinessRelationshipContext;
                    break;
                case "Suffix" when (dataObjectName.EndsWith(configuration.LsatTablePrefixValue) && additionalInformation != ""):
                    localType = TableTypes.NaturalBusinessRelationshipContextDrivingKey;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.StgTablePrefixValue):
                    localType = TableTypes.StagingArea;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.PsaTablePrefixValue):
                    localType = TableTypes.PersistentStagingArea;
                    break;
                // Presentation Layer
                case "Suffix" when presentationLayerLabelArray.Any(s => dataObjectName.EndsWith(s)):
                    localType = TableTypes.Presentation;
                    break;
                // Transformation / derived
                case "Suffix" when transformationLabelArray.Any(s => dataObjectName.EndsWith(s)):
                    localType = TableTypes.Derived;
                    break;
                case "Suffix":
                    localType = TableTypes.Source;
                    break;
                default:
                    localType = TableTypes.Unknown;
                    break;
            }
            // Return the table type
            return localType;
        }

        /// <summary>
        /// This method returns the ETL loading 'direction' based on the source and target mapping.
        /// </summary>
        /// <param name="sourceDataObjectName"></param>
        /// <param name="targetDataObjectName"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static string GetDataObjectMappingLoadVector(string sourceDataObjectName, string targetDataObjectName, TeamConfiguration teamConfiguration)
        {
            // This is used to evaluate the correct connection for the generated ETL processes.

            TableTypes evaluatedSource = GetDataObjectType(sourceDataObjectName, "", teamConfiguration);
            TableTypes evaluatedTarget = GetDataObjectType(targetDataObjectName, "", teamConfiguration);

            string loadVector = "";

            if (evaluatedSource == TableTypes.StagingArea && evaluatedTarget == TableTypes.PersistentStagingArea)
            {
                loadVector = "Landing to Persistent Staging Area";
            }
            // If the source is not a DWH table, but the target is a DWH table then it's a base ('Raw') Data Warehouse ETL (load vector). - 'Staging Layer to Raw Data Warehouse'.
            else if (!new[] {TableTypes.CoreBusinessConcept, TableTypes.Context, TableTypes.NaturalBusinessRelationship, TableTypes.NaturalBusinessRelationshipContext, TableTypes.Derived }.Contains(evaluatedSource) && new[] { TableTypes.CoreBusinessConcept, TableTypes.Context, TableTypes.NaturalBusinessRelationship, TableTypes.NaturalBusinessRelationshipContext }.Contains(evaluatedTarget))
            {
                loadVector = "Staging Layer to Raw Data Warehouse";
            }
            // If the source is a DWH or Derived table, and the target is a DWH table then it's a Derived ('Business') DWH ETL - 'Raw Data Warehouse to Interpreted'.
            else if (new[] { TableTypes.CoreBusinessConcept, TableTypes.Context, TableTypes.NaturalBusinessRelationship, TableTypes.NaturalBusinessRelationshipContext, TableTypes.Derived }.Contains(evaluatedSource) && new[] { TableTypes.CoreBusinessConcept, TableTypes.Context, TableTypes.NaturalBusinessRelationship, TableTypes.NaturalBusinessRelationshipContext }.Contains(evaluatedTarget))
            {
                loadVector = "Raw Data Warehouse to Interpreted";
            }
            // If the source is a DWH table, but target is not a DWH table then it's a Presentation Layer ETL. - 'Data Warehouse to Presentation Layer'.
            else if 
            (new[] { TableTypes.CoreBusinessConcept, TableTypes.Context, TableTypes.NaturalBusinessRelationship, TableTypes.NaturalBusinessRelationshipContext }.Contains(evaluatedSource) 
                     && 
                     new[] { TableTypes.Presentation }.Contains(evaluatedTarget)
            )
            {
                loadVector = "Data Warehouse to Presentation Layer";
            }
            else if (evaluatedSource == TableTypes.Source && new[] { TableTypes.StagingArea, TableTypes.PersistentStagingArea}.Contains(evaluatedTarget))
            {
                loadVector = "Source to Staging Layer";
            }
            else // Return error
            {
                loadVector = "'" + evaluatedSource+"' to '"+evaluatedTarget+"'";
            }

            return loadVector;
        }

        /// <summary>
        ///   Return only the table name (without the schema), in case a fully qualified name is available (including schema etc.).
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string GetNonQualifiedTableName(string tableName)
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
        /// Separates the schema from the table name (if available), and returns both as individual values in a Dictionary key/value pair (key schema/ value table).
        /// If no schema is defined, the connection information will be used to determine the schema. If all else fails 'dbo' will set as default.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="teamConnection"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetFullyQualifiedDataObjectName(string tableName, TeamConnection teamConnection)
        {
            Dictionary<string, string> fullyQualifiedTableName = new Dictionary<string, string>();
            string schemaName = "";
            string returnTableName = "";

            if (tableName.Contains('.')) // Split the string
            {
                var splitName = tableName.Split('.').ToList();

                schemaName = splitName[0];
                returnTableName = splitName[1];

                fullyQualifiedTableName.Add(schemaName, returnTableName);

            }
            else // Return the default (e.g. [dbo])
            {
                if (teamConnection is null)
                {
                    schemaName = "dbo";
                }
                else
                {
                    schemaName = teamConnection.DatabaseServer.SchemaName ?? "dbo";
                }

                returnTableName = tableName;

                fullyQualifiedTableName.Add(schemaName, returnTableName);
            }

            return fullyQualifiedTableName;
        }


        /// <summary>
        /// Returns a list of Business Key attributes as they are defined in the target Hub table (virtual setup)
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="versionId"></param>
        /// <param name="queryMode"></param>
        /// <returns></returns>
        public static List<string> GetHubTargetBusinessKeyListVirtual(string fullyQualifiedTableName, TeamConnection teamConnection, int versionId, TeamConfiguration configuration)
        {
            // Obtain the business key as it is known in the target Hub table. Can be multiple due to composite keys.

            var fullyQualifiedName = GetFullyQualifiedDataObjectName(fullyQualifiedTableName, teamConnection).FirstOrDefault();

            // The metadata connection can be used.
            var conn = new SqlConnection
            {
                ConnectionString = configuration.MetadataConnection.CreateSqlServerConnectionString(false)
            };

            try
            {
                conn.Open();
            }
            catch (Exception)
            {
                configuration.ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The connection to the metadata repository could not be established via {conn.ConnectionString}."));
            }

            var sqlStatementForBusinessKeys = new StringBuilder();

            var keyText = configuration.DwhKeyIdentifier;
            var localkeyLength = keyText.Length;
            var localkeySubstring = localkeyLength + 1;

            // Make sure brackets are removed
            var schemaName = fullyQualifiedName.Key?.Replace("[", "").Replace("]", "");
            var tableName = fullyQualifiedName.Value?.Replace("[", "").Replace("]", "");

            //Ignore version is not checked, so versioning is used - meaning the business key metadata is sourced from the version history metadata.
            sqlStatementForBusinessKeys.AppendLine("SELECT COLUMN_NAME");
            sqlStatementForBusinessKeys.AppendLine("FROM TMP_MD_VERSION_ATTRIBUTE");
            sqlStatementForBusinessKeys.AppendLine("WHERE SUBSTRING(COLUMN_NAME,LEN(COLUMN_NAME)-" + localkeyLength +
                                                   "," + localkeySubstring + ")!='_" +
                                                   configuration.DwhKeyIdentifier + "'");
            sqlStatementForBusinessKeys.AppendLine("  AND TABLE_NAME= '" + tableName + "'");
            sqlStatementForBusinessKeys.AppendLine("  AND SCHEMA_NAME= '" + schemaName + "'");
            sqlStatementForBusinessKeys.AppendLine("  AND COLUMN_NAME NOT IN ('" +
                                                   configuration.RecordSourceAttribute + "','" +
                                                   configuration.AlternativeRecordSourceAttribute +
                                                   "','" +
                                                   configuration.AlternativeLoadDateTimeAttribute +
                                                   "','" + configuration.AlternativeSatelliteLoadDateTimeAttribute + "','" +
                                                   configuration.EtlProcessAttribute + "','" +
                                                   configuration.LoadDateTimeAttribute + "')");
            sqlStatementForBusinessKeys.AppendLine("  AND VERSION_ID = " + versionId + "");

            var keyList = Utility.GetDataTable(ref conn, sqlStatementForBusinessKeys.ToString());

            if (keyList == null)
            {
                //SetTextDebug("An error has occurred defining the Hub Business Key in the model for " + hubTableName + ". The Business Key was not found when querying the underlying metadata. This can be either that the attribute is missing in the metadata or in the table (depending if versioning is used). If the 'ignore versioning' option is checked, then the metadata will be retrieved directly from the data dictionary. Otherwise the metadata needs to be available in the repository (manage model metadata).");
            }

            var businessKeyList = new List<string>();
            foreach (DataRow row in keyList.Rows)
            {
                if (!businessKeyList.Contains((string) row["COLUMN_NAME"]))
                {
                    businessKeyList.Add((string) row["COLUMN_NAME"]);
                }
            }

            return businessKeyList;
        }

        /// <summary>
        /// Returns a list of Business Key attributes as they are defined in the target Hub table (physical setup).
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="versionId"></param>
        /// <param name="queryMode"></param>
        /// <returns></returns>
        public static List<string> GetRegularTableBusinessKeyListPhysical(string fullyQualifiedTableName, TeamConnection teamConnection, string connectionstring, TeamConfiguration configuration)
        {
            // Obtain the business key as it is known in the target Hub table. Can be multiple due to composite keys.
            var fullyQualifiedName = MetadataHandling.GetFullyQualifiedDataObjectName(fullyQualifiedTableName, teamConnection).FirstOrDefault();

           // If the querymode is physical the real connection needs to be asserted based on the connection associated with the table.
            var conn = new SqlConnection
            {
                ConnectionString = connectionstring
            };

            try
            {
                conn.Open();
            }
            catch (Exception)
            {
                configuration.ConfigurationSettingsEventLog.Add(Event.CreateNewEvent(EventTypes.Error, $"The connection to the database for object {fullyQualifiedTableName} could not be established via {conn.ConnectionString}."));
            }

            var sqlStatementForBusinessKeys = new StringBuilder();

            var keyText = configuration.DwhKeyIdentifier;
            var localkeyLength = keyText.Length;
            var localkeySubstring = localkeyLength + 1;

            // Make sure brackets are removed
            var schemaName = fullyQualifiedName.Key?.Replace("[", "").Replace("]", "");
            var tableName = fullyQualifiedName.Value?.Replace("[", "").Replace("]", "");

            // Make sure the live database is hit when the checkbox is ticked
            sqlStatementForBusinessKeys.AppendLine("SELECT");
            sqlStatementForBusinessKeys.AppendLine("  SCHEMA_NAME(TAB.SCHEMA_ID) AS[SCHEMA_NAME],");
            sqlStatementForBusinessKeys.AppendLine("  PK.[NAME] AS PK_NAME,");
            sqlStatementForBusinessKeys.AppendLine("  IC.INDEX_COLUMN_ID AS COLUMN_ID,");
            sqlStatementForBusinessKeys.AppendLine("  COL.[NAME] AS COLUMN_NAME,");
            sqlStatementForBusinessKeys.AppendLine("  TAB.[NAME] AS TABLE_NAME");
            sqlStatementForBusinessKeys.AppendLine("FROM SYS.TABLES TAB");
            sqlStatementForBusinessKeys.AppendLine("INNER JOIN SYS.INDEXES PK");
            sqlStatementForBusinessKeys.AppendLine("  ON TAB.OBJECT_ID = PK.OBJECT_ID");
            sqlStatementForBusinessKeys.AppendLine(" AND PK.IS_PRIMARY_KEY = 1");
            sqlStatementForBusinessKeys.AppendLine("INNER JOIN SYS.INDEX_COLUMNS IC");
            sqlStatementForBusinessKeys.AppendLine("  ON IC.OBJECT_ID = PK.OBJECT_ID");
            sqlStatementForBusinessKeys.AppendLine(" AND IC.INDEX_ID = PK.INDEX_ID");
            sqlStatementForBusinessKeys.AppendLine("INNER JOIN SYS.COLUMNS COL");
            sqlStatementForBusinessKeys.AppendLine("  ON PK.OBJECT_ID = COL.OBJECT_ID");
            sqlStatementForBusinessKeys.AppendLine(" AND COL.COLUMN_ID = IC.COLUMN_ID");
            sqlStatementForBusinessKeys.AppendLine("WHERE");
            sqlStatementForBusinessKeys.AppendLine("      SCHEMA_NAME(TAB.SCHEMA_ID)= '" + schemaName + "'");
            sqlStatementForBusinessKeys.AppendLine("  AND TABLE_NAME= '" + tableName + "'");
            sqlStatementForBusinessKeys.AppendLine("ORDER BY");
            sqlStatementForBusinessKeys.AppendLine("  SCHEMA_NAME(TAB.SCHEMA_ID),");
            sqlStatementForBusinessKeys.AppendLine("  PK.[NAME],");
            sqlStatementForBusinessKeys.AppendLine("  IC.INDEX_COLUMN_ID");


            var tableKeyList = Utility.GetDataTable(ref conn, sqlStatementForBusinessKeys.ToString());

            if (tableKeyList == null)
            {
                //SetTextDebug("An error has occurred defining the Hub Business Key in the model for " + hubTableName + ". The Business Key was not found when querying the underlying metadata. This can be either that the attribute is missing in the metadata or in the table (depending if versioning is used). If the 'ignore versioning' option is checked, then the metadata will be retrieved directly from the data dictionary. Otherwise the metadata needs to be available in the repository (manage model metadata).");
            }

            var keyList = new List<string>();
            foreach (DataRow row in tableKeyList.Rows)
            {
                if (!keyList.Contains((string)row["COLUMN_NAME"]))
                {
                    keyList.Add((string)row["COLUMN_NAME"]);
                }
            }

            return keyList;
        }

        /// <summary>
        /// Returns a list of Business Key attributes as they are defined in the target Hub table (physical setup).
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="versionId"></param>
        /// <param name="queryMode"></param>
        /// <returns></returns>
        public static List<string> GetHubTargetBusinessKeyListPhysical(string fullyQualifiedTableName, TeamConnection teamConnection, TeamConfiguration configuration)
        {
            // Obtain the business key as it is known in the target Hub table. Can be multiple due to composite keys.

            var fullyQualifiedName = GetFullyQualifiedDataObjectName(fullyQualifiedTableName, teamConnection).FirstOrDefault();

            // If the querymode is physical the real connection needs to be asserted based on the connection associated with the table.
            var conn = new SqlConnection
            {
                ConnectionString = teamConnection.CreateSqlServerConnectionString(false)
            };

            try
            {
                conn.Open();
            }
            catch (Exception)
            {
                configuration.ConfigurationSettingsEventLog
                    .Add(Event.CreateNewEvent(EventTypes.Error,
                    $"The connection to the database for object {fullyQualifiedTableName} could not be established via {conn.ConnectionString}."));
            }

            var sqlStatementForBusinessKeys = new StringBuilder();

            var keyText = configuration.DwhKeyIdentifier;
            var localkeyLength = keyText.Length;
            var localkeySubstring = localkeyLength + 1;

            // Make sure brackets are removed
            var schemaName = fullyQualifiedName.Key?.Replace("[", "").Replace("]", "");
            var tableName = fullyQualifiedName.Value?.Replace("[", "").Replace("]", "");

            // Make sure the live database is hit when the checkbox is ticked
            sqlStatementForBusinessKeys.AppendLine("SELECT COLUMN_NAME");
            sqlStatementForBusinessKeys.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS");
            sqlStatementForBusinessKeys.AppendLine("WHERE SUBSTRING(COLUMN_NAME,LEN(COLUMN_NAME)-" + localkeyLength +
                                                   "," + localkeySubstring + ")!='_" +
                                                   configuration.DwhKeyIdentifier + "'");
            sqlStatementForBusinessKeys.AppendLine("AND TABLE_SCHEMA = '" + schemaName + "'");
            sqlStatementForBusinessKeys.AppendLine("  AND TABLE_NAME= '" + tableName + "'");
            sqlStatementForBusinessKeys.AppendLine("  AND COLUMN_NAME NOT IN ('" +
                                                   configuration.RecordSourceAttribute + "','" +
                                                   configuration.AlternativeRecordSourceAttribute +
                                                   "','" + configuration.AlternativeLoadDateTimeAttribute + "','" +
                                                   configuration.AlternativeSatelliteLoadDateTimeAttribute + "','" +
                                                   configuration.EtlProcessAttribute + "','" +
                                                   configuration.LoadDateTimeAttribute + "')");

            var keyList = Utility.GetDataTable(ref conn, sqlStatementForBusinessKeys.ToString());

            List<string> businessKeyList = new List<string>();
            if (keyList != null)
            {
                foreach (DataRow row in keyList.Rows)
                {
                    if (!businessKeyList.Contains((string) row["COLUMN_NAME"]))
                    {
                        businessKeyList.Add((string) row["COLUMN_NAME"]);
                    }
                }
            }

            return businessKeyList;
        }

        /// <summary>
        /// Returns a list of Business Key attributes as they are defined in the target Natural Business Relationship table.
        /// </summary>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="versionId"></param>
        /// <param name="queryMode"></param>
        /// <returns></returns>
        public static List<string> GetLinkTargetBusinessKeyList(string schemaName, string tableName, int versionId, string connectionString)
        {
            // Obtain the business key as it is known in the target Hub table. Can be multiple due to composite keys.

            var conn = new SqlConnection { ConnectionString = connectionString };

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
            sqlStatementForBusinessKeys.AppendLine("FROM [MD_HUB_LINK_XREF] xref");
            sqlStatementForBusinessKeys.AppendLine("JOIN [MD_HUB] hub ON xref.HUB_NAME = hub.HUB_NAME");
            sqlStatementForBusinessKeys.AppendLine($"WHERE [LINK_NAME] = '{schemaName}.{tableName}'");
            sqlStatementForBusinessKeys.AppendLine("ORDER BY [HUB_ORDER]");

            var keyList = Utility.GetDataTable(ref conn, sqlStatementForBusinessKeys.ToString());

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

        /// <summary>
        /// Add an additional quote to support adding hard-coded values.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static string QuoteStringValuesForAttributes(string attributeName)
        {
            if (attributeName != null)
            {
                if (attributeName.StartsWith("'") || attributeName.EndsWith("'"))
                {
                    attributeName = attributeName.Replace("'", "''");
                }
            }

            return attributeName;
        }
    }

    public class AttributeSelection
    {
        public StringBuilder CreatePhysicalModelSet(string databaseName, string filterObjects)
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
            returnValue.AppendLine("  CAST(COALESCE(A.[scale],0) AS VARCHAR(100)) AS[NUMERIC_SCALE], ");
            returnValue.AppendLine("  CAST(A.[column_id] AS VARCHAR(100)) AS [ORDINAL_POSITION],");
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
            returnValue.AppendLine("       D.name AS [SCHEMA_NAME]");
            returnValue.AppendLine("       C.name AS COLUMN_NAME");
            returnValue.AppendLine("     FROM [" + databaseName + "].sys.index_columns A");
            returnValue.AppendLine("     JOIN [" + databaseName + "].sys.indexes B");
            returnValue.AppendLine("     ON A.OBJECT_ID= B.OBJECT_ID AND A.index_id= B.index_id");
            returnValue.AppendLine("     JOIN [" + databaseName + "].sys.columns C");
            returnValue.AppendLine("     ON A.column_id= C.column_id AND A.OBJECT_ID= C.OBJECT_ID");
            returnValue.AppendLine("     JOIN [" + databaseName + "].sys.tables sc on sc.OBJECT_ID = A.OBJECT_ID");
            returnValue.AppendLine("     JOIN [" + databaseName + "].sys.schemas D ON sc.SCHEMA_ID = D.schema_id");
            returnValue.AppendLine("     WHERE is_primary_key = 1");
            returnValue.AppendLine(" ) keysub");
            returnValue.AppendLine("    ON OBJECT_NAME(A.OBJECT_ID, DB_ID('" + databaseName + "')) = keysub.[TABLE_NAME]");
            returnValue.AppendLine("   AND OBJECT_SCHEMA_NAME(OBJECT_ID, DB_ID('" + databaseName + "'))= keysub.[SCHEMA_NAME]");
            returnValue.AppendLine("   AND A.[name] = keysub.COLUMN_NAME");
            returnValue.AppendLine("    WHERE A.[OBJECT_ID] IN (" + filterObjects + ")");

            return returnValue;
        }
    }
}
