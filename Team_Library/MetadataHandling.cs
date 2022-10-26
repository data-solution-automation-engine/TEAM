using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using DataWarehouseAutomation;
using DataObject = DataWarehouseAutomation.DataObject;

namespace TEAM_Library
{
    public class MetadataHandling
    {
        /// <summary>
        /// Definition of the allowed table types. These are used everywhere to derive approach based on conventions.
        /// </summary>
        public enum DataObjectTypes
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

        public static DataTable GetPhysicalModelDataTable(SqlConnection conn)
        {
            // Retrieve the physical model.
            // Add data type details, if requested.
            var physicalModelQuery = @"SELECT
                                             [DATABASE_NAME]
                                            ,[SCHEMA_NAME]
                                            ,[TABLE_NAME]
                                            ,[COLUMN_NAME]
                                            ,[DATA_TYPE]
                                            ,[CHARACTER_MAXIMUM_LENGTH]
                                            ,[NUMERIC_PRECISION]
                                            ,[NUMERIC_SCALE]
                                            ,[ORDINAL_POSITION]
                                            ,[PRIMARY_KEY_INDICATOR]
                                       FROM [interface].[INTERFACE_PHYSICAL_MODEL]";

            var physicalModelDataTable = Utility.GetDataTable(ref conn, physicalModelQuery);
            return physicalModelDataTable;
        }

        public static void GetFullSourceDataItemPresentationOld(string dataObjectName, TeamConnection teamConnection, DataTable physicalModelDataTable, DataRow column, DataItem dataItem, string sourceOrTarget = "Regular")
        {
            var tableSchema = GetFullyQualifiedDataObjectName(dataObjectName, teamConnection);

            var columnNameFilter = "";
            if (sourceOrTarget == "Source")
            {
                columnNameFilter = QuoteStringValuesForAttributes((string)column[3]);
            }
            else if (sourceOrTarget == "Target")
            {
                columnNameFilter = QuoteStringValuesForAttributes((string)column[5]);
            }

            DataRow physicalModelRow = physicalModelDataTable.Select($"[TABLE_NAME] = '" + tableSchema.Values.FirstOrDefault() + "' " +
                                                                     "AND [SCHEMA_NAME] = '" + tableSchema.Keys.FirstOrDefault() + "' " +
                                                                     "AND [COLUMN_NAME] = '" + columnNameFilter + "' " +
                                                                     "AND [DATABASE_NAME] = '" + teamConnection.DatabaseServer.DatabaseName + "'" +
                                                                     "").FirstOrDefault();

            PrepareDataItemDataType(dataItem, physicalModelRow);

        }




        public static void GetFullSourceDataItem(DataObject dataObject, TeamConnection teamConnection, DataTable physicalModelDataTable, DataRow column, DataItem dataItem, bool AddParentDataObject, string sourceOrTarget = "Regular")
        {
            var tableSchema = GetFullyQualifiedDataObjectName(dataObject.name, teamConnection);

            var columnNameFilter = "";
            if (sourceOrTarget == "Source")
            {
                columnNameFilter = QuoteStringValuesForAttributes((string)column[$"SOURCE_ATTRIBUTE_NAME"]);
            }
            else if (sourceOrTarget == "Target")
            {
                columnNameFilter = QuoteStringValuesForAttributes((string)column[$"TARGET_ATTRIBUTE_NAME"]);
            }

            DataRow physicalModelRow = physicalModelDataTable.Select($"[TABLE_NAME] = '" +tableSchema.Values.FirstOrDefault() + "' " +
                                                                     "AND [SCHEMA_NAME] = '" + tableSchema.Keys.FirstOrDefault() + "' " +
                                                                     "AND [COLUMN_NAME] = '" + columnNameFilter + "' " +
                                                                     "AND [DATABASE_NAME] = '" + teamConnection.DatabaseServer.DatabaseName + "'" +
                                                                     "").FirstOrDefault();
            PrepareDataItemDataType(dataItem, physicalModelRow);

            // Add the Data Object, if required.
            if (AddParentDataObject)
            {
                dataItem.dataObject = dataObject;
            }
        }
        
