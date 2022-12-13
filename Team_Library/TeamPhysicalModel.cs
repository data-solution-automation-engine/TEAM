using System.Collections.Generic;
using System.Data;
using System.IO;
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

        public static void CreateEmptyPhysicalModelJson(string fileName, EventLog eventLog)
        {
            File.WriteAllText(fileName, "[]");
            eventLog.Add(Event.CreateNewEvent(EventTypes.Information, "A new physical model file is created, because it did not exist yet."));
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
                DataTable.Clear();
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
                //SetDataTableSorting();
            }
        }
    }
}
