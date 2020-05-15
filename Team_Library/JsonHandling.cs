using System;
using System.Data;
using System.IO;
using System.Text;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TEAM
{
    public class TableMappingJson
    {
        //JSON representation of the table mapping metadata
        public string tableMappingHash { get; set; }
        public string versionId { get; set; }
        public string sourceTable { get; set; }
        public string targetTable { get; set; }
        public string businessKeyDefinition { get; set; }
        public string drivingKeyDefinition { get; set; }
        public string filterCriteria { get; set; }
        public string processIndicator { get; set; }
    }

    public class AttributeMappingJson
    {
        //JSON representation of the attribute mapping metadata
        public string attributeMappingHash { get; set; }
        public string versionId { get; set; }
        public string sourceTable { get; set; }
        public string sourceAttribute { get; set; }
        public string targetTable { get; set; }
        public string targetAttribute { get; set; }
        public string transformationRule { get; set; }
    }

    public class PhysicalModelMetadataJson
    {
        //JSON representation of the physical model metadata
        public string versionAttributeHash { get; set; }
        public string versionId { get; set; }
        public string databaseName { get; set; }
        public string schemaName { get; set; }
        public string tableName { get; set; }
        public string columnName { get; set; }
        public string dataType { get; set; }
        public string characterMaximumLength { get; set; }
        public string numericPrecision { get; set; }
        public string ordinalPosition { get; set; }
        public string primaryKeyIndicator { get; set; }
        public string multiActiveIndicator { get; set; }
    }

    public class SetTeamDataTableMapping
    {
        public static void SetAttributeDataTableColumns(DataTable dataTable)
        {
            dataTable.Columns[0].ColumnName = "ATTRIBUTE_MAPPING_HASH";
            dataTable.Columns[1].ColumnName = "VERSION_ID";
            dataTable.Columns[2].ColumnName = "SOURCE_TABLE";
            dataTable.Columns[3].ColumnName = "SOURCE_COLUMN";
            dataTable.Columns[4].ColumnName = "TARGET_TABLE";
            dataTable.Columns[5].ColumnName = "TARGET_COLUMN";
            dataTable.Columns[6].ColumnName = "TRANSFORMATION_RULE";
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
            dataTable.Columns[0].ColumnName = "TABLE_MAPPING_HASH";
            dataTable.Columns[1].ColumnName = "VERSION_ID";
            dataTable.Columns[2].ColumnName = "SOURCE_TABLE";
            dataTable.Columns[3].ColumnName = "TARGET_TABLE";
            dataTable.Columns[4].ColumnName = "BUSINESS_KEY_ATTRIBUTE";
            dataTable.Columns[5].ColumnName = "DRIVING_KEY_ATTRIBUTE";
            dataTable.Columns[6].ColumnName = "FILTER_CRITERIA";
            dataTable.Columns[7].ColumnName = "PROCESS_INDICATOR";
        }

        /// <summary>
        /// Set the sort order for a data table according to the requirements for Table Mapping datatable.
        /// </summary>
        /// <param name="dataTable"></param>
        public static void SetTableDataTableSorting(DataTable dataTable)
        {
            dataTable.DefaultView.Sort = "[SOURCE_TABLE] ASC, [TARGET_TABLE] ASC, [BUSINESS_KEY_ATTRIBUTE] ASC";
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