        public static void PrepareDataItemDataType(DataItem dataItem, DataRow physicalModelRow)
        {
            if (physicalModelRow != null)
            {
                var dataType = physicalModelRow.ItemArray[4].ToString();
                dataItem.dataType = dataType;

                switch (dataType)
                {
                    case "varchar":
                    case "nvarchar":
                    case "binary":
                        dataItem.characterLength = (int) physicalModelRow.ItemArray[5];
                        break;
                    case "numeric":
                        dataItem.numericPrecision = (int) physicalModelRow.ItemArray[6];
                        dataItem.numericScale = (int) physicalModelRow.ItemArray[7];
                        break;
                    case "int":
                        // No length etc.
                        break;
                    case "datetime":
                    case "datetime2":
                    case "date":
                        dataItem.numericScale = (int) physicalModelRow.ItemArray[7];
                        break;
                }

                dataItem.ordinalPosition = (int) physicalModelRow.ItemArray[8];
            }
        }

        /// <summary>
        /// Return the Surrogate Key for a given table using the TEAM settings (i.e. prefix/suffix settings etc.).
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="teamConnection"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns>surrogateKey</returns>
        public static string GetSurrogateKey(string dataObjectName, TeamConnection teamConnection, TeamConfiguration teamConfiguration)
        {
            // Get the fully qualified name
            KeyValuePair<string, string> fullyQualifiedName = GetFullyQualifiedDataObjectName(dataObjectName, teamConnection).FirstOrDefault();
            
            // Initialise the return value
            string surrogateKey = "";
            string newDataObjectName = fullyQualifiedName.Value;
            string keyLocation = teamConfiguration.DwhKeyIdentifier;

            string[] prefixSuffixArray = {
                teamConfiguration.HubTablePrefixValue,
                teamConfiguration.SatTablePrefixValue,
                teamConfiguration.LinkTablePrefixValue,
                teamConfiguration.LsatTablePrefixValue
            };

            if (newDataObjectName != "Not applicable")
            {
                // Removing the table pre- or suffixes from the table name based on the TEAM configuration settings.
                if (teamConfiguration.TableNamingLocation == "Prefix")
                {
                    foreach (string prefixValue in prefixSuffixArray)
                    {
                        if (newDataObjectName.StartsWith(prefixValue))
                        {
                            newDataObjectName = newDataObjectName.Replace(prefixValue, "");
                        }
                    }
                }
                else
                {
                    foreach (string suffixValue in prefixSuffixArray)
                    {
                        if (newDataObjectName.EndsWith(suffixValue))
                        {
                            newDataObjectName = newDataObjectName.Replace(suffixValue, "");
                        }
                    }
                }

                // Define the surrogate key using the table name and key prefix/suffix settings.
                if (teamConfiguration.KeyNamingLocation == "Prefix")
                {
                    surrogateKey = keyLocation + newDataObjectName;
                }
                else
                {
                    surrogateKey = newDataObjectName + keyLocation;
                }
            }

            return surrogateKey;
        }
        
