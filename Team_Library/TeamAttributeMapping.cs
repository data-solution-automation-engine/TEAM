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




    }
}
