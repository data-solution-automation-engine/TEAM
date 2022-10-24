using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;

namespace TEAM_Library
{

    public class PhysicalModelMetadataJson
    {
        //JSON representation of the physical model metadata
        public string attributeHash { get; set; }
        public string databaseName { get; set; }
        public string schemaName { get; set; }
        public string tableName { get; set; }
        public string columnName { get; set; }
        public string dataType { get; set; }
        public string characterLength { get; set; }
        public string numericPrecision { get; set; }
        public string numericScale { get; set; }
        public string ordinalPosition { get; set; }
        public string primaryKeyIndicator { get; set; }
        public string multiActiveIndicator { get; set; }
    }

    /// <summary>
    /// Enumerator to hold the column index for the columns (headers) in the Physical Model Metadata data grid view.
    /// </summary>
    public enum PhysicalModelMappingMetadataColumns
    {
        Row_Checksum = 0,
        Database_Name = 1,
        Schema_Name = 2,
        Table_Name = 3,
        Column_Name = 4,
        Data_Type = 5,
        Character_Length = 6,
        Numeric_Precision = 7,
        Numeric_Scale = 8,
        Ordinal_Position = 9,
        Primary_Key_Indicator = 10,
        Multi_Active_Indicator = 11
    }

    /// <summary>
    /// The metadata collection object containing all dataObject to dataObject (source-to-target) mappings.
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
            DataTable.DefaultView.Sort = $"[{PhysicalModelMappingMetadataColumns.Database_Name}] ASC, [{PhysicalModelMappingMetadataColumns.Schema_Name}] ASC, [{PhysicalModelMappingMetadataColumns.Table_Name}] ASC, [{PhysicalModelMappingMetadataColumns.Ordinal_Position}] ASC";
        }

        /// <summary>
        /// Set the column names for the data table.
        /// </summary>
        public void SetDataTableColumns()
        {
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Row_Checksum].ColumnName = PhysicalModelMappingMetadataColumns.Row_Checksum.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Database_Name].ColumnName = PhysicalModelMappingMetadataColumns.Database_Name.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Schema_Name].ColumnName = PhysicalModelMappingMetadataColumns.Schema_Name.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Table_Name].ColumnName = PhysicalModelMappingMetadataColumns.Table_Name.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Column_Name].ColumnName = PhysicalModelMappingMetadataColumns.Column_Name.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Data_Type].ColumnName = PhysicalModelMappingMetadataColumns.Data_Type.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Character_Length].ColumnName = PhysicalModelMappingMetadataColumns.Character_Length.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Numeric_Precision].ColumnName = PhysicalModelMappingMetadataColumns.Numeric_Precision.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Numeric_Scale].ColumnName = PhysicalModelMappingMetadataColumns.Numeric_Scale.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Ordinal_Position].ColumnName = PhysicalModelMappingMetadataColumns.Ordinal_Position.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Primary_Key_Indicator].ColumnName = PhysicalModelMappingMetadataColumns.Primary_Key_Indicator.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.Multi_Active_Indicator].ColumnName = PhysicalModelMappingMetadataColumns.Multi_Active_Indicator.ToString();
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
    }
}
