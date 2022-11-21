using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace TEAM_Library
{
    /// <summary>
    /// JSON representation of the physical model metadata.
    /// </summary>
    public class PhysicalModelMetadataJson
    {
        public string databaseName { get; set; }
        public string schemaName { get; set; }
        public string tableName { get; set; }
        public string columnName { get; set; }
        public string dataType { get; set; }
        public string characterLength { get; set; }
        public string numericPrecision { get; set; }
        public string numericScale { get; set; }
        public int ordinalPosition { get; set; }
        public string primaryKeyIndicator { get; set; }
        public string multiActiveIndicator { get; set; }
    }

    /// <summary>
    /// Enum to hold the column index for the columns (headers) in the Physical Model Metadata data grid view.
    /// </summary>
    public enum PhysicalModelMappingMetadataColumns
    {
        databaseName = 0,
        schemaName = 1,
        tableName = 2,
        columnName = 3,
        dataType = 4,
        characterLength = 5,
        numericPrecision = 6,
        numericScale = 7,
        ordinalPosition = 8,
        primaryKeyIndicator = 9,
        multiActiveIndicator = 10
    }

    /// <summary>
    /// The metadata collection containing the physical model snapshot.
    /// </summary>
    public class TeamPhysicalModel
    {
        public EventLog EventLog { get; set; }

        public List<PhysicalModelMetadataJson> JsonList { get; set; }

        public DataTable DataTable { get; set; }

        public TeamPhysicalModel()
        {
            EventLog = new EventLog();
            DataTable = new DataTable();
            JsonList = new List<PhysicalModelMetadataJson>();
        }

        /// <summary>
        /// Set the sort order for the data table.
        /// </summary>
        public void SetDataTableSorting()
        {
            DataTable.DefaultView.Sort = $"[{PhysicalModelMappingMetadataColumns.databaseName}] ASC, [{PhysicalModelMappingMetadataColumns.schemaName}] ASC, [{PhysicalModelMappingMetadataColumns.tableName}] ASC, [{PhysicalModelMappingMetadataColumns.ordinalPosition}] ASC";
        }

        /// <summary>
        /// Set the column names for the data table.
        /// </summary>
        public void SetDataTableColumns()
        {
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.databaseName].ColumnName = PhysicalModelMappingMetadataColumns.databaseName.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.schemaName].ColumnName = PhysicalModelMappingMetadataColumns.schemaName.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.tableName].ColumnName = PhysicalModelMappingMetadataColumns.tableName.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.columnName].ColumnName = PhysicalModelMappingMetadataColumns.columnName.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.dataType].ColumnName = PhysicalModelMappingMetadataColumns.dataType.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.characterLength].ColumnName = PhysicalModelMappingMetadataColumns.characterLength.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.numericPrecision].ColumnName = PhysicalModelMappingMetadataColumns.numericPrecision.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.numericScale].ColumnName = PhysicalModelMappingMetadataColumns.numericScale.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.ordinalPosition].ColumnName = PhysicalModelMappingMetadataColumns.ordinalPosition.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.primaryKeyIndicator].ColumnName = PhysicalModelMappingMetadataColumns.primaryKeyIndicator.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.multiActiveIndicator].ColumnName = PhysicalModelMappingMetadataColumns.multiActiveIndicator.ToString();
        }

        /// <summary>
        /// Creates a  object (Json List and DataTable) from a Json file.
        /// </summary>
        /// <returns></returns>
        public void GetMetadata(string fileName)
        {
            EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"Retrieving metadata from {fileName}."));

            // Check if the file exists
            if (!File.Exists(fileName))
            {
                EventLog.Add(Event.CreateNewEvent(EventTypes.Warning, "No Json Table Mapping file was found."));
            }
            else
            {
                EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"Reading file {fileName}"));
                // Load the file, convert it to a DataTable and bind it to the source
                List<PhysicalModelMetadataJson> jsonArray = JsonConvert.DeserializeObject<List<PhysicalModelMetadataJson>>(File.ReadAllText(fileName));

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

        public static string PhysicalModelQuery(string databaseName, string filterObjects)
        {
            var returnValue = new StringBuilder();

            returnValue.AppendLine("SELECT ");
            returnValue.AppendLine($" [{PhysicalModelMappingMetadataColumns.databaseName}] ");
            returnValue.AppendLine($",[{PhysicalModelMappingMetadataColumns.schemaName}]");
            returnValue.AppendLine($",[{PhysicalModelMappingMetadataColumns.tableName}]");
            returnValue.AppendLine($",[{PhysicalModelMappingMetadataColumns.columnName}]");
            returnValue.AppendLine($",[{PhysicalModelMappingMetadataColumns.dataType}]");
            returnValue.AppendLine($",[{PhysicalModelMappingMetadataColumns.characterLength}]");
            returnValue.AppendLine($",[{PhysicalModelMappingMetadataColumns.numericPrecision}]");
            returnValue.AppendLine($",[{PhysicalModelMappingMetadataColumns.numericScale}]");
            returnValue.AppendLine($",[{PhysicalModelMappingMetadataColumns.ordinalPosition}]");
            returnValue.AppendLine($",[{PhysicalModelMappingMetadataColumns.primaryKeyIndicator}]");
            returnValue.AppendLine("FROM");
            returnValue.AppendLine("(");

            returnValue.AppendLine("SELECT");
            returnValue.AppendLine($"  DB_NAME(DB_ID('{databaseName}')) AS [{PhysicalModelMappingMetadataColumns.columnName}],");
            returnValue.AppendLine($"  OBJECT_SCHEMA_NAME(OBJECT_ID, DB_ID('{databaseName}')) AS [{PhysicalModelMappingMetadataColumns.schemaName}],");
            returnValue.AppendLine($"  OBJECT_NAME(A.OBJECT_ID, DB_ID('{databaseName}')) AS [{PhysicalModelMappingMetadataColumns.tableName}],");
            returnValue.AppendLine("  A.OBJECT_ID,");
            returnValue.AppendLine($"  A.[name] AS [{PhysicalModelMappingMetadataColumns.columnName}],");
            returnValue.AppendLine($"  t.[name] AS [{PhysicalModelMappingMetadataColumns.dataType}], ");
            returnValue.AppendLine("  CAST(COALESCE(");
            returnValue.AppendLine("    CASE WHEN UPPER(t.[name]) = 'NVARCHAR' THEN A.[max_length]/2 ");
            returnValue.AppendLine("    ELSE A.[max_length]");
            returnValue.AppendLine("    END");
            returnValue.AppendLine($"   ,0) AS VARCHAR(100)) AS [{PhysicalModelMappingMetadataColumns.characterLength}],");
            returnValue.AppendLine($"  CAST(COALESCE(A.[precision],0) AS VARCHAR(100)) AS [{PhysicalModelMappingMetadataColumns.numericPrecision}], ");
            returnValue.AppendLine($"  CAST(COALESCE(A.[scale],0) AS VARCHAR(100)) AS [{PhysicalModelMappingMetadataColumns.numericScale}], ");
            returnValue.AppendLine($"  CAST(A.[column_id] AS VARCHAR(100)) AS [{PhysicalModelMappingMetadataColumns.ordinalPosition}],");
            returnValue.AppendLine("  CASE");
            returnValue.AppendLine("    WHEN keysub.COLUMN_NAME IS NULL");
            returnValue.AppendLine("    THEN 'N' ");
            returnValue.AppendLine("    ELSE 'Y' ");
            returnValue.AppendLine($"  END AS [{PhysicalModelMappingMetadataColumns.primaryKeyIndicator}]");
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

            return returnValue.ToString();
        }
    }
}
