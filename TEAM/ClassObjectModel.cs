using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    class VEDW_DataObjectMapping : DataObjectMapping
    {
        // The following columns are VEDW specific for the moment. These can probably be deprecated but for now remain as backward-compatibility while moving to the generic schema
        public string lookupTable { get; set; }
        public string targetTableHashKey { get; set; }
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
        public string sourceDatabaseName { get; } = FormBase.ConfigurationSettings.SourceDatabaseName;
        public string sourceDatabaseConnection { get; } = FormBase.ConfigurationSettings.ConnectionStringSource;
        public string stagingAreaDatabaseName { get; } = FormBase.ConfigurationSettings.StagingDatabaseName;

        public string stagingAreaDatabaseConnection { get; } =
            FormBase.ConfigurationSettings.ConnectionStringSource;

        public string persistentStagingDatabaseName { get; } = FormBase.ConfigurationSettings.PsaDatabaseName;

        public string persistentStagingDatabaseConnection { get; } =
            FormBase.ConfigurationSettings.ConnectionStringSource;

        public string persistentStagingSchemaName { get; } = FormBase.ConfigurationSettings.SchemaName;
        public string integrationDatabaseName { get; } = FormBase.ConfigurationSettings.IntegrationDatabaseName;

        public string integrationDatabaseConnection { get; } =
            FormBase.ConfigurationSettings.ConnectionStringSource;

        public string presentationDatabaseName { get; } = FormBase.ConfigurationSettings.PresentationDatabaseName;

        public string presentationDatabaseConnection { get; } =
            FormBase.ConfigurationSettings.ConnectionStringSource;

        //public string vedwSchemaName { get; } = FormBase.VedwConfigurationSettings.VedwSchema;

        // Attributes
        public string changeDataCaptureAttribute { get; set; } =
            FormBase.ConfigurationSettings.ChangeDataCaptureAttribute;

        public string recordSourceAttribute { get; } = FormBase.ConfigurationSettings.RecordSourceAttribute;
        public string loadDateTimeAttribute { get; } = FormBase.ConfigurationSettings.LoadDateTimeAttribute;
        public string eventDateTimeAttribute { get; set; } = FormBase.ConfigurationSettings.EventDateTimeAttribute;

        public string recordChecksumAttribute { get; set; } =
            FormBase.ConfigurationSettings.RecordChecksumAttribute;

        public string etlProcessAttribute { get; } = FormBase.ConfigurationSettings.EtlProcessAttribute;
        public string sourceRowIdAttribute { get; } = FormBase.ConfigurationSettings.RowIdAttribute;
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
                    //businessKeyEval = "HardCoded";
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

