using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;

namespace TEAM_Library
{
    public class DataItemMappingJson
    {
        //JSON representation of the attribute mapping metadata.
        public string attributeMappingHash { get; set; }
        public string sourceTable { get; set; }
        public string sourceAttribute { get; set; }
        public string targetTable { get; set; }
        public string targetAttribute { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string notes { get; set; }
    }
    /// <summary>
    /// Enumerator to hold the column index for the columns (headers) in the Table Metadata data grid view.
    /// </summary>
    public enum DataItemMappingGridColumns
    {
        HashKey = 0,
        SourceDataObject = 1,
        SourceDataItem = 2,
        TargetDataObject = 3,
        TargetDataItem = 4,
        Notes = 5
    }

    public class TeamDataItemMapping
    {
        public EventLog EventLog { get; set; }

        public List<DataItemMappingJson> JsonList { get; set; }

        public DataTable DataTable { get; set; }

        public TeamDataItemMapping()
        {
            EventLog = new EventLog();
            DataTable = new DataTable();
            JsonList = new List<DataItemMappingJson>();
        }

        /// <summary>
        /// Set the sort order for the data table.
        /// </summary>
        public void SetDataTableSorting()
        {
            DataTable.DefaultView.Sort = $"[{DataItemMappingGridColumns.TargetDataObject}] ASC, [{DataItemMappingGridColumns.TargetDataItem}] ASC, [{DataItemMappingGridColumns.TargetDataObject}] ASC, [{DataItemMappingGridColumns.SourceDataObject}] ASC";
        }

        /// <summary>
        /// Set the column names for the data table.
        /// </summary>
        public void SetDataTableColumns()
        {
            DataTable.Columns[(int)DataItemMappingGridColumns.HashKey].ColumnName = DataItemMappingGridColumns.HashKey.ToString();
            DataTable.Columns[(int)DataItemMappingGridColumns.SourceDataObject].ColumnName = DataItemMappingGridColumns.SourceDataObject.ToString();
            DataTable.Columns[(int)DataItemMappingGridColumns.SourceDataItem].ColumnName = DataItemMappingGridColumns.SourceDataItem.ToString();
            DataTable.Columns[(int)DataItemMappingGridColumns.TargetDataObject].ColumnName = DataItemMappingGridColumns.TargetDataObject.ToString();
            DataTable.Columns[(int)DataItemMappingGridColumns.TargetDataItem].ColumnName = DataItemMappingGridColumns.TargetDataItem.ToString();
            DataTable.Columns[(int)DataItemMappingGridColumns.Notes].ColumnName = DataItemMappingGridColumns.Notes.ToString();
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
                DataTable.Clear();
            }
            else
            {
                EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"Reading file {fileName}"));
                // Load the file, convert it to a DataTable and bind it to the source
                List<DataItemMappingJson> jsonArray = JsonConvert.DeserializeObject<List<DataItemMappingJson>>(File.ReadAllText(fileName));

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
                //SetDataTableSorting();
            }
        }
    }
}
