using System;
using DataWarehouseAutomation;
using TEAM_Library;

namespace TEAM
{
    /// <summary>
    /// The parent object containing the list of source-to-target mappings. This is the highest level and contains the list of mappings (as individual objects
    /// but also the parameters inherited from TEAM and VEDW.
    /// </summary>
    public class VDW_DataObjectMappingList : DataObjectMappings
    {
        // Generic interface definitions
        //public List<DataObjectMapping> dataObjectMapping { get; set; }

        // TEAM and VDW specific details
        public MetadataConfiguration metadataConfiguration { get; set; }
        public GenerationSpecificMetadata generationSpecificMetadata { get; set; }
    }

    /// <summary>
    /// Specific metadata related for generation purposes, but which is relevant to use in templates.
    /// </summary>
    public class GenerationSpecificMetadata
    {
        public DataObject selectedDataObject { get; set; }
        //public DateTime generationDateTime { get; } = DateTime.Now;

        public GenerationSpecificMetadata(DataObject dataObject)
        {
            selectedDataObject = dataObject;
        }
    }

    /// <summary>
    /// The parameters that have been inherited from TEAM or are set in VDW, passed as properties of the metadata - and can be used in the templates.
    /// </summary>
    public class MetadataConfiguration
    {
        // Attributes
        public string changeDataCaptureAttribute { get; set; }
        public string recordSourceAttribute { get; set; }
        public string loadDateTimeAttribute { get; set; }
        public string expiryDateTimeAttribute { get; set; }
        public string eventDateTimeAttribute { get; set; }
        public string recordChecksumAttribute { get; set; }
        public string etlProcessAttribute { get; set; }
        public string sourceRowIdAttribute { get; set; }

        public MetadataConfiguration(TeamConfiguration teamConfiguration)
        {
            changeDataCaptureAttribute = teamConfiguration.ChangeDataCaptureAttribute;
            recordSourceAttribute = teamConfiguration.RecordSourceAttribute;
            loadDateTimeAttribute = teamConfiguration.LoadDateTimeAttribute;
            expiryDateTimeAttribute = teamConfiguration.ExpiryDateTimeAttribute;
            eventDateTimeAttribute = teamConfiguration.EventDateTimeAttribute;
            recordChecksumAttribute = teamConfiguration.RecordChecksumAttribute;
            etlProcessAttribute = teamConfiguration.EtlProcessAttribute;
            sourceRowIdAttribute = teamConfiguration.RowIdAttribute;
        }
    }
}