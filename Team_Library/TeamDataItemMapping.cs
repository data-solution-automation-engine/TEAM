using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;

namespace TEAM_Library
{
    public class AttributeMappingJson
    {
        //JSON representation of the attribute mapping metadata
        public string attributeMappingHash { get; set; }
        public string sourceTable { get; set; }
        public string sourceAttribute { get; set; }
        public string targetTable { get; set; }
        public string targetAttribute { get; set; }
        public string notes { get; set; }
    }
    /// <summary>
    /// Enumerator to hold the column index for the columns (headers) in the Table Metadata data grid view.
    /// </summary>
    public enum DataItemMappingMetadataColumns
    {
        HashKey = 0,
        SourceTable = 1,
        SourceColumn = 2,
        TargetTable = 3,
        TargetColumn = 4,
        Notes = 5
    }

    public class TeamDataItemMapping
    {
        public EventLog EventLog { get; set; }

        public List<AttributeMappingJson> JsonList { get; set; }

        public DataTable DataTable { get; set; }

        public TeamDataItemMapping()
        {
            EventLog = new EventLog();
            DataTable = new DataTable();
            JsonList = new List<AttributeMappingJson>();
        }

        /// <summary>
        /// Set the sort order for the data table.
        /// </summary>
        public void SetDataTableSorting()
        {
            DataTable.DefaultView.Sort = $"[{DataItemMappingMetadataColumns.TargetTable}] ASC, [{DataItemMappingMetadataColumns.TargetColumn}] ASC, [{DataItemMappingMetadataColumns.TargetTable}] ASC, [{DataItemMappingMetadataColumns.SourceTable}] ASC";
        }

        /// <summary>
        /// Set the column names for the data table.
        /// </summary>
        public void SetDataTableColumns()
        {
            DataTable.Columns[(int)DataItemMappingMetadataColumns.HashKey].ColumnName = DataItemMappingMetadataColumns.HashKey.ToString();
            DataTable.Columns[(int)DataItemMappingMetadataColumns.SourceTable].ColumnName = DataItemMappingMetadataColumns.SourceTable.ToString();
            DataTable.Columns[(int)DataItemMappingMetadataColumns.SourceColumn].ColumnName = DataItemMappingMetadataColumns.SourceColumn.ToString();
            DataTable.Columns[(int)DataItemMappingMetadataColumns.TargetTable].ColumnName = DataItemMappingMetadataColumns.TargetTable.ToString();
            DataTable.Columns[(int)DataItemMappingMetadataColumns.TargetColumn].ColumnName = DataItemMappingMetadataColumns.TargetColumn.ToString();
            DataTable.Columns[(int)DataItemMappingMetadataColumns.Notes].ColumnName = DataItemMappingMetadataColumns.Notes.ToString();
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
                List<AttributeMappingJson> jsonArray = JsonConvert.DeserializeObject<List<AttributeMappingJson>>(File.ReadAllText(fileName));

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
    }
}
