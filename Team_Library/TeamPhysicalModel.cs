using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TEAM_Library
{
    /// <summary>
    /// JSON representation of (a row in) the physical model metadata.
    /// </summary>
    public class PhysicalModelGridRow
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
    /// An object in a database or file system, uniquely identified by database/instance and folder/schema.
    /// </summary>
    public class PhysicalModelObject
    {

    }

    public class PhysicalModelDatabase
    {
        public string name { get; set; }
        
        public List<PhysicalModelSchema> schemas { get; set; }
    }

    public class PhysicalModelSchema
    {
        public string name { get; set; }

        public List<PhysicalModelTable> tables { get; set; }

    }

    public class PhysicalModelTable
    {
        public string name { get; set; }

        public string database { get; set; }

        public string schema { get; set; }

        public List<PhysicalModelColumn> columns { get; set; }

        public PhysicalModelTable()
        {
            columns = new List<PhysicalModelColumn>();
        }
    }

    public class PhysicalModelColumn
    {
        public string name { get; set; }
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

        public List<PhysicalModelGridRow> JsonList { get; set; }

        public DataTable DataTable { get; set; }

        public TeamPhysicalModel()
        {
            EventLog = new EventLog();
            DataTable = new DataTable();
            JsonList = new List<PhysicalModelGridRow>();
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
        public static void SetDataTableColumnNames(DataTable dataTable)
        {
            dataTable.Columns[(int)PhysicalModelMappingMetadataColumns.databaseName].ColumnName = PhysicalModelMappingMetadataColumns.databaseName.ToString();
            dataTable.Columns[(int)PhysicalModelMappingMetadataColumns.schemaName].ColumnName = PhysicalModelMappingMetadataColumns.schemaName.ToString();
            dataTable.Columns[(int)PhysicalModelMappingMetadataColumns.tableName].ColumnName = PhysicalModelMappingMetadataColumns.tableName.ToString();
            dataTable.Columns[(int)PhysicalModelMappingMetadataColumns.columnName].ColumnName = PhysicalModelMappingMetadataColumns.columnName.ToString();
            dataTable.Columns[(int)PhysicalModelMappingMetadataColumns.dataType].ColumnName = PhysicalModelMappingMetadataColumns.dataType.ToString();
            dataTable.Columns[(int)PhysicalModelMappingMetadataColumns.characterLength].ColumnName = PhysicalModelMappingMetadataColumns.characterLength.ToString();
            dataTable.Columns[(int)PhysicalModelMappingMetadataColumns.numericPrecision].ColumnName = PhysicalModelMappingMetadataColumns.numericPrecision.ToString();
            dataTable.Columns[(int)PhysicalModelMappingMetadataColumns.numericScale].ColumnName = PhysicalModelMappingMetadataColumns.numericScale.ToString();
            dataTable.Columns[(int)PhysicalModelMappingMetadataColumns.ordinalPosition].ColumnName = PhysicalModelMappingMetadataColumns.ordinalPosition.ToString();
            dataTable.Columns[(int)PhysicalModelMappingMetadataColumns.primaryKeyIndicator].ColumnName = PhysicalModelMappingMetadataColumns.primaryKeyIndicator.ToString();
            dataTable.Columns[(int)PhysicalModelMappingMetadataColumns.multiActiveIndicator].ColumnName = PhysicalModelMappingMetadataColumns.multiActiveIndicator.ToString();
        }

        /// <summary>
        /// Creates a  object (Json List and DataTable) from a Json file.
        /// </summary>
        /// <returns></returns>
        public void GetMetadata(string directoryName)
        {
            EventLog.Add(Event.CreateNewEvent(EventTypes.Information, $"Retrieving physical model metadata from {directoryName}."));

            DataTable.Clear();

            string[] allFiles = Directory.GetFiles(directoryName, "*.json", SearchOption.AllDirectories);

            List<PhysicalModelGridRow> jsonList = new List<PhysicalModelGridRow>();

            try
            {
                foreach (var file in allFiles)
                {
                    // Deserialize the file.
                    PhysicalModelTable physicalModelTable = JsonConvert.DeserializeObject<PhysicalModelTable>(File.ReadAllText(file));

                    // And convert it to a flat structure fit for the data table.
                    foreach (var column in physicalModelTable.columns)
                    {
                        PhysicalModelGridRow physicalModelGridRow = new PhysicalModelGridRow();
                        physicalModelGridRow.databaseName = physicalModelTable.database;
                        physicalModelGridRow.schemaName = physicalModelTable.schema;
                        physicalModelGridRow.tableName = physicalModelTable.name;
                        physicalModelGridRow.columnName = column.name;
                        physicalModelGridRow.ordinalPosition = column.ordinalPosition;
                        physicalModelGridRow.characterLength = column.characterLength;
                        physicalModelGridRow.numericPrecision = column.numericPrecision;
                        physicalModelGridRow.numericScale = column.numericScale;
                        physicalModelGridRow.dataType = column.dataType;
                        physicalModelGridRow.multiActiveIndicator = column.multiActiveIndicator;
                        physicalModelGridRow.primaryKeyIndicator = column.primaryKeyIndicator;

                        jsonList.Add(physicalModelGridRow);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"A serious error occurred when loading the physical model. The reported error is {exception.Message}");
            }

            // Convert the json list to a DataTable and bind it to the source.
            JsonList = jsonList;
            var dataTable = Utility.ConvertToDataTable(jsonList);

            //Make sure the changes are seen as committed, so that changes can be detected later on.
            dataTable.AcceptChanges();

            // Commit it to the object itself
            DataTable = dataTable;

            // Set the column names.
            SetDataTableColumnNames(DataTable);
        }
    }
}
