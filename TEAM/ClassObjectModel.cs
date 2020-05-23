using System;
using System.Collections.Generic;
using System.Linq;
using DataWarehouseAutomation;

namespace TEAM
{
    /// <summary>
    /// The parent object containing the list of source-to-target mappings. This is the highest level and contains the list of mappings (as individual objects
    /// but also the parameters inherited from TEAM and VEDW.
    /// </summary>
    class VEDW_DataObjectMappingList : DataObjectMappingList
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
        public string selectedDataObject { get; set; }
        public DateTime generationDateTime { get; } = DateTime.Now;
    }

    /// <summary>
    /// The parameters that have been inherited from TEAM or are set in VDW, passed as properties of the metadata - and can be used in the templates.
    /// </summary>
    class MetadataConfiguration
    {
        // Databases
        public string sourceDatabaseName { get; } = Form_Base.ConfigurationSettings.SourceDatabaseName;
        public string sourceDatabaseConnection { get; } = Form_Base.ConfigurationSettings.ConnectionStringSource;
        public string stagingAreaDatabaseName { get; } = Form_Base.ConfigurationSettings.StagingDatabaseName;

        public string stagingAreaDatabaseConnection { get; } =
            Form_Base.ConfigurationSettings.ConnectionStringSource;

        public string persistentStagingDatabaseName { get; } = Form_Base.ConfigurationSettings.PsaDatabaseName;

        public string persistentStagingDatabaseConnection { get; } =
            Form_Base.ConfigurationSettings.ConnectionStringSource;

        public string persistentStagingSchemaName { get; } = Form_Base.ConfigurationSettings.SchemaName;
        public string integrationDatabaseName { get; } = Form_Base.ConfigurationSettings.IntegrationDatabaseName;

        public string integrationDatabaseConnection { get; } =
            Form_Base.ConfigurationSettings.ConnectionStringSource;

        public string presentationDatabaseName { get; } = Form_Base.ConfigurationSettings.PresentationDatabaseName;

        public string presentationDatabaseConnection { get; } =
            Form_Base.ConfigurationSettings.ConnectionStringSource;

        //public string vedwSchemaName { get; } = FormBase.VedwConfigurationSettings.VedwSchema;

        // Attributes
        public string changeDataCaptureAttribute { get; set; } =
            Form_Base.ConfigurationSettings.ChangeDataCaptureAttribute;

        public string recordSourceAttribute { get; } = Form_Base.ConfigurationSettings.RecordSourceAttribute;
        public string loadDateTimeAttribute { get; } = Form_Base.ConfigurationSettings.LoadDateTimeAttribute;
        public string eventDateTimeAttribute { get; set; } = Form_Base.ConfigurationSettings.EventDateTimeAttribute;

        public string recordChecksumAttribute { get; set; } =
            Form_Base.ConfigurationSettings.RecordChecksumAttribute;

        public string etlProcessAttribute { get; } = Form_Base.ConfigurationSettings.EtlProcessAttribute;
        public string sourceRowIdAttribute { get; } = Form_Base.ConfigurationSettings.RowIdAttribute;
    }

    class InterfaceHandling
    {
        public static List<DataItemMapping> BusinessKeyComponentMappingList(string sourceBusinessKeyDefinition, string targetBusinessKeyDefinition)
        {
            // Set the return type
            List<DataItemMapping> returnList = new List<DataItemMapping>();

            // Evaluate key components for source and target key definitions
            var sourceBusinessKeyComponentList = businessKeyComponentList(sourceBusinessKeyDefinition);
            var targetBusinessKeyComponentList = businessKeyComponentList(targetBusinessKeyDefinition);

            int counter = 0;

            foreach (string keyPart in sourceBusinessKeyComponentList)
            {
                bool businessKeyEval = false;

                if (keyPart.StartsWith("'") && keyPart.EndsWith("'"))
                {
                    businessKeyEval = true;
                }

                DataItemMapping keyComponent = new DataItemMapping();

                DataItem sourceColumn = new DataItem();
                DataItem targetColumn = new DataItem();

                sourceColumn.name = keyPart;
                sourceColumn.isHardCodedValue = businessKeyEval;

                keyComponent.sourceDataItem = sourceColumn;

                var indexExists = targetBusinessKeyComponentList.ElementAtOrDefault(counter) != null;
                if (indexExists)
                {
                    targetColumn.name = targetBusinessKeyComponentList[counter];
                }
                else
                {
                    targetColumn.name = "";
                }

                keyComponent.targetDataItem = targetColumn;

                returnList.Add(keyComponent);
                counter++;
            }

            return returnList;
        }

        private static List<string> businessKeyComponentList(string sourceBusinessKeyDefinition)
        {
            List<string> temporaryBusinessKeyComponentList = new List<string>();
            temporaryBusinessKeyComponentList =
                sourceBusinessKeyDefinition.Split(',').ToList(); // Split by the comma first to get the key parts

            List<string> sourceBusinessKeyComponentList = new List<string>();

            foreach (var keyComponent in temporaryBusinessKeyComponentList)
            {
                var keyPart = keyComponent.TrimStart().TrimEnd();
                keyPart = keyComponent.Replace("(", "").Replace(")", "").Replace(" ", "");

                if (keyPart.StartsWith("COMPOSITE"))
                {
                    keyPart = keyPart.Replace("COMPOSITE", "");

                    var temporaryKeyPartList = keyPart.Split(';').ToList();
                    foreach (var item in temporaryKeyPartList)
                    {
                        sourceBusinessKeyComponentList.Add(item);
                    }
                }
                else if (keyPart.StartsWith("CONCATENATE"))
                {
                    keyPart = keyPart.Replace("CONCATENATE", "");
                    keyPart = keyPart.Replace(";", "+");

                    sourceBusinessKeyComponentList.Add(keyPart);
                }
                else
                {
                    sourceBusinessKeyComponentList.Add(keyPart);
                }
            }

            sourceBusinessKeyComponentList = sourceBusinessKeyComponentList.Select(t => t.Trim()).ToList();
            return sourceBusinessKeyComponentList;
        }

        internal static string EvaluateBusinessKey(DataItemMapping businessKey)
        {
            var businessKeyEval = "";
            if (businessKey.sourceDataItem.name.Contains("'"))
            {
                businessKeyEval = businessKey.sourceDataItem.name;
            }
            else
            {
                businessKeyEval = "[" + businessKey.sourceDataItem.name + "]";
            }

            return businessKeyEval;
        }
    }
}