        /// <summary>
        /// This method returns the type (classification) of Data Object as an TableTypes enumerator based on the name and active conventions.
        /// Requires fully qualified name, or at least ignores schemas in the name.
        /// </summary>
        /// <param name="dataObjectName"></param>
        /// <param name="additionalInformation"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static DataObjectTypes GetDataObjectType(string dataObjectName, string additionalInformation, TeamConfiguration configuration)
        {
            DataObjectTypes localType;

            var presentationLayerLabelArray = Utility.SplitLabelIntoArray(configuration.PresentationLayerLabels);
            var transformationLabelArray = Utility.SplitLabelIntoArray(configuration.TransformationLabels);

            // Remove schema, if one is set
            dataObjectName = GetNonQualifiedTableName(dataObjectName);

            switch (configuration.TableNamingLocation)
            {
                // I.e. HUB_CUSTOMER
                case "Prefix" when dataObjectName.StartsWith(configuration.SatTablePrefixValue):
                    localType = DataObjectTypes.Context;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.HubTablePrefixValue):
                    localType = DataObjectTypes.CoreBusinessConcept;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.LinkTablePrefixValue):
                    localType = DataObjectTypes.NaturalBusinessRelationship;
                    break;
                case "Prefix" when (dataObjectName.StartsWith(configuration.LsatTablePrefixValue) && additionalInformation==""):
                    localType = DataObjectTypes.NaturalBusinessRelationshipContext;
                    break;
                case "Prefix" when (dataObjectName.StartsWith(configuration.LsatTablePrefixValue) && additionalInformation != ""):
                    localType = DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.StgTablePrefixValue):
                    localType = DataObjectTypes.StagingArea;
                    break;
                case "Prefix" when dataObjectName.StartsWith(configuration.PsaTablePrefixValue):
                    localType = DataObjectTypes.PersistentStagingArea;
                    break;
                // Presentation Layer
                case "Prefix" when presentationLayerLabelArray.Any(s => dataObjectName.StartsWith(s)):
                    localType = DataObjectTypes.Presentation;
                    break;
                // Derived or transformation
                case "Prefix" when transformationLabelArray.Any(s => dataObjectName.StartsWith(s)):
                    localType = DataObjectTypes.Derived;
                    break;
                // Source
                case "Prefix":
                    localType = DataObjectTypes.Source;
                    break;
                // Suffix
                // I.e. CUSTOMER_HUB
                case "Suffix" when dataObjectName.EndsWith(configuration.SatTablePrefixValue):
                    localType = DataObjectTypes.Context;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.HubTablePrefixValue):
                    localType = DataObjectTypes.CoreBusinessConcept;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.LinkTablePrefixValue):
                    localType = DataObjectTypes.NaturalBusinessRelationship;
                    break;
                case "Suffix" when (dataObjectName.EndsWith(configuration.LsatTablePrefixValue) && additionalInformation == ""):
                    localType = DataObjectTypes.NaturalBusinessRelationshipContext;
                    break;
                case "Suffix" when (dataObjectName.EndsWith(configuration.LsatTablePrefixValue) && additionalInformation != ""):
                    localType = DataObjectTypes.NaturalBusinessRelationshipContextDrivingKey;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.StgTablePrefixValue):
                    localType = DataObjectTypes.StagingArea;
                    break;
                case "Suffix" when dataObjectName.EndsWith(configuration.PsaTablePrefixValue):
                    localType = DataObjectTypes.PersistentStagingArea;
                    break;
                // Presentation Layer
                case "Suffix" when presentationLayerLabelArray.Any(s => dataObjectName.EndsWith(s)):
                    localType = DataObjectTypes.Presentation;
                    break;
                // Transformation / derived
                case "Suffix" when transformationLabelArray.Any(s => dataObjectName.EndsWith(s)):
                    localType = DataObjectTypes.Derived;
                    break;
                case "Suffix":
                    localType = DataObjectTypes.Source;
                    break;
                default:
                    localType = DataObjectTypes.Unknown;
                    break;
            }
            // Return the table type
            return localType;
        }

        /// <summary>
        /// This method returns the ETL loading 'direction' based on the source and target mapping.
        /// </summary>
        /// <param name="sourceDataObjectName"></param>
        /// <param name="TargetDataObject"></param>
        /// <param name="teamConfiguration"></param>
        /// <returns></returns>
        public static string GetDataObjectMappingLoadVector(string sourceDataObjectName, string TargetDataObject, TeamConfiguration teamConfiguration)
        {
            // This is used to evaluate the correct connection for the generated ETL processes.

            DataObjectTypes evaluatedSource = GetDataObjectType(sourceDataObjectName, "", teamConfiguration);
            DataObjectTypes evaluatedTarget = GetDataObjectType(TargetDataObject, "", teamConfiguration);

            string loadVector = "";

            if (evaluatedSource == DataObjectTypes.StagingArea && evaluatedTarget == DataObjectTypes.PersistentStagingArea)
            {
                loadVector = "Landing to Persistent Staging Area";
            }
            // If the source is not a DWH table, but the target is a DWH table then it's a base ('Raw') Data Warehouse ETL (load vector). - 'Staging Layer to Raw Data Warehouse'.
            else if (!new[] {DataObjectTypes.CoreBusinessConcept, DataObjectTypes.Context, DataObjectTypes.NaturalBusinessRelationship, DataObjectTypes.NaturalBusinessRelationshipContext, DataObjectTypes.Derived }.Contains(evaluatedSource) && new[] { DataObjectTypes.CoreBusinessConcept, DataObjectTypes.Context, DataObjectTypes.NaturalBusinessRelationship, DataObjectTypes.NaturalBusinessRelationshipContext }.Contains(evaluatedTarget))
            {
                loadVector = "Staging Layer to Raw Data Warehouse";
            }
            // If the source is a DWH or Derived table, and the target is a DWH table then it's a Derived ('Business') DWH ETL - 'Raw Data Warehouse to Interpreted'.
            else if (new[] { DataObjectTypes.CoreBusinessConcept, DataObjectTypes.Context, DataObjectTypes.NaturalBusinessRelationship, DataObjectTypes.NaturalBusinessRelationshipContext, DataObjectTypes.Derived }.Contains(evaluatedSource) && new[] { DataObjectTypes.CoreBusinessConcept, DataObjectTypes.Context, DataObjectTypes.NaturalBusinessRelationship, DataObjectTypes.NaturalBusinessRelationshipContext }.Contains(evaluatedTarget))
            {
                loadVector = "Raw Data Warehouse to Interpreted";
            }
            // If the source is a DWH table, but target is not a DWH table then it's a Presentation Layer ETL. - 'Data Warehouse to Presentation Layer'.
            else if 
            (new[] { DataObjectTypes.CoreBusinessConcept, DataObjectTypes.Context, DataObjectTypes.NaturalBusinessRelationship, DataObjectTypes.NaturalBusinessRelationshipContext }.Contains(evaluatedSource) 
                     && 
                     new[] { DataObjectTypes.Presentation }.Contains(evaluatedTarget)
            )
            {
                loadVector = "Data Warehouse to Presentation Layer";
            }
            else if (evaluatedSource == DataObjectTypes.Source && new[] { DataObjectTypes.StagingArea, DataObjectTypes.PersistentStagingArea}.Contains(evaluatedTarget))
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
        /// <returns></returns>
        public static List<string> GetHubTargetBusinessKeyListVirtual(string fullyQualifiedTableName, TeamConnection teamConnection, TeamConfiguration configuration)
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

            // Make sure brackets are removed.
            var schemaName = fullyQualifiedName.Key?.Replace("[", "").Replace("]", "");
            var tableName = fullyQualifiedName.Value?.Replace("[", "").Replace("]", "");

            // The business key metadata is sourced from the version history metadata.
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
            sqlStatementForBusinessKeys.AppendLine("  AND VERSION_ID = 0");

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
        /// <returns></returns>
        public static List<string> GetHubTargetBusinessKeyListPhysical(string fullyQualifiedTableName, TeamConnection teamConnection, TeamConfiguration configuration)
        {
            // Obtain the business key as it is known in the target Hub table. Can be multiple due to composite keys.

            var fullyQualifiedName = GetFullyQualifiedDataObjectName(fullyQualifiedTableName, teamConnection).FirstOrDefault();

            // If the query mode is physical the real connection needs to be asserted based on the connection associated with the table.
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
                    .Add(Event.CreateNewEvent(EventTypes.Error, $"The connection to the database for object {fullyQualifiedTableName} could not be established via {conn.ConnectionString}."));
            }

            var sqlStatementForBusinessKeys = new StringBuilder();

            var keyText = configuration.DwhKeyIdentifier;
            var localkeyLength = keyText.Length;
            var localkeySubstring = localkeyLength - 1;

            // Make sure brackets are removed
            var schemaName = fullyQualifiedName.Key?.Replace("[", "").Replace("]", "");
            var tableName = fullyQualifiedName.Value?.Replace("[", "").Replace("]", "");

            // Make sure the live database is hit when the checkbox is ticked
            sqlStatementForBusinessKeys.AppendLine("SELECT COLUMN_NAME");
            sqlStatementForBusinessKeys.AppendLine("FROM INFORMATION_SCHEMA.COLUMNS");

            if (configuration.KeyNamingLocation == "Prefix")
            {
                sqlStatementForBusinessKeys.AppendLine($"WHERE SUBSTRING(COLUMN_NAME,1,{localkeyLength})!='{configuration.DwhKeyIdentifier}'");
            }
            else
            {
                sqlStatementForBusinessKeys.AppendLine($"WHERE SUBSTRING(COLUMN_NAME,LEN(COLUMN_NAME)-{localkeySubstring},{localkeyLength})!='{configuration.DwhKeyIdentifier}'");
            }
            

            sqlStatementForBusinessKeys.AppendLine("AND TABLE_SCHEMA = '" + schemaName + "'");
            sqlStatementForBusinessKeys.AppendLine("  AND TABLE_NAME= '" + tableName + "'");
            sqlStatementForBusinessKeys.AppendLine("  AND COLUMN_NAME NOT IN ('" +
                                                   configuration.RecordSourceAttribute + "','" +
                                                   configuration.AlternativeRecordSourceAttribute + "','" + 
                                                   configuration.AlternativeLoadDateTimeAttribute + "','" +
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
        /// <returns></returns>
        public static List<string> GetLinkTargetBusinessKeyList(string schemaName, string tableName, string connectionString)
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

            var businessKeyList = new List<string>();
            foreach (DataRow row in keyList.Rows)
            {
                businessKeyList.Add((string)row["BUSINESS_KEY"]);
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

            returnValue.AppendLine("SELECT ");
            returnValue.AppendLine(" [DATABASE_NAME] ");
            returnValue.AppendLine(",[SCHEMA_NAME]");
            returnValue.AppendLine(",[TABLE_NAME]");
            returnValue.AppendLine(",[COLUMN_NAME]");
            returnValue.AppendLine(",[DATA_TYPE]");
            returnValue.AppendLine(",[CHARACTER_MAXIMUM_LENGTH]");
            returnValue.AppendLine(",[NUMERIC_PRECISION]");
            returnValue.AppendLine(",[NUMERIC_SCALE]");
            returnValue.AppendLine(",[ORDINAL_POSITION]");
            returnValue.AppendLine(",[PRIMARY_KEY_INDICATOR]");
            returnValue.AppendLine("FROM");
            returnValue.AppendLine("(");

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
            returnValue.AppendLine("       D.name AS [SCHEMA_NAME],");
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
            returnValue.AppendLine("   AND OBJECT_SCHEMA_NAME(OBJECT_ID, DB_ID('" + databaseName + "')) = keysub.[SCHEMA_NAME]");
            returnValue.AppendLine("   AND A.[name] = keysub.COLUMN_NAME");
            returnValue.AppendLine("    WHERE A.[OBJECT_ID] IN (" + filterObjects + ")");
            
            returnValue.AppendLine(") sub");
            
            return returnValue;
        }
    }
}
