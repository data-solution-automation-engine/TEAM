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
    class VDW_DataObjectMappingList : DataObjectMappings
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
        // Attributes
        public string changeDataCaptureAttribute { get; set; } 

        public string recordSourceAttribute { get; set; } 
        public string loadDateTimeAttribute { get; set; } 
        public string eventDateTimeAttribute { get; set; } 

        public string recordChecksumAttribute { get; set; }

        public string etlProcessAttribute { get; set; } 
        public string sourceRowIdAttribute { get; set; }
        
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

                List<dynamic> sourceColumns = new List<dynamic>();

                DataItem sourceColumn = new DataItem();
                DataItem targetColumn = new DataItem();

                sourceColumn.name = keyPart;
                sourceColumn.isHardCodedValue = businessKeyEval;

                sourceColumns.Add(sourceColumn);

                keyComponent.sourceDataItems = sourceColumns;

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
            if (businessKey.sourceDataItems[0].name.Contains("'"))
            {
                businessKeyEval = businessKey.sourceDataItems[0].name;
            }
            else
            {
                businessKeyEval = "[" + businessKey.sourceDataItems[0].name + "]";
            }

            return businessKeyEval;
        }
    }
}

