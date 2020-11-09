using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

namespace TEAM
{
    public class TableMappingJson
    {
        //JSON representation of the table mapping metadata
        public bool enabledIndicator { get; set; }
        public string tableMappingHash { get; set; }
        public int versionId { get; set; }
        public string sourceTable { get; set; }
        public string sourceConnectionKey { get; set; }
        public string targetTable { get; set; }
        public string targetConnectionKey { get; set; }
        public string businessKeyDefinition { get; set; }
        public string drivingKeyDefinition { get; set; }
        public string filterCriteria { get; set; }
    }

    public class AttributeMappingJson
    {
        //JSON representation of the attribute mapping metadata
        public string attributeMappingHash { get; set; }
        public int versionId { get; set; }
        public string sourceTable { get; set; }
        public string sourceAttribute { get; set; }
        public string targetTable { get; set; }
        public string targetAttribute { get; set; }
        public string notes { get; set; }
    }

    public class PhysicalModelMetadataJson
    {
        //JSON representation of the physical model metadata
        public string versionAttributeHash { get; set; }
        public int versionId { get; set; }
        public string databaseName { get; set; }
        public string schemaName { get; set; }
        public string tableName { get; set; }
        public string columnName { get; set; }
        public string dataType { get; set; }
        public string characterLength { get; set; }
        public string numericPrecision { get; set; }
        public string numericScale{ get; set; }
        public string ordinalPosition { get; set; }
        public string primaryKeyIndicator { get; set; }
        public string multiActiveIndicator { get; set; }
    }
    }
