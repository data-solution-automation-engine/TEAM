using System.Data;

namespace TEAM
{
    /// <summary>
    /// Enumerator to hold the column index for the columns (headers) in the Table Metadata data grid view.
    /// </summary>
    public enum TableMetadataColumns
    {
        Enabled = 0,
        HashKey = 1,
        VersionId = 2,
        SourceTable = 3,
        SourceConnection = 4,
        TargetTable = 5,
        TargetConnection = 6,
        BusinessKeyDefinition = 7,
        DrivingKeyDefinition = 8,
        FilterCriterion = 9
    }

    /// <summary>
    /// Enumerator to hold the column index for the columns in the Table Metadata data table.
    /// </summary>
    //public enum TableMetadataColumns
    //{
    //    Enabled = 0,
    //    HashKey = 1,
    //    VersionId = 2,
    //    SourceTable = 3,
    //    SourceConnection = 4,
    //    TargetTable = 5,
    //    TargetConnection = 6,
    //    BusinessKeyDefinition = 7,
    //    DrivingKeyDefinition = 8,
    //    FilterCriterion = 9
    //}

    public class SetTeamDataTableProperties
    {
        public static void SetAttributeDataTableColumns(DataTable dataTable)
        {
            dataTable.Columns[0].ColumnName = "ATTRIBUTE_MAPPING_HASH";
            dataTable.Columns[1].ColumnName = "VERSION_ID";
            dataTable.Columns[2].ColumnName = "SOURCE_TABLE";
            dataTable.Columns[3].ColumnName = "SOURCE_COLUMN";
            dataTable.Columns[4].ColumnName = "TARGET_TABLE";
            dataTable.Columns[5].ColumnName = "TARGET_COLUMN";
            dataTable.Columns[6].ColumnName = "NOTES";
        }

        public static void SetAttributeDatTableSorting(DataTable dataTable)
        {
            dataTable.DefaultView.Sort = "[SOURCE_TABLE] ASC, [SOURCE_COLUMN] ASC, [TARGET_TABLE] ASC, [TARGET_COLUMN] ASC";
        }

        /// <summary>
        /// Set the column names for a data table according to the requirements for a Table Mapping datatable.
        /// </summary>
        /// <param name="dataTable"></param>
        public static void SetTableDataTableColumns(DataTable dataTable)
        {

            dataTable.Columns[(int)TableMetadataColumns.Enabled].ColumnName = TableMetadataColumns.Enabled.ToString();
            dataTable.Columns[(int)TableMetadataColumns.HashKey].ColumnName = TableMetadataColumns.HashKey.ToString();
            dataTable.Columns[(int)TableMetadataColumns.VersionId].ColumnName = TableMetadataColumns.VersionId.ToString();
            dataTable.Columns[(int)TableMetadataColumns.SourceTable].ColumnName = TableMetadataColumns.SourceTable.ToString();
            dataTable.Columns[(int)TableMetadataColumns.SourceConnection].ColumnName = TableMetadataColumns.SourceConnection.ToString();
            dataTable.Columns[(int)TableMetadataColumns.TargetTable].ColumnName = TableMetadataColumns.TargetTable.ToString();
            dataTable.Columns[(int)TableMetadataColumns.TargetConnection].ColumnName = TableMetadataColumns.TargetConnection.ToString();
            dataTable.Columns[(int)TableMetadataColumns.BusinessKeyDefinition].ColumnName = TableMetadataColumns.BusinessKeyDefinition.ToString();
            dataTable.Columns[(int)TableMetadataColumns.DrivingKeyDefinition].ColumnName = TableMetadataColumns.DrivingKeyDefinition.ToString();
            dataTable.Columns[(int)TableMetadataColumns.FilterCriterion].ColumnName = TableMetadataColumns.FilterCriterion.ToString();
        }

        /// <summary>
        /// Set the sort order for a data table according to the requirements for Table Mapping datatable.
        /// </summary>
        /// <param name="dataTable"></param>
        public static void SetTableDataTableSorting(DataTable dataTable)
        {
            dataTable.DefaultView.Sort = $"[{TableMetadataColumns.SourceTable.ToString()}] ASC, [{TableMetadataColumns.TargetTable.ToString()}] ASC, [{TableMetadataColumns.BusinessKeyDefinition.ToString()}] ASC";
        }

        /// <summary>
        /// Set the column names for a data table according to the requirements for a Physical Model datatable.
        /// </summary>
        /// <param name="dataTable"></param>
        public static void SetPhysicalModelDataTableColumns(DataTable dataTable)
        {
            dataTable.Columns[0].ColumnName = "VERSION_ATTRIBUTE_HASH";
            dataTable.Columns[1].ColumnName = "VERSION_ID";
            dataTable.Columns[2].ColumnName = "DATABASE_NAME";
            dataTable.Columns[3].ColumnName = "SCHEMA_NAME";
            dataTable.Columns[4].ColumnName = "TABLE_NAME";
            dataTable.Columns[5].ColumnName = "COLUMN_NAME";
            dataTable.Columns[6].ColumnName = "DATA_TYPE";
            dataTable.Columns[7].ColumnName = "CHARACTER_MAXIMUM_LENGTH";
            dataTable.Columns[8].ColumnName = "NUMERIC_PRECISION";
            dataTable.Columns[9].ColumnName = "ORDINAL_POSITION";
            dataTable.Columns[10].ColumnName = "PRIMARY_KEY_INDICATOR";
            dataTable.Columns[11].ColumnName = "MULTI_ACTIVE_INDICATOR";
        }

        /// <summary>
        /// Set the sort order for a data table according to the requirements for Physical Model datatable.
        /// </summary>
        /// <param name="dataTable"></param>
        public static void SetPhysicalModelDataTableSorting(DataTable dataTable)
        {
            dataTable.DefaultView.Sort = "[DATABASE_NAME] ASC, [SCHEMA_NAME] ASC, [TABLE_NAME] ASC, [ORDINAL_POSITION] ASC";
        }
    }
}
