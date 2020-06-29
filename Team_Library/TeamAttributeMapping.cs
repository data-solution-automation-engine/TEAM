using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;


namespace TEAM
{
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
