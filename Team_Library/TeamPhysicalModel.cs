﻿using System.Collections.Generic;
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
        HashKey = 0,
        DatabaseName = 1,
        SchemaName = 2,
        TableName = 3,
        ColumnName = 4,
        DataType = 5,
        CharacterLength = 6,
        NumericPrecision = 7,
        NumericScale = 8,
        OrdinalPosition = 9,
        PrimaryKeyIndicator = 10,
        MultiActiveIndicator = 11
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
            DataTable.DefaultView.Sort = $"[{PhysicalModelMappingMetadataColumns.DatabaseName}] ASC, [{PhysicalModelMappingMetadataColumns.SchemaName}] ASC, [{PhysicalModelMappingMetadataColumns.TableName}] ASC, [{PhysicalModelMappingMetadataColumns.OrdinalPosition}] ASC";
        }

        /// <summary>
        /// Set the column names for the data table.
        /// </summary>
        public void SetDataTableColumns()
        {
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.HashKey].ColumnName = PhysicalModelMappingMetadataColumns.HashKey.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.DatabaseName].ColumnName = PhysicalModelMappingMetadataColumns.DatabaseName.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.SchemaName].ColumnName = PhysicalModelMappingMetadataColumns.SchemaName.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.TableName].ColumnName = PhysicalModelMappingMetadataColumns.TableName.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.ColumnName].ColumnName = PhysicalModelMappingMetadataColumns.ColumnName.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.DataType].ColumnName = PhysicalModelMappingMetadataColumns.DataType.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.CharacterLength].ColumnName = PhysicalModelMappingMetadataColumns.CharacterLength.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.NumericPrecision].ColumnName = PhysicalModelMappingMetadataColumns.NumericPrecision.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.NumericScale].ColumnName = PhysicalModelMappingMetadataColumns.NumericScale.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.OrdinalPosition].ColumnName = PhysicalModelMappingMetadataColumns.OrdinalPosition.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator].ColumnName = PhysicalModelMappingMetadataColumns.PrimaryKeyIndicator.ToString();
            DataTable.Columns[(int)PhysicalModelMappingMetadataColumns.MultiActiveIndicator].ColumnName = PhysicalModelMappingMetadataColumns.MultiActiveIndicator.ToString();
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
